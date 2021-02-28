using System.Linq;
using System.Threading.Tasks;

using Discord;
using Discord.Commands;
using Discord.WebSocket;

using Namoroka.Services;

namespace Namoroka.Modules.Moderation
{
    public sealed partial class Moderation : NamorokaModuleService
    {
        [Command("clear")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task ClearAsync(int amount)
        {
            var msgs = await Context.Channel.GetMessagesAsync(amount + 1).FlattenAsync();
            var messages = msgs as IMessage[] ?? msgs.ToArray();
            await (Context.Channel as SocketTextChannel)!.DeleteMessagesAsync(messages);

            var message = await Context.Channel.SendMessageAsync($"{messages.Length} messages deleted successfully");
            await Task.Delay(2500);
            await message.DeleteAsync();
        }
    }
}