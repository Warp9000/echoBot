using Discord;
using Discord.Commands;

namespace echoBot
{
    public class CommandHandler
    {
        public readonly Discord.WebSocket.DiscordSocketClient _client;
        public static CommandService _commands;

        // Retrieve client and CommandService instance via ctor
        public CommandHandler(Discord.WebSocket.DiscordSocketClient client, CommandService commands)
        {
            _commands = commands;
            _client = client;
        }

        public async Task InstallCommandsAsync()
        {
            l.Verbose("Starting...", "CommandHandler");
            // Hook the MessageReceived event into our command handler
            _client.MessageReceived += HandleCommandAsync;
            _client.MessageDeleted += HandleDeletedAsync;
            _client.ButtonExecuted += HandleButtonAsync;
            // Here we discover all of the command modules in the entry 
            // assembly and load them. Starting from Discord.NET 2.0, a
            // service provider is required to be passed into the
            // module registration method to inject the 
            // required dependencies.
            //
            // If you do not use Dependency Injection, pass null.
            // See Dependency Injection guide for more information.
            await _commands.AddModulesAsync(assembly: System.Reflection.Assembly.GetEntryAssembly(),
                                            services: null);

            Program.Commands = _commands.Commands.Count();
        }

        private async Task HandleCommandAsync(Discord.WebSocket.SocketMessage messageParam)
        {
            // Don't process the command if it was a system message
            var message = messageParam as Discord.WebSocket.SocketUserMessage;
            if (message == null) return;

            // Create a number to track where the prefix ends and the command begins
            int argPos = 0;
            // Determine if the message is a command based on the prefix and make sure no bots trigger commands
            if (message.Author.IsBot)
                return;
            if (!message.HasStringPrefix(Program.Config.prefix, ref argPos) &&
                !message.HasStringPrefix(Program.GetServerConfig(new SocketCommandContext(_client, message).Guild.Id).prefix, ref argPos) &&
                !message.HasMentionPrefix(_client.CurrentUser, ref argPos))
                return;

            l.Verbose($"\"{messageParam.Content}\" sent by {messageParam.Author.Username + "#" + messageParam.Author.Discriminator} in #{messageParam.Channel.Name}({messageParam.Channel.Id})", "CommandHandler");

            // Create a WebSocket-based command context based on the message
            var context = new SocketCommandContext(_client, message);

            // Execute the command with the command context we just
            // created, along with the service provider for precondition checks.
            await _commands.ExecuteAsync(
                context: context,
                argPos: argPos,
                services: null);
        }
        private Task HandleDeletedAsync(Cacheable<IMessage, UInt64> m, Cacheable<IMessageChannel, UInt64> ch)
        {
            if (m.HasValue)
                l.Verbose($"\"{m.Value}\" deleted by {m.Value.Author.Username}#{m.Value.Author.Discriminator} in {ch.Value}({ch.Id})", "DeleteHandler");
            else
                l.Verbose($"Message deleted in {ch.Value}({ch.Id})", "DeleteHandler");
            return Task.CompletedTask;
        }
        private async Task HandleButtonAsync(Discord.WebSocket.SocketMessageComponent component)
        {
            HelpLink? link = null;
            try
            {
                switch (component.Data.CustomId)
                {
                    case "help-button-next":
                        link = HelpLinks.Find(x => x.Message.Id == component.Message.Id);
                        if (link == null)
                            return;
                        if (link.UserId != component.User.Id)
                            await component.RespondAsync("You are not the author of this message.", ephemeral: true);

                        var e = Program.DefaultEmbed();
                        var mp = new MessageProperties();
                        var page = int.Parse(component.Message.Embeds.ToList()[0].Title.Substring(6, 1));
                        ModuleInfo[] ml = _commands.Modules.ToArray();
                        ModuleInfo m = ml[page];
                        if (page + 1 <= ml.Length)
                        {
                            page++;
                            m = _commands.Modules.ToArray()[page - 1];
                        }

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
                            v += $"{Program.Config.prefix}{cmd.Name}{p} - {cmd.Summary}\n";
                            p = " ";
                        }
                        e.AddField(m.Name, v);
                        e.Title = $"Help ({page}/{ml.Length})";
                        e.Description = "[required] <optional>";
                        var c = new ComponentBuilder();
                        if (page > 1)
                            c.WithButton("Prev", "help-button-prev");
                        else
                            c.WithButton("Prev", "help-button-prev-f", ButtonStyle.Secondary, disabled: true);
                        if (page < ml.Length)
                            c.WithButton("Next", "help-button-next");
                        else
                            c.WithButton("Next", "help-button-next-f", ButtonStyle.Secondary, disabled: true);
                        await component.UpdateAsync(x =>
                        {
                            x.Embed = e.Build();
                            x.Components = c.Build();
                        });
                        break;



                    case "help-button-prev":
                        link = HelpLinks.Find(x => x.Message.Id == component.Message.Id);
                        if (link == null)
                            return;
                        if (link.UserId != component.User.Id)
                            await component.RespondAsync("You are not the author of this message.", ephemeral: true);

                        e = Program.DefaultEmbed();
                        mp = new MessageProperties();
                        page = int.Parse(component.Message.Embeds.ToList()[0].Title.Substring(6, 1));
                        ml = _commands.Modules.ToArray();
                        m = ml[page - 1];
                        if (page - 1 >= 0)
                        {
                            page--;
                            m = _commands.Modules.ToArray()[page - 1];
                        }

                        v = "";
                        p = "";
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
                            v += $"{Program.Config.prefix}{cmd.Name}{p} - {cmd.Summary}\n";
                            p = " ";
                        }
                        e.AddField(m.Name, v);
                        e.Title = $"Help ({page}/{ml.Length})";
                        e.Description = "[required] <optional>";
                        c = new ComponentBuilder();
                        if (page > 1)
                            c.WithButton("Prev", "help-button-prev");
                        else
                            c.WithButton("Prev", "help-button-prev-f", ButtonStyle.Secondary, disabled: true);
                        if (page < ml.Length)
                            c.WithButton("Next", "help-button-next");
                        else
                            c.WithButton("Next", "help-button-next-f", ButtonStyle.Secondary, disabled: true);
                        await component.UpdateAsync(x =>
                        {
                            x.Embed = e.Build();
                            x.Components = c.Build();
                        });
                        break;
                }
            }
            catch (Exception ex)
            {
                l.Error(ex.StackTrace!, "HelpButtonHandler");
            }
        }
        public static List<HelpLink> HelpLinks = new List<HelpLink>();
        public static EmbedFooterBuilder GetFooter()
        {
            var fb = new EmbedFooterBuilder()
            {
                Text = $"echoBot {Program.version}",
                IconUrl = "https://cdn.discordapp.com/avatars/869399518267969556/7d05a852cbea15a1028540a913ae43b5.png?size=4096"
            };
            return fb;
        }
    }
    public class HelpLink
    {
        public IUserMessage Message { get; set; }
        public ulong UserId { get; set; }
        public HelpLink(IUserMessage m, ulong u)
        {
            Message = m;
            UserId = u;
        }
    }
}