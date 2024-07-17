using FileServer.Database;
using FileServer.Services.Application;
using FileServer.StaticData;
using FileServer.ViewModels;
using FileServer.ViewModels.Enum;
using FileServer.ViewModels.Setting;
using ImageProcessor;
using ImageProcessor.Imaging.Formats;
using ImageProcessor.Plugins.WebP.Imaging.Formats;
using LazZiya.ImageResize;
using Microsoft.Extensions.Options;
using MimeDetective;
using Newtonsoft.Json;
using System.Drawing;
using System.Text.RegularExpressions;

namespace FileServer.Services.Document
{
    public class DocumentService : IDocumentService
    {
        #region Fields
        private readonly IOptionsSnapshot<SiteSettings> _siteSettings;
        private readonly Microsoft.AspNetCore.Hosting.IHostingEnvironment _hostingEnvironment;
        private IRepository<Models.Document> _documentRepository;
        private IRepository<Models.Application> _applicationRepository;
        private readonly IApplicationService _applicationService;

        #endregion
        public DocumentService(
            IOptionsSnapshot<SiteSettings> siteSettings,
            Microsoft.AspNetCore.Hosting.IHostingEnvironment hostingEnvironment,
            IRepository<Models.Document> documentRepository,
            IRepository<Models.Application> applicationRepository,
            IApplicationService applicationService
        )
        {
            _siteSettings = siteSettings;
            _hostingEnvironment = hostingEnvironment;
            _documentRepository = documentRepository;
            _applicationRepository = applicationRepository;
            _applicationService = applicationService;
        }

        #region AddDocumentProccess
        public async Task<Result<Models.Document>> AddDocumentProccess(AddDocument document, string apiKey, CancellationToken cancellationToken)
        {
            var result = new Result<Models.Document>();
            var app = await _applicationService.GetApplication(apiKey: apiKey, isActive: true);
            string clientDirectory = Path.Combine(app.Data.DirectoryPath, StaticVariables.OriginalDirectory);
            if (document.Tag != null)
                clientDirectory = Path.Combine(clientDirectory, document.Tag);

            var exportedFileId = Guid.NewGuid();
            var addImageToDirectory = await AddImageToFolder(
                        file: document.FileStream,
                        filePathDirectory: clientDirectory,
                        fileId: exportedFileId,
                        applicationId: app.Data.Id,
                        cancellationToken: cancellationToken);
            if (!addImageToDirectory.Success)
            {
                result.Success = false;
                result.Message = addImageToDirectory.Message;
                return result;
            }
            FileInfo exportedFileInfo = new FileInfo(addImageToDirectory.Data);

            System.Drawing.Image img = System.Drawing.Image.FromFile(addImageToDirectory.Data);
            var addDocumentInput = new AddDocumentInput();
            addDocumentInput.Id = exportedFileId;
            addDocumentInput.FileName = document.FileName ?? null;
            addDocumentInput.FileLength = document.FileStream.Length;
            addDocumentInput.FileWidth = img.Width;
            addDocumentInput.FileHeight = img.Height;
            addDocumentInput.FileExtention = exportedFileInfo.Extension;
            addDocumentInput.ClientDocumentId = document.Id;
            addDocumentInput.Tag = document.Tag;
            addDocumentInput.AppId = app.Data.Id;
            var savedDocument = await AddDocument(addDocumentInput, cancellationToken);

            img.Dispose();

            if (!savedDocument.Success)
            {
                result.Success = false;
                result.Message = savedDocument.Message;
                return result;
            }
            result.Data = savedDocument.Data;
            return result;
        }
        #endregion
        #region DeleteDocumentProccess
        public async Task<Result> DeleteDocumentProccess(Guid documentId, string apiKey, CancellationToken cancellationToken)
        {
            var result = new Result();

            var app = await _applicationService.GetApplication(apiKey: apiKey, isActive: true);

            var selectedDocument = await _documentRepository.GetAsync(predicate: x => x.Id == documentId, cancellationToken: cancellationToken);

            if (selectedDocument == null)
            {
                result.Success = false;
                result.Message = Resources.Global.DocumentNotFound;
                return result;
            }
            if (app.Data.Id != selectedDocument.ApplicationId)
            {
                result.Success = false;
                result.Message = Resources.Global.AppNotFound;
                return result;
            }
            // delete from db
            await _documentRepository.DeleteAsync(selectedDocument, cancellationToken);

            // delete original Document
            var originalDocuments = await GetDocumentPath(document: selectedDocument, pathType: DocumentPathType.Original, cancellationToken: cancellationToken);
            if (originalDocuments.FirstOrDefault() != null && System.IO.File.Exists(originalDocuments.FirstOrDefault()))
                System.IO.File.Delete(originalDocuments.First());

            // delete cache Documents
            var cacheDocuments = await GetDocumentPath(document: selectedDocument, pathType: DocumentPathType.Cache, cancellationToken: cancellationToken);
            foreach (var cacheDocument in cacheDocuments)
            {
                if (System.IO.File.Exists(cacheDocument))
                    System.IO.File.Delete(cacheDocument);
            }
            return result;
        }
        #endregion

        #region GetDocumentProccess
        public async Task<Result<string>> GetDocumentProccess(Guid documentId, string parameters, CancellationToken cancellationToken, IInclude<Models.Document> include = null)
        {
            var result = new Result<string>();
            try
            {
                var documentParameters = GetDocumentParameters(parameters, cancellationToken);
                if (!documentParameters.Success)
                    throw new Exception(documentParameters.Message);

                var selectedDocument = await _documentRepository.GetAsync(predicate: x => x.Id == documentId, include: include, cancellationToken: cancellationToken);
                if (selectedDocument == null)
                    throw new Exception(Resources.Global.DocumentNotFound);

                var appsettings = selectedDocument.Application.AppSettings;
                string originalImagePath = (await GetDocumentPath(document: selectedDocument, pathType: DocumentPathType.Original, cancellationToken: cancellationToken)).FirstOrDefault();

                string exportImagePath = Path.Combine(_hostingEnvironment.WebRootPath, selectedDocument.Application.DirectoryPath, StaticVariables.CacheDirectory);
                if (!Directory.Exists(exportImagePath)) Directory.CreateDirectory(exportImagePath);
                exportImagePath = exportImagePath + "//" + selectedDocument.Id;

                var resizeWidth = documentParameters.Data.DocumentSize?.Width;
                var resizeHeight = documentParameters.Data.DocumentSize?.Height;
                if (resizeWidth != null)
                    exportImagePath = exportImagePath + StaticVariables.CharacterSeparator + "w" + resizeWidth.Value;
                if (resizeHeight != null)
                    exportImagePath = exportImagePath + StaticVariables.CharacterSeparator + "h" + resizeHeight.Value;

                if (documentParameters.Data.Format != null)
                    exportImagePath = exportImagePath + "." + documentParameters.Data.Format;
                else
                    exportImagePath = exportImagePath + selectedDocument.FileExtention;

                if (System.IO.File.Exists(exportImagePath))
                {
                    result.Data = exportImagePath;
                    return result;
                }

                string tempoImagePath = string.Empty;

                var appSettingObject = (await _applicationService.GetApplicationSettings(selectedDocument.ApplicationId, cancellationToken)).Data;

                #region addWatermarkToImage

                if (appsettings != null && appSettingObject.Watermarks != null)
                {
                    if (tempoImagePath != string.Empty)
                        originalImagePath = tempoImagePath;

                    var watermarks = appSettingObject.Watermarks.ToList();

                    var task = new Task<Result<string>>(() =>
                    {
                        var watermarked = WatermarkImage(
                        watermarks: watermarks,
                        document: selectedDocument,
                        srcImagePath: originalImagePath,
                        cancellationToken: cancellationToken
                        );
                        return watermarked;

                    });
                    task.RunSynchronously();

                    var watermarkedImage = task.Result;

                    if (System.IO.File.Exists(tempoImagePath))
                        System.IO.File.Delete(tempoImagePath);

                    if (watermarkedImage.Success)
                        tempoImagePath = watermarkedImage.Data;
                }
                #endregion


                #region resizeImage
                if (resizeWidth != null || resizeHeight != null)
                {
                    if (tempoImagePath != string.Empty)
                        originalImagePath = tempoImagePath;

                    var resizedImage = ResizeImage(documentSize: documentParameters.Data.DocumentSize, srcPatch: originalImagePath, document: selectedDocument, cancellationToken: cancellationToken);

                    if (!resizedImage.Success)
                        throw new Exception(resizedImage.Message);

                    if (System.IO.File.Exists(tempoImagePath))
                        System.IO.File.Delete(tempoImagePath);

                    tempoImagePath = resizedImage.Data;

                }
                #endregion

                #region changeImageFormat
                if (documentParameters.Data.Format != null)
                {
                    if (tempoImagePath != string.Empty)
                        originalImagePath = tempoImagePath;

                    if (appSettingObject.FileSettings != null && appSettingObject.FileSettings.PermittedExtentions != null)
                    {
                        if (!appSettingObject.FileSettings.PermittedExtentions.Contains(selectedDocument.FileExtention.Replace(".", "")))
                            throw new Exception(Resources.Global.FileFormatIsIncorrect);
                    }

                    var changeImageFormat = ChangeImageFormat(
                        srcImagePath: originalImagePath,
                        exportImagePath: exportImagePath,
                        imageFormat: documentParameters.Data.Format.ToLower(),
                        cancellationToken: cancellationToken
                        );

                    if (!changeImageFormat.Success)
                        throw new Exception(changeImageFormat.Message);

                    if (System.IO.File.Exists(tempoImagePath))
                        System.IO.File.Delete(tempoImagePath);
                }
                #endregion
                result.Data = exportImagePath;

            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
                result.Success = false;
            }
            return result;
        }
        #endregion

        #region AddWatermarkProccess
        public async Task<Result> AddWatermarkProccess(AddWatermark addWatermark, string apiKey, CancellationToken cancellationToken)
        {
            var result = new Result();
            try
            {
                var app = await _applicationService.GetApplication(apiKey: apiKey, isActive: true);

                var appSettingObject = (await _applicationService.GetApplicationSettings(app.Data.Id, cancellationToken)).Data;

                if (appSettingObject.Watermarks == null)
                    appSettingObject.Watermarks = new List<WatermarkInput>().ToArray();

                var watermarks = appSettingObject.Watermarks.ToList();

                foreach (var watermark in watermarks)
                {
                    if (addWatermark.Tags.Any(tag => watermark.Tags.Contains(tag)))
                        throw new Exception(Resources.Global.TagHadWatermark);
                }

                WatermarkInput newWatermark = new WatermarkInput();
                newWatermark.Id = Guid.NewGuid();
                newWatermark.Tags = addWatermark.Tags;

                if (addWatermark.TextWaterMark == null && addWatermark.ImageWaterMark == null)
                    throw new Exception(Resources.Global.WatermarkHasNotType);

                if (addWatermark.TextWaterMark != null)
                {
                    newWatermark.WatermarkType = newWatermark.WatermarkType | WatermarkType.Text;
                    newWatermark.TextWaterMarkOptions = addWatermark.TextWaterMark.TextWatermarkOptions;
                    newWatermark.TextWaterMark = addWatermark.TextWaterMark.Title;
                }
                if (addWatermark.ImageWaterMark != null)
                {
                    newWatermark.WatermarkType = newWatermark.WatermarkType | WatermarkType.Image;
                    newWatermark.ImageWatermarkOptions = addWatermark.ImageWaterMark.ImageWatermarkOptions;

                    var addImageToDirectory = await AddImageToFolder(
                        file: addWatermark.ImageWaterMark.FileStream,
                        filePathDirectory: Path.Combine(app.Data.DirectoryPath, StaticVariables.OriginalDirectory, StaticVariables.WatermarkDirectory),
                        fileId: newWatermark.Id,
                        applicationId: app.Data.Id,
                        cancellationToken: cancellationToken);

                    if (!addImageToDirectory.Success)
                        throw new Exception(addImageToDirectory.Message);
                }

                watermarks.Add(newWatermark);
                appSettingObject.Watermarks = watermarks.ToArray();

                var newAppSettings = JsonConvert.SerializeObject(appSettingObject);
                app.Data.AppSettings = newAppSettings;
                _applicationRepository.Update(app.Data);

            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
                result.Success = false;
            }
            return result;
        }
        #endregion

        #region AddDocument
        public async Task<Result<Models.Document>> AddDocument(AddDocumentInput addDocumentInput, CancellationToken cancellationToken)
        {
            var result = new Result<Models.Document>();
            var document = new Models.Document
            {
                Id = addDocumentInput.Id,
                ApplicationId = addDocumentInput.AppId,
                ClientDocumentId = addDocumentInput.ClientDocumentId,
                FileExtention = addDocumentInput.FileExtention,
                FileName = addDocumentInput.FileName,
                Tag = addDocumentInput.Tag,
                FileLength = addDocumentInput.FileLength,
                FileWidth = addDocumentInput.FileWidth,
                FileHeight = addDocumentInput.FileHeight
            };
            await _documentRepository.AddAsync(document, cancellationToken);
            result.Data = document;
            return result;
        }
        #endregion

        #region GetDocumentPath
        private async Task<List<string>> GetDocumentPath(Models.Document document, DocumentPathType pathType, CancellationToken cancellationToken)
        {
            var patchList = new List<string>();
            try
            {
                var app = await _applicationRepository.GetAsync(predicate: x => x.Id == document.ApplicationId,
                                   cancellationToken: cancellationToken);
                var documentDirectory = "";
                if (pathType == DocumentPathType.Original)
                    documentDirectory = StaticVariables.OriginalDirectory;
                else
                    documentDirectory = StaticVariables.CacheDirectory;

                documentDirectory = Path.Combine(app.DirectoryPath, documentDirectory);

                if (pathType == DocumentPathType.Original)
                {
                    if (document.Tag != null)
                        documentDirectory = Path.Combine(documentDirectory, document.Tag);

                    patchList.Add(Path.Combine(_hostingEnvironment.WebRootPath, documentDirectory) + "//" + document.Id + document.FileExtention);
                }
                else
                {
                    DirectoryInfo d = new DirectoryInfo(Path.Combine(_hostingEnvironment.WebRootPath, documentDirectory)); //Assuming Test is your Folder
                    FileInfo[] files = d.GetFiles(document.Id.ToString() + StaticVariables.CharacterSeparator + "*"); //Getting Text files
                    foreach (var file in files)
                    {
                        patchList.Add(file.FullName);
                    }
                }
            }
            catch (Exception ex)
            {
            }

            return patchList;
        }
        #endregion

        #region GetDocumentParameters
        private Result<DocumentOperation> GetDocumentParameters(string parameters, CancellationToken cancellationToken)
        {
            var result = new Result<DocumentOperation>();
            try
            {
                result.Data = new DocumentOperation();
                var imageRegex = new Regex(@"([^image](?<resize>resize,([^\/]*|.*$))|(?<format>format,([^\/]*|.*$)))");
                var imageRegexmatches = imageRegex.Matches(parameters);

                if (imageRegexmatches.Count() < 1)
                    return result;


                var matchedResize = imageRegexmatches.Where(i => i.Groups["resize"].Value != "");
                var matchedFormat = imageRegexmatches.Where(i => i.Groups["format"].Value != "");

                if (matchedResize.Count() > 1 || matchedFormat.Count() > 1)
                    throw new Exception(Resources.Global.InputFormatIsIncorrect);


                if (matchedResize.Count() == 1)
                {

                    var matchedResizeValue = matchedResize.FirstOrDefault().Value
                            .Replace("resize", "")
                            .Replace("/", "")
                            .Trim();
                    var regex = new Regex(@"((?<width>w_([1-9][0-9]{2,3}(,|\/|$)))|(?<height>h_([1-9][0-9]{2,3}(,|\/|$))))");
                    var matches = regex.Matches(matchedResizeValue);

                    var matchedWidth = matches.Where(i => i.Groups["width"].Value != "");
                    var matchedHeight = matches.Where(i => i.Groups["height"].Value != "");

                    if (matchedWidth.Count() > 1 || matchedHeight.Count() > 1)
                    {
                        throw new Exception(Resources.Global.InputFormatIsIncorrect);
                    }
                    var documentSize = new DocumentSize();
                    if (matchedWidth.Count() == 1)
                    {
                        var matchedValue = matchedWidth.FirstOrDefault().Value
                            .Replace("w_", "")
                            .Replace(",", "")
                            .Replace("/", "")
                            .Trim();

                        documentSize.Width = Int32.Parse(matchedValue);
                    }
                    if (matchedHeight.Count() == 1)
                    {
                        var matchedValue = matchedHeight.FirstOrDefault().Value
                            .Replace("h_", "")
                            .Replace(",", "")
                            .Replace("/", "")
                            .Trim();

                        documentSize.Height = Int32.Parse(matchedValue);
                    }
                    result.Data.DocumentSize = documentSize;
                }
                if (matchedFormat.Count() == 1)
                {
                    var matchedFormatValue = matchedFormat.FirstOrDefault().Value
                            .Replace("format", "")
                            .Replace(",", "")
                            .Replace("/", "")
                            .Trim();

                    result.Data.Format = matchedFormatValue;
                }
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
                result.Success = false;
            }

            return result;
        }
        #endregion


        #region WatermarkImage
        private Result<string> WatermarkImage(
            List<WatermarkInput> watermarks,
            Models.Document document,
            string srcImagePath,
            CancellationToken cancellationToken)
        {
            var result = new Result<string>();
            var tempoImagePath = "";
            try
            {
                if (watermarks.Count() < 1)
                    throw new Exception(Resources.Global.ImageHasNotWatermark);

                foreach (var watermark in watermarks)
                {
                    if (watermark.Tags.Contains(document.Tag))
                    {
                        using (var img = Image.FromFile(srcImagePath))
                        {
                            FileInfo exportedFileInfo = new FileInfo(srcImagePath);
                            tempoImagePath = Path.Combine(_hostingEnvironment.WebRootPath, document.Application.DirectoryPath, StaticVariables.CacheDirectory) + "//" + document.Id + StaticVariables.CharacterSeparator + "wm" + exportedFileInfo.Extension;
                            if (watermark.WatermarkType.HasFlag(WatermarkType.Text))
                            {
                                img.AddTextWatermark(watermark.TextWaterMark).SaveAs(tempoImagePath);
                            }
                            if (watermark.WatermarkType.HasFlag(WatermarkType.Image))
                            {
                                var watermarkImage = Path.Combine(_hostingEnvironment.WebRootPath, document.Application.DirectoryPath, StaticVariables.CacheDirectory) + "//" + watermark.Id + StaticVariables.CharacterSeparator + "w" + document.FileWidth + StaticVariables.CharacterSeparator + "h" + document.FileHeight + ".png";
                                if (!System.IO.File.Exists(watermarkImage))
                                {
                                    var watermarksrc = Path.Combine(_hostingEnvironment.WebRootPath, document.Application.DirectoryPath, StaticVariables.OriginalDirectory, StaticVariables.WatermarkDirectory) + "//" + watermark.Id + ".png";
                                    using (var image = Image.FromFile(watermarksrc))
                                    {
                                        if (document.FileWidth >= document.FileHeight)
                                            image.ScaleByHeight((int)document.FileHeight)
                                                .SaveAs(watermarkImage);
                                        else
                                            image.ScaleByWidth((int)document.FileWidth)
                                                .SaveAs(watermarkImage);
                                        image.Dispose();
                                    }
                                }
                                img.AddImageWatermark(watermarkImage).SaveAs(tempoImagePath);                                
                            }
                            img.Dispose();
                        }
                    }
                    else
                        throw new Exception(Resources.Global.ImageHasNotWatermark);

                    result.Data = tempoImagePath;
                }
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message;
            }
            return result;
        }
        #endregion


        #region ResizeImage
        private Result<string> ResizeImage(DocumentSize documentSize, string srcPatch, Models.Document document, CancellationToken cancellationToken)
        {
            var result = new Result<string>();
            string resizedImagePatch = Path.Combine(_hostingEnvironment.WebRootPath, document.Application.DirectoryPath, StaticVariables.CacheDirectory) + "//" + document.Id;
            if (documentSize.Width != null) resizedImagePatch = resizedImagePatch + StaticVariables.CharacterSeparator + "w" + documentSize.Width;
            if (documentSize.Height != null) resizedImagePatch = resizedImagePatch + StaticVariables.CharacterSeparator + "h" + documentSize.Height;
            resizedImagePatch = resizedImagePatch + document.FileExtention;
            try
            {
                var widthRatio = documentSize?.Width == null ? 0 : (double)documentSize?.Width / (double)document.FileWidth;
                var heightRatio = documentSize?.Height == null ? 0 : (double)documentSize?.Height / (double)document.FileHeight;
                if (widthRatio > 1 || heightRatio > 1)
                {
                    //throw new Exception(Resources.Global.ResizeParametersIsBigerThanOriginal);
                    documentSize.Width = Convert.ToInt32(document.FileWidth);
                    documentSize.Height = Convert.ToInt32(document.FileHeight);
                }

                using (var img = Image.FromFile(srcPatch))
                {
                    if (documentSize.Width != null && documentSize.Height != null)
                    {
                        if (widthRatio >= heightRatio)
                            img.ScaleByHeight(documentSize.Height.Value)
                                .SaveAs(resizedImagePatch);
                        else
                            img.ScaleByWidth(documentSize.Width.Value)
                                .SaveAs(resizedImagePatch);
                    }
                    else if (documentSize.Width != null && documentSize.Height == null)
                        img.ScaleByWidth(documentSize.Width.Value)
                            .SaveAs(resizedImagePatch);
                    else
                        img.ScaleByHeight(documentSize.Height.Value)
                            .SaveAs(resizedImagePatch);

                    img.Dispose();
                }
                result.Data = resizedImagePatch;

            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message;
            }

            return result;
        }
        #endregion

        #region ChangeImageFormat
        private Result<string> ChangeImageFormat(
            string srcImagePath,
            string exportImagePath,
            string imageFormat,
            CancellationToken cancellationToken)
        {
            var result = new Result<string>();
            try
            {
                using (var fileStream = new FileStream(exportImagePath, FileMode.Create))
                {
                    using (ImageFactory imageFactory = new ImageFactory(preserveExifData: false))
                    {
                        using (var sourceImage = Image.FromFile(srcImagePath))
                        {
                            imageFactory.Load(sourceImage);

                            if (imageFormat == "webp")
                                imageFactory.Format(new WebPFormat());
                            else if (imageFormat == "jpeg")
                                imageFactory.Format(new JpegFormat());
                            else if (imageFormat == "png")
                                imageFactory.Format(new PngFormat());
                            else
                                throw new Exception(Resources.Global.FileFormatIsIncorrect);

                            imageFactory.Quality(100)
                            .Save(fileStream);

                            sourceImage.Dispose();
                        }

                        imageFactory.Dispose();
                    }
                    fileStream.Dispose();
                }
                result.Data = exportImagePath;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message;
            }
            return result;
        }
        #endregion


        #region AddImageToFolder
        private async Task<Result<string>> AddImageToFolder(byte[] file, string filePathDirectory, Guid fileId, int applicationId, CancellationToken cancellationToken)
        {
            var result = new Result<string>();

            var appSettingObject = (await _applicationService.GetApplicationSettings(applicationId, cancellationToken)).Data;

            var allowedFileExtensions = new List<string>();
            double maximumFileSize = _siteSettings.Value.MaximumFileSize;
            if (appSettingObject.FileSettings != null)
            {
                if (appSettingObject.FileSettings.PermittedExtentions != null)
                    allowedFileExtensions = appSettingObject.FileSettings.PermittedExtentions.Split(',').ToList();

                if (appSettingObject.FileSettings.MaximumFileSize != null)
                    maximumFileSize = appSettingObject.FileSettings.MaximumFileSize.Value;
            }

            try
            {
                if (maximumFileSize < file.Length)
                    throw new Exception(Resources.Global.FileSizeIsBig);

                string fileDirectory = Path.Combine(_hostingEnvironment.WebRootPath, filePathDirectory);
                if (!Directory.Exists(fileDirectory)) Directory.CreateDirectory(fileDirectory);

                string filePath = "";
                var Inspector = new ContentInspectorBuilder()
                {
                    Definitions = MimeDetective.Definitions.Default.All()
                }.Build();

                using (var ms = new MemoryStream(file))
                {
                    var definitionMatches = Inspector.Inspect(file);

                    if (definitionMatches.Length == 0)
                        throw new Exception(Resources.Global.FileFormatIsIncorrect);

                    string exportedFileExtension = definitionMatches[0].Definition.File.Extensions[0];
                    if (allowedFileExtensions.Count() > 0 && !allowedFileExtensions.Contains(exportedFileExtension))
                        throw new Exception(Resources.Global.FileFormatIsIncorrect);

                    filePath = fileDirectory + "//" + fileId + "." + exportedFileExtension;
                    using (var fileStream = new FileStream(filePath, System.IO.FileMode.Create, System.IO.FileAccess.Write))
                    {
                        ms.CopyTo(fileStream);
                        fileStream.Dispose();
                    }
                    ms.Dispose();
                }
                result.Data = filePath;
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
                result.Success = false;
            }
            return result;
        }
        #endregion
    }
}