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
        [Alias("stop")]
        public async Task PingAsync([Remainder] string? _ = null)
        {
            if (Context.User.Id == Program.Warp)
            {
                await ReplyAsync("Shutting down...");
                await Context.Client.SetStatusAsync(UserStatus.Invisible);
                await Context.Client.LogoutAsync();
                Environment.Exit(0);
            }
            else
            {
                await ReplyAsync("You are not the owner of this bot!");
            }
        }
        [Command("delchannels")]
        [Summary("Deletes channels that contain a certain string")]
        [Alias("delch")]
        public async Task DelChannelsAsync([Name("string")][Remainder] string? ch = null)
        {
            var executor = Context.User as Discord.WebSocket.SocketGuildUser;
            if (!executor.GuildPermissions.ManageChannels)
            {
                await ReplyAsync("You don't have the permissions to do that!");
                return;
            }
            var channels = Context.Guild.Channels.Where(x => x.Name.Contains(ch));
            foreach (var channel in channels)
            {
                await channel.DeleteAsync();
            }
            await ReplyAsync("Done!");
        }
    }
}