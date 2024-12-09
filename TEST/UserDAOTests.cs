﻿using System.Transactions;
using DataAccess;
using DataAccess.DAO;
using DataAccess.Models;
using Xunit;

namespace TEST
{
    public class UserDAOTests
    {


        [Fact]
        public void Test_UsernameExists_ReturnsTrue_WhenUsernameExists()
        {
            using (var scope = new TransactionScope())
            {
                // Arrange
                var userDAO = new UserDAO();
                var username = "AccountDALTestUser";

                using (var context = new BevososContext())
                {
                    var user = new User
                    {
                        Username = username,
                        ProfilePictureId = 1,
                        Account = new Account
                        {
                            Email = "testuser@example.com",
                            PasswordHash = "hashed_password"
                        }
                    };
                    context.Users.Add(user);
                    context.SaveChanges();
                }

                // Act
                bool result = userDAO.UsernameExists(username);

                // Assert
                Assert.True(result);
            }
            // TransactionScope ensures the database changes are rolled back
        }

        [Fact]
        public void Test_UsernameExists_ReturnsFalse_WhenUsernameDoesNotExist()
        {
            using (var scope = new TransactionScope())
            {
                var userDAO = new UserDAO();
                var username = "nonExistingUsername";

                bool result = userDAO.UsernameExists(username);

                Assert.False(result);
            }
        }

        [Fact]
        public void Test_GetUserByEmail_ReturnsUser_WhenEmailExists()
        {
            using (var scope = new TransactionScope())
            {
                // Arrange
                var userDAO = new UserDAO();
                var email = "accountdal_test@example.com";
                var username = "AccountDALTestUser";

                // Create a user with the specified email
                using (var context = new BevososContext())
                {
                    var user = new User
                    {
                        Username = username,
                        ProfilePictureId = 1,
                        Account = new Account
                        {
                            Email = email,
                            PasswordHash = "hashed_password"
                        }
                    };
                    context.Users.Add(user);
                    context.SaveChanges();
                }

                // Act
                User result = userDAO.GetUserByEmail(email);

                // Assert
                Assert.NotNull(result);
                Assert.Equal(username, result.Username);
            }
        }

        [Fact]
        public void Test_GetUserByEmail_ReturnsNull_WhenEmailDoesNotExist()
        {
            using (var scope = new TransactionScope())
            {
                var userDAO = new UserDAO();
                var email = "nonExistingEmail@example.com";

                User result = userDAO.GetUserByEmail(email);

                Assert.Null(result);
            }
        }



        [Fact]
        public void Test_GetUserById_ReturnsUser_WhenUserIdExists()
        {
            using (var scope = new TransactionScope())
            {
                // Arrange
                int testUserId;
                string testUsername;
                using (var context = new BevososContext())
                {
                    var testUser = new User
                    {
                        Username = "TestUser1",
                        ProfilePictureId = 1,
                        Account = new Account
                        {
                            Email = "testuser1@example.com",
                            PasswordHash = "hashed_password"
                        }
                    };

                    context.Users.Add(testUser);
                    context.SaveChanges();

                    testUserId = testUser.UserId;
                    testUsername = testUser.Username;
                }
                var userDAO = new UserDAO();

                // Act
                User result = userDAO.GetUserById(testUserId);

                // Assert
                Assert.NotNull(result);
                Assert.Equal(testUserId, result.UserId);
                Assert.Equal(testUsername, result.Username);
            }
        }


        [Fact]
        public void Test_GetUserById_ReturnsNull_WhenUserIdDoesNotExist()
        {
            using (var scope = new TransactionScope())
            {
                var userDAO = new UserDAO();
                int userId = -1;

                User result = userDAO.GetUserById(userId);

                Assert.Null(result);
            }
        }



        [Fact]
        public void Test_UpdateUserNames_UpdatesUsernameInDatabase()
        {
            using (var scope = new TransactionScope())
            {
                // Arrange
                var userDao = new UserDAO();
                var email = "accountdal_test@example.com";
                var originalUsername = "OriginalUser";
                var newUsername = "newUsername";

                // Create user
                using (var context = new BevososContext())
                {
                    var user = new User
                    {
                        Username = originalUsername,
                        ProfilePictureId = 1,
                        Account = new Account
                        {
                            Email = email,
                            PasswordHash = "hashed_password"
                        }
                    };
                    context.Users.Add(user);
                    context.SaveChanges();
                }

                User userTest = userDao.GetUserByEmail(email);
                int userId = userTest.UserId;

                // Act
                userDao.UpdateUserNames(userId, newUsername);

                // Assert
                User updatedUser = userDao.GetUserById(userId);
                Assert.NotNull(updatedUser);
                Assert.Equal(newUsername, updatedUser.Username);
            }
        }

        [Fact]
        public void Test_UpdateUser_ReturnsTrueIfUpdatesUserInDataBase()
        {
            using (var scope = new TransactionScope())
            {
                // Arrange
                var userDao = new UserDAO();
                var email = "userTestUpdate@example.com";
                var originalUsername = "User1";
                var newUsername = "newUsername";

                // Create user
                int userId;
                using (var context = new BevososContext())
                {
                    var user1 = new User
                    {
                        Username = originalUsername,
                        ProfilePictureId = 1,
                        Account = new Account
                        {
                            Email = email,
                            PasswordHash = "hashed_password"
                        }
                    };
                    context.Users.Add(user1);
                    context.SaveChanges();

                    userId = user1.UserId;
                }

                User userTest = userDao.GetUserById(userId);

                // Modify user
                userTest.Username = newUsername;
                userTest.ProfilePictureId = 2;

                // Act
                bool result = userDao.UpdateUser(userTest);

                // Assert
                Assert.True(result);

                // Verify the changes
                User updatedUser = userDao.GetUserById(userId);
                Assert.NotNull(updatedUser);
                Assert.Equal(newUsername, updatedUser.Username);
                Assert.Equal(2, updatedUser.ProfilePictureId);
            }
        }

        [Fact]
        public void Test_UpdateUser_ReturnsFalseIfUpdatesUserInDataBase()
        {
            using (var scope = new TransactionScope())
            {
                var userDAO = new UserDAO();
                var nonExistingUser = new User
                {
                    UserId = -1,
                    Username = "NonExistentUser",
                    ProfilePictureId = 2,
                    Account = new Account
                    {
                        Email = "notExistingUser@gmail.com",
                        PasswordHash = "hashed_password",
                        UserId = -1
                    }
                };

                bool result = userDAO.UpdateUser(nonExistingUser);

                Assert.False(result);
            }
        }

        [Theory]
        [InlineData("name", 3)]
        [InlineData("notname", 0)]
        [InlineData("Roberto", 1)]

        public void Test_GetUsersByName_ReturnsListOfUsers_WhenNameExists(string name, int expectedCount)
        {
            using (var scope = new TransactionScope())
            {
                // Arrange
                var userDao = new UserDAO();
                var email = "elpepe1@example.com";
                var username = "name";
                var profilePictureId = 2;

                // Create users
                using (var context = new BevososContext())
                {
                    for (var i = 0; i < 3; i++)
                    {
                        var account = new Account
                        {
                            Email = email + i,
                            PasswordHash = "hashed_password"
                        };
                        var user = new User
                        {
                            Username = username + i,
                            ProfilePictureId = profilePictureId,
                            Account = account
                        };
                        context.Users.Add(user);
                    }
                    var accountRoberto = new Account
                    {
                        Email = email + 3,
                        PasswordHash = "hashed_password"
                    };
                    var userRoberto = new User
                    {
                        Username = "Roberto",
                        ProfilePictureId = profilePictureId,
                        Account = accountRoberto
                    };

                    context.Users.Add(userRoberto);
                    context.SaveChanges();
                }
                Assert.Equal(expectedCount, userDao.GetUsersByName(name,1).Count);
            }
        }
    }
}
