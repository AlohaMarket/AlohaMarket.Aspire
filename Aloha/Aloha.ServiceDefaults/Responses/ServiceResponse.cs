namespace Aloha.ServiceDefaults.Responses
{
    public class ErrorDetail
    {
        public string ErrorCode { get; set; }
        public string Message { get; set; }
        public string CorrelationId { get; set; }
        public string Service { get; set; }
        public string Domain { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class ServiceResponse<T>
    {
        public T Data { get; set; }
        public ErrorDetail Error { get; set; }
        public bool IsSuccess => Error == null;
    }
}
