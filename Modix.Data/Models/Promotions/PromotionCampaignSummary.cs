﻿using System;
using System.Linq;
using System.Collections.Generic;
using System.Linq.Expressions;

using Modix.Data.Models.Core;
using Modix.Data.Projectables;

namespace Modix.Data.Models.Promotions
{
    /// <summary>
    /// Describes a summary view of a <see cref="PromotionCampaignEntity"/>, for use in higher layers of the application.
    /// </summary>
    public class PromotionCampaignSummary
    {
        /// <summary>
        /// See <see cref="PromotionCampaignEntity.Id"/>.
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// See <see cref="PromotionCampaignEntity.GuildId"/>.
        /// </summary>
        public ulong GuildId { get; set; }

        /// <summary>
        /// See <see cref="PromotionCampaignEntity.Subject"/>.
        /// </summary>
        public GuildUserBrief Subject { get; set; }

        /// <summary>
        /// See <see cref="PromotionCampaignEntity.TargetRole"/>.
        /// </summary>
        public GuildRoleBrief TargetRole { get; set; }

        /// <summary>
        /// See <see cref="PromotionCampaignEntity.CreateAction"/>.
        /// </summary>
        public PromotionActionBrief CreateAction { get; set; }

        /// <summary>
        /// See <see cref="PromotionCampaignEntity.Outcome"/>.
        /// </summary>
        public PromotionCampaignOutcome? Outcome { get; set; }

        /// <summary>
        /// See <see cref="PromotionCampaignEntity.CloseAction"/>.
        /// </summary>
        public PromotionActionBrief CloseAction { get; set; }

        /// <summary>
        /// A summarization of <see cref="PromotionCampaignEntity.Comments"/>,
        /// summarizing the total number of comments, grouped by <see cref="PromotionCommentEntity.Sentiment"/>.
        /// </summary>
        public IReadOnlyDictionary<PromotionSentiment, int> CommentCounts { get; set; }

        internal static Expression<Func<PromotionCampaignEntity, PromotionCampaignSummary>> FromEntityProjection
            = entity => new PromotionCampaignSummary()
            {
                Id = entity.Id,
                GuildId = (ulong)entity.GuildId,
                Subject = entity.Subject.Project(GuildUserBrief.FromEntityProjection),
                TargetRole = entity.TargetRole.Project(GuildRoleBrief.FromEntityProjection),
                CreateAction = entity.CreateAction.Project(PromotionActionBrief.FromEntityProjection),
                // https://github.com/aspnet/EntityFrameworkCore/issues/12834
                //Outcome = entity.Outcome,
                Outcome = (entity.Outcome == null)
                    ? null
                    : (PromotionCampaignOutcome?)Enum.Parse<PromotionCampaignOutcome>(entity.Outcome.ToString()),
                CloseAction = (entity.CloseAction == null)
                    ? null
                    : entity.CloseAction.Project(PromotionActionBrief.FromEntityProjection),
                CommentCounts = entity.Comments
                    .GroupBy(x => x.Sentiment)
                    .Select(x => new { x.Key, Count = x.Count() })
                    .ToDictionary(x => x.Key, x => x.Count)
            };
    }
}
