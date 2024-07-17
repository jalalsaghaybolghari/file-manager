using FileServer.Database;
using FileServer.Framework.Authentication;
using FileServer.Services.Document;
using FileServer.ViewModels;
using FileServer.ViewModels.Setting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using System.Text.Json;

namespace FileServer.Controllers
{
    [Route("api/v1")]
    [ApiController]
    public class MainController : ControllerBase
    {
        #region Fields
        private readonly IOptionsSnapshot<SiteSettings> _siteSettings;
        private readonly IDocumentService _documentService;
        #endregion
        #region Constructor
        public MainController(
            IOptionsSnapshot<SiteSettings> siteSettings,
            IDocumentService documentService
        )
        {
            _siteSettings = siteSettings;
            _documentService = documentService;
        }
        #endregion
        

        #region Add Document
        [HttpPost]
        [Route("addDocument")]
        [ServiceFilter(typeof(ApiKeyAuthFilter))]
        public async Task<Result<string>> AddDocument(
            [FromBody] AddDocument document,
            CancellationToken cancellationToken
        )
        {
            var result = new Result<string>();
            try
            {
                Request.Headers.TryGetValue(_siteSettings.Value.ApiKeyHeaderName, out var extractedApiKey);

                var addDocumentResult = await _documentService.AddDocumentProccess(document: document, apiKey: extractedApiKey, cancellationToken: cancellationToken);
                if (!addDocumentResult.Success)
                    throw new Exception(addDocumentResult.Message);

                result.Data = addDocumentResult.Data.Id.ToString();
                
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message;
            }
            return result;
        }
        #endregion

        #region Get Document
        [HttpGet]
        [Route("getDocument/{id}")]
        public async Task GetDocument(
        [FromRoute] string id,
        [FromQuery(Name = "x-oss-process")] string parameters,
        CancellationToken cancellationToken
        )
        {
            var result = new Result();
            try
            {                
                Guid documentId = new Guid(id);
                var res = await _documentService.GetDocumentProccess(
                    documentId: documentId,
                    parameters: parameters,
                    include: new Include<Models.Document>(query =>
                    {
                        query = query.Include(x => x.Application);
                        return query;
                    }),
                    cancellationToken: cancellationToken);

                if(!res.Success)
                    throw new Exception(res.Message);

                var fileStream = System.IO.File.Open(res.Data, FileMode.Open);
                var memoryStream = new MemoryStream();
                fileStream.CopyTo(memoryStream);
                fileStream.Close();
                byte[] byteArray = memoryStream.ToArray();
                HttpContext.Response.ContentType = "image/" + Path.GetExtension(res.Data).Replace(".", "");
                HttpContext.Response.Headers[HeaderNames.CacheControl] = "public, max-age=2592000";
                await HttpContext.Response.Body.WriteAsync(byteArray);

            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message;
                HttpContext.Response.ContentType = "application/json";
                var json = JsonSerializer.Serialize(result);
                await HttpContext.Response.WriteAsync(json);
            }
        }
        #endregion

        #region DELETE Document
        [HttpDelete]
        [Route("deleteDocument/{id}")]
        [ServiceFilter(typeof(ApiKeyAuthFilter))]
        public async Task<Result> Delete(
            [FromRoute] string id,
            CancellationToken cancellationToken
        )
        {
            var result = new Result();
            try
            {
                Request.Headers.TryGetValue(_siteSettings.Value.ApiKeyHeaderName, out var extractedApiKey);

                var documentResult = await _documentService.DeleteDocumentProccess(documentId: new Guid(id), apiKey: extractedApiKey , cancellationToken: cancellationToken);
                if (!documentResult.Success)
                    throw new Exception(documentResult.Message);

            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message;
            }

            return result;
        }
        #endregion


        #region Add Watermark
        [HttpPost]
        [Route("addWatermark")]
        [ServiceFilter(typeof(ApiKeyAuthFilter))]
        public async Task<Result<string>> AddWatermark(
            [FromBody] AddWatermark watermark,
            CancellationToken cancellationToken
        )
        {
            var result = new Result<string>();
            try
            {
                Request.Headers.TryGetValue(_siteSettings.Value.ApiKeyHeaderName, out var extractedApiKey);

                var addWatermarkResult = await _documentService.AddWatermarkProccess(addWatermark: watermark, apiKey: extractedApiKey, cancellationToken: cancellationToken);
                if (!addWatermarkResult.Success)
                    throw new Exception(addWatermarkResult.Message);

            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message;
            }
            return result;
        }
        #endregion
    }
}
