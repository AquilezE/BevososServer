using System.ServiceModel;

namespace BevososService
{
    [ServiceContract (CallbackContract = typeof (IStatsManagerCallback))]
    internal interface IStatsManager
    {
        [OperationContract (IsOneWay = true)]
        void GetCurrentUserStats(int userId);
    }

    [ServiceContract]
    internal interface IStatsManagerCallback
    {
        [OperationContract (IsOneWay = true)]
        void OnStatsReceived(int wins, int monsters, int babies);
    }
}
