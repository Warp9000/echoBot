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
        [Summary("Shows a list of commands or info about a specified command")]
        public async Task HelpAsync([Remainder][Name("<command>")][Summary("The command to get help for")] string? command = null)
        {
            try
            {
                var builder = Program.DefaultEmbed();
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
                                    if (par.Name != "_")
                                        p += $" {par.Name}";
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
                        builder.WithColor(Discord.Color.Red);
                        builder.WithDescription($"Try `{Program.Config.prefix}help` to see a list of commands");
                        embeds.Add(builder.Build());
                    }
                }
                await ReplyAsync("", false, embeds: embeds.ToArray());
            }
            catch (Exception e)
            {
                await ReplyAsync(e.Message + "\n" + e.StackTrace, false);
            }
        }



        [Command("botinfo")]
        [Summary("Returns info about the bot")]
        [Alias("bot", "info")]
        public async Task BotInfoAsync([Remainder] string? _ = null)
        {
            var e = Program.DefaultEmbed();
            e.Title = "echoBot";
            e.Description = "By Warp#8703";
            e.ThumbnailUrl = "https://cdn.discordapp.com/avatars/869399518267969556/7d05a852cbea15a1028540a913ae43b5.png?size=4096";
            e.Url = "https://warp.tf";
            e.AddField("Links", "[Discord](https://discord.gg/cfcFFECJ4X)\n[Source Code](https://github.com/WarpABoi/echoBot)");
            // e.AddField("Field2", "FieldValue2");
            await ReplyAsync("", false, e.Build());
        }
        [Command("ping")]
        [Summary("Returns the bot's ping")]
        [Alias("pong")]
        public async Task PingAsync([Remainder] string? _ = null)
        {
            var e = Program.DefaultEmbed();
            if (Context.Message.Content.StartsWith(Program.Config.prefix + "ping", true, null))
                e.Title = "Pong!";
            else
                e.Title = "Ping!";
            e.Description = $"{Context.Client.Latency}ms";
            await ReplyAsync("", false, e.Build());
        }
        [Command("uptime")]
        [Summary("Returns the bot's uptime")]
        [Alias("up")]
        public async Task UptimeAsync([Remainder] string? _ = null)
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
        public async Task StatsAsync([Remainder] string? _ = null)
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
        public async Task ServerInfoAsync([Remainder] string? _ = null)
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
        public async Task PrefixAsync([Name("<prefix>")][Summary("The prefix to set")] string? prefix = null, [Remainder] string? _ = null)
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
                    e.Color = Discord.Color.Red;
                    await ReplyAsync("", false, e.Build());
                    return;
                }
                foreach (var c in Program.ServerConfigs)
                {
                    if (c.id == Context.Guild.Id)
                    {
                        e.Title = "Prefix";
                        c.prefix = prefix;
                        e.Color = Discord.Color.Green;
                        e.Description = $"The prefix has been set to `{prefix}`";
                        break;
                    }
                    else
                    {
                        e.Title = "Error";
                        e.Color = Discord.Color.Red;
                        e.Description = $"The prefix has not been set";
                    }
                }
            }
            await ReplyAsync("", false, e.Build());
        }
    }


}