using FileServer.Database;
using FileServer.ViewModels;
using FileServer.ViewModels.Interface;

namespace FileServer.Services.Document
{
    public interface IDocumentService : IScopedDependency
    {
        Task<Result<Models.Document>> AddDocumentProccess(AddDocument document, string apiKey, CancellationToken cancellationToken);
        Task<Result> DeleteDocumentProccess(Guid documentId, string apiKey, CancellationToken cancellationToken);
        Task<Result<string>> GetDocumentProccess(Guid documentId, string parameters, CancellationToken cancellationToken, IInclude<Models.Document> include = null);
        Task<Result> AddWatermarkProccess(AddWatermark addWatermark, string apiKey, CancellationToken cancellationToken);
        Task<Result<Models.Document>> AddDocument(AddDocumentInput addDocumentInput, CancellationToken cancellationToken);
    }
}
