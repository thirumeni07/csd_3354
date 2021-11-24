using Mailjet.Client;
using Mailjet.Client.Resources;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CSD_3354_Project_Utility
{
    public class EmailSender : IEmailSender
    {
        //Api key and secrete key prerequisite or injection code begins here
        private readonly IConfiguration _configuration;

        public MailJetSettings _mailJetSettings { get; set; }

        public EmailSender(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        //Api key and secrete key prerequisite or injection code ends here
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            return Execute(email, subject, htmlMessage);
        }
        // all password: Temp1234*
        public async Task Execute(string email, string subject, string body)
        {
            //MailjetClient client = new MailjetClient("439651cb51a1b6fd7d165cee63cc141a", "64b80acb77d64b80bf3aafa8f151153c");
            //{ Version = ApiVersion.V3_1,};
            // MailJet is the key in appsettings.json file
            _mailJetSettings = _configuration.GetSection("MailJet").Get<MailJetSettings>();

            MailjetClient client = new MailjetClient(_mailJetSettings.ApiKey, _mailJetSettings.SecretKey);
            MailjetRequest request = new MailjetRequest
            {
                Resource = Send.Resource,
            }
             .Property(Send.Messages, new JArray {
     new JObject {
      {
       "From",
       new JObject {
        //{"Email", "mac071092@gmail.com"},
        {"Email", "mac071092@protonmail.com"},
        {"Name", "Mac"}
       }
      }, {
       "To",
       new JArray {
        new JObject {
         {
          "Email",
          email
         }, {
          "Name",
          "CSharpDotNet"
         }
        }
       }
      }, {
       "Subject",
       subject
      }, {
       "HTMLPart",
       body
      }
     }
             });
            await client.PostAsync(request);
        }
    }
}
