using Serilog;
using System;

namespace DataAccess.Utils
{

    public static class ExceptionManager
    {

        private static readonly ILogger Logger = LoggerService.GetLogger();

        public static void LogErrorException(Exception ex)
        {
            Logger.Error("Error encountered in method '{0}'.\nMessage: {1}\nStackTrace:\n{2}", ex.TargetSite,
                ex.Message, ex.StackTrace);
        }


        public static void LogFatalException(Exception ex)
        {
            Logger.Fatal("Fatal error in method '{0}'.\nMessage: {1}\nStackTrace:\n{2}",
                ex.TargetSite, ex.Message, ex.StackTrace);
        }

    }

}