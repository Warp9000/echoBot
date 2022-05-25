using Discord;
using Discord.Commands;
using Newtonsoft.Json;
using System.Net;


namespace echoBot
{
    [Name("Users")]
    [Summary("Users Commands")]
    public class UsersModule : ModuleBase<SocketCommandContext>
    {
        [Command("userinfo")]
        [Summary
        ("Returns info about a user")]
        [Alias("user", "whois")]
        public async Task UserInfoAsync([Name("<user>")][Summary("The user to show info about")] Discord.WebSocket.SocketUser? user = null, [Remainder] string? _ = null)
        {
            var userInfo = user ?? Context.User;
            var eUserInfo = userInfo as Discord.WebSocket.SocketGuildUser;
            l.Debug(userInfo.Username + "#" + userInfo.Discriminator, "UserInfoAsync");
            Color c = new Color(0, 0, 0);
            switch (eUserInfo.Status)
            {
                case UserStatus.Online:
                    c = new Color(59, 165, 93);
                    break;
                case UserStatus.Idle:
                    c = new Color(250, 168, 26);
                    break;
                case UserStatus.DoNotDisturb:
                    c = new Color(237, 66, 69);
                    break;
                case UserStatus.Offline:
                    c = new Color(116, 127, 141);
                    break;
                default:
                    c = new Color(255, 0, 255);
                    break;
            }

            var e = Program.DefaultEmbed();
            e.Color = c;
            // e.Title = userInfo.Username + "#" + userInfo.Discriminator;
            e.Description = userInfo.Mention + "\nID: " + userInfo.Id.ToString();
            e.ThumbnailUrl = userInfo.GetAvatarUrl();
            e.AddField("Joined", eUserInfo.JoinedAt.ToString().Substring(0, 19), true);
            e.AddField("Created", userInfo.CreatedAt.ToString().Substring(0, 19), true);
            var fb = new EmbedFieldBuilder();
            foreach (var item in eUserInfo.Roles.Reverse())
            {
                fb.Value += $"<@&{item.Id}>, ";
            }
            e.AddField(eUserInfo.Roles.Count + " Roles", fb.Build().Value.TrimEnd(',', ' '));
            e.Author = new EmbedAuthorBuilder
            {
                Name = userInfo.Username + "#" + userInfo.Discriminator,
                IconUrl = userInfo.GetAvatarUrl()
            };
            await ReplyAsync("", false, e.Build());
        }

        [Command("avatar")]
        [Summary("Returns the user's avatar")]
        [Alias("pfp")]
        public async Task AvatarAsync([Name("<user>")][Summary("The user to show avatar of")] Discord.WebSocket.SocketUser? user = null, [Remainder] string? _ = null)
        {
            var userInfo = user ?? Context.User;
            var e = Program.DefaultEmbed();
            e.Title = userInfo.Username + "#" + userInfo.Discriminator;
            e.ImageUrl = userInfo.GetAvatarUrl();
            await ReplyAsync("", false, e.Build());
        }
    }

}