using Discord;
using Discord.Commands;
using Newtonsoft.Json;
using System.Net;


namespace echoBot
{
    [Name("Info")]
    [Summary("Info Commands")]
    public class InfoModule : ModuleBase<SocketCommandContext>
    {
        [Command("feedback")]
        [Summary("Send feedback to Warp")]
        public async Task Feedback([Remainder][Name("[feedback]")][Summary("String of the feedback you want to give")] string feedback)
        {
            var warp = Context.Client.GetUser(Program.Warp);
            var dm = await warp.CreateDMChannelAsync();
            var e = Program.DefaultEmbed();
            e.Title = "Feedback";
            e.Description = feedback;
            e.Author = new EmbedAuthorBuilder()
            {
                Name = Context.User.Username + "#" + Context.User.Discriminator,
                IconUrl = Context.User.GetAvatarUrl()
            };
            await dm.SendMessageAsync("", false, e.Build());
            await Context.Message.AddReactionAsync(new Emoji("✅"));
        }



        [Command("help")]
        [Summary("Shows the new help menu using reactions")]
        public async Task HelpAsync()
        {
            var e = Program.DefaultEmbed();
            var m = CommandHandler._commands.Modules.ToArray()[0];
            List<ModuleInfo> ml = CommandHandler._commands.Modules.ToList();
            e.Title = $"Help (1/{ml.Count})";
            e.Description = "[required] <optional>";
            var v = "";
            var p = "";
            foreach (var cmd in m.Commands)
            {
                if (cmd.Parameters.Count > 0)
                {
                    foreach (var par in cmd.Parameters)
                    {
                        p += $" {par.Name}";
                    }
                    // p.TrimEnd(' ');
                }
                v += $"{Program.Config.gPrefix}{cmd.Name}{p} - {cmd.Summary}\n";
                p = " ";
            }
            e.AddField(m.Name, v);
            var c = new ComponentBuilder().WithButton("Prev", "help-button-prev-f", ButtonStyle.Secondary, disabled: true).WithButton("Next", "help-button-next").Build();
            var msg = await ReplyAsync("", false, e.Build(), components: c);
            var pages = (int)Math.Ceiling((double)ml.Count);
        }



        [Command("botinfo")]
        [Summary("Returns info about the bot")]
        [Alias("bot", "info")]
        public async Task BotInfoAsync()
        {
            var e = new EmbedBuilder();
            e.Color = Color.DarkPurple;
            e.Title = "echoBot";
            e.Description = "By Warp#8703";
            e.ThumbnailUrl = "https://cdn.discordapp.com/avatars/869399518267969556/7d05a852cbea15a1028540a913ae43b5.png?size=4096";
            e.Url = "https://warp.tf";
            e.AddField("Links", "[Discord](https://discord.gg/cfcFFECJ4X)\n[Source Code](https://github.com/WarpABoi/echoBot)");
            e.Footer = CommandHandler.GetFooter();
            e.WithCurrentTimestamp();
            // e.AddField("Field2", "FieldValue2");
            await ReplyAsync("", false, e.Build());
        }
        [Command("ping")]
        [Summary("Returns the bot's ping")]
        [Alias("pong")]
        public async Task PingAsync()
        {
            var e = new EmbedBuilder();
            e.Color = Color.DarkPurple;
            if (Context.Message.Content.StartsWith(Program.Config.gPrefix + "ping", true, null))
                e.Title = "Pong!";
            else
                e.Title = "Ping!";
            e.Description = $"{Context.Client.Latency}ms";
            e.Footer = CommandHandler.GetFooter();
            e.WithCurrentTimestamp();
            await ReplyAsync("", false, e.Build());
        }
        [Command("uptime")]
        [Summary("Returns the bot's uptime")]
        [Alias("up")]
        public async Task UptimeAsync()
        {
            var e = Program.DefaultEmbed();
            e.Title = "Uptime";
            var u = DateTime.Now - Program.startTime;
            if (u.Days > 0)
                e.Description += $"{u.Days} days, {u.Hours} hours, {u.Minutes} minutes, {u.Seconds} seconds";
            else if (u.Hours > 0)
                e.Description += $"{u.Hours} hours, {u.Minutes} minutes, {u.Seconds} seconds";
            else if (u.Minutes > 0)
                e.Description += $"{u.Minutes} minutes, {u.Seconds} seconds";
            else
                e.Description += $"{u.Seconds} seconds";
            await ReplyAsync("", false, e.Build());
        }

        [Command("stats")]
        [Summary("Returns the bot's stats")]
        public async Task StatsAsync()
        {
            var e = Program.DefaultEmbed();
            e.Title = "Stats";
            e.AddField("Guilds", Context.Client.Guilds.Count, true);
            e.AddField("Users", Context.Client.Guilds.Sum(x => x.Users.Count), true);
            e.AddField("Channels", Context.Client.Guilds.Sum(x => x.Channels.Count), true);
            e.AddField("Commands", Program.Commands, true);
            var u = DateTime.Now - Program.startTime;
            string uptime = "";
            if (u.Days > 0)
                uptime += $"{u.Days}d, ";
            if (u.Hours > 0)
                uptime += $"{u.Hours}h, ";
            if (u.Minutes > 0)
                uptime += $"{u.Minutes}m, ";
            if (u.Seconds > 0)
                uptime += $"{u.Seconds}s";
            e.AddField("Uptime", uptime, true);
            e.AddField("Version", Program.version, true);
            await ReplyAsync("", false, e.Build());
        }

        [Command("serverinfo")]
        [Summary("Returns the server's info")]
        [Alias("server", "guild")]
        public async Task ServerInfoAsync()
        {
            var e = Program.DefaultEmbed();
            e.Title = Context.Guild.Name;
            e.AddField("ID", Context.Guild.Id, true);
            e.AddField("Owner", Context.Guild.Owner.Mention, true);
            e.AddField("Created", Context.Guild.CreatedAt.ToString("dd/MM/yyyy"), true);
            e.AddField("Members", Context.Guild.MemberCount, true);
            e.AddField("Channels", Context.Guild.Channels.Count, true);
            e.AddField("Roles", Context.Guild.Roles.Count, true);
            e.ThumbnailUrl = Context.Guild.IconUrl;
            await ReplyAsync("", false, e.Build());
        }

        [Command("prefix")]
        [Summary("Gets or sets the bot's prefix")]
        public async Task PrefixAsync([Name("<prefix>")][Summary("The prefix to set")] string? prefix = null)
        {
            var executor = Context.User as Discord.WebSocket.SocketGuildUser;
            var e = Program.DefaultEmbed();
            e.Title = "Prefix";
            if (prefix == null)
            {
                e.Description = $"The current prefix is `{Program.GetServerConfig(Context.Guild.Id).prefix}`";
            }
            else
            {
                if (!executor.GuildPermissions.ManageGuild)
                {
                    e.Description = "You don't have the permissions to do that!";
                    e.Color = Color.Red;
                    await ReplyAsync("", false, e.Build());
                    return;
                }
                foreach (var c in Program.ServerConfigs)
                {
                    if (c.id == Context.Guild.Id)
                    {
                        e.Title = "Prefix";
                        c.prefix = prefix;
                        e.Color = Color.Green;
                        e.Description = $"The prefix has been set to `{prefix}`";
                        break;
                    }
                    else
                    {
                        e.Title = "Error";
                        e.Color = Color.Red;
                        e.Description = $"The prefix has not been set";
                    }
                }
            }
            await ReplyAsync("", false, e.Build());
        }
    }


}