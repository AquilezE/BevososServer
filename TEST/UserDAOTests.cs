using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading.Tasks;
using DataAccess;
using DataAccess.DAO;
using DataAccess.Models;
using Xunit;

namespace TEST
{
    public class UserDAOTests : IClassFixture<DatabaseFixture>
    {
        private readonly DatabaseFixture _fixture;

        public UserDAOTests(DatabaseFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public void Test_UsernameExists_ReturnsTrue_WhenUsernameExists()
        {
            // Arrange
            var userDAO = new UserDAO();
            var username = "AccountDALTestUser";

            // Act
            var result = userDAO.UsernameExists(username);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void Test_UsernameExists_ReturnsFalse_WhenUsernameDoesNotExist()
        {
            // Arrange
            var userDAO = new UserDAO();
            var username = "nonExistingUsername";

            // Act
            var result = userDAO.UsernameExists(username);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void Test_GetUserByEmail_ReturnsUser_WhenEmailExists()
        {
            // Arrange
            var userDAO = new UserDAO();
            var email = "accountdal_test@example.com";

            // Act
            var result = userDAO.GetUserByEmail(email);

            Assert.Equal("AccountDALTestUser", result.Username);
        }

        [Fact]
        public void Test_GetUserByEmail_ReturnsNull_WhenEmailDoesNotExist()
        {
            // Arrange
            var userDAO = new UserDAO();
            var email = "nonExistingEmail@example.com";

            // Act
            var result = userDAO.GetUserByEmail(email);

            // Assert
            Assert.Null(result);
        }



        [Fact]
        public void Test_GetUserById_ReturnsUser_WhenUserIdExists()
        {

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


            var result = userDAO.GetUserById(testUserId);

            Assert.NotNull(result);
            Assert.Equal(testUserId, result.UserId);
            Assert.Equal(testUsername, result.Username);
            Assert.NotNull(result);
        }

        [Fact]
        public void Test_GetUserById_ReturnsNull_WhenUserIdDoesNotExist()
        {
            // Arrange
            var userDAO = new UserDAO();
            var userId = 999;

            // Act
            var result = userDAO.GetUserById(userId);

            // Assert
            Assert.Null(result);
        }



        [Fact]
        public void Test_UpdateUserNames_UpdatesUsernameInDatabase()
        {

            var userDao = new UserDAO();
            User userTest= userDao.GetUserByEmail("accountdal_test@example.com");
            int userId = userTest.UserId;
            var newUsername = "newUsername";


            // Act
            userDao.UpdateUserNames(userId, newUsername);

            // Assert
            var updatedUser = userDao.GetUserById(userId);
            Assert.NotNull(updatedUser);
            Assert.Equal(newUsername, updatedUser.Username);
        }
    }
}
