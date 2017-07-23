using NLog;
using NLog.Config;
using NLog.Layouts;
using NLog.Targets;

namespace Wolfteam.Server.Config
{
    internal static class NLogConfig
    {
        public static LoggingConfiguration Create()
        {
            var config = new LoggingConfiguration();

            config.AddTarget(new ColoredConsoleTarget
            {
                Name = "ColoredOutput",
                Layout = Layout.FromString("${longdate} ${pad:padding=5:inner=${level:uppercase=true}} ${message} ${exception:format=tostring}"),
                UseDefaultRowHighlightingRules = true,
                ErrorStream = false
            });

            config.AddRule(LogLevel.Trace, LogLevel.Off, "ColoredOutput");

            return config;
        }
    }
}
