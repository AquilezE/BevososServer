using BevososService.Exceptions;
using DataAccess.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace BevososService.Implementations
{
    public partial class ServiceImplementation
    {
        public ServiceImplementation()
        {
            GlobalDeck.InitializeDeck();

            foreach (var item in GlobalDeck.Deck.Select(item => item.Value))
            {
                Console.WriteLine("Card " + item.CardId + " Element " + item.Element + " Type: " + item.Type + " Damage: " + item.Damage);
            }
        }

        private FaultException<BevososServerExceptions> CreateAndLogFaultException(Exception innerException)
        {
            BevososServerExceptions serverException = new BevososServerExceptions
            {
                Message = innerException.Message,
                StackTrace = innerException.StackTrace
            };

            ExceptionManager.LogErrorException(innerException);

            return new FaultException<BevososServerExceptions>(serverException, new FaultReason(serverException.Message));
        }


    }




}
