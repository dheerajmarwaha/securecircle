using System;
using System.Collections.Generic;
using System.Linq;
using Arya.Vis.Core.Entities;
using Arya.Vis.Core.Enums;

namespace Arya.Vis.Api.Commands {
    public class InterviewCreateCommand {
        public Guid Id { get; set; }
        public string Code { get; set; }
        public string Title { get; set; }
        public InterviewStatus Status { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime ExpirationTime { get; set; }
        public IEnumerable<InterviewRoundCreateCommand> Rounds { get; set; }
        public InterviewContext Context { get; set; }
        // Didnot include CreatedAt, ModifiedAt, CreatedBy, ModifiedBy because they can be computed on server side.
        // Didnot include IsPublished because interview will not be published at creation.
        public static Interview MapToInterview(InterviewCreateCommand command) {
            var interview = new Interview{
                Id = command.Id,
                Code = command.Code,
                Title = command.Title,
                Status = command.Status,
                StartTime = command.StartTime,
                ExpirationTime = command.ExpirationTime,
                Context = command.Context
            };
            if(command.Rounds?.Any() == true) {
                interview.Rounds = command.Rounds.Select(x => InterviewRoundCreateCommand.MapToInterviewRound(x));
            }
            return interview;
        }
    }
}
