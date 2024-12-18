using BevososService.DTOs;
using System.ServiceModel;
using BevososService.Exceptions;

namespace BevososService
{
    [ServiceContract]
    internal interface IStatsManager
    {
        /// <summary>
        /// Retrieves the current user's statistics.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        [OperationContract]
        [FaultContract(typeof(BevososServerExceptions))]
        StatsDTO GetCurrentUserStats(int userId);
    }
}