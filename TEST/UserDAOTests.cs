using System.Collections.Generic;
using System.Transactions;
using DataAccess;
using DataAccess.DAO;
using DataAccess.Models;
using Xunit;

namespace TEST
{

    public class UserDAOTests
    {

        [Fact]
        public void UsernameExists_ReturnsTrue_WhenUsernameExists()
        {
            using (var scope = new TransactionScope())
            {
                var userDAO = new UserDAO();
                string username = "AccountDALTestUser";

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

                bool result = userDAO.UsernameExists(username);

                Assert.True(result);
            }
        }

        [Fact]
        public void UsernameExists_ReturnsFalse_WhenUsernameDoesNotExist()
        {
            using (var scope = new TransactionScope())
            {
                var userDAO = new UserDAO();
                string username = "nonExistingUsername";

                bool result = userDAO.UsernameExists(username);

                Assert.False(result);
            }
        }

        [Fact]
        public void GetUserByEmail_ReturnsUser_WhenEmailExists()
        {
            using (var scope = new TransactionScope())
            {
                var userDAO = new UserDAO();
                string email = "accountdal_test@example.com";
                string username = "AccountDALTestUser";

                User expectedUser;

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
                    expectedUser = user;
                }

                User result = userDAO.GetUserByEmail(email);

                Assert.Equal(expectedUser, result);
            }
        }

        [Fact]
        public void GetUserByEmail_ReturnsNull_WhenEmailDoesNotExist()
        {
            using (var scope = new TransactionScope())
            {
                var userDAO = new UserDAO();
                string email = "nonExistingEmail@example.com";

                User result = userDAO.GetUserByEmail(email);

                Assert.Null(result);
            }
        }

        [Fact]
        public void GetUserById_ReturnsUser_WhenUserIdExists()
        {
            using (var scope = new TransactionScope())
            {
                User expectedUser;
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
                    expectedUser = testUser;
                }

                var userDAO = new UserDAO();

                User result = userDAO.GetUserById(testUserId);

                Assert.Equal(expectedUser, result);
            }
        }

        [Fact]
        public void GetUserById_ReturnsNull_WhenUserIdDoesNotExist()
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
        public void UpdateUserNames_UpdatesUsernameInDatabase()
        {
            using (var scope = new TransactionScope())
            {
                var userDao = new UserDAO();
                string email = "accountdal_test@example.com";
                string originalUsername = "OriginalUser";
                string newUsername = "newUsername";

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

                userDao.UpdateUserNames(userId, newUsername);

                User updatedUser = userDao.GetUserById(userId);

                Assert.Equal(newUsername, updatedUser.Username);
            }
        }

        [Fact]
        public void UpdateUser_ReturnsTrueIfUpdatesUserInDataBase()
        {
            using (var scope = new TransactionScope())
            {
                var userDao = new UserDAO();
                string email = "userTestUpdate@example.com";
                string originalUsername = "User1";
                string newUsername = "newUsername";

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

                userTest.Username = newUsername;
                userTest.ProfilePictureId = 2;

                bool result = userDao.UpdateUser(userTest);


                Assert.True(result);
            }
        }

        [Fact]
        public void UpdateUser_ReturnsFalseIfUpdatesUserNotInDataBase()
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

        [Fact]
        public void GetUsersByName_ReturnsListOfThreeUsers_WhenThreeNamesMatch()
        {
            using (var scope = new TransactionScope())
            {
                var expectedList = new List<User>();
                using (var context = new BevososContext())
                {
                    string email = "ExampleEmail";
                    string username = "name";
                    int profilePictureId = 1;
                    for (int i = 1; i < 4; i++)
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
                        expectedList.Add(user);
                    }

                    context.SaveChanges();
                }

                var userDao = new UserDAO();

                List<User> result = userDao.GetUsersByName("name", 1);

                Assert.Equal(expectedList, result);
            }
        }


        [Fact]
        public void GetUsersByName_ReturnsListOfZeroUsers_WhenZeroNamesMatch()
        {
            using (var scope = new TransactionScope())
            {
                var userDao = new UserDAO();
                List<User> result = userDao.GetUsersByName("notname", 1);

                Assert.Empty(result);
            }
        }

        [Fact]
        public void GetUsersByName_ReturnsListOfOneUser_WhenOneNameMatches()
        {
            using (var scope = new TransactionScope())
            {
                string email = "ExampleEmail";
                int profilePictureId = 1;

                var expectedList = new List<User>();

                using (var context = new BevososContext())
                {
                    var accountRoberto = new Account
                    {
                        Email = email + 4,
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
                    expectedList.Add(userRoberto);
                }

                var userDao = new UserDAO();
                List<User> result = userDao.GetUsersByName("Roberto", 1);
                Assert.Equal(expectedList, result);
            }
        }

    }

}