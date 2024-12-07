using Serilog;
using System;

namespace DataAccess.Utils
{
    public static class ExceptionManager
    {
        private static readonly ILogger _logger = LoggerService.GetLogger();

        public static void LogErrorException(Exception ex)
        {
            Console.WriteLine("Error encountered in method '{0}'.\nMessage: {1}\nStackTrace:\n{2}", ex.TargetSite, ex.Message, ex.StackTrace);
            _logger.Error("Error encountered in method '{0}'.\nMessage: {1}\nStackTrace:\n{2}", ex.TargetSite, ex.Message, ex.StackTrace);
        }


        public static void LogFatalException(Exception ex)
        {
            Console.WriteLine(
                "Fatal error in method '{0}'.\nMessage: {1}\nStackTrace:\n{2}",
                ex.TargetSite, ex.Message, ex.StackTrace
            ); 
            _logger.Fatal("Fatal error in method '{0}'.\nMessage: {1}\nStackTrace:\n{2}",
                ex.TargetSite, ex.Message, ex.StackTrace);
        }

    }
}
