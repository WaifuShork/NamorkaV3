using System;
using System.Linq;
using System.Threading.Tasks;

using Discord;
using Discord.Commands;

using Namoroka.Services;
using Namoroka.Utilities;
using Namoroka.Infrastructure;

namespace Namoroka.Modules
{
    public sealed class Configuration : NamorokaModuleService
    {
        private readonly Servers _servers;
        private readonly RanksHelper _ranksHelper;
        private readonly Ranks _ranks;
        private readonly AutoRolesHelper _autoRolesHelper;
        private readonly AutoRoles _autoRoles;

        public Configuration(Servers servers, RanksHelper ranksHelper, Ranks ranks, AutoRolesHelper autoRolesHelper, AutoRoles autoRoles)
        {
            _servers = servers;
            _ranksHelper = ranksHelper;
            _ranks = ranks;
            _autoRolesHelper = autoRolesHelper;
            _autoRoles = autoRoles;
        }
        
        [Command("ranks", RunMode = RunMode.Async)]
        public async Task RanksAsync()
        {
            var ranks = await _ranksHelper.GetRanksAsync(Context.Guild);

            if (ranks.Count == 0)
            {
                await ReplyAsync("This server does not have any ranks");
                return;
            }

            await Context.Channel.TriggerTypingAsync();

            var description = "This message lists all available ranks\nIn order to add a rank, you can use the name or Id of the rank";
            foreach (var rank in ranks)
            {
                description += $"\n- {rank.Mention} ({rank.Id})";
            }

            await ReplyAsync(description);
        }

        [Command("addrank", RunMode = RunMode.Async)]
        [RequireUserPermission(GuildPermission.Administrator)]
        [RequireBotPermission(ChannelPermission.ManageRoles)]
        public async Task AddRankAsync([Remainder] string name)
        {
            await Context.Channel.TriggerTypingAsync();
            var ranks = await _ranksHelper.GetRanksAsync(Context.Guild);

            var role = Context.Guild.Roles.FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.CurrentCultureIgnoreCase));
            if (role == null)
            {
                await ReplyAsync("The given role does not exist");
                return;
            }

            if (role.Position > Context.Guild.CurrentUser.Hierarchy)
            {
                await ReplyAsync("That role has a higher position that I have access to");
                return;
            }

            if (ranks.Any(x => x.Id == role.Id))
            {
                await ReplyAsync("That role is already a rank");
            }

            await _ranks.AddRankAsync(Context.Guild.Id, role.Id);
            await ReplyAsync($"The role {role.Mention} was successfully added to the rank table");

        }

        [Command("delrank", RunMode = RunMode.Async)]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task DeleteRankAsync([Remainder] string name)
        {
            await Context.Channel.TriggerTypingAsync();
            var ranks = await _ranksHelper.GetRanksAsync(Context.Guild);
            
            var role = Context.Guild.Roles.FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.CurrentCultureIgnoreCase));
            if (role == null)
            {
                await ReplyAsync("The given role does not exist");
                return;
            }

            if (ranks.Any(x => x.Id != role.Id))
            {
                await ReplyAsync("The given role is not a rank");
                return;
            }

            await _ranks.RemoveRankAsync(Context.Guild.Id, role.Id);
            await ReplyAsync($"The role {role.Mention} was removed from the rank table");
        }

        [Command("autoroles", RunMode = RunMode.Async)]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task AutoRolesAsync()
        {
            var autoRoles = await _autoRolesHelper.GetAutoRolesAsync(Context.Guild);

            if (autoRoles.Count == 0)
            {
                await ReplyAsync("This server does not have any auto roles");
                return;
            }

            await Context.Channel.TriggerTypingAsync();

            var description = "This message lists all auto roles\nIn order to remove an auto role, use the name or Id";
            foreach (var autoRole in autoRoles)
            {
                description += $"\n- {autoRole.Mention} ({autoRole.Id})";
            }

            await ReplyAsync(description);
        }

        [Command("addrole", RunMode = RunMode.Async)]
        [RequireUserPermission(GuildPermission.Administrator)]
        [RequireBotPermission(ChannelPermission.ManageRoles)]
        public async Task AddAutoRoleAsync([Remainder] string name)
        {
            await Context.Channel.TriggerTypingAsync();
            var autoRoles = await _autoRolesHelper.GetAutoRolesAsync(Context.Guild);

            var role = Context.Guild.Roles.FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.CurrentCultureIgnoreCase));
            if (role == null)
            {
                await ReplyAsync("The given role does not exist");
                return;
            }

            if (role.Position > Context.Guild.CurrentUser.Hierarchy)
            {
                await ReplyAsync("That role has a higher position that I have access to");
                return;
            }

            if (autoRoles.Any(x => x.Id == role.Id))
            {
                await ReplyAsync("That role is already an auto role");
            }

            await _autoRoles.AddAutoRoleAsync(Context.Guild.Id, role.Id);
            await ReplyAsync($"The role {role.Mention} was successfully added as an auto role");

        }
        
        [Command("delautorole", RunMode = RunMode.Async)]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task DeleteAutoRoleAsync([Remainder] string name)
        {
            await Context.Channel.TriggerTypingAsync();
            var autoRoles = await _autoRolesHelper.GetAutoRolesAsync(Context.Guild);
            
            var role = Context.Guild.Roles.FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.CurrentCultureIgnoreCase));
            if (role == null)
            {
                await ReplyAsync("The given role does not exist");
                return;
            }

            if (autoRoles.Any(x => x.Id != role.Id))
            {
                await ReplyAsync("The given role is not an auto role");
                return;
            }

            await _autoRoles.RemoveAutoRoleAsync(Context.Guild.Id, role.Id);
            await ReplyAsync($"The role {role.Mention} was removed from the auto role sequence");
        }
        
        [Command("prefix", RunMode = RunMode.Async)]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task ChangePrefixAsync(string prefix = null)
        {
            if (prefix == null)
            {
                var pf = await _servers.GetGuildPrefix(Context.Guild.Id) ?? "!";
                await ReplyAsync($"Prefix is: {pf}");
                return;
            }

            if (prefix.Length > 8)
            {
                await ReplyAsync("Please choose a shorter prefix");
                return;
            }

            await _servers.ModifyGuildPrefix(Context.Guild.Id, prefix);
            await ReplyAsync($"Changed prefixed to: {prefix}");
        }
    }
}