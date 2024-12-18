using BevososService.Exceptions;
using DataAccess.Utils;
using System;
using System.Linq;
using System.ServiceModel;
using BevososService.GameModels;
using System.Collections.Generic;

namespace BevososService.Implementations
{

    public partial class ServiceImplementation
    {

        public ServiceImplementation()
        {
            GlobalDeck.InitializeDeck();
        }

        private static void Shuffle<T>(IList<T> list)
        {
            var rng = new Random();
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        private static FaultException<BevososServerExceptions> CreateAndLogFaultException(Exception innerException)
        {
            var serverException = new BevososServerExceptions
            {
                Message = innerException.Message,
                StackTrace = innerException.StackTrace
            };

            ExceptionManager.LogErrorException(innerException);

            return new FaultException<BevososServerExceptions>(serverException,
                new FaultReason(serverException.Message));
        }

    }

}