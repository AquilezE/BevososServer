using Serilog;
using System;
using System.IO;

namespace DataAccess.Utils
{

    public sealed class LoggerService
    {

        private const string DateFormat = "dd-MM-yyyy";
        private const string IdFileName = "Log";
        private const string CharacterSeparator = "_";
        private const string FileExtension = ".txt";
        private const string RelativeLogFilePath = @"Logsitos";

        private static ILogger _logger;

        private static readonly object Lock = new object();

        private LoggerService()
        {
            ConfigureLogger(BuildLogFilePath());
            _logger.Information("Logger initialized and ready to write logs.");
        }

        private static void ConfigureLogger(string logFilePath)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .Enrich.FromLogContext()
                .WriteTo.File(
                    logFilePath,
                    retainedFileCountLimit: 7,
                    outputTemplate:
                    "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
                )
                .CreateLogger();
        }

        private static string BuildLogFilePath()
        {
            DateTime currentDate = DateTime.Now;
            string date = currentDate.ToString(DateFormat);

            string logFileName = $"{IdFileName}{CharacterSeparator}{date}{FileExtension}";
            string absoluteLogDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, RelativeLogFilePath);

            if (!Directory.Exists(absoluteLogDirectory))
            {
                Directory.CreateDirectory(absoluteLogDirectory);
            }

            string logFilePath = Path.Combine(absoluteLogDirectory, logFileName);

            Console.WriteLine($"Absolute Log Directory: {absoluteLogDirectory}");
            Console.WriteLine($"Log File Path: {logFilePath}");

            return logFilePath;
        }

        public static ILogger GetLogger()
        {
            if (_logger == null)
            {
                string logPath = BuildLogFilePath();
                ConfigureLogger(logPath);
            }

            _logger = Log.Logger;
            return _logger;
        }

        public static void CloseAndFlush()
        {
            lock (Lock)
            {
                (_logger as IDisposable)?.Dispose();
                Log.CloseAndFlush();
                _logger = null;
            }
        }

        public static void ResetLogger()
        {
            CloseAndFlush();
            ConfigureLogger(BuildLogFilePath());
        }

    }

}