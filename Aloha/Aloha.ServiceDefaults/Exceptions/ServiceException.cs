namespace Aloha.ServiceDefaults.Exceptions
{
    public class ServiceException : Exception
    {
        public string CorrelationId { get; }
        public string ServiceName { get; }
        public ServiceException(string message, string correlationId, string serviceName)
            : base(message)
        {
            CorrelationId = correlationId;
            ServiceName = serviceName;
        }
    }

    public class ServiceNotFoundException : ServiceException
    {
        public ServiceNotFoundException(string message, string correlationId, string serviceName)
            : base(message, correlationId, serviceName)
        {
        }
    }

    public class ServiceTimeoutException : ServiceException
    {
        public ServiceTimeoutException(
            string serviceName,
            string correlationId = null)
            : base($"Service {serviceName} request timed out", serviceName, correlationId)
        {
        }
    }

    public class ServiceValidationException : ServiceException
    {
        public ServiceValidationException(
            string message,
            string serviceName,
            string correlationId = null)
            : base(message, serviceName, correlationId)
        {
        }
    }

    public class ServiceCommunicationException : ServiceException
    {
        public ServiceCommunicationException(
            string message,
            string serviceName,
            string correlationId = null)
            : base(message, serviceName, correlationId)
        {
        }
    }
}
