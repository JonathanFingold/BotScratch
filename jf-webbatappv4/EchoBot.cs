// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;

namespace AspNetCore_EchoBot_With_State
{
    public class EchoBot : IBot
    {
        private BotAccessors _accessors;

        public EchoBot(BotAccessors accessors)
        {
            _accessors = accessors;
        }

        /// <summary>
        /// Every Conversation turn for our EchoBot will call this method. In here
        /// the bot checks the Activty type to verify it's a message, bumps the 
        /// turn conversation 'Turn' count, and then echoes the users typing
        /// back to them. 
        /// </summary>
        /// <param name="context">Turn scoped context containing all the data needed
        /// for processing this conversation turn. </param>        
        public async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            ConversationInfo info = await _accessors.ConversationInfoAccessor.GetAsync(
                turnContext,
                () => new ConversationInfo(),
                cancellationToken);

            switch (turnContext.Activity.Type)
            {
                case ActivityTypes.Message:

                    if (info.IsNameExpected)
                    {
                        string text = turnContext.Activity.Text.Trim();
                        info.Name = string.IsNullOrWhiteSpace(text)
                            ? turnContext.Activity.From.Name : text;
                        await turnContext.SendActivityAsync($"Pleased to meet you, {info.Name}.");
                        info.IsNameExpected = false;
                    }
                    else
                    {
                        // Bump the count and echo back to the user whatever they typed.
                        info.TurnCount++;
                        string annotation = $"[{info.Name}:{info.TurnCount:00}]";
                        await turnContext.SendActivityAsync(
                            $"{annotation}: You said '{turnContext.Activity.Text}'",
                            cancellationToken: cancellationToken);
                    }

                    break;

                case ActivityTypes.ConversationUpdate:

                    IConversationUpdateActivity activity = turnContext.Activity.AsConversationUpdateActivity();
                    if (activity.MembersAdded.Any(m => m.Id != activity.Recipient.Id))
                    {
                        await turnContext.SendActivityAsync("Welcome new user. What's your name?");
                        info.IsNameExpected = true;
                    }

                    break;
            }

            await _accessors.ConversationInfoAccessor.SetAsync(turnContext, info, cancellationToken);
            await _accessors.ConversationState.SaveChangesAsync(turnContext, false, cancellationToken);
        }
    }
}
