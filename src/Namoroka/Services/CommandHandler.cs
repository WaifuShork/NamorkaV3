using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

using Discord;
using Discord.Addons.Hosting;
using Discord.Commands;
using Discord.WebSocket;
using Namoroka.Utilities;
using Namoroka.Infrastructure;
using Victoria;

namespace Namoroka.Services
{
    public class CommandHandler : InitializedService
    {
        private readonly DiscordSocketClient _client;
        private readonly IServiceProvider _provider;
        private readonly CommandService _commands;
        private readonly IConfiguration _configuration;
        private readonly Servers _servers;
        private readonly AutoRolesHelper _autoRolesHelper;
        private readonly LavaNode _lavaNode;
        
        public CommandHandler(DiscordSocketClient client, CommandService commands, IConfiguration configuration, IServiceProvider provider, Servers servers, AutoRolesHelper autoRolesHelper, LavaNode lavaNode)
        {
            _client = client;
            _commands = commands;
            _provider = provider;
            _configuration = configuration;
            _servers = servers;
            _autoRolesHelper = autoRolesHelper;
            _lavaNode = lavaNode;
        }

        public override async Task InitializeAsync(CancellationToken cancellationToken)
        {
            _client.MessageReceived += OnMessageReceived;
            _client.UserJoined += OnUserJoined;
            _client.Ready += OnReadyAsync;

            _commands.CommandExecuted += OnCommandExecuted; 
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _provider);
        }
        
        private async Task OnReadyAsync() 
        {
            if (!_lavaNode.IsConnected) 
            {
                await _lavaNode.ConnectAsync();
            }
        }

        private async Task OnUserJoined(IGuildUser user)
        {
            var roles = await _autoRolesHelper.GetAutoRolesAsync(user.Guild);
            if (roles.Count < 1) return;
            await user.AddRolesAsync(roles);
        }

        private async Task OnMessageReceived(SocketMessage msg)
        {
            if (!(msg is SocketUserMessage message)) return;
            if (message.Source != MessageSource.User) return;

            var argPos = 0;
            var prefix = await _servers.GetGuildPrefix((message.Channel as SocketGuildChannel).Guild.Id) ?? "!";
            if (!message.HasStringPrefix(prefix, ref argPos) && !message.HasMentionPrefix(_client.CurrentUser, ref argPos))
                return;

            var context = new SocketCommandContext(_client, message);
            await _commands.ExecuteAsync(context, argPos, _provider);
        }

        private async Task OnCommandExecuted(Optional<CommandInfo> command, ICommandContext context, IResult result)
        {
            if (command.IsSpecified && !result.IsSuccess) await context.Channel.SendMessageAsync($"Error: {result}");
        }
    }
}