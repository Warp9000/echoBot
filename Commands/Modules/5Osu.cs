using Discord.Commands;
using Newtonsoft.Json;
using System.Text;


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
            public string error = "none";
            // --------------------------------------------------
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
        public async Task Beatmap([Name("[query]")][Summary("name or id to lookup")][Remainder] string? beatmapId)
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
                    await ReplyAsync("query is null");
                    return;
                }

                var url = RequestUrl + "beatmaps/lookup";
                if (int.TryParse(beatmapId, out _))
                {
                    url += "?id=" + beatmapId;
                }
                else
                {
                    url += "?filename=" + beatmapId;
                }
                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", cc.access_token);
                var responseMessage = await client.SendAsync(request);

                var s = responseMessage.Content.ReadAsStringAsync().Result;
                File.WriteAllText("beatmap.json", "/*\n" + responseMessage.StatusCode + "\n\n" + responseMessage.Headers.ToString() + "\n*/\n" + JsonPrettify(s));

                var map = JsonConvert.DeserializeObject<OsuBeatmap>(s);

                if (map.error != "none")
                {
                    await ReplyAsync("```Error: " + map.error ?? "null" + "\nCode: " + responseMessage.StatusCode + "```");
                    return;
                }

                var em = Program.DefaultEmbed();
                em.Title = map.beatmapset.artist + " - " + map.beatmapset.title;
                em.ThumbnailUrl = map.beatmapset.covers.list;
                em.Description = $"**Length:** {new TimeSpan(0, 0, map.total_length).ToString(@"mm\:ss")} **BPM:** {map.bpm}\n[Download]({map.url})";
                em.AddField($"{map.mode} - {map.version}",
                $"**▸Difficulty:** {map.difficulty_rating} **▸Max Combo:** {map.max_combo.ToString() ?? "NaN"}\n" +
                $"**▸AR:** {map.ar} **▸OD:** {map.accuracy} **▸CS:** {map.cs} **▸HP:** {map.drain}\n" +
                "pp in progress...");

                await ReplyAsync("", embed: em.Build());
            }
            catch (Exception e)
            {
                l.Error(e.Message, "Beatmap");
                l.Error(e.StackTrace ?? "no stack trace", "Beatmap");
                await ReplyAsync("```\n" + e.ToString() + "\n```");
            }
        }

        public class OsuScore
        {
            public string error = "none";
            // --------------------------------------------------
            public int id;
            public int best_id;
            public int user_id;
            public float accuracy;
            public string[]? mods;
            public int score;
            public int max_combo;
            public bool perfect;
            public OsuScoreStatistics statistics = new OsuScoreStatistics();
            public bool passed;
            public float pp;
            public string rank = "";
            public DateTime created_at;
            public string mode = "";
            public int mode_int;
            public bool replay;
            // --------------------------------------------------
            public OsuBeatmap? beatmap;
            public OsuBeatmapset? beatmapset;
            public int? rank_country;
            public int? rank_global;
            public float? weight;
            public int? user;
            public string? match;
        }
        public class OsuScoreJson
        {
            public string error = "none";
            public OsuScore[] scores;
        }
        public class OsuScoreStatistics
        {
            [JsonProperty("count_50")]
            public int count_50;
            [JsonProperty("count_100")]
            public int count_100;
            [JsonProperty("count_300")]
            public int count_300;
            [JsonProperty("count_geki")]
            public int count_geki;
            [JsonProperty("count_katu")]
            public int count_katu;
            [JsonProperty("count_miss")]
            public int count_miss;
        }

        [Command("scores")]
        [Summary("get scores on map")]
        public async Task Score([Name("[map]")][Summary("map id")] ulong? mapId, [Name("[user]")][Summary("user id")] ulong? userId)
        {
            try
            {
                await Context.Channel.TriggerTypingAsync();
                if (!IsCredentialsValid())
                {
                    await ReplyAsync("credentials are not valid");
                    return;
                }
                if (mapId == null)
                {
                    await ReplyAsync("map id is null");
                    return;
                }
                if (userId == null)
                {
                    await ReplyAsync("user id is null");
                    return;
                }

                var url = RequestUrl + $"beatmaps/{mapId}/scores/users/{userId}/all";
                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", cc.access_token);
                var responseMessage = await client.SendAsync(request);

                var s = responseMessage.Content.ReadAsStringAsync().Result;
                File.WriteAllText("scores.json", "/*\n" + responseMessage.StatusCode + "\n\n" + responseMessage.Headers.ToString() + "\n*/\n" + JsonPrettify(s));

                var scoresJson = JsonConvert.DeserializeObject<OsuScoreJson>(s) ?? new OsuScoreJson();

                if (scoresJson.scores.Length == 0)
                {
                    await ReplyAsync("no scores found");
                    return;
                }

                var url2 = RequestUrl + "beatmaps/lookup?id=" + mapId;
                var request2 = new HttpRequestMessage(HttpMethod.Get, url2);
                request2.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", cc.access_token);
                var responseMessage2 = await client.SendAsync(request2);

                var s2 = responseMessage2.Content.ReadAsStringAsync().Result;
                File.WriteAllText("beatmap.json", "/*\n" + responseMessage2.StatusCode + "\n\n" + responseMessage2.Headers.ToString() + "\n*/\n" + JsonPrettify(s2));

                var map = JsonConvert.DeserializeObject<OsuBeatmap>(s2);

                if (map.error != "none")
                {
                    await ReplyAsync("```Error: " + map.error ?? "null" + "\nCode: " + responseMessage2.StatusCode + "```");
                    return;
                }


                var em = Program.DefaultEmbed();
                em.Title = map.beatmapset.artist + " - " + map.beatmapset.title;
                em.ThumbnailUrl = map.beatmapset.covers.list;
                foreach (var score in scoresJson.scores)
                {
                    em.AddField($"{score.mode} - {score.mods?.FirstOrDefault() ?? "No Mods"}",
                    $"**▸pp:** {score.pp} **▸Acc:** {score.accuracy}\n" +
                    $"**▸Score:** {score.score} **▸Combo:** {score.max_combo} ▸[{score.statistics.count_geki}/{score.statistics.count_300}/{score.statistics.count_katu}/{score.statistics.count_100}/{score.statistics.count_50}/{score.statistics.count_miss}]\n" +
                    $"{score.created_at.ToString("yyyy/MM/dd HH:mm:ss")}");
                }
                await ReplyAsync("", embed: em.Build());
            }
            catch (Exception e)
            {
                l.Error(e.Message, "Score");
                l.Error(e.StackTrace ?? "no stack trace", "Score");
                await ReplyAsync("```\n" + e.ToString() + "\n```");
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
