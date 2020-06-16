using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Adaptive;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Actions;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Conditions;
using System.Collections.Generic;

namespace Microsoft.BotBuilderSamples.DialogRootBot.Dialogs
{
    public class AlternateDialog : AdaptiveDialog
    {
        public AlternateDialog() : base("AdaptiveMainDialog")
        {

            base.Triggers = new List<OnCondition>
            {
                new OnConversationUpdateActivity()
                {
                    Actions = WelcomeUsersSteps
                },
                new OnUnknownIntent()
                {
                    Actions= AccessASkillSteps
                }
            };
        }

        private static List<Dialog> WelcomeUsersSteps { get; } =
            new List<Dialog>
            {
                new Foreach()
                {
                    ItemsProperty = "turn.activity.membersAdded",
                    Actions = new List<Dialog>
                    {
                        new IfCondition()
                        {
                            Condition = "$foreach.value.name != turn.activity.recipient.name",
                            Actions = new List<Dialog>
                            {
                                new SendActivity("Welcome to the Dialog Skill Prototype!"),
                                new BreakLoop(),
                            }
                        }
                    }
                }
            };

        private static List<Dialog> AccessASkillSteps { get; } =
            new List<Dialog>
            {
                new SendActivity("Ta da!")
            };
    }
}
