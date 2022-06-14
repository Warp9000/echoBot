using Discord;
using Discord.Commands;
using Newtonsoft.Json;
using System.Net;


namespace echoBot
{
    [Name("Moderation")]
    [Summary("Moderation Commands")]
    public class ModerationModule : ModuleBase<SocketCommandContext>
    {
        [Command("kick")]
        [Summary("Kicks a user")]
        public async Task KickAsync([Name("[user]")][Summary("The user to kick")] Discord.WebSocket.SocketUser user, [Remainder][Name("<reason>")][Summary("The reason for the kick")] string? reason = null)
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
                    em.Color = Discord.Color.Red;
                    em.Title = "Error";
                    em.Description = $"Unkown error, user was not kicked";
                    await ReplyAsync("", false, em.Build());
                    return;
                }

                var e = Program.DefaultEmbed();
                e.Title = "Kicked";
                e.Color = Discord.Color.Green;
                e.Description = $"{user.Username}#{user.Discriminator} was succesfully kicked by {executor.Username}#{executor.Discriminator}";
                await ReplyAsync("", false, e.Build());
                Program.Log("Kick", $"{user.Username}#{user.Discriminator} was kicked", Context);
            }
            else
            {
                var e = Program.DefaultEmbed();
                e.Color = Discord.Color.Red;
                e.Title = "Error";
                e.Description = "You don't have the permissions to do that!";
                await ReplyAsync("", false, e.Build());
            }
        }

        [Command("ban")]
        [Summary("Bans a user")]
        public async Task BanAsync([Name("[user]")][Summary("The user to ban")] Discord.WebSocket.SocketUser user, [Remainder][Name("<reason>")][Summary("The reason for the ban")] string? reason = null)
        {
            var executor = Context.User as Discord.WebSocket.SocketGuildUser;
            var eUserInfo = user as Discord.WebSocket.SocketGuildUser;
            l.Debug(user.Username + "#" + user.Discriminator, "BanAsync");
            if ((executor.GuildPermissions.Has(GuildPermission.BanMembers) && executor.Roles.Last().Position > eUserInfo.Roles.Last().Position) || executor.Id == Program.Warp)
            {
                await eUserInfo.BanAsync(0, reason);
                await Task.Delay(Context.Client.Latency + 100);
                await Context.Guild.DownloadUsersAsync();
                if (Context.Guild.GetUser(user.Id) == null)
                {
                    var em = Program.DefaultEmbed();
                    em.Color = Discord.Color.Red;
                    em.Title = "Error";
                    em.Description = $"Unkown error, user was not banned";
                    await ReplyAsync("", false, em.Build());
                    return;
                }

                var e = Program.DefaultEmbed();
                e.Title = "Banned";
                e.Color = Discord.Color.Green;
                e.Description = $"{user.Username}#{user.Discriminator} was succesfully banned by {executor.Username}#{executor.Discriminator}";
                await ReplyAsync("", false, e.Build());
                Program.Log("Ban", $"{user.Username}#{user.Discriminator} was banned", Context);
            }
            else
            {
                var e = Program.DefaultEmbed();
                e.Color = Discord.Color.Red;
                e.Title = "Error";
                e.Description = "You don't have the permissions to do that!";
                await ReplyAsync("", false, e.Build());
            }
        }

        [Command("unban")]
        [Summary("Unbans a user")]
        public async Task UnbanAsync([Name("[user]")][Summary("The user to unban")] Discord.WebSocket.SocketUser user, [Remainder] string? _ = null)
        {
            var executor = Context.User as Discord.WebSocket.SocketGuildUser;
            var eUserInfo = user as IUser;
            l.Debug(user.Username + "#" + user.Discriminator, "UnbanAsync");
            if (executor.GuildPermissions.Has(GuildPermission.BanMembers) || executor.Id == Program.Warp)
            {
                if (Context.Guild.GetUser(user.Id) != null)
                {
                    var em = Program.DefaultEmbed();
                    em.Color = Discord.Color.Red;
                    em.Title = "Error";
                    em.Description = $"{user.Username}#{user.Discriminator} is not banned";
                    await ReplyAsync("", false, em.Build());
                    return;
                }
                await executor.Guild.RemoveBanAsync(user);
                var e = Program.DefaultEmbed();
                e.Title = "Unbanned";
                e.Color = Discord.Color.Green;
                e.Description = $"{user.Username}#{user.Discriminator} was succesfully unbanned by {executor.Username}#{executor.Discriminator}";
                await ReplyAsync("", false, e.Build());
                Program.Log("Unban", $"{user.Username}#{user.Discriminator} was unbanned", Context);
            }
            else
            {
                var e = Program.DefaultEmbed();
                e.Color = Discord.Color.Red;
                e.Title = "Error";
                e.Description = "You don't have the permissions to do that!";
                await ReplyAsync("", false, e.Build());
            }
        }

        [Command("purge")]
        [Summary("Purges a number of messages")]
        public async Task PurgeAsync([Name("[amount]")][Summary("The amount of messages to purge")] int amount, [Remainder] string? _ = null)
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
                    em.Color = Discord.Color.Red;
                    em.Title = "Error";
                    em.Description = "No messages to purge";
                    await ReplyAsync("", false, em.Build());
                    return;
                }
                await ch.DeleteMessagesAsync(messages);
                var e = Program.DefaultEmbed();
                e.Title = "Purged";
                e.Color = Discord.Color.Green;
                var purged = messages.Count() - 1;
                e.Description = $"{purged}/{amount} messages were purged by {executor.Username}#{executor.Discriminator}";
                var m = await ReplyAsync("", false, e.Build());
                Program.Log("Purge", $"{purged}/{amount} messages Purged in <#{Context.Channel.Id}>", Context);
                await Task.Delay(1500);
                await m.DeleteAsync();

            }
            else
            {
                var e = Program.DefaultEmbed();
                e.Color = Discord.Color.Red;
                e.Title = "Error";
                e.Description = "You don't have the permissions to do that!";
                await ReplyAsync("", false, e.Build());
            }
        }

        [Command("logchannel")]
        [Summary("Sets the log channel")]
        public async Task LogChannelAsync([Name("[channel]")][Summary("The channel to set the log channel to")] Discord.WebSocket.SocketTextChannel channel, [Remainder] string? _ = null)
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
                        e.Color = Discord.Color.Green;
                        e.Description = $"The log channel has been set to <#{channel.Id}>";
                        break;
                    }
                    else
                    {
                        e.Title = "Error";
                        e.Color = Discord.Color.Red;
                        e.Description = $"The log channel has not been set";
                    }
                }
                await ReplyAsync("", false, e.Build());
            }
            else
            {
                var e = Program.DefaultEmbed();
                e.Color = Discord.Color.Red;
                e.Title = "Error";
                e.Description = "You don't have the permissions to do that!";
                await ReplyAsync("", false, e.Build());
            }
        }
    }

}