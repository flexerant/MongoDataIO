using SlackBotMessages;
using SlackBotMessages.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flexerant.MongoDataIO.Core
{
    public class Slack
    {
        private const string ERROR_COLOUR = "#780000";
        private const string SUCCESS_COLOUR = "#49be25";

        public static void SendMessageToSlack(DumpLog log, string slackChannel)
        {
            List<Attachment> attachments = new();

            if (log.Success)
            {
                attachments.Add(new Attachment()
                {
                    Color = SUCCESS_COLOUR,
                    Fields = new List<Field> {
                        new Field { Title = $"{log.DatabaseName.ToUpper()} BACKUP SUCCESS" },
                        new Field { Value = log.Message }
                    }
                });
            }
            else
            {
                attachments.Add(new Attachment()
                {
                    Color = ERROR_COLOUR,
                    Fields = new List<Field> {
                        new Field { Title = $"{log.DatabaseName.ToUpper()} BACKUP FAIL" },
                        new Field { Value = log.Message }
                    }
                });
            }

            var client = new SbmClient(slackChannel);
            var message = new Message() { Text = $"MongoDB Dump", Attachments = attachments };

            client.Send(message);
        }
    }
}
