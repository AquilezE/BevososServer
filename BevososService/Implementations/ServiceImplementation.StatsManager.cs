using DataAccess.DAO;
using DataAccess.Models;
using System;
using System.ServiceModel;

namespace BevososService.Implementations
{
    public partial class ServiceImplementation : IStatsManager
    {
        public void GetCurrentUserStats(int userId)
        {
            int userWins = -1;
            int userMonsters = -1;
            int userBabies = -1;

            try
            {
                if (new StatsDAO().UserStatsExists(userId))
                {
                    Stats statsDAO = new StatsDAO().GetUserStats(userId);

                    userWins = statsDAO.Wins;
                    userMonsters = statsDAO.MonstersCreated;
                    userBabies = statsDAO.AnnihilatedBabies;

                    var callback = OperationContext.Current.GetCallbackChannel<IStatsManagerCallback>();
                    callback.OnStatsReceived(userWins, userMonsters, userBabies);
                }
                else
                {
                    userWins = 0;
                    userMonsters = 0;
                    userBabies = 0;

                    var callback = OperationContext.Current.GetCallbackChannel<IStatsManagerCallback>();
                    callback.OnStatsReceived(userWins, userMonsters, userBabies);
                    
                }
            }
            catch (Exception ex)
            {
                CreateAndLogFaultException(ex);

                var callback = OperationContext.Current.GetCallbackChannel<IStatsManagerCallback>();
                callback.OnStatsReceived(userWins, userMonsters, userBabies);
            }

        }
    }
}
