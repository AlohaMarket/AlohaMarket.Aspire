namespace Aloha.ServiceDefaults.Responses
{
    public class ServiceResponse<T>
    {
        public bool Success { get; set; }
        public string CorrelationId { get; set; }
        public string ServiceName { get; set; }
        public T Data { get; set; }
        public ServiceError Error { get; set; }
    }
    public class ServiceError
    {
        public string Code { get; set; }
        public string Message { get; set; }
        public string Source { get; set; }
        public DateTime? RetryAfter { get; set; }
    }
}
