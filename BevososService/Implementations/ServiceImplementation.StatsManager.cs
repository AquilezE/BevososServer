using DataAccess.DAO;
using DataAccess.Models;
using System;
using System.ServiceModel;
using DataAccess.Exceptions;
using DataAccess.Utils;
using BevososService.DTOs;

namespace BevososService.Implementations
{
    public partial class ServiceImplementation : IStatsManager
    {
        public StatsDTO GetCurrentUserStats(int userId)
        {
            int userWins = 0;
            int userMonsters = 0;
            int userBabies = 0;

            var userStats = new StatsDTO();

            userStats.Wins = userWins;
            userStats.MonstersCreated = userMonsters;
            userStats.AnihilatedBabies = userBabies;

            try
            {
                if (new StatsDAO().UserStatsExists(userId))
                {
                    Stats statsDAO = new StatsDAO().GetUserStats(userId);

                    userWins = statsDAO.Wins;
                    userMonsters = statsDAO.MonstersCreated;
                    userBabies = statsDAO.AnnihilatedBabies;

                    userStats.Wins = userWins;
                    userStats.MonstersCreated = userMonsters;
                    userStats.AnihilatedBabies = userBabies;

                    return userStats;
                }

                return userStats;   
            }
            catch (DataBaseException ex)
            {
                throw CreateAndLogFaultException(ex);
            }
            catch (CommunicationException ex)
            {
                ExceptionManager.LogErrorException(ex);
            }
            catch (TimeoutException ex)
            {
                ExceptionManager.LogErrorException(ex);
            }
            catch (Exception ex)
            {
                ExceptionManager.LogFatalException(ex);
            }
            return userStats;
        }
    }
}