
using System.Runtime.CompilerServices;
using Discord;
using Discord.Commands;
using Newtonsoft.Json;

public class JsonConfig
{
    public string token { get; set; } = "";
    public string prefix { get; set; } = "!";
    public string status { get; set; } = "";
    public string game { get; set; } = "";
}
public class Program
{
    public static Task Main(string[] args) => new Program().MainAsync();

    private Discord.WebSocket.DiscordSocketClient? _client;
    public static Discord.WebSocket.DiscordSocketConfig config = new Discord.WebSocket.DiscordSocketConfig();
    public static JsonConfig Config = new JsonConfig();

    public async Task MainAsync()
    {
        Config = JsonConvert.DeserializeObject<JsonConfig>(File.ReadAllText("config.json"));
        config = new Discord.WebSocket.DiscordSocketConfig
        {
            AlwaysDownloadUsers = true,
            MessageCacheSize = 250,
            LogLevel = LogSeverity.Debug,
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
        await _client.SetGameAsync(Config.game);
        await _client.SetStatusAsync((UserStatus)Enum.Parse(typeof(UserStatus), Config.status));

        var ch = new echoBot.CommandHandler(_client, new CommandService());
        await ch.InstallCommandsAsync();

        // Block this task until the program is closed.
        await Task.Delay(-1);
    }
    public Task Log(LogMessage msg)
    {
        l.Log(msg);
        return Task.CompletedTask;
    }
}
public class l
{
    public static void Log(LogMessage msg)
    {
        if (msg.Severity <= Program.config.LogLevel)
        Console.WriteLine($"[{System.DateTime.Now.ToString()}] [{msg.Severity}] [{msg.Source}] {msg.Message}");
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


