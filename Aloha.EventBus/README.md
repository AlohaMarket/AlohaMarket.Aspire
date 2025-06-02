sequenceDiagram
    participant Service as Service/Microservice
    participant Publisher as IEventPublisher
    participant KafkaPub as KafkaEventPublisher
    participant Kafka as Apache Kafka
    participant Consumer as EventHandlingService
    participant Factory as IIntegrationEventFactory
    participant Mediator as IMediator (MediatR)
    participant Handler as IIntegrationEventHandler<T>

    %% Event Publishing Flow
    rect rgb(240, 248, 255)
        Note over Service,Handler: Event Publishing Flow
        Service->>Service: Create IntegrationEvent instance
        Note right of Service: EventId and EventCreationDate<br/>are automatically generated
        Service->>Publisher: PublishAsync<TEvent>(event)
        activate Publisher
        Publisher->>KafkaPub: PublishAsync<TEvent>(event)
        activate KafkaPub
        KafkaPub->>KafkaPub: Serialize event to JSON
        KafkaPub->>KafkaPub: Create MessageEnvelop
        Note right of KafkaPub: Contains event type name<br/>and serialized event data
        KafkaPub->>Kafka: ProduceAsync(topic, message)
        activate Kafka
        Kafka-->>KafkaPub: Confirmation
        deactivate Kafka
        KafkaPub-->>Publisher: Return true/false (success/failure)
        deactivate KafkaPub
        Publisher-->>Service: Return success/failure
        deactivate Publisher
    end

    %% Event Consuming Flow
    rect rgb(255, 240, 245)
        Note over Service,Handler: Event Consuming Flow

        %% Startup and subscription
        Consumer->>Consumer: ExecuteAsync starts on service start
        activate Consumer
        Consumer->>Kafka: Subscribe(topics)
        activate Kafka
        Kafka-->>Consumer: Confirmation
        deactivate Kafka
        
        %% Continuous consumption loop
        loop Until cancellation requested
            Consumer->>Kafka: Consume(timeout)
            activate Kafka
            
            alt Message received
                Kafka-->>Consumer: ConsumeResult with Message
                deactivate Kafka
                
                Consumer->>Consumer: Create service scope
                Consumer->>Consumer: Get IMediator from scope
                
                Consumer->>Consumer: ProcessMessageAsync
                activate Consumer
                Consumer->>Factory: CreateEvent(typeName, serializedEvent)
                activate Factory
                Factory->>Factory: Deserialize to IntegrationEvent
                Factory-->>Consumer: IntegrationEvent instance
                deactivate Factory
                
                alt Event created successfully
                    Consumer->>Consumer: AcceptEvent(event)
                    
                    alt Event accepted
                        Consumer->>Mediator: Send(event)
                        activate Mediator
                        
                        %% MediatR sends to all handlers
                        Mediator->>Handler: Handle(event)
                        activate Handler
                        Handler->>Handler: Process event logic
                        Handler-->>Mediator: Task completed
                        deactivate Handler
                        
                        Mediator-->>Consumer: Task completed
                        deactivate Mediator
                        
                    else Event filtered out
                        Consumer->>Consumer: Log debug message
                    end
                else Event type not found
                    Consumer->>Consumer: Log warning
                end
                deactivate Consumer
            else No message available
                Kafka-->>Consumer: null
                Consumer->>Consumer: Delay for 100ms
            end
        end
        deactivate Consumer
    end

    %% Configuration Flow
    rect rgb(240, 255, 240)
        Note over Service,Handler: Service Configuration
        Service->>Service: AddKafkaProducer(connectionName)
        Note right of Service: Configures Kafka producer
        Service->>Service: AddKafkaEventPublisher(topic)
        Note right of Service: Registers IEventPublisher
        Service->>Service: AddKafkaEventConsumer(options)
        Note right of Service: Configures event consumer<br/>with topics and filters
    end