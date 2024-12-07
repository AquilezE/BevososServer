using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAccess.Exceptions;
using DataAccess.Models;

namespace DataAccess.DAO
{
    public class StatsDAO
    {
        public bool UserStatsExists(int userId)
        {
            return ExceptionHelper.ExecuteWithExceptionHandling(() =>
            {
                using (BevososContext context = new BevososContext())
                {
                    Stats userStats = context.Stats.FirstOrDefault(u => u.UserId == userId);
                    return userStats != null;
                }
            });
        }

        public bool AddNewUserStats(int userId, Stats UserStats)
        {
            return ExceptionHelper.ExecuteWithExceptionHandling(() =>
            {
                using (BevososContext context = new BevososContext())
                {
                    if (UserStats != null)
                    {
                        context.Stats.Add(UserStats);
                        context.SaveChanges();

                        return true;
                    }
                    return false;
                }
            });
        }

        public Stats GetUserStats(int userId)
        {
            return ExceptionHelper.ExecuteWithExceptionHandling(() =>
            {
                using (BevososContext context = new BevososContext())
                {
                    return context.Stats.FirstOrDefault(u => u.UserId == userId);
                }
            });
        }

        public bool UpdateUserStats(int userId, Stats userStats)
        {
            return ExceptionHelper.ExecuteWithExceptionHandling(() =>
            {
                using (BevososContext context = new BevososContext())
                {
                    Stats stats = context.Stats.FirstOrDefault(u => u.UserId == userId);
                    if (stats != null)
                    {
                        stats.Wins = userStats.Wins;
                        stats.MonstersCreated = userStats.MonstersCreated;
                        stats.AnnihilatedBabies = userStats.AnnihilatedBabies;

                        context.SaveChanges();
                        return true;
                    }
                    return false;
                }
            });
        }
    }
}
