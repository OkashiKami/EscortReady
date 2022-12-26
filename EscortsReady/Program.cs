using Microsoft.Extensions.Logging;

namespace EscortsReady
{
    public class Program
    {
        public static CancellationToken cancelToken;
        public static IConfiguration config;
        internal static ILogger logger;

        public static void Main(params string[] args) => new WebApi(args);
    }
}







