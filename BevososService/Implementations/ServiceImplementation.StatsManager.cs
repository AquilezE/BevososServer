using DataAccess.DAO;
using DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

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

                    IStatsManagerCallback callback = OperationContext.Current.GetCallbackChannel<IStatsManagerCallback>();
                    callback.OnStatsReceived(userWins, userMonsters, userBabies);
                }
                else
                {
                    userWins = 0;
                    userMonsters = 0;
                    userBabies = 0;

                    IStatsManagerCallback callback = OperationContext.Current.GetCallbackChannel<IStatsManagerCallback>();
                    callback.OnStatsReceived(userWins, userMonsters, userBabies);
                    
                }
            }
            catch (Exception ex)
            {
                CreateAndLogFaultException(ex);

                IStatsManagerCallback callback = OperationContext.Current.GetCallbackChannel<IStatsManagerCallback>();
                callback.OnStatsReceived(userWins, userMonsters, userBabies);
            }

        }
    }
}
