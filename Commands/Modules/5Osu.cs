using System.Net.WebSockets;
using Discord;
using Discord.Commands;
using System.Net;
using System.Net.Sockets;
using Newtonsoft.Json;
using System.Text;
using System.Collections;

namespace echoBot
{
    [Group("osu")]
    [Name("Osu")]
    [Summary("Osu commands")]
    public class OsuModule : ModuleBase<SocketCommandContext>
    {
        public Dictionary<string, string> Headers = new();
        public string RequestUrl = "https://osu.ppy.sh/api/v2/";
        public class ClientCredentials
        {
            public string token_type = "";
            public int expires_in;
            public string access_token = "";
            public DateTime created = DateTime.Now;
        }
        public ClientCredentials cc;
        public HttpClient client = new();
        public bool IsCredentialsValid()
        {
            if (File.Exists("osu.json"))
                cc = JsonConvert.DeserializeObject<ClientCredentials>(File.ReadAllText("osu.json"));
            if (cc == null) return false;
            return cc.created.AddSeconds(cc.expires_in) > DateTime.Now;
        }


        [Command("cauth")]
        [Summary("client credentials")]
        public async Task ClientAuth([Name("<force>")][Summary("force a refresh of the credentials")] bool force = false, [Remainder] string? _ = null)
        {
            if (!force && IsCredentialsValid())
            {
                await ReplyAsync("```cached```\n`token type`: " + cc.token_type + "\n`expires in`: " + ((int)(cc.created.AddSeconds(cc.expires_in) - DateTime.Now).TotalSeconds) + " seconds" + "\n`access token`: " + cc.access_token);
                return;
            }

            // var client = new HttpClient();

            Headers = new Dictionary<string, string>
            {
                { "client_id", "15047"},
                { "client_secret", "RWEXmWOKhrVa7XLAxqGNkRRfzA9UMaDEI3OuIFPM"},
                { "grant_type", "client_credentials"},
                { "scope", "public "}
            };


            var json = JsonConvert.SerializeObject(Headers);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var responseMessage = await client.PostAsync("https://osu.ppy.sh/oauth/token", content);

            cc = JsonConvert.DeserializeObject<ClientCredentials>(await responseMessage.Content.ReadAsStringAsync()) ?? new();
            File.WriteAllText("osu.json", JsonConvert.SerializeObject(cc));
            cc.created = DateTime.Now;
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + cc.access_token);
            try
            {
                await ReplyAsync("```cached```\n`token type`: " + cc.token_type + "\n`expires in`: " + ((int)(cc.created.AddSeconds(cc.expires_in) - DateTime.Now).TotalSeconds) + " seconds" + "\n`access token`: " + cc.access_token);
            }
            catch (Exception e)
            {
                l.Error(e.ToString(), "ClientAuth");
            }
            // await ReplyAsync(responseMessage.Content.ReadAsStringAsync().Result);
            l.Debug(cc.ToString(), "ClientAuth");
        }

        public class OsuBeatmap
        {
            public int beatmapset_id;
            public float difficulty_rating;
            public int id;
            public string mode = "";
            public string status = "";
            public int total_length;
            public int user_id;
            public string version = "";
            // --------------------------------------------------
            public float accuracy; // OD
            public float ar;
            // public int beatmapset_id;
            public float? bpm;
            public bool convert;
            public int count_circles;
            public int count_sliders;
            public int count_spinners;
            public float cs;
            public DateTime? deleted_at;
            public float drain;
            public int hit_length;
            public bool is_scoreable;
            public DateTime last_updated;
            public int mode_int;
            public int passcount;
            public int playcount;
            public int ranked;
            public string url = "";
            // --------------------------------------------------
            public OsuBeatmapset? beatmapset;
            public string? checksum;
            public OsuFailTimes? failtimes;
            public int? max_combo;

        }
        public class OsuBeatmapset
        {
            public string artist = "";
            public string artist_unicode = "";
            public OsuCovers? covers;
            public string creator = "";
            public int favourite_count;
            public int id;
            public bool nsfw;
            public int play_count;
            public string preview_url = "";
            public string source = "";
            public string status = "";
            public string title = "";
            public string title_unicode = "";
            public int user_id;
            public bool video;

        }
        public class OsuFailTimes
        {
            public int[]? exit;
            public int[]? fail;
        }
        public class OsuCovers
        {
            public string cover = "";
            public string card = "";
            public string list = "";
            public string slimcover = "";
        }

        [Command("beatmap")]
        [Summary("get beatmap info")]
        public async Task Beatmap([Name("[id]")][Summary("the beatmap id")] int? beatmapId, [Remainder] string? _ = null)
        {
            try
            {
                await Context.Channel.TriggerTypingAsync();
                if (!IsCredentialsValid())
                {
                    await ReplyAsync("credentials are not valid");
                    return;
                }
                if (beatmapId == null)
                {
                    await ReplyAsync("beatmap id is null");
                    return;
                }

                var request = new HttpRequestMessage(HttpMethod.Get, RequestUrl + "beatmaps/" + beatmapId);
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", cc.access_token);
                var responseMessage = await client.SendAsync(request);

                var s = responseMessage.Content.ReadAsStringAsync().Result;
                // s = JsonPrettify(s);
                // if (s.Length > 2000)
                // {
                //     await ReplyAsync("```json\n" + s.Substring(0, Math.Min(s.Length, 1986)) + "\n```");
                //     await ReplyAsync("```json\n" + s.Substring(1986, Math.Min(s.Length - 1986, 1986)) + "\n```");
                //     if (s.Length > 4000)
                //     {
                //         await ReplyAsync("`Content continues...`");
                //     }
                // }
                // else
                //     await ReplyAsync(s);

                // embed
                var map = JsonConvert.DeserializeObject<OsuBeatmap>(s);

                var em = Program.DefaultEmbed();
                em.Title = map.beatmapset.artist + " - " + map.beatmapset.title;
                em.ThumbnailUrl = map.beatmapset.covers.list;
                em.Description = $"**Length:** {new TimeSpan(0, 0, map.total_length).ToString(@"mm\:ss")} **BPM:** {map.bpm}\n[Download]({map.url})";
                em.AddField($"{map.mode} - {map.version}",
                $"**▸Difficulty:** {map.difficulty_rating} **▸Max Combo:** {map.max_combo}\n" +
                $"**▸AR:** {map.ar} **▸OD:** {map.accuracy} **▸CS:** {map.cs} **▸HP:** {map.drain}\n" +
                "pp in progress...");

                await ReplyAsync("", embed: em.Build());
            }
            catch (Exception e)
            {
                l.Error(e.Message, "Beatmap");
                l.Error(e.StackTrace, "Beatmap");
            }
        }
        public static string JsonPrettify(string json)
        {
            using (var stringReader = new StringReader(json))
            using (var stringWriter = new StringWriter())
            {
                var jsonReader = new JsonTextReader(stringReader);
                var jsonWriter = new JsonTextWriter(stringWriter) { Formatting = Formatting.Indented };
                jsonWriter.WriteToken(jsonReader);
                return stringWriter.ToString();
            }
        }
    }
}