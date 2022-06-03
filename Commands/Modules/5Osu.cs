using System.Net.WebSockets;
using Discord;
using Discord.Commands;
using System.Net;
using System.Net.Sockets;

namespace echoBot
{
    [Group("osu")]
    [Name("Osu")]
    [Summary("Osu commands")]
    public class OsuModule : ModuleBase<SocketCommandContext>
    {
        [Command("test")]
        [Summary("test osu api")]
        public async Task Test([Remainder] string? _ = null)
        {
            // get from an api
            var client = new HttpClient();
            var s = client.PostAsync("https://osu.ppy.sh/oauth/token", new FormUrlEncodedContent(new Dictionary<string, string>()
            {
                { "grant_type", "client_credentials" },
                { "client_id", "15047" },
                { "client_secret", "RWEXmWOKhrVa7XLAxqGNkRRfzA9UMaDEI3OuIFPM" },
                { "scope", "public" }
            })).Result;
            await ReplyAsync(s.Content.ToString());
        }
    }
}