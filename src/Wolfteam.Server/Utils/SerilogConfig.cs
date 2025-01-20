using Serilog;
using Serilog.Templates;
using Serilog.Templates.Themes;

namespace Wolfteam.Server.Utils;

public static class SerilogConfig
{
    public static TemplateTheme Theme { get; } = new(new Dictionary<TemplateThemeStyle, string>
    {
        [TemplateThemeStyle.Text] = "\u001b[38;5;0015m",
        [TemplateThemeStyle.SecondaryText] = "\u001b[38;5;0007m",
        [TemplateThemeStyle.TertiaryText] = "\u001b[38;5;0008m",
        [TemplateThemeStyle.Invalid] = "\u001b[38;5;0011m",
        [TemplateThemeStyle.Null] = "\u001b[38;5;0027m",
        [TemplateThemeStyle.Name] = "\u001b[38;5;0007m",
        [TemplateThemeStyle.String] = "\u001b[38;5;0031m",
        [TemplateThemeStyle.Number] = "\u001b[38;5;0211m",
        [TemplateThemeStyle.Boolean] = "\u001b[38;5;0027m",
        [TemplateThemeStyle.Scalar] = "\u001b[38;5;0078m",
        [TemplateThemeStyle.LevelVerbose] = "\u001b[38;5;0239m",
        [TemplateThemeStyle.LevelDebug] = "\u001b[38;5;0013m",
        [TemplateThemeStyle.LevelInformation] = "\u001b[38;5;0006m",
        [TemplateThemeStyle.LevelWarning] = "\u001b[38;5;0011m",
        [TemplateThemeStyle.LevelError] = "\u001b[38;5;0015m\u001b[48;5;0196m",
        [TemplateThemeStyle.LevelFatal] = "\u001b[38;5;0015m\u001b[48;5;0196m",
    });

    public static LoggerConfiguration CreateDefault()
    {
        return new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .WriteTo.Console(
                formatter: new ExpressionTemplate("[{@t:HH:mm:ss} {@l:u3} {Substring(SourceContext, LastIndexOf(SourceContext, '.') + 1),-20}] {@m}\n{@x}",
                    theme: SerilogConfig.Theme))
            .WriteTo.File("logs/log.txt", rollingInterval: RollingInterval.Day);
    }
}