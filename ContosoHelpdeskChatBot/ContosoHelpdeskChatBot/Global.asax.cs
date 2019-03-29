using System;
using System.Configuration;
using System.Reflection;
using System.Web.Http;
using Autofac;
using Autofac.Integration.WebApi;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector.Authentication;

namespace ContosoHelpdeskChatBot
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);
            GlobalConfiguration.Configure(BotConfig.Register);

            log4net.Config.XmlConfigurator.Configure();
        }

        //setting Bot data store policy to use last write win
        //example if bot service got restarted, existing conversation would just overwrite data to store
        public static class BotConfig
        {
            public static void Register(HttpConfiguration config)
            {
                var builder = new ContainerBuilder();
                builder.RegisterApiControllers(Assembly.GetExecutingAssembly());

                ICredentialProvider credentialProvider = new SimpleCredentialProvider(
                    ConfigurationManager.AppSettings[MicrosoftAppCredentials.MicrosoftAppIdKey],
                    ConfigurationManager.AppSettings[MicrosoftAppCredentials.MicrosoftAppPasswordKey]);

                builder.RegisterInstance(credentialProvider);

                // The Memory Storage used here is for local bot debugging only. When the bot
                // is restarted, everything stored in memory will be gone.
                IStorage dataStore = new MemoryStorage();

                // Create Conversation State object.
                // The Conversation State object is where we persist anything at the conversation-scope.
                var conversationState = new ConversationState(dataStore);
                builder.RegisterInstance(conversationState);

                // Create the Bot Framework Adapter with error handling enabled. 
                BotFrameworkAdapter adapter =
                    new AdapterWithErrorHandler(credentialProvider, conversationState);
                builder.RegisterInstance(adapter);

                // The Dialog that will be run by the bot.
                Dialog rootDialog = new Dialogs.RootDialog(nameof(Dialogs.RootDialog));
                builder.RegisterInstance(rootDialog);

                // The bot.
                IBot bot = new DialogBot(conversationState, rootDialog);
                builder.RegisterInstance(bot);

                var container = builder.Build();
                var resolver = new AutofacWebApiDependencyResolver(container);
                config.DependencyResolver = resolver;

                Console.Error.WriteLine("Finished BotConfig.Register().");
            }
        }
    }
}
