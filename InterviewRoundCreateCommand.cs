using System.Collections.Generic;
using System.Linq;
using Arya.Vis.Core.Entities;

namespace Arya.Vis.Api.Commands {
    public class InterviewRoundCreateCommand {
        public int SequenceNumber { get; set; }
        public IEnumerable<InterchangeTemplateCreateCommand> Questionnaire { get; set; }
        // Todo Interviewers ??

        public static InterviewRound MapToInterviewRound(InterviewRoundCreateCommand command) {
            var interviewRound = new InterviewRound{
                SequenceNumber = command.SequenceNumber
            };
            if (command.Questionnaire?.Any() == true) {
                interviewRound.Questionnaire = command.Questionnaire.Select(x => InterchangeTemplateCreateCommand.MapToInterchangeTemplate(x));
            }
            return interviewRound;
        }
    }
}
