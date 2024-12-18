using DataAccess.DAO;
using DataAccess.Exceptions;
using DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using Xunit;

namespace EXCEPTIONTESTS
{
    public class StatsDAOExTests
    {
        [Fact]
        public void UserStatsExists_ThrowsDBException_WhenDBNotAvaliable()
        {
            using (var scope = new TransactionScope())
            {
                var dao = new StatsDAO();
                Assert.Throws<DataBaseException>(() => dao.UserStatsExists(1));
            }
        }

        [Fact]
        public void AddNewUserStats_ThrowsDBException_WhenDBNotAvaliable()
        {
            using (var scope = new TransactionScope())
            {
                var dao = new StatsDAO();
                var userStats = new Stats();
                Assert.Throws<DataBaseException>(() => dao.AddNewUserStats(1, userStats));
            }
        }

        [Fact]
        public void GetUserStats_ThrowsDBException_WhenDBNotAvaliable()
        {
            using (var scope = new TransactionScope())
            {
                var dao = new StatsDAO();
                Assert.Throws<DataBaseException>(() => dao.GetUserStats(1));
            }
        }

        [Fact]
        public void UpdateUserStats_ThrowsDBException_WhenDBNotAvaliable()
        {
            using (var scope = new TransactionScope())
            {
                var dao = new StatsDAO();
                var userStats = new Stats();
                Assert.Throws<DataBaseException>(() => dao.UpdateUserStats(1, userStats));
            }
        }
    }
}
