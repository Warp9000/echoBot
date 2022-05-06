
using System.Runtime.CompilerServices;
using Discord;
using Discord.Commands;
using Newtonsoft.Json;

public class GlobalConfig
{
    public string token { get; set; } = "";
    public string gPrefix { get; set; } = ";;";
    public string status { get; set; } = "";
    public string game { get; set; } = "";
}
public class ServerConfig
{
    public ulong id { get; set; } = 0;
    public string prefix { get; set; } = ";;";
    public ulong logChannel { get; set; } = 0;
}
public class Program
{
    public static Task Main(string[] args) => new Program().MainAsync();
    public static string version = "0.0.3 Dev";
    public static Discord.WebSocket.DiscordSocketClient? _client;
    public static Discord.WebSocket.DiscordSocketConfig config = new Discord.WebSocket.DiscordSocketConfig();
    public static GlobalConfig Config = new GlobalConfig();
    public static DateTime startTime = DateTime.Now;
    public static int Commands = 0;
    public static ulong Warp = 408615875252322305;
    public static List<ServerConfig>? ServerConfigs = new List<ServerConfig>();
    public async Task MainAsync()
    {
        Config = JsonConvert.DeserializeObject<GlobalConfig>(File.ReadAllText("config.json"));
        config = new Discord.WebSocket.DiscordSocketConfig
        {
            AlwaysDownloadUsers = true,
            MessageCacheSize = 250,
            LogLevel = LogSeverity.Debug,
            GatewayIntents = GatewayIntents.All
        };
        _client = new Discord.WebSocket.DiscordSocketClient(config);

        _client.Log += Log;

        if (string.IsNullOrEmpty(Config.token))
        {
            l.Critical("Token is empty!", "MainAsync");
            return;
        }
        if (string.IsNullOrEmpty(Config.status))
        {
            l.Info("No status set. Defaulting to online", "MainAsync");
            Config.status = "Online";
        }

        await _client.LoginAsync(TokenType.Bot, Config.token);
        await _client.StartAsync();
        await _client.SetActivityAsync(new Game(Config.game, ActivityType.Watching, details: "https://warp.tf/"));
        // await _client.SetGameAsync(Config.game);
        await _client.SetStatusAsync((UserStatus)Enum.Parse(typeof(UserStatus), Config.status));

        CommandServiceConfig csc = new CommandServiceConfig
        {
            CaseSensitiveCommands = false,
            // DefaultRunMode = RunMode.Async,
            LogLevel = config.LogLevel
        };

        var ch = new echoBot.CommandHandler(_client, new CommandService(csc));
        await ch.InstallCommandsAsync();
        if (File.Exists("servers.json"))
            ServerConfigs = JsonConvert.DeserializeObject<List<ServerConfig>>(File.ReadAllText("servers.json"));
        List<ulong> servers = new List<ulong>();
        foreach (var server in _client.Guilds)
        {
            servers.Clear();
            for (var i = 0; i < ServerConfigs.Count; i++)
            {
                servers.Add(ServerConfigs[i].id);
            }
            l.Info($"Adding server {server.Name} to config", "MainAsync");
            if (!servers.Contains(server.Id))
                ServerConfigs.Add(new ServerConfig
                {
                    id = server.Id,
                    prefix = Config.gPrefix,
                    logChannel = 0
                });

        }
        File.WriteAllText("servers.json", JsonConvert.SerializeObject(ServerConfigs));

        // console commands
        while (true)
        {
            string input = Console.ReadLine();
            switch (input)
            {
                case "exit":
                    File.WriteAllText("servers.json", JsonConvert.SerializeObject(ServerConfigs));
                    Task.Delay(1000).Wait();
                    Environment.Exit(0);
                    break;
                case "reload":
                    Config = JsonConvert.DeserializeObject<GlobalConfig>(File.ReadAllText("config.json"));
                    break;
                case "save":
                    servers = new List<ulong>();
                    foreach (var server in _client.Guilds)
                    {
                        servers.Clear();
                        for (var i = 0; i < ServerConfigs.Count; i++)
                        {
                            servers.Add(ServerConfigs[i].id);
                        }
                        l.Info($"Adding server {server.Name} to config", "MainAsync");
                        if (!servers.Contains(server.Id))
                            ServerConfigs.Add(new ServerConfig
                            {
                                id = server.Id,
                                prefix = Config.gPrefix,
                                logChannel = 0
                            });

                    }
                    File.WriteAllText("servers.json", JsonConvert.SerializeObject(ServerConfigs));
                    break;
                case "help":
                    Console.WriteLine("exit - exits the program");
                    Console.WriteLine("reload - reloads the config");
                    Console.WriteLine("save - saves the server configs");
                    Console.WriteLine("help - shows this message");
                    break;
                default:
                    Console.WriteLine("Unknown command. Use 'help' to see a list of commands.");
                    break;
            }
        }

        // Block this task until the program is closed.
        // await Task.Delay(-1);
    }
    public Task Log(LogMessage msg)
    {
        switch (msg.Severity)
        {
            case LogSeverity.Critical:
                l.Critical(msg.Message, msg.Source);
                break;
            case LogSeverity.Error:
                l.Error(msg.Message, msg.Source);
                break;
            case LogSeverity.Warning:
                l.Warning(msg.Message, msg.Source);
                break;
            case LogSeverity.Info:
                l.Info(msg.Message, msg.Source);
                break;
            case LogSeverity.Verbose:
                l.Verbose(msg.Message, msg.Source);
                break;
            case LogSeverity.Debug:
                l.Debug(msg.Message, msg.Source);
                break;
        }
        return Task.CompletedTask;
    }
    public static EmbedBuilder DefaultEmbed()
    {
        return new EmbedBuilder().WithColor(Color.DarkPurple).WithCurrentTimestamp().WithFooter(new EmbedFooterBuilder()
        {
            Text = $"echoBot {version}",
            IconUrl = "https://cdn.discordapp.com/avatars/869399518267969556/7d05a852cbea15a1028540a913ae43b5.png?size=4096"
        });
    }
    public static ServerConfig GetServerConfig(ulong id)
    {
        foreach (var server in ServerConfigs)
        {
            if (server.id == id)
                return server;
        }
        return null;
    }
    public static ITextChannel GetLogChannel(ulong id)
    {
        return _client.GetChannel(id) as ITextChannel;
    }
    public static void Log(string title, string message, SocketCommandContext context)
    {
        var e = DefaultEmbed();
        e.WithAuthor(context.User);
        e.WithTitle(title);
        e.WithDescription(message);
        GetLogChannel(GetServerConfig(context.Guild.Id).logChannel).SendMessageAsync(null, false, e.Build());
    }
}

public class l
{
    public static void Log(LogMessage msg)
    {
        if (msg.Severity <= Program.config.LogLevel)
            Console.WriteLine($"[{System.DateTime.Now.ToString()}] [{msg.Severity}] [{msg.Source}] {msg.Message} {msg.Exception}");
    }
    public static void Debug(string msg, string source = "?")
    {
        Log(new LogMessage(LogSeverity.Debug, source, msg));
    }
    public static void Verbose(string msg, string source = "?")
    {
        Log(new LogMessage(LogSeverity.Verbose, source, msg));
    }
    public static void Info(string msg, string source = "?")
    {
        Log(new LogMessage(LogSeverity.Info, source, msg));
    }
    public static void Warning(string msg, string source = "?")
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Log(new LogMessage(LogSeverity.Warning, source, msg));
        Console.ResetColor();
    }
    public static void Error(string msg, string source = "?")
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Log(new LogMessage(LogSeverity.Error, source, msg));
        Console.ResetColor();
    }
    public static void Critical(string msg, string source = "?")
    {
        Console.ForegroundColor = ConsoleColor.DarkRed;
        Log(new LogMessage(LogSeverity.Critical, source, msg));
        Console.ResetColor();
    }
}


