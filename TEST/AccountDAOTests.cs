using System.Linq;
using System.Transactions;
using DataAccess;
using DataAccess.DAO;
using DataAccess.Models;
using Xunit;

namespace TEST
{
    public class AccountDAOTests
    {

        [Fact]
        public void GetAccountByEmail_ShouldReturnAccount_WhenEmailExists()
        {
            using (var scope = new TransactionScope())
            {
                // Arrange
                var accountDAO = new AccountDAO();
                var email = "accountdal_test@example.com";
                var username = "AccountDALTestUser";

                // Create User and Account
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
                var result = accountDAO.GetAccountByEmail(email);

                // Assert
                Assert.NotNull(result);
                Assert.Equal(email, result.Email);
                Assert.NotNull(result.User);
                Assert.Equal(username, result.User.Username);
            }
        }

        [Theory]
        [InlineData("existingEmail@example.com", true)]
        [InlineData("doesntExist@gmail.com", false)]
        public void EmailExistsTest(string email, bool expectedResult)
        {
            using (var scope = new TransactionScope())
            {
                // Arrange
                var accountDAO = new AccountDAO();

                if (expectedResult)
                {
                    // Create Account only if expectedResult is true
                    using (var context = new BevososContext())
                    {
                        var user = new User
                        {
                            Username = "ExistingUser",
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
                }

                // Act
                var result = accountDAO.EmailExists(email);

                // Assert
                Assert.Equal(expectedResult, result);
            }
        }


        [Theory]
        [InlineData("accountdal_test@example.com", "newHashedPassword")]
        public void UpdatePasswordTest(string email, string newHashedPassword)
        {
            using (var scope = new TransactionScope())
            {
                var accountDAO = new AccountDAO();

                using (var context = new BevososContext())
                {
                    var user = new User
                    {
                        Username = "UserForPasswordUpdate",
                        ProfilePictureId = 1,
                        Account = new Account
                        {
                            Email = email,
                            PasswordHash = "old_hashed_password"
                        }
                    };
                    context.Users.Add(user);
                    context.SaveChanges();
                }

                var result = accountDAO.UpdatePasswordByEmail(email, newHashedPassword);

                Assert.True(result);

                using (var context = new BevososContext())
                {
                    var account = context.Accounts.FirstOrDefault(a => a.Email == email);
                    Assert.NotNull(account);
                    Assert.Equal(newHashedPassword, account.PasswordHash);
                }
            }
        }



        [Fact]
        public void AddUserWithAccount_ShouldReturnTrue_WhenUserIsAdded()
        {
            using (var scope = new TransactionScope())
            {
                // Arrange
                var accountDAO = new AccountDAO();
                var userDAO = new UserDAO();
                var user = new User
                {
                    Username = "newUser",
                    ProfilePictureId = 1,
                    // Account will be linked via AddUserWithAccount
                };
                var account = new Account
                {
                    Email = "newAccountWithUser@email.com",
                    PasswordHash = "passwordHash"
                    // User will be linked via AddUserWithAccount
                };

                var result = accountDAO.AddUserWithAccount(user, account);

                Assert.True(result);

                var accountResult = accountDAO.GetAccountByEmail(account.Email);
                var userResult = userDAO.GetUserByEmail(account.Email);

                Assert.NotNull(accountResult);
                Assert.NotNull(userResult);
                Assert.Equal(userResult.UserId, accountResult.UserId);
                Assert.Equal(user.Username, userResult.Username);
            }
        }


        [Fact]
        public void Test_UpdatePasswordByUserId_ReturnsTrue_WhenAccountExists()
        {
            using (var scope = new TransactionScope())
            {
                var accountDAO = new AccountDAO();
                var userDAO = new UserDAO();
                var email = "accountTestUpdate@example.com";
                var initialPasswordHash = "test_hashed_password";
                var newPasswordHash = "new_hashed_password";

                using (var context = new BevososContext())
                {
                    var user1 = new User
                    {
                        Username = "User1",
                        ProfilePictureId = 1,
                        Account = new Account
                        {
                            Email = email,
                            PasswordHash = initialPasswordHash
                        }
                    };

                    context.Users.Add(user1);
                    context.SaveChanges();
                }

                var accountTest = accountDAO.GetAccountByEmail(email);
                Assert.NotNull(accountTest);

                var result = accountDAO.UpdatePasswordByUserId(accountTest.UserId, newPasswordHash);

                Assert.True(result);


                var updatedAccount = accountDAO.GetAccountByUserId(accountTest.UserId);
                Assert.NotNull(updatedAccount);
                Assert.Equal(newPasswordHash, updatedAccount.PasswordHash);
            }
        }


        [Fact]
        public void Test_UpdatePasswordByUserId_ReturnsFalse_WhenAccountDoesNotExist()
        {
            using (var scope = new TransactionScope())
            {
                var accountDAO = new AccountDAO();
                var nonExistingUserId = -1;
                var newPasswordHash = "newPasswordHashed";

                var result = accountDAO.UpdatePasswordByUserId(nonExistingUserId, newPasswordHash);

                Assert.False(result);
            }
        }

    }
}

