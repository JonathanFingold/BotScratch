// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace Microsoft.BotBuilderSamples.DialogRootBot.Bots
{
    public class RootBot<T> : ActivityHandler
        where T : Dialog
    {
        private Dialog MainDialog { get; }
        private DialogManager DialogManager { get; }

        public RootBot(T mainDialog)
        {
            MainDialog = mainDialog;
            DialogManager = new DialogManager(MainDialog);
        }

        public override async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default)
        {
            await DialogManager.OnTurnAsync(turnContext, cancellationToken);
        }

        // Load attachment from embedded resource.
        private Attachment CreateAdaptiveCardAttachment()
        {
            var cardResourcePath = "Microsoft.BotBuilderSamples.DialogRootBot.Cards.welcomeCard.json";

            using var stream = GetType().Assembly.GetManifestResourceStream(cardResourcePath);
            using var reader = new StreamReader(stream);
            var adaptiveCard = reader.ReadToEnd();
            return new Attachment
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JsonConvert.DeserializeObject(adaptiveCard)
            };
        }
    }
}
