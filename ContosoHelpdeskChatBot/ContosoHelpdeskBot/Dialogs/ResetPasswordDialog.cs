﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using ContosoHelpdeskChatBot.Models;
using ContosoHelpdeskSms;
using Bot.Builder.Community.Dialogs.FormFlow;
using System.Threading;

namespace ContosoHelpdeskChatBot.Dialogs
{
    [Serializable]
    public class ResetPasswordDialog : ComponentDialog
    {
        public ResetPasswordDialog()
        : base(nameof(ResetPasswordDialog))
        {
            AddDialog(FormDialog.FromForm(this.BuildResetPasswordForm, FormOptions.PromptInStart));
        }

        public override async Task<DialogTurnResult> BeginDialogAsync(DialogContext outerDc, object options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            await outerDc.PostAsync("Alright I will help you create a temp password");

            if (sendPassCode(outerDc))
            {
                var dialog = FindDialog(nameof(ResetPasswordPrompt));
                return await outerDc.BeginDialogAsync(nameof(ResetPasswordPrompt));
            }
            else
            {
                //here we can simply fail the current dialog because we have root dialog handling all exceptions
                throw new Exception("Failed to send SMS. Make sure email & phone number has been added to database.");
            }            
        }

        public override async Task<DialogTurnResult> ResumeDialogAsync(DialogContext outerDc, DialogReason reason, object result = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            var prompt = result as ResetPasswordPrompt;
            var email = outerDc.Context.Activity.From.Id;
            int? passcode;

            using (var db = new ContosoHelpdeskContext())
            {
                passcode = db.ResetPasswords.Where(r => r.EmailAddress == email).First().PassCode;
            }

            if (prompt.PassCode == passcode)
            {
                string temppwd = "TempPwd" + new Random().Next(0, 5000);
                await outerDc.PostAsync($"Your temp password is {temppwd}");
            }

            return new DialogTurnResult(DialogTurnStatus.Complete);
        }

        private bool sendPassCode(DialogContext context)
        {
            bool result = false;

            //Recipient Id varies depending on channel
            //refer ChannelAccount class https://docs.botframework.com/en-us/csharp/builder/sdkreference/dd/def/class_microsoft_1_1_bot_1_1_connector_1_1_channel_account.html#a0b89cf01fdd73cbc00a524dce9e2ad1a
            //as well as Activity class https://docs.botframework.com/en-us/csharp/builder/sdkreference/dc/d2f/class_microsoft_1_1_bot_1_1_connector_1_1_activity.html
            var email = context.Context.Activity.From.Id;
            int passcode = new Random().Next(1000, 9999);
            Int64? smsNumber = 0;
            string smsMessage = "Your Contoso Pass Code is ";
            string countryDialPrefix = "+1";

            //save PassCode to database
            using (var db = new ContosoHelpdeskContext())
            {
                var reset = db.ResetPasswords.Where(r => r.EmailAddress == email).ToList();
                if (reset.Count >= 1)
                {
                    reset.First().PassCode = passcode;
                    smsNumber = reset.First().MobileNumber;
                    result = true;
                }
                
                db.SaveChanges();
            }

            if (result)
            {
                result = Helper.SendSms($"{countryDialPrefix}{smsNumber.ToString()}", $"{smsMessage} {passcode}");
            }

            return result;
        }

        private IForm<ResetPasswordPrompt> BuildResetPasswordForm()
        {
            return new FormBuilder<ResetPasswordPrompt>()
                .Field(nameof(ResetPasswordPrompt.PassCode))
                .Build();
        }

        //private async Task ResumeAfterResetPasswordFormDialog(DialogContext context, IAwaitable<ResetPasswordPrompt> userReply)
        //{
        //    var prompt = await userReply;
        //    var email = context.Context.Activity.From.Id;
        //    int? passcode;

        //    using (var db = new ContosoHelpdeskContext())
        //    {
        //        passcode = db.ResetPasswords.Where(r => r.EmailAddress == email).First().PassCode;
        //    }

        //    if (prompt.PassCode == passcode)
        //    {
        //        string temppwd = "TempPwd" + new Random().Next(0, 5000);
        //        await context.PostAsync($"Your temp password is {temppwd}");
        //    }

        //    context.Done<object>(null);
        //}
    }
}