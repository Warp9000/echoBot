using Discord;
using Discord.Commands;
using Newtonsoft.Json;
using System.Net;


namespace echoBot
{
    [Name("Fun")]
    [Summary("Fun Commands")]
    public class FunModule : ModuleBase<SocketCommandContext>
    {
        [Command("roll")]
        [Summary("Rolls a random number")]
        [Alias("rand", "random")]
        public async Task RollAsync([Name("<max>")][Summary("The max number to roll")] int max = 100, [Remainder] string? _ = null)
        {
            var e = Program.DefaultEmbed();
            e.Title = "Roll";
            e.Description = $"You rolled a {new Random().Next(1, max)}";
            await ReplyAsync("", false, e.Build());
        }

        [Command("flip")]
        [Summary("Flips a coin")]
        [Alias("coinflip", "flipcoin")]
        public async Task FlipAsync([Remainder] string? _ = null)
        {
            var e = Program.DefaultEmbed();
            e.Title = "Flip";
            e.Description = $"You flipped a {(new Random().Next(0, 2) == 0 ? "Heads" : "Tails")}";
            await ReplyAsync("", false, e.Build());
        }

        [Command("8ball")]
        [Summary("Ask the magic 8 ball a question")]
        [Alias("8")]
        public async Task EightBallAsync([Remainder][Name("[question]")][Summary("The question to ask")] string question)
        {
            var e = Program.DefaultEmbed();
            e.Title = "8Ball";
            int seed = question.GetHashCode();
            e.Description = $"{EightBall[new Random(seed).Next(0, EightBall.Length)]}";
            await ReplyAsync("", false, e.Build());
        }

        public static string[] EightBall = {
            "It is certain",
            "It is decidedly so",
            "Without a doubt",
            "Yes, definitely",
            "You may rely on it",
            "As I see it, yes",
            "Most likely",
            "Outlook good",
            "Yes",
            "Signs point to yes",
            "Reply hazy, try again",
            "Ask again later",
            "Better not tell you now",
            "Cannot predict now",
            "Concentrate and ask again",
            "Don't count on it",
            "My reply is no",
            "My sources say no",
            "Outlook not so good",
            "Very doubtful"
        };

        [Command("fox")]
        [Summary("Gets a random fox image")]
        public async Task FoxAsync([Remainder] string? _ = null)
        {
            var e = Program.DefaultEmbed();
            e.Title = Fox[new Random().Next(0, Fox.Length)];
            string url = JsonConvert.DeserializeAnonymousType(new WebClient().DownloadString("https://randomfox.ca/floof/"), new { image = "" }).image;
            e.ImageUrl = url;
            await ReplyAsync("", false, e.Build());
        }

        public static string[] Fox = {
            "Cute Fox",
            "Fox",
            "Foxy",
            "Fuzzy",
            "Fuzzy Fox",
            "Fuzzy Foxy",
        };

        [Command("duck")]
        [Summary("Gets a random duck image")]
        public async Task DuckAsync([Remainder] string? _ = null)
        {
            var e = Program.DefaultEmbed();
            e.Title = Duck[new Random().Next(0, Duck.Length)];
            string url = JsonConvert.DeserializeAnonymousType(new WebClient().DownloadString("https://random-d.uk/api/v1/random"), new { url = "" }).url;
            e.ImageUrl = url;
            await ReplyAsync("", false, e.Build());
        }

        public static string[] Duck = {
            "Cute Duck",
            "Duck",
            "Ducky",
        };

        [Command("shiba")]
        [Summary("Gets a random shiba image")]
        public async Task ShibaAsync([Remainder] string? _ = null)
        {
            var e = Program.DefaultEmbed();
            e.Title = Shiba[new Random().Next(0, Shiba.Length)];
            string url = new WebClient().DownloadString("https://shibe.online/api/shibes?count=1").TrimStart('[').TrimEnd(']').TrimStart('"').TrimEnd('"');
            e.ImageUrl = url;
            await ReplyAsync("", false, e.Build());
        }

        public static string[] Shiba = {
            "Cute Shiba",
            "Shiba",
            "Shibby",
        };

        [Command("cat")]
        [Summary("Gets a random cat image")]
        public async Task CatAsync([Remainder] string? _ = null)
        {
            var e = Program.DefaultEmbed();
            e.Title = Cat[new Random().Next(0, Cat.Length)];
            string url = JsonConvert.DeserializeAnonymousType(new WebClient().DownloadString("https://api.thecatapi.com/v1/images/search").TrimStart('[').TrimEnd(']'), new { url = "" }).url;
            e.ImageUrl = url;
            await ReplyAsync("", false, e.Build());
        }

        public static string[] Cat = {
            "Cute Cat",
            "Cat",
            "Cathy",
        };
    }

}