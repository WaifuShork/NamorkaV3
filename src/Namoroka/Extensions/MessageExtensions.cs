using System.Threading.Tasks;

using Discord;

namespace Namoroka.Extensions
{
    public static class MessageExtensions
    {
        public static async Task SendToChannel(this Embed e, IMessageChannel channel) => await channel.SendMessageAsync(string.Empty, false, e);
        
        public static EmbedBuilder AddFieldConditional(this EmbedBuilder eb, bool condition, string name, object value, bool inline = false)
        {
            if (condition) {
                var toPost = value?.ToString();

                eb.AddField(name, CropToLength(toPost, EmbedFieldBuilder.MaxFieldValueLength), inline);
            }
            return eb;
        }

        public static async Task<bool> TrySendMessageAsync(this IUser user, string text = null, bool isTTS = false, Embed embed = null)
        {
            try
            {
                await user.SendMessageAsync(CropToLength(text, EmbedBuilder.MaxDescriptionLength), isTTS, embed);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static string CropToLength(string msg, int length) 
        {
            if (msg?.Length > length) 
                return msg.Substring(0, length - 3) + "...";
            return msg;
        }

        public static async void TimedDeletion(this IMessage message, int milliseconds) 
        {
            await Task.Delay(milliseconds);
            await message.DeleteAsync();
        }
    }
}