using Discord;
using Discord.Commands;
using Newtonsoft.Json;
using System.Net;


namespace echoBot
{
    [Name("Test")]
    [Summary("Test commands")]
    public class TestCommands : ModuleBase<SocketCommandContext>
    {
        [Command("shutdown")]
        [Summary("Shuts down the bot")]
        public async Task StopAsync([Name("<time>")][Summary("Ammount of time in minutes for the bot to shutdown for")] float? time, [Remainder] string? _ = null)
        {
            if (Context.User.Id == Program.Warp)
            {
                await ReplyAsync("Shutting down...");
                await Context.Client.SetStatusAsync(UserStatus.Invisible);
                await Context.Client.LogoutAsync();
            }
            else
            {
                await ReplyAsync("You are not the owner of this bot!");
            }
        }
    }

}