using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using CsInvite.Messaging.Base;

namespace CsInvite.Messaging.Discord
{
    public class Discord: MessagingService
    {
        private DiscordClient discord;
        public Discord()
        {
            discord = new DiscordClient(new DiscordConfiguration()
            {
                Token = Secret.DiscordToken,
                TokenType = TokenType.Bot
            });
            discord.MessageCreated += OnMessageCreated;
        }

        private async Task OnMessageCreated(DSharpPlus.EventArgs.MessageCreateEventArgs e)
        {
            if (e.Author.IsBot)
            {
                return;
            }

            OnMessageReceived(new Message()
            {
                Text = e.Message.Content,
                Chat = new Chat()
                {
                    Channel = e.Channel
                }
            });
        }

        public async Task Connect()
        {
            await discord.ConnectAsync();
        }

        public override void SendMessage(Base.Chat chat, string message)
        {
            var dChat = chat as Chat;
            if (dChat == null)
            {
                return;
            }

            discord.SendMessageAsync(dChat.Channel, message);

        }
    }
}
