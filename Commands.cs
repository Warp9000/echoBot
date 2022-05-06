using System.Globalization;
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
                        v += $"{Program.Config.gPrefix}{cmd.Name}{p} - {cmd.Summary}\n";
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
                    builder.WithDescription($"Try `{Program.Config.gPrefix}help` to see a list of commands");
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



    [Name("Users")]
    [Summary("Users Commands")]
    public class UsersModule : ModuleBase<SocketCommandContext>
    {
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
        public async Task AvatarAsync([Name("<user>")][Summary("The user to show avatar of")] Discord.WebSocket.SocketUser? user = null)
        {
            var userInfo = user ?? Context.User;
            var e = Program.DefaultEmbed();
            e.Title = userInfo.Username + "#" + userInfo.Discriminator;
            e.ThumbnailUrl = userInfo.GetAvatarUrl();
            await ReplyAsync("", false, e.Build());
        }
    }


    [Name("Moderation")]
    [Summary("Moderation Commands")]
    public class ModerationModule : ModuleBase<SocketCommandContext>
    {
        [Command("kick")]
        [Summary("Kicks a user")]
        public async Task KickAsync([Name("[user]")][Summary("The user to kick")] Discord.WebSocket.SocketUser user, [Name("<reason>")][Summary("The reason for the kick")] string? reason = null)
        {
            var executor = Context.User as Discord.WebSocket.SocketGuildUser;
            var eUserInfo = user as Discord.WebSocket.SocketGuildUser;
            l.Debug(user.Username + "#" + user.Discriminator, "KickAsync");
            if ((executor.GuildPermissions.Has(GuildPermission.KickMembers) && executor.Roles.Last().Position > eUserInfo.Roles.Last().Position) || executor.Id == Program.Warp)
            {
                await eUserInfo.KickAsync(reason);
                await Task.Delay(Context.Client.Latency + 100);
                await Context.Guild.DownloadUsersAsync();
                if (Context.Guild.GetUser(user.Id) == null)
                {
                    var em = Program.DefaultEmbed();
                    em.Color = Color.Red;
                    em.Title = "Error";
                    em.Description = $"Unkown error, user was not kicked";
                    await ReplyAsync("", false, em.Build());
                    return;
                }

                var e = Program.DefaultEmbed();
                e.Title = "Kicked";
                e.Color = Color.Green;
                e.Description = $"{user.Username}#{user.Discriminator} was succesfully kicked by {executor.Username}#{executor.Discriminator}";
                await ReplyAsync("", false, e.Build());
                Program.Log("Kick", $"{user.Username}#{user.Discriminator} was kicked", Context);
            }
            else
            {
                var e = Program.DefaultEmbed();
                e.Color = Color.Red;
                e.Title = "Error";
                e.Description = "You don't have the permissions to do that!";
                await ReplyAsync("", false, e.Build());
            }
        }

        [Command("ban")]
        [Summary("Bans a user")]
        public async Task BanAsync([Name("[user]")][Summary("The user to ban")] Discord.WebSocket.SocketUser user, [Name("<reason>")][Summary("The reason for the ban")] string? reason = null, [Name("<days>")][Summary("The amount of days to delete messages for")] int? days = null)
        {
            var executor = Context.User as Discord.WebSocket.SocketGuildUser;
            var eUserInfo = user as Discord.WebSocket.SocketGuildUser;
            l.Debug(user.Username + "#" + user.Discriminator, "BanAsync");
            if ((executor.GuildPermissions.Has(GuildPermission.BanMembers) && executor.Roles.Last().Position > eUserInfo.Roles.Last().Position) || executor.Id == Program.Warp)
            {
                await eUserInfo.BanAsync(days.GetValueOrDefault(), reason);
                await Task.Delay(Context.Client.Latency + 100);
                await Context.Guild.DownloadUsersAsync();
                if (Context.Guild.GetUser(user.Id) == null)
                {
                    var em = Program.DefaultEmbed();
                    em.Color = Color.Red;
                    em.Title = "Error";
                    em.Description = $"Unkown error, user was not banned";
                    await ReplyAsync("", false, em.Build());
                    return;
                }

                var e = Program.DefaultEmbed();
                e.Title = "Banned";
                e.Color = Color.Green;
                e.Description = $"{user.Username}#{user.Discriminator} was succesfully banned by {executor.Username}#{executor.Discriminator}";
                await ReplyAsync("", false, e.Build());
                Program.Log("Ban", $"{user.Username}#{user.Discriminator} was banned", Context);
            }
            else
            {
                var e = Program.DefaultEmbed();
                e.Color = Color.Red;
                e.Title = "Error";
                e.Description = "You don't have the permissions to do that!";
                await ReplyAsync("", false, e.Build());
            }
        }

        [Command("unban")]
        [Summary("Unbans a user")]
        public async Task UnbanAsync([Name("[user]")][Summary("The user to unban")] Discord.WebSocket.SocketUser user, [Name("<reason>")][Summary("The reason for the unban")] string? reason = null)
        {
            var executor = Context.User as Discord.WebSocket.SocketGuildUser;
            var eUserInfo = user as IUser;
            l.Debug(user.Username + "#" + user.Discriminator, "UnbanAsync");
            if (executor.GuildPermissions.Has(GuildPermission.BanMembers) || executor.Id == Program.Warp)
            {
                if (Context.Guild.GetUser(user.Id) != null)
                {
                    var em = Program.DefaultEmbed();
                    em.Color = Color.Red;
                    em.Title = "Error";
                    em.Description = $"{user.Username}#{user.Discriminator} is not banned";
                    await ReplyAsync("", false, em.Build());
                    return;
                }
                await executor.Guild.RemoveBanAsync(user);
                var e = Program.DefaultEmbed();
                e.Title = "Unbanned";
                e.Color = Color.Green;
                e.Description = $"{user.Username}#{user.Discriminator} was succesfully unbanned by {executor.Username}#{executor.Discriminator}";
                await ReplyAsync("", false, e.Build());
                Program.Log("Unban", $"{user.Username}#{user.Discriminator} was unbanned", Context);
            }
            else
            {
                var e = Program.DefaultEmbed();
                e.Color = Color.Red;
                e.Title = "Error";
                e.Description = "You don't have the permissions to do that!";
                await ReplyAsync("", false, e.Build());
            }
        }

        [Command("purge")]
        [Summary("Purges a number of messages")]
        public async Task PurgeAsync([Name("[amount]")][Summary("The amount of messages to purge")] int amount)
        {
            var executor = Context.User as Discord.WebSocket.SocketGuildUser;
            l.Debug(amount.ToString(), "PurgeAsync");
            if (executor.GuildPermissions.Has(GuildPermission.ManageMessages) || executor.Id == Program.Warp)
            {
                var messages = await Context.Channel.GetMessagesAsync(amount + 1).FlattenAsync();
                var ch = Context.Channel as ITextChannel;
                if (ch == null || messages.Count() <= 0)
                {
                    var em = Program.DefaultEmbed();
                    em.Color = Color.Red;
                    em.Title = "Error";
                    em.Description = "No messages to purge";
                    await ReplyAsync("", false, em.Build());
                    return;
                }
                await ch.DeleteMessagesAsync(messages);
                var e = Program.DefaultEmbed();
                e.Title = "Purged";
                e.Color = Color.Green;
                var purged = messages.Count() - 1;
                e.Description = $"{purged} messages were purged by {executor.Username}#{executor.Discriminator}";
                var m = await ReplyAsync("", false, e.Build());
                Program.Log("Purge", $"{amount} messages Purged in <#{Context.Channel.Id}>", Context);
                await Task.Delay(1500);
                await m.DeleteAsync();

            }
            else
            {
                var e = Program.DefaultEmbed();
                e.Color = Color.Red;
                e.Title = "Error";
                e.Description = "You don't have the permissions to do that!";
                await ReplyAsync("", false, e.Build());
            }
        }

        [Command("logchannel")]
        [Summary("Sets the log channel")]
        public async Task LogChannelAsync([Name("[channel]")][Summary("The channel to set the log channel to")] Discord.WebSocket.SocketTextChannel channel)
        {
            var executor = Context.User as Discord.WebSocket.SocketGuildUser;
            l.Debug(channel.Name, "LogChannelAsync");
            if (executor.GuildPermissions.Has(GuildPermission.ManageChannels) || executor.Id == Program.Warp)
            {
                var e = Program.DefaultEmbed();
                e.Title = "Log Channel";
                foreach (var c in Program.ServerConfigs)
                {
                    if (c.id == Context.Guild.Id)
                    {
                        c.logChannel = channel.Id;
                        e.Color = Color.Green;
                        e.Description = $"The log channel has been set to <#{channel.Id}>";
                        break;
                    }
                    else
                    {
                        e.Title = "Error";
                        e.Color = Color.Red;
                        e.Description = $"The log channel has not been set";
                    }
                }
                await ReplyAsync("", false, e.Build());
            }
            else
            {
                var e = Program.DefaultEmbed();
                e.Color = Color.Red;
                e.Title = "Error";
                e.Description = "You don't have the permissions to do that!";
                await ReplyAsync("", false, e.Build());
            }
        }
    }

}
