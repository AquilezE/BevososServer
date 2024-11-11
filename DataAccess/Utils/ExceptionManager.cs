using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Utils
{
    public static class ExceptionManager
    {
        private static readonly ILogger _logger = LoggerService.GetLogger();

        public static void LogErrorException(Exception ex)
        {
            _logger.Error("Error encountered in method '{MethodName}'.\nMessage: {Message}\nStackTrace:\n{StackTrace}", ex.TargetSite, ex.Message, ex.StackTrace);
        }


        public static void LogFatalException(Exception ex)
        {
            _logger.Fatal("Fatal error in method '{MethodName}'.\nMessage: {Message}\nStackTrace:\n{StackTrace}", ex.TargetSite, ex.Message, ex.StackTrace);
        }

    }
}
