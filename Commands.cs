using Discord;
using Discord.Commands;
using System.IO;


namespace echoBot
{
    // public class Commands : ModuleBase<SocketCommandContext>
    // {

    // }


    [Name("Info")]
    [Summary("Info Commands")]
    public class InfoModule : ModuleBase<SocketCommandContext>
    {
        [Command("help")]
        [Summary("Shows a list of commands")]
        public async Task HelpAsync([Name("<command>")][Summary("The command to get help for")] string? command = null)
        {
            var builder = new EmbedBuilder();
            builder.WithFooter(CommandHandler.GetFooter());
            builder.WithColor(Color.DarkPurple);
            builder.WithCurrentTimestamp();
            List<Embed> embeds = new List<Embed>();
            if (command == null)
            {
                builder.WithTitle("Help");
                builder.WithDescription("[required] <optional>");
                foreach (var item in CommandHandler._commands.Modules)
                {
                    var v = "";
                    var p = " ";
                    foreach (var cmd in item.Commands)
                    {
                        if (cmd.Parameters.Count > 0)
                        {
                            foreach (var par in cmd.Parameters)
                            {
                                p += $"{par.Name} ";
                            }
                            p.TrimEnd(' ');
                        }
                        v += $"{Program.Config.prefix}{cmd.Name}{p} - {cmd.Summary}\n";
                        p = " ";
                    }
                    builder.AddField(item.Name, v);
                }
                embeds.Add(builder.Build());
            }
            else
            {
                var cmd = CommandHandler._commands.Search(command);
                if (cmd.IsSuccess)
                {
                    for (var i = 0; i < cmd.Commands.Count; i++)
                    {
                        builder.WithTitle($"{string.Join(", ", cmd.Commands[i].Command.Aliases)}".TrimEnd(' ', ','));
                        builder.WithDescription(cmd.Commands[i].Command.Summary);
                        foreach (var par in cmd.Commands[i].Command.Parameters)
                        {
                            builder.AddField(par.Name, par.Summary);
                        }
                        if (!embeds.Contains(builder.Build()))
                            embeds.Add(builder.Build());
                    }
                }
                else
                {
                    builder.WithTitle("Command not found");
                    builder.WithColor(Color.Red);
                    builder.WithDescription($"Try `{Program.Config.prefix}help` to see a list of commands");
                    embeds.Add(builder.Build());
                }
            }
            await ReplyAsync("", false, embeds: embeds.ToArray());
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


        [Command("userinfo")]
        [Summary
        ("Returns info about a user")]
        [Alias("user", "whois")]
        public async Task UserInfoAsync([Name("<user>")][Summary("The user to show info about")] Discord.WebSocket.SocketUser? user = null)
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

            var e = new EmbedBuilder();
            e.Color = c;
            // e.Title = userInfo.Username + "#" + userInfo.Discriminator;
            e.Description = userInfo.Mention + "\nID: " + userInfo.Id.ToString();
            e.ThumbnailUrl = userInfo.GetAvatarUrl();
            e.AddField("Joined", eUserInfo.JoinedAt.ToString().Substring(0,19), true);
            e.AddField("Created", userInfo.CreatedAt.ToString().Substring(0,19), true);
            var fb = new EmbedFieldBuilder();
            foreach (var item in eUserInfo.Roles.Reverse())
            {
                fb.Value += $"<@&{item.Id}>, ";
            }
            e.AddField(eUserInfo.Roles.Count + " Roles", fb.Build().Value.TrimEnd(',', ' '));
            e.Footer = CommandHandler.GetFooter();
            e.WithCurrentTimestamp();
            e.Author = new EmbedAuthorBuilder
            {
                Name = userInfo.Username + "#" + userInfo.Discriminator,
                IconUrl = userInfo.GetAvatarUrl()
            };
            await ReplyAsync("", false, e.Build());
        }
    }
}