using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Console;

namespace ConversationalSpeaker
{
    /// <summary>
    /// A color-coded single-line per message console log formatter.
    /// </summary>
    internal class CleanConsoleFormatter : ConsoleFormatter
    {
        const string DefaultForegroundColor = "\x1B[39m\x1B[22m";
        const string DefaultBackgroundColor = "\x1B[49m";

        public CleanConsoleFormatter()
            : base(nameof(CleanConsoleFormatter))
        {
            // do nothing
        }

        public override void Write<TState>(in LogEntry<TState> logEntry, IExternalScopeProvider scopeProvider, TextWriter textWriter)
        {
            string timestamp = DateTime.Now.ToString("hh:mm:ss");
            string level = logEntry.LogLevel.ToString().Substring(0, 4).ToLower();


            textWriter.Write(DefaultForegroundColor);
            textWriter.Write(DefaultBackgroundColor);
            textWriter.Write($"[{timestamp}] ");

            switch (logEntry.LogLevel)
            {
                case LogLevel.Information:
                    textWriter.Write(GetForegroundColorEscapeCode(ConsoleColor.Green));
                    break;
                case LogLevel.Warning:
                    textWriter.Write(GetForegroundColorEscapeCode(ConsoleColor.Yellow));
                    break;
                case LogLevel.Error:
                case LogLevel.Critical:
                    textWriter.Write(GetForegroundColorEscapeCode(ConsoleColor.Red));
                    break;
            }

            textWriter.Write($"{level}");
            textWriter.Write(DefaultForegroundColor);
            textWriter.Write(DefaultBackgroundColor);
            textWriter.WriteLine($": {logEntry.State}");
        }

        // From https://learn.microsoft.com/en-us/dotnet/core/extensions/console-log-formatter
        static string GetForegroundColorEscapeCode(ConsoleColor color) =>
        color switch
        {
            ConsoleColor.Black => "\x1B[30m",
            ConsoleColor.DarkRed => "\x1B[31m",
            ConsoleColor.DarkGreen => "\x1B[32m",
            ConsoleColor.DarkYellow => "\x1B[33m",
            ConsoleColor.DarkBlue => "\x1B[34m",
            ConsoleColor.DarkMagenta => "\x1B[35m",
            ConsoleColor.DarkCyan => "\x1B[36m",
            ConsoleColor.Gray => "\x1B[37m",
            ConsoleColor.Red => "\x1B[1m\x1B[31m",
            ConsoleColor.Green => "\x1B[1m\x1B[32m",
            ConsoleColor.Yellow => "\x1B[1m\x1B[33m",
            ConsoleColor.Blue => "\x1B[1m\x1B[34m",
            ConsoleColor.Magenta => "\x1B[1m\x1B[35m",
            ConsoleColor.Cyan => "\x1B[1m\x1B[36m",
            ConsoleColor.White => "\x1B[1m\x1B[37m",

            _ => DefaultForegroundColor
        };

        // From https://learn.microsoft.com/en-us/dotnet/core/extensions/console-log-formatter
        static string GetBackgroundColorEscapeCode(ConsoleColor color) =>
            color switch
            {
                ConsoleColor.Black => "\x1B[40m",
                ConsoleColor.DarkRed => "\x1B[41m",
                ConsoleColor.DarkGreen => "\x1B[42m",
                ConsoleColor.DarkYellow => "\x1B[43m",
                ConsoleColor.DarkBlue => "\x1B[44m",
                ConsoleColor.DarkMagenta => "\x1B[45m",
                ConsoleColor.DarkCyan => "\x1B[46m",
                ConsoleColor.Gray => "\x1B[47m",

                _ => DefaultBackgroundColor
            };
    }
}