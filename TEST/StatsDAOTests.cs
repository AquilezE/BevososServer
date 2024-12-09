using DataAccess.DAO;
using DataAccess.Models;
using DataAccess;
using System;
using System.Linq;
using System.Transactions;
using Xunit;

namespace TEST
{
    public class StatsDAOTests
    {

        [Fact]
        public void UserStatsExists_ReturnsTrue_WhenStatsExist()
        {
            using (var scope = new TransactionScope())
            {
                var statsDAO = new StatsDAO();
                int userId = AddTestUserWithStats();

                bool exists = statsDAO.UserStatsExists(userId);

                Assert.True(exists);
            }
        }

        [Fact]
        public void UserStatsExists_ReturnsFalse_WhenStatsDoNotExist()
        {
            using (var scope = new TransactionScope())
            {
                var statsDAO = new StatsDAO();
                int userId = AddTestUserWithoutStats();

                bool exists = statsDAO.UserStatsExists(userId);

                Assert.False(exists);
            }
        }

        [Fact]
        public void UserStatsExists_ReturnsFalse_WhenUserDoesNotExist()
        {
            using (var scope = new TransactionScope())
            {
                var statsDAO = new StatsDAO();
                var nonExistingUserId = 99999;

                bool exists = statsDAO.UserStatsExists(nonExistingUserId);

                Assert.False(exists);
            }
        }

        [Fact]
        public void UserStatsExists_ReturnsFalse_WhenUserIdIsInvalid()
        {
            using (var scope = new TransactionScope())
            {
                var statsDAO = new StatsDAO();
                int invalidUserId = -1;

                bool exists = statsDAO.UserStatsExists(invalidUserId);

                Assert.False(exists);
            }
        }

        [Fact]
        public void AddNewUserStats_ReturnsTrue_WhenValidStatsProvided()
        {
            using (var scope = new TransactionScope())
            {
                var statsDAO = new StatsDAO();
                int userId = AddTestUserWithoutStats();
                var newStats = new Stats
                {
                    UserId = userId,
                    Wins = 10,
                    MonstersCreated = 5,
                    AnnihilatedBabies = 2
                };

                bool result = statsDAO.AddNewUserStats(userId, newStats);

                Assert.True(result);
                using (var context = new BevososContext())
                {
                    Stats stats = context.Stats.FirstOrDefault(s => s.UserId == userId);
                    Assert.NotNull(stats);
                    Assert.Equal(10, stats.Wins);
                    Assert.Equal(5, stats.MonstersCreated);
                    Assert.Equal(2, stats.AnnihilatedBabies);
                }
            }
        }

        [Fact]
        public void AddNewUserStats_ReturnsFalse_WhenStatsAreNull()
        {
            using (var scope = new TransactionScope())
            {
                var statsDAO = new StatsDAO();
                int userId = AddTestUserWithoutStats();
                Stats nullStats = null;

                bool result = statsDAO.AddNewUserStats(userId, nullStats);

                Assert.False(result);
            }
        }

        [Fact]
        public void AddNewUserStats_ReturnsFalse_WhenUserDoesNotExist()
        {
            using (var scope = new TransactionScope())
            {
                var statsDAO = new StatsDAO();
                var nonExistingUserId = 99999;
                var newStats = new Stats
                {
                    UserId = nonExistingUserId,
                    Wins = 5,
                    MonstersCreated = 3,
                    AnnihilatedBabies = 1
                };

                bool result = statsDAO.AddNewUserStats(nonExistingUserId, newStats);

                Assert.False(result);
            }
        }


        [Fact]
        public void GetUserStats_ReturnsStats_WhenStatsExist()
        {
            using (var scope = new TransactionScope())
            {
                var statsDAO = new StatsDAO();
                int userId = AddTestUserWithStats();

                Stats stats = statsDAO.GetUserStats(userId);

                Assert.NotNull(stats);
                Assert.Equal(userId, stats.UserId);
            }
        }

        [Fact]
        public void GetUserStats_ReturnsNull_WhenStatsDoNotExist()
        {
            using (var scope = new TransactionScope())
            {
                var statsDAO = new StatsDAO();
                int userId = AddTestUserWithoutStats();

                Stats stats = statsDAO.GetUserStats(userId);

                Assert.Null(stats);
            }
        }

        [Fact]
        public void GetUserStats_ReturnsNull_WhenUserDoesNotExist()
        {
            using (var scope = new TransactionScope())
            {
                var statsDAO = new StatsDAO();
                var nonExistingUserId = 99999;

                Stats stats = statsDAO.GetUserStats(nonExistingUserId);

                Assert.Null(stats);
            }
        }

        [Fact]
        public void GetUserStats_ReturnsNull_WhenUserIdIsInvalid()
        {
            using (var scope = new TransactionScope())
            {

                var statsDAO = new StatsDAO();
                int invalidUserId = -1;

                Stats stats = statsDAO.GetUserStats(invalidUserId);

                Assert.Null(stats);
            }
        }


        [Fact]
        public void UpdateUserStats_ReturnsTrue_WhenStatsExistAndUpdated()
        {
            using (var scope = new TransactionScope())
            {
                var statsDAO = new StatsDAO();
                int userId = AddTestUserWithStats();
                var updatedStats = new Stats
                {
                    Wins = 20,
                    MonstersCreated = 10,
                    AnnihilatedBabies = 5
                };

                bool result = statsDAO.UpdateUserStats(userId, updatedStats);

                Assert.True(result);
                using (var context = new BevososContext())
                {
                    Stats stats = context.Stats.FirstOrDefault(s => s.UserId == userId);
                    Assert.NotNull(stats);
                    Assert.Equal(20, stats.Wins);
                    Assert.Equal(10, stats.MonstersCreated);
                    Assert.Equal(5, stats.AnnihilatedBabies);
                }
            }
        }

        [Fact]
        public void UpdateUserStats_ReturnsFalse_WhenStatsDoNotExist()
        {
            using (var scope = new TransactionScope())
            {
                var statsDAO = new StatsDAO();
                int userId = AddTestUserWithoutStats();
                var updatedStats = new Stats
                {
                    Wins = 15,
                    MonstersCreated = 7,
                    AnnihilatedBabies = 3
                };

                bool result = statsDAO.UpdateUserStats(userId, updatedStats);

                Assert.False(result);
            }
        }

        [Fact]
        public void UpdateUserStats_ReturnsFalse_WhenStatsAreNull()
        {
            using (var scope = new TransactionScope())
            {
                var statsDAO = new StatsDAO();
                int userId = AddTestUserWithStats();
                Stats nullStats = null;

                bool result = statsDAO.UpdateUserStats(userId, nullStats);

                Assert.False(result);
            }
        }

        [Fact]
        public void UpdateUserStats_ReturnsFalse_WhenUserIdIsInvalid()
        {
            using (var scope = new TransactionScope())
            {
                var statsDAO = new StatsDAO();
                int invalidUserId = -1;
                var updatedStats = new Stats
                {
                    Wins = 5,
                    MonstersCreated = 2,
                    AnnihilatedBabies = 1
                };

                bool result = statsDAO.UpdateUserStats(invalidUserId, updatedStats);

                Assert.False(result);
            }
        }

        private int AddTestUserWithStats()
        {
            using (var context = new BevososContext())
            {
                var user = new User
                {
                    Username = $"TestUser_{Guid.NewGuid()}",
                    ProfilePictureId = 1,
                    Account = new Account
                    {
                        Email = $"testuser_{Guid.NewGuid()}@example.com",
                        PasswordHash = "hashed_password"
                    },
                    Stats = new Stats
                    {
                        Wins = 5,
                        MonstersCreated = 3,
                        AnnihilatedBabies = 1
                    }
                };
                context.Users.Add(user);
                context.SaveChanges();
                return user.UserId;
            }
        }

        private int AddTestUserWithoutStats()
        {
            using (var context = new BevososContext())
            {
                var user = new User
                {
                    Username = $"TestUser_{Guid.NewGuid()}",
                    ProfilePictureId = 1,
                    Account = new Account
                    {
                        Email = $"testuser_{Guid.NewGuid()}@example.com",
                        PasswordHash = "hashed_password"
                    }
                };
                context.Users.Add(user);
                context.SaveChanges();
                return user.UserId;
            }
        }
    }
}

