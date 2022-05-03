using Discord;
using Discord.Commands;


namespace echoBot
{
    public class Commands : ModuleBase<SocketCommandContext>
    {
        [Command("embed")]
        [Summary("Sends an embed message")]
        public async Task Echo()
        {
            var e = new EmbedBuilder();
            e.Color = Color.Blue;
            e.Title = "Title";
            e.Description = "Description";
            e.ImageUrl = "https://warp.tf/i/1sthq.png";
            e.ThumbnailUrl = "https://warp.tf/i/9hukc.png";
            e.Url = "https://warp.tf";
            e.AddField("Field1", "FieldValue1");
            e.AddField("Field2", "FieldValue2");
            await ReplyAsync("", false, e.Build());
        }
    }
    // Keep in mind your module **must** be public and inherit ModuleBase.
    // If it isn't, it will not be discovered by AddModulesAsync!
    // Create a module with no prefix
    public class InfoModule : ModuleBase<SocketCommandContext>
    {
        // ~say hello world -> hello world
        [Command("say")]
        [Summary("Echoes a message.")]
        public Task SayAsync([Remainder][Summary("The text to echo")] string echo)
            => ReplyAsync(echo);

        // ReplyAsync is a method on ModuleBase 
    }

    // Create a module with the 'test' prefix
    // [Group("test")]
    public class SampleModule : ModuleBase<SocketCommandContext>
    {
        // ~sample square 20 -> 400
        [Command("square")]
        [Summary("Squares a number.")]
        public async Task SquareAsync(
            [Summary("The number to square.")]
            int num)
        {
            // We can also access the channel from the Command Context.
            await Context.Channel.SendMessageAsync($"{num}^2 = {Math.Pow(num, 2)}");
        }

        // ~sample userinfo --> foxbot#0282
        // ~sample userinfo @Khionu --> Khionu#8708
        // ~sample userinfo Khionu#8708 --> Khionu#8708
        // ~sample userinfo Khionu --> Khionu#8708
        // ~sample userinfo 96642168176807936 --> Khionu#8708
        // ~sample whois 96642168176807936 --> Khionu#8708
        [Command("userinfo")]
        [Summary
        ("Returns info about the current user, or the user parameter, if one passed.")]
        [Alias("user", "whois")]
        public async Task UserInfoAsync(
            [Summary("The (optional) user to get info from")]
            Discord.WebSocket.SocketUser? user = null)
        {
            var userInfo = user  ?? Context.User;
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

            var e = new EmbedBuilder();
            e.Color = c;
            // e.Title = userInfo.Username + "#" + userInfo.Discriminator;
            e.Description = userInfo.Mention;
            e.ThumbnailUrl = userInfo.GetAvatarUrl();
            e.AddField("Joined", eUserInfo.JoinedAt, true);
            e.AddField("Created", userInfo.CreatedAt, true);
            var fb = new EmbedFieldBuilder();
            foreach (var item in eUserInfo.Roles)
            {
                fb.Value += $"<@&{item.Id}>, ";
            }
            e.AddField(eUserInfo.Roles.Count + " Roles", fb.Build().Value.TrimEnd(',', ' '));
            e.Footer = new EmbedFooterBuilder
            {
                Text = "ID:" + userInfo.Id.ToString() + " | " + DateTime.UtcNow + " UTC | " + "echoBot"
            };
            e.Author = new EmbedAuthorBuilder
            {
                Name = userInfo.Username + "#" + userInfo.Discriminator,
                IconUrl = userInfo.GetAvatarUrl()
            };
            await ReplyAsync("", false, e.Build());
        }
    }
}