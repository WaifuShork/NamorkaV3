using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

using Discord;
using Discord.Commands;

using Namoroka.Services;

namespace Namoroka.Modules.Fun
{
    public class Meme : NamorokaModuleService
    {
        [Command("meme")]
        [Alias("reddit")]
        public async Task SendMemeAsync(string subreddit = null)
        {
            var client = new HttpClient();
            var result = await client.GetStringAsync($"https://www.reddit.com/r/{subreddit ?? "memes"}/random.json?limit=1");
            if (!result.StartsWith("["))
            {
                await Context.Channel.SendMessageAsync($"{subreddit} subreddit does not exit");
                return;
            }
            
            var arr = JArray.Parse(result);
            var post = JObject.Parse(arr[0]["data"]["children"][0]["data"].ToString());
            var embed = new EmbedBuilder()
                .WithImageUrl(post["url"].ToString())
                .WithColor(new Color(33, 176, 252))
                .WithTitle(post["title"].ToString());

            await Context.Channel.SendMessageAsync(embed: embed.Build());
        }
        
        [Command("mommy")]
        public async Task SendMommyAsync()
        {
            await ReplyAsync("https://i.redd.it/859447wh1kf61.jpg");
        }
    }
}