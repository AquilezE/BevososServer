using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Utils
{
    /// <summary>
    /// Provides logging functionality using Serilog.
    /// Implements a thread-safe singleton pattern.
    /// </summary>
    public sealed class LoggerService
    {
        const string DATE_FORMAT = "dd-MM-yyyy";
        const string ID_FILE_NAME = "Log";
        const string CHARACTER_SEPARATOR = "_";
        const string FILE_EXTENSION = ".txt";
        const string RELATIVE_LOG_FILE_PATH = @"C:\MyCustomLogsDirectory";

        private static readonly Lazy<LoggerService> _instance = new Lazy<LoggerService>(() => new LoggerService());

        private static ILogger _logger;

        private static readonly object _lock = new object();

        /// <summary>
        /// Gets the singleton instance of the LoggerService.
        /// </summary>
        public static LoggerService Instance => _instance.Value;

        /// <summary>
        /// Private constructor to prevent external instantiation.
        /// Configures the logger upon creation.
        /// </summary>
        private LoggerService()
        {
            ConfigureLogger(BuildLogFilePath());
        }

        /// <summary>
        /// Configures the Serilog logger with specified settings.
        /// </summary>
        /// <param name="logFilePath">The file path where logs will be written.</param>
        private static void ConfigureLogger(string logFilePath)
        {
            _logger = new LoggerConfiguration()
                .MinimumLevel.Verbose() 
                .Enrich.FromLogContext() // Enrich logs with contextual information
                .WriteTo.File(
                    path: logFilePath,
                    rollingInterval: RollingInterval.Day, 
                    retainedFileCountLimit: 7, 
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
                )
                .CreateLogger();
        }

        /// <summary>
        /// Builds the log file path based on the current date.
        /// </summary>
        /// <returns>The full path to the log file.</returns>
        private static string BuildLogFilePath()
        {


            DateTime currentDate = DateTime.Now;
            string date = currentDate.ToString(DATE_FORMAT);

            string logFileName = $"{ID_FILE_NAME}{CHARACTER_SEPARATOR}{date}{FILE_EXTENSION}";
            string absoluteLogDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, RELATIVE_LOG_FILE_PATH);

            if (!Directory.Exists(absoluteLogDirectory))
            {
                Directory.CreateDirectory(absoluteLogDirectory);
            }

            string logFilePath = Path.Combine(absoluteLogDirectory, logFileName);

            return logFilePath;
        }

        /// <summary>
        /// Gets the current logger instance.
        /// </summary>
        /// <returns>The Serilog ILogger instance.</returns>
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

        /// <summary>
        /// Closes and flushes the logger, releasing any resources.
        /// </summary>
        public static void CloseAndFlush()
        {
            lock (_lock)
            {
                (_logger as IDisposable)?.Dispose();
                Log.CloseAndFlush();
                _logger = null;
            }
        }

        /// <summary>
        /// Resets the logger by closing the current instance and reconfiguring it.
        /// Useful for scenarios where the logger needs to be reinitialized.
        /// </summary>
        public static void ResetLogger()
        {
            CloseAndFlush();
            ConfigureLogger(BuildLogFilePath());
        }
    }
}
