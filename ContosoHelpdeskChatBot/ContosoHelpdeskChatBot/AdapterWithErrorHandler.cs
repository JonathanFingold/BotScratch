using Microsoft.Bot.Builder;
using System;
using Microsoft.Bot.Connector.Authentication;

namespace ContosoHelpdeskChatBot
{
    public class AdapterWithErrorHandler : BotFrameworkAdapter
    {
        public AdapterWithErrorHandler(
            ICredentialProvider credentialProvider,
            ConversationState conversationState = null)
            : base(credentialProvider)
        {
            OnTurnError = async (turnContext, exception) =>
            {
                // Send a catch-all appology to the user.
                await turnContext.SendActivityAsync("Sorry, it looks like something went wrong.");
                Console.Error.WriteLine($"Adapter caught unhandled exception: {exception}");

                if (conversationState != null)
                {
                    try
                    {
                        // Delete the conversationState for the current conversation to prevent the
                        // bot from getting stuck in a error-loop caused by being in a bad state.
                        // ConversationState should be thought of as similar to "cookie-state" in a Web pages.
                        await conversationState.DeleteAsync(turnContext);
                    }
                    catch (Exception e)
                    {
                        Console.Error.WriteLine(
                            $"{e.GetType().Name} caught on attempting to Delete ConversationState : {e.Message}");
                    }
                }
            };
        }
    }
}