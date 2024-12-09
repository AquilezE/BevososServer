using BevososService.Exceptions;
using DataAccess.Utils;
using System;
using System.Linq;
using System.ServiceModel;
using BevososService.GameModels;

namespace BevososService.Implementations
{
    public partial class ServiceImplementation
    {
        public ServiceImplementation()
        {
            GlobalDeck.InitializeDeck();

            foreach (Card item in GlobalDeck.Deck.Select(item => item.Value))
            {
                Console.WriteLine("Card " + item.CardId + " Element " + item.Element + " Type: " + item.Type + " Damage: " + item.Damage + " Parte: " + item.BodyPartIndex);
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

            return new FaultException<BevososServerExceptions>(serverException, new FaultReason(serverException.Message));
        }


    }




}
