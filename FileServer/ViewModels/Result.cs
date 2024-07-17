using System;
namespace FileServer.ViewModels
{
    public class Result
    {
        public Result()
        {
            Success = true;
        }
        public bool Success { get; set; }
        public Exception Exception { get; set; }
        public string Message { get; set; }
    }
    public class Result<T> : Result
    {
        public T Data { get; set; }
        public PagingInfo PagingInfo { get; set; }
        public dynamic DataInfo { get; set; }
    }

    public class PagingInfo
    {

        public PagingInfo(int totalCount, int? pageSize, int? pageNumber)
        {
            TotalCount = totalCount;
            PageSize = pageSize;
            PageNumber = pageNumber;
        }
        public int TotalCount { get; }
        public int? PageSize { get; }
        public int? PageNumber { get; }
        public int PageCount
        {
            get
            {
                return (PageSize != null && PageSize > 0) ? (int)Math.Ceiling(TotalCount / (decimal)PageSize) : 0;
            }
        }
    }
}
