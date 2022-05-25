using Discord;
using Discord.Commands;
using Newtonsoft.Json;
using System.Net;


namespace echoBot
{
    [Group("old")]
    [Name("Old")]
    [Summary("Old commands")]
    public class OldCommands : ModuleBase<SocketCommandContext>
    {
        [Command("help")]
        [Summary("Shows a list of commands")]
        public async Task HelpAsync([Remainder][Name("<command>")][Summary("The command to get help for")] string? command = null)
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
    }

}