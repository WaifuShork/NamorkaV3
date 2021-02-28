using System.Threading.Tasks;

namespace Namoroka
{
    internal static class Program
    {
        private static async Task Main(string[] args) => await new NamorokaBuilder().RunAsync(args);
    }
}