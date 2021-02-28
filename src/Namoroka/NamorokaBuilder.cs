using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Discord;
using Discord.Addons.Hosting;
using Discord.Commands;
using Discord.WebSocket;

using Namoroka.Services;
using Namoroka.Utilities;
using Namoroka.Infrastructure;
using Victoria;

namespace Namoroka
{
    public class NamorokaBuilder
    {
        public NamorokaBuilder() { }
        
        public async Task RunAsync(string[] args)
        {
            var builder = new NamorokaBuilder();
            await builder.BuildAsync();
        }

        private async Task BuildAsync()
        {
            var builder = new HostBuilder()
                .ConfigureAppConfiguration(x =>
                {
                    var configuration = new ConfigurationBuilder()
                        .SetBasePath(Directory.GetCurrentDirectory())
                        .AddJsonFile("src/Namoroka/_config.json", false, true)
                        .Build();

                    x.AddConfiguration(configuration);
                })
                .ConfigureLogging(x =>
                {
                    x.AddConsole();
                    x.SetMinimumLevel(LogLevel.Debug);
                })
                .ConfigureDiscordHost<DiscordSocketClient>((context, config) =>
                {
                    config.SocketConfig = new DiscordSocketConfig
                    {
                        LogLevel = LogSeverity.Verbose,
                        AlwaysDownloadUsers = true,
                        MessageCacheSize = 200,
                    };

                    config.Token = context.Configuration["token"];
                })
                .UseCommandService((context, config) =>
                {
                    config.CaseSensitiveCommands = false;
                    config.LogLevel = LogSeverity.Verbose;
                    config.DefaultRunMode = RunMode.Sync;
                })
                .ConfigureServices((context, services) =>
                {
                    services
                        .AddHostedService<CommandHandler>()
                        .AddDbContext<NamorokaContext>()
                        .AddSingleton<Servers>()
                        .AddSingleton<Images>()
                        .AddSingleton<Ranks>()
                        .AddSingleton<RanksHelper>()
                        .AddSingleton<AutoRoles>()
                        .AddSingleton<AutoRolesHelper>()
                        .AddLavaNode(x =>
                        {
                            x.SelfDeaf = true;
                            x.LogSeverity = LogSeverity.Verbose;
                        });
                })
                .UseConsoleLifetime();
            
            // await LaunchLavalink();
            var host = builder.Build();
            using (host)
            {
                await host.RunAsync();
            }
        }

        private static async Task LaunchLavalink()
        {
            var process = new Process
            {
                StartInfo =
                {
                    FileName = "cmd.exe", 
                    Arguments = @"java -jar src\Namoroka\LavaLink\Lavalink.jar",
                    UseShellExecute = true,
                }
            };

            process.Start();
            await Task.CompletedTask;
        }
    }
}