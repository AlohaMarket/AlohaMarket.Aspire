using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using Aloha.EventBus.Abstractions;

namespace Aloha.EventBus.Kafka
{
    public abstract class BaseEventHandler<TContext, TEventHandler> where TContext : DbContext
    {
        protected readonly TContext dbContext;
        protected readonly ILogger<TEventHandler> logger;
        protected readonly IEventPublisher eventPublisher;

        public BaseEventHandler(
            TContext dbContext,
            ILogger<TEventHandler> logger,
            IEventPublisher eventPublisher
        )
        {
            this.dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.eventPublisher = eventPublisher ?? throw new ArgumentNullException(nameof(eventPublisher));
        }

        protected async Task ExecuteWithTransactionAsync(Func<Task> action, CancellationToken cancellationToken = default)
        {
            var strategy = dbContext.Database.CreateExecutionStrategy();
            await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
                try
                {
                    logger.LogDebug($"Beginning {typeof(TEventHandler).Name} database transaction");
                    await action();
                    await transaction.CommitAsync(cancellationToken);
                    logger.LogDebug($"Transaction committed successfully for {typeof(TEventHandler).Name}");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, $"Error executing transaction for {typeof(TEventHandler).Name}. Rolling back");
                    await transaction.RollbackAsync(cancellationToken);
                    throw;
                }
            });
        }

        protected async Task<TResult> ExecuteWithTransactionAsync<TResult>(Func<Task<TResult>> func, CancellationToken cancellationToken = default)
        {
            var strategy = dbContext.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
                try
                {
                    logger.LogDebug($"Beginning {typeof(TEventHandler).Name} database transaction");
                    var result = await func();
                    await transaction.CommitAsync(cancellationToken);
                    logger.LogDebug($"Transaction committed successfully for {typeof(TEventHandler).Name}");
                    return result;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, $"Error executing transaction for {typeof(TEventHandler).Name}. Rolling back");
                    await transaction.RollbackAsync(cancellationToken);
                    throw;
                }
            });
        }
    }
}