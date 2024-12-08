using System.ServiceModel;

namespace BevososService
{

    [ServiceContract(CallbackContract = typeof(IStatsManagerCallback))]
    internal interface IStatsManager
    {
        /// <summary>
        /// Retrieves the current user's statistics.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        [OperationContract(IsOneWay = true)]
        void GetCurrentUserStats(int userId);
    }

    [ServiceContract]
    internal interface IStatsManagerCallback
    {
        /// <summary>
        /// Callback method invoked when the user's statistics are received.
        /// </summary>
        /// <param name="wins">The number of wins.</param>
        /// <param name="monsters">The number of monsters.</param>
        /// <param name="babies">The number of babies.</param>
        [OperationContract(IsOneWay = true)]
        void OnStatsReceived(int wins, int monsters, int babies);
    }
}
