using Discord;
using Discord.Commands;
using Newtonsoft.Json;
using System.Net;


namespace echoBot
{
    [Group("test")]
    [Name("Test")]
    [Summary("Test commands")]
    public class TestCommands : ModuleBase<SocketCommandContext>
    {
        [Command("ping")]
        [Summary("Pings the bot")]
        public async Task PingAsync()
        {
            await ReplyAsync("Pong!" + Environment.NewLine + "Ping: " + Context.Client.Latency);
        }
    }

}