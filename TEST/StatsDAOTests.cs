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
        #region UserStatsExists Tests

        [Fact]
        public void UserStatsExists_ReturnsTrue_WhenStatsExist()
        {
            using (var scope = new TransactionScope())
            {
                // Arrange
                var statsDAO = new StatsDAO();
                int userId = AddTestUserWithStats();

                // Act
                bool exists = statsDAO.UserStatsExists(userId);

                // Assert
                Assert.True(exists);
            }
        }

        [Fact]
        public void UserStatsExists_ReturnsFalse_WhenStatsDoNotExist()
        {
            using (var scope = new TransactionScope())
            {
                // Arrange
                var statsDAO = new StatsDAO();
                int userId = AddTestUserWithoutStats();

                // Act
                bool exists = statsDAO.UserStatsExists(userId);

                // Assert
                Assert.False(exists);
            }
        }

        [Fact]
        public void UserStatsExists_ReturnsFalse_WhenUserDoesNotExist()
        {
            using (var scope = new TransactionScope())
            {
                // Arrange
                var statsDAO = new StatsDAO();
                var nonExistingUserId = 99999;

                // Act
                bool exists = statsDAO.UserStatsExists(nonExistingUserId);

                // Assert
                Assert.False(exists);
            }
        }

        [Fact]
        public void UserStatsExists_ReturnsFalse_WhenUserIdIsInvalid()
        {
            using (var scope = new TransactionScope())
            {
                // Arrange
                var statsDAO = new StatsDAO();
                int invalidUserId = -1;

                // Act
                bool exists = statsDAO.UserStatsExists(invalidUserId);

                // Assert
                // Dependiendo de la implementación, podrías esperar que devuelva false o manejarlo de otra manera
                Assert.False(exists);
            }
        }

        #endregion

        #region AddNewUserStats Tests

        [Fact]
        public void AddNewUserStats_ReturnsTrue_WhenValidStatsProvided()
        {
            using (var scope = new TransactionScope())
            {
                // Arrange
                var statsDAO = new StatsDAO();
                int userId = AddTestUserWithoutStats();
                var newStats = new Stats
                {
                    UserId = userId,
                    Wins = 10,
                    MonstersCreated = 5,
                    AnnihilatedBabies = 2
                };

                // Act
                bool result = statsDAO.AddNewUserStats(userId, newStats);

                // Assert
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
                // Arrange
                var statsDAO = new StatsDAO();
                int userId = AddTestUserWithoutStats();
                Stats nullStats = null;

                // Act
                bool result = statsDAO.AddNewUserStats(userId, nullStats);

                // Assert
                Assert.False(result);
            }
        }

        [Fact]
        public void AddNewUserStats_ReturnsFalse_WhenUserDoesNotExist()
        {
            using (var scope = new TransactionScope())
            {
                // Arrange
                var statsDAO = new StatsDAO();
                var nonExistingUserId = 99999;
                var newStats = new Stats
                {
                    UserId = nonExistingUserId,
                    Wins = 5,
                    MonstersCreated = 3,
                    AnnihilatedBabies = 1
                };

                // Act
                bool result = statsDAO.AddNewUserStats(nonExistingUserId, newStats);

                // Assert
                Assert.False(result);
            }
        }

        [Fact]
        public void AddNewUserStats_ReturnsFalse_WhenDuplicateStats()
        {
            using (var scope = new TransactionScope())
            {
                // Arrange
                var statsDAO = new StatsDAO();
                int userId = AddTestUserWithStats();
                var duplicateStats = new Stats
                {
                    UserId = userId,
                    Wins = 7,
                    MonstersCreated = 4,
                    AnnihilatedBabies = 3
                };

                // Act
                bool result = statsDAO.AddNewUserStats(userId, duplicateStats);

                // Assert
                // Dependiendo de la implementación, podrías esperar que devuelva false si ya existen stats
                Assert.False(result);
            }
        }

        #endregion

        #region GetUserStats Tests

        [Fact]
        public void GetUserStats_ReturnsStats_WhenStatsExist()
        {
            using (var scope = new TransactionScope())
            {
                // Arrange
                var statsDAO = new StatsDAO();
                int userId = AddTestUserWithStats();

                // Act
                Stats stats = statsDAO.GetUserStats(userId);

                // Assert
                Assert.NotNull(stats);
                Assert.Equal(userId, stats.UserId);
            }
        }

        [Fact]
        public void GetUserStats_ReturnsNull_WhenStatsDoNotExist()
        {
            using (var scope = new TransactionScope())
            {
                // Arrange
                var statsDAO = new StatsDAO();
                int userId = AddTestUserWithoutStats();

                // Act
                Stats stats = statsDAO.GetUserStats(userId);

                // Assert
                Assert.Null(stats);
            }
        }

        [Fact]
        public void GetUserStats_ReturnsNull_WhenUserDoesNotExist()
        {
            using (var scope = new TransactionScope())
            {
                // Arrange
                var statsDAO = new StatsDAO();
                var nonExistingUserId = 99999;

                // Act
                Stats stats = statsDAO.GetUserStats(nonExistingUserId);

                // Assert
                Assert.Null(stats);
            }
        }

        [Fact]
        public void GetUserStats_ReturnsNull_WhenUserIdIsInvalid()
        {
            using (var scope = new TransactionScope())
            {
                // Arrange
                var statsDAO = new StatsDAO();
                int invalidUserId = -1;

                // Act
                Stats stats = statsDAO.GetUserStats(invalidUserId);

                // Assert
                // Dependiendo de la implementación, podrías esperar que devuelva null
                Assert.Null(stats);
            }
        }

        #endregion

        #region UpdateUserStats Tests

        [Fact]
        public void UpdateUserStats_ReturnsTrue_WhenStatsExistAndUpdated()
        {
            using (var scope = new TransactionScope())
            {
                // Arrange
                var statsDAO = new StatsDAO();
                int userId = AddTestUserWithStats();
                var updatedStats = new Stats
                {
                    Wins = 20,
                    MonstersCreated = 10,
                    AnnihilatedBabies = 5
                };

                // Act
                bool result = statsDAO.UpdateUserStats(userId, updatedStats);

                // Assert
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
                // Arrange
                var statsDAO = new StatsDAO();
                int userId = AddTestUserWithoutStats();
                var updatedStats = new Stats
                {
                    Wins = 15,
                    MonstersCreated = 7,
                    AnnihilatedBabies = 3
                };

                // Act
                bool result = statsDAO.UpdateUserStats(userId, updatedStats);

                // Assert
                Assert.False(result);
            }
        }

        [Fact]
        public void UpdateUserStats_ReturnsFalse_WhenStatsAreNull()
        {
            using (var scope = new TransactionScope())
            {
                // Arrange
                var statsDAO = new StatsDAO();
                int userId = AddTestUserWithStats();
                Stats nullStats = null;

                // Act
                bool result = statsDAO.UpdateUserStats(userId, nullStats);

                // Assert
                Assert.False(result);
            }
        }

        [Fact]
        public void UpdateUserStats_ReturnsFalse_WhenUserIdIsInvalid()
        {
            using (var scope = new TransactionScope())
            {
                // Arrange
                var statsDAO = new StatsDAO();
                int invalidUserId = -1;
                var updatedStats = new Stats
                {
                    Wins = 5,
                    MonstersCreated = 2,
                    AnnihilatedBabies = 1
                };

                // Act
                bool result = statsDAO.UpdateUserStats(invalidUserId, updatedStats);

                // Assert
                Assert.False(result);
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Adds a test user with stats to the database y devuelve el UserId.
        /// </summary>
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

        /// <summary>
        /// Adds a test user without stats to la base de datos y devuelve el UserId.
        /// </summary>
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
                    // Stats es null por defecto
                };
                context.Users.Add(user);
                context.SaveChanges();
                return user.UserId;
            }
        }

        #endregion
    }
}

