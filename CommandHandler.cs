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
            if (!message.HasStringPrefix(Program.Config.gPrefix, ref argPos) && !message.HasStringPrefix(Program.GetServerConfig(new SocketCommandContext(_client, message).Guild.Id).prefix, ref argPos))
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
            l.Verbose($"\"{m.Value}\" deleted by {m.Value.Author.Username}#{m.Value.Author.Discriminator} in {ch.Value}({ch.Id})", "DeleteHandler");
            return Task.CompletedTask;
        }
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
}