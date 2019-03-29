using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;

namespace ContosoHelpdeskChatBot
{
    public class MessagesController : ApiController
    {
        private readonly BotFrameworkAdapter _adapter;
        private readonly IBot _bot;

        public MessagesController(BotFrameworkAdapter adapter, IBot bot)
        {
            _adapter = adapter;
            _bot = bot;
        }

        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {
            var invokeResponse = await _adapter.ProcessActivityAsync(
                Request.Headers.Authorization?.ToString(),
                activity,
                _bot.OnTurnAsync,
                default(CancellationToken));

            if (invokeResponse == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            else
            {
                return Request.CreateResponse(invokeResponse.Status);
            }
        }
    }
}