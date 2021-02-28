using System.IO;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using Namoroka.Services;
using Namoroka.Utilities;

namespace Namoroka.Modules.Fun
{
    public class ImageCommand : NamorokaModuleService
    {
        private readonly Images _images;

        public ImageCommand(Images images)
        {
            _images = images;
        }

        [Command("image", RunMode = RunMode.Async)]
        public async Task ImageAsync(SocketGuildUser user)
        {
            var path = await _images.CreateImageAsync(user);
            await Context.Channel.SendFileAsync(path);
            File.Delete(path);
        }
    }
}