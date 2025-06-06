namespace Aloha.EventBus.Abstractions
{
    public class MessageEnvelop
    {
        /// <summary>
        /// Initializes a new, empty instance of the <see cref="MessageEnvelop"/> class.
        /// </summary>
        public MessageEnvelop()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageEnvelop"/> class using the specified type and message content.
        /// </summary>
        /// <param name="type">The type associated with the message.</param>
        /// <param name="message">The message content.</param>
        public MessageEnvelop(Type type, string message) : this(type.FullName!, message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageEnvelop"/> class with the specified message type name and message content.
        /// </summary>
        /// <param name="messageTypeName">The name of the message type.</param>
        /// <param name="message">The message content.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="messageTypeName"/> or <paramref name="message"/> is null.</exception>
        public MessageEnvelop(string messageTypeName, string message)
        {
            MessageTypeName = messageTypeName ?? throw new ArgumentNullException(nameof(messageTypeName));
            Message = message ?? throw new ArgumentNullException(nameof(message));
        }

        public string MessageTypeName { get; set; } = default!;

        public string Message { get; set; } = default!;
    }
}
