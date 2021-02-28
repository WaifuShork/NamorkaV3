using System;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Namoroka.Services;
using Victoria;
using Victoria.Enums;
using Victoria.EventArgs;

namespace Namoroka.Modules.Music
{
    public sealed class Music : NamorokaModuleService
    {
        private readonly LavaNode _lavaNode;

        public Music(LavaNode lavaNode)
        {
            _lavaNode = lavaNode;
        }
        
        [Command("join", RunMode = RunMode.Async)]
        public async Task JoinAsync() 
        {
            if (_lavaNode.HasPlayer(Context.Guild)) 
            {
                await ReplyAsync("I'm already connected to a voice channel!");
                return;
            }
            var voiceState = Context.User as IVoiceState;
            
            if (voiceState?.VoiceChannel == null) 
            {
                await ReplyAsync("You must be connected to a voice channel!");
                return;
            }

            try 
            {
                await _lavaNode.JoinAsync(voiceState.VoiceChannel, Context.Channel as ITextChannel);
                await ReplyAsync($"Joined {voiceState.VoiceChannel.Name}!");
            }
            catch (Exception exception) 
            {
                await ReplyAsync(exception.Message);
            }
        }

        [Command("play", RunMode = RunMode.Async)]
        public async Task PlayAsync([Remainder] string query) 
        {
            
            if (string.IsNullOrWhiteSpace(query)) 
            {
                await ReplyAsync("Please provide search terms.");
                return;
            }   

            if (!_lavaNode.HasPlayer(Context.Guild)) 
            {
                await ReplyAsync("I'm not connected to a voice channel.");
                return;
            }
            
            var searchResponse = await _lavaNode.SearchYouTubeAsync(query);
            if (searchResponse.LoadStatus == LoadStatus.LoadFailed || searchResponse.LoadStatus == LoadStatus.NoMatches) 
            {
                await ReplyAsync($"I wasn't able to find anything for `{query}`.");
                return; 
            }

            var player = _lavaNode.GetPlayer(Context.Guild);
            
            if (player.PlayerState == PlayerState.Playing || player.PlayerState == PlayerState.Paused) 
            {
                var track = searchResponse.Tracks[0];
                player.Queue.Enqueue(track);
                
                var thumbnail = track.FetchArtworkAsync();
                var enqueued = $"{track.Author} :: {track.Title} :: {track.Duration}";
                var embed = new EmbedBuilder()
                    .AddField("Enqueued", enqueued)
                    .WithThumbnailUrl(thumbnail.ToString());
                
                await ReplyAsync(embed: embed.Build());
            }
            else 
            {
                var track = searchResponse.Tracks[0];
                
                var thumbnail = track.FetchArtworkAsync();
                var currentTrack = $"{track.Author} :: {track.Title} :: {track.Duration}";
                var embed = new EmbedBuilder()
                    .AddField("Now Playing", currentTrack)
                    .WithThumbnailUrl(thumbnail.ToString());
                
                await ReplyAsync(embed: embed.Build());
                await player.PlayAsync(track);
            }
        }
        
        [Command("playbar", RunMode = RunMode.Async)]
        public async Task PlayBarVisualizerAsync()
        {
            var voiceState = Context.User as IVoiceState;
            if (voiceState?.VoiceChannel == null) 
            {
                await ReplyAsync("You must be connected to a voice channel!");
                return;
            }
            
            if (!_lavaNode.HasPlayer(Context.Guild)) 
            {
                await ReplyAsync("I'm not connected to a voice channel.");
                return;
            }

            var player = _lavaNode.GetPlayer(Context.Guild);

            if (voiceState.VoiceChannel != player.VoiceChannel)
            {
                await ReplyAsync("You need to be in the same voice channel as me.");
                return;
            }
            
            if (player.PlayerState == PlayerState.Paused || player.PlayerState == PlayerState.Stopped)
            {
                await ReplyAsync("Can't view playbar with the song paused");
                return;
            }

            var embed = new EmbedBuilder
            {

            };

            await ReplyAsync(embed: embed.Build());
        }

        [Command("skip", RunMode = RunMode.Async)]
        public async Task SkipAsync()
        {
            var voiceState = Context.User as IVoiceState;
            if (voiceState?.VoiceChannel == null) 
            {
                await ReplyAsync("You must be connected to a voice channel!");
                return;
            }
            
            if (!_lavaNode.HasPlayer(Context.Guild)) 
            {
                await ReplyAsync("I'm not connected to a voice channel.");
                return;
            }

            var player = _lavaNode.GetPlayer(Context.Guild);

            if (voiceState.VoiceChannel != player.VoiceChannel)
            {
                await ReplyAsync("You need to be in the same voice channel as me.");
                return;
            }

            if (player.Queue.Count == 0)
            {
                await ReplyAsync("There's no songs left to skip");
                return;
            }

            await player.SkipAsync();

            var playerTrack = player.Track;
            var thumbnail = playerTrack.FetchArtworkAsync();
            var track = $"{playerTrack.Author} :: {playerTrack.Title} :: {playerTrack.Duration}";
            var embed = new EmbedBuilder()
                .AddField("Now Playing", track)
                .WithThumbnailUrl(thumbnail.ToString());
                
            await ReplyAsync(embed: embed.Build());
        }
        
        [Command("pause", RunMode = RunMode.Async)]
        public async Task PauseAsync()
        {
            var voiceState = Context.User as IVoiceState;
            if (voiceState?.VoiceChannel == null) 
            {
                await ReplyAsync("You must be connected to a voice channel!");
                return;
            }
            
            if (!_lavaNode.HasPlayer(Context.Guild)) 
            {
                await ReplyAsync("I'm not connected to a voice channel.");
                return;
            }

            var player = _lavaNode.GetPlayer(Context.Guild);

            if (voiceState.VoiceChannel != player.VoiceChannel)
            {
                await ReplyAsync("You need to be in the same voice channel as me.");
                return;
            }

            if (player.PlayerState == PlayerState.Paused || player.PlayerState == PlayerState.Stopped)
            {
                await ReplyAsync("The music is already paused");
                return;
            }
            
            var playerTrack = player.Track;
            var thumbnail = playerTrack.FetchArtworkAsync();
            var paused = $"{playerTrack.Author} :: {playerTrack.Title} :: {playerTrack.Duration}";
            var embed = new EmbedBuilder()
                .AddField("Paused", paused)
                .WithThumbnailUrl(thumbnail.ToString());
                
            await ReplyAsync(embed: embed.Build());
            
            await player.PauseAsync();
        }
        
        [Command("resume", RunMode = RunMode.Async)]
        public async Task ResumeAsync()
        {
            var voiceState = Context.User as IVoiceState;
            if (voiceState?.VoiceChannel == null) 
            {
                await ReplyAsync("You must be connected to a voice channel!");
                return;
            }
            
            if (!_lavaNode.HasPlayer(Context.Guild)) 
            {
                await ReplyAsync("I'm not connected to a voice channel.");
                return;
            }

            var player = _lavaNode.GetPlayer(Context.Guild);

            if (voiceState.VoiceChannel != player.VoiceChannel)
            {
                await ReplyAsync("You need to be in the same voice channel as me.");
                return;
            }

            if (player.PlayerState == PlayerState.Playing)
            {
                await ReplyAsync("The music is already playing");
                return;
            }
            
            var playerTrack = player.Track;
            var thumbnail = playerTrack.FetchArtworkAsync();
            var resumed = $"{playerTrack.Author} :: {playerTrack.Title} :: {playerTrack.Duration}";
            var embed = new EmbedBuilder()
                .AddField("Resumed", resumed)
                .WithThumbnailUrl(thumbnail.ToString());
                
            await ReplyAsync(embed: embed.Build());
            
            await player.ResumeAsync();
        }
        
        [Command("leave", RunMode = RunMode.Async)]
        public async Task LeaveAsync()
        {
            var voiceState = Context.User as IVoiceState;
            if (voiceState?.VoiceChannel == null) 
            {
                await ReplyAsync("You must be connected to a voice channel!");
                return;
            }
            
            if (!_lavaNode.HasPlayer(Context.Guild)) 
            {
                await ReplyAsync("I'm not connected to a voice channel.");
                return;
            }

            var player = _lavaNode.GetPlayer(Context.Guild);

            if (voiceState.VoiceChannel != player.VoiceChannel)
            {
                await ReplyAsync("You need to be in the same voice channel as me.");
                return;
            }

            await _lavaNode.LeaveAsync(player.VoiceChannel);
        }
        
        [Command("queue")]
        public async Task QueueAsync()
        {
            var voiceState = Context.User as IVoiceState;
            if (voiceState?.VoiceChannel == null) 
            {
                await ReplyAsync("You must be connected to a voice channel!");
                return;
            }
            
            if (!_lavaNode.HasPlayer(Context.Guild)) 
            {
                await ReplyAsync("I'm not connected to a voice channel.");
                return;
            }

            var player = _lavaNode.GetPlayer(Context.Guild);

            if (voiceState.VoiceChannel != player.VoiceChannel)
            {
                await ReplyAsync("You need to be in the same voice channel as me.");
                return;
            }

            var builder = new EmbedBuilder();
            var stringBuilder = new StringBuilder();
            if (player.Queue.Count >= 1)
            {
                foreach (var item in player.Queue)
                {
                    stringBuilder.AppendLine($"{item.Author} :: {item.Title} :: {item.Duration}\n");
                }

                builder.AddField("------ Currently Playing ------\n",
                    $"{player.Track.Author} :: {player.Track.Title} :: {player.Track.Duration}");
                builder.AddField("------ Tracks ------\n", stringBuilder.ToString());
                var buildEmbed = builder.Build();
                await ReplyAsync(embed: buildEmbed);
            }
            else if (player.Queue.Count == 0)
            {
                builder = new EmbedBuilder().AddField("------ Currently Playing ------\n",
                    $"{player.Track.Author} :: {player.Track.Title} :: {player.Track.Duration}");
                var embed = builder.Build();
                await ReplyAsync(embed: embed);
            }
        }
        
        private async Task OnTrackEnded(TrackEndedEventArgs args) 
        {
            if (!args.Reason.ShouldPlayNext()) 
            {
                return;
            }

            var player = args.Player;
            if (!player.Queue.TryDequeue(out var queueable)) 
            {
                await player.TextChannel.SendMessageAsync("Queue completed! Please add more tracks to rock n' roll!");
                return;
            }

            if (!(queueable is LavaTrack track)) 
            {
                await player.TextChannel.SendMessageAsync("Next item in queue is not a track.");
                return;
            }

            await args.Player.PlayAsync(track);
            await args.Player.TextChannel.SendMessageAsync($"{args.Reason}: {args.Track.Title}\nNow playing: {track.Title}");
        }
    }
}