using Microsoft.Extensions.Logging;

namespace Dodgeball.TrustServer.Api
{

    public static class DodgeballApiVersion
    {
        public static readonly string V1 = "v1";
    }

    public class DodgeballConfig
    {
        public bool? isEnabled;
        public ILogger? Logger;
        public string? ApiUrl;
        public string? ApiVersion;
        public LogLevel? LogLevel;
    }
}