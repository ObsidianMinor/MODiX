﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Modix.Data.Models.Moderation;

namespace Modix.Data.Repositories
{
    /// <summary>
    /// Describes a repository for managing <see cref="DeletedMessageEntity"/> entities, within an underlying data storage provider.
    /// </summary>
    public interface IDeletedMessageRepository
    {
        /// <summary>
        /// Begins a new transaction to create deleted messages within the repository.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> that will complete, with the requested transaction object,
        /// when no other transactions are active upon the repository.
        /// </returns>
        Task<IRepositoryTransaction> BeginCreateTransactionAsync();

        /// <summary>
        /// Creates a new deleted message within the repository.
        /// </summary>
        /// <param name="data">The data for the deleted message to be created.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="data"/>.</exception>
        /// <returns>
        /// A <see cref="Task"/> which will complete when the operation is complete,
        /// containing the auto-generated <see cref="DeletedMessageEntity.Id"/> value assigned to the new deleted message.
        /// </returns>
        Task CreateAsync(DeletedMessageCreationData data);
    }

    /// <inheritdoc />
    public class DeletedMessageRepository : RepositoryBase, IDeletedMessageRepository
    {
        /// <summary>
        /// Creates a new <see cref="DeletedMessageRepository"/>.
        /// See <see cref="RepositoryBase(ModixContext)"/> for details.
        /// </summary>
        public DeletedMessageRepository(ModixContext modixContext, IEnumerable<IModerationActionEventHandler> moderationActionEventHandlers)
            : base(modixContext)
        {
            ModerationActionEventHandlers = moderationActionEventHandlers;
        }

        /// <inheritdoc />
        public Task<IRepositoryTransaction> BeginCreateTransactionAsync()
            => _createTransactionFactory.BeginTransactionAsync(ModixContext.Database);

        /// <inheritdoc />
        public async Task CreateAsync(DeletedMessageCreationData data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            var entity = data.ToEntity();

            await ModixContext.DeletedMessages.AddAsync(entity);
            await ModixContext.SaveChangesAsync();

            entity.CreateAction.DeletedMessageId = entity.MessageId;
            await ModixContext.SaveChangesAsync();

            await RaiseModerationActionCreatedAsync(entity.CreateActionId, new ModerationActionCreationData()
            {
                GuildId = (ulong)entity.CreateAction.GuildId,
                Type = entity.CreateAction.Type,
                Created = entity.CreateAction.Created,
                CreatedById = (ulong)entity.CreateAction.CreatedById
            });
        }

        /// <summary>
        /// A set of <see cref="IModerationActionEventHandler"/> objects to receive information about moderation actions
        /// affected by this repository.
        /// </summary>
        internal protected IEnumerable<IModerationActionEventHandler> ModerationActionEventHandlers { get; }

        private async Task RaiseModerationActionCreatedAsync(long moderationActionId, ModerationActionCreationData data)
        {
            foreach (var handler in ModerationActionEventHandlers)
                await handler.OnModerationActionCreatedAsync(moderationActionId, data);
        }

        private static readonly RepositoryTransactionFactory _createTransactionFactory
            = new RepositoryTransactionFactory();
    }
}