using System.Linq;
using System.Threading.Tasks;

using Discord;
using Discord.Commands;

using Namoroka.Extensions;
using Namoroka.Services;

namespace Namoroka.Modules.Fun
{
    public sealed class UserInfoCommand : NamorokaModuleService
    {
        [Command("userinfo")]
        [Summary("Displays information about a specified user.")]
        [Remarks("userinfo <user>")]
        public async Task ShowUserInfoAsync(IGuildUser user)
        {
            var userColor = Color.LightGrey;
            if (user.RoleIds.Count > 1)
                userColor = user.RoleIds
                    .Select(id => Context.Guild.GetRole(id))
                    .OrderByDescending(role => role.Position)
                    .First().Color;

            var builder = new EmbedBuilder()
                .WithTitle($"Information about {user}:")
                .WithThumbnailUrl(user.GetAvatarUrl())
                .WithColor(userColor)
                .AddField("Username", user.ToString(), true)
                .AddField("ID", user.Id.ToString(), true)
                .AddFieldConditional(!string.IsNullOrEmpty(user.Nickname), "Nickname", user.Nickname, true)
                .AddFieldConditional(user.JoinedAt.HasValue, "Join Date", user.JoinedAt?.ToShortDateString(), true)
                .AddField("User Created", user.CreatedAt.ToShortDateString(), true);

            await builder.Build().SendToChannel(Context.Channel);
        }
    }
}