using System.Linq;
using System.Security.Cryptography.X509Certificates;
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
                var accountDAO = new AccountDAO();
                string email = "accountdal_test@example.com";
                string username = "AccountDALTestUser";

                var account = new Account
                {
                    Email = email,
                    PasswordHash = "hashed_password"
                };

                using (var context = new BevososContext())
                {
                    var user = new User
                    {
                        Username = username,
                        ProfilePictureId = 1,
                        Account = account
                    };
                    context.Users.Add(user);
                    context.SaveChanges();
                }

                Account result = accountDAO.GetAccountByEmail(email);

                Assert.Equal(result, account);
            }
        }

        [Fact]
        public void EmailExists_ReturnsTrue_WhenAccountExists()
        {
            using (var scope = new TransactionScope())
            {
                var accountDAO = new AccountDAO();


                using (var context = new BevososContext())
                {
                    var user = new User
                    {
                        Username = "ExistingUser",
                        ProfilePictureId = 1,
                        Account = new Account
                        {
                            Email = "existingEmail@example.com",
                            PasswordHash = "hashed_password"
                        }
                    };
                    context.Users.Add(user);
                    context.SaveChanges();
                }


                bool result = accountDAO.EmailExists("existingEmail@example.com");

                Assert.True(result);
            }
        }

        [Fact]
        public void EmailExists_ReturnsFalse_WhenAccountDoesNotExist()
        {
            string email = "doesntExist@example.com";

            var accountDAO = new AccountDAO();

            bool result = accountDAO.EmailExists(email);

            Assert.False(result);
        }

        [Fact]
        public void UpdatePasswordByEmail_ShouldReturnTrueIfUserExist()
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
                            Email = "accountdal_test@example.com",
                            PasswordHash = "old_hashed_password"
                        }
                    };
                    context.Users.Add(user);
                    context.SaveChanges();
                }

                bool result = accountDAO.UpdatePasswordByEmail("accountdal_test@example.com", "newHashedPassword");

                Assert.True(result);
            }
        }

        [Fact]
        public void AddUserWithAccount_ShouldReturnTrue_WhenUserIsAdded()
        {
            using (var scope = new TransactionScope())
            {
                var accountDAO = new AccountDAO();
                var userDAO = new UserDAO();
                var user = new User
                {
                    Username = "newUser",
                    ProfilePictureId = 1
                };
                var account = new Account
                {
                    Email = "newAccountWithUser@email.com",
                    PasswordHash = "passwordHash"
                };

                bool result = accountDAO.AddUserWithAccount(user, account);

                Assert.True(result);

                using (var context = new BevososContext())
                {
                    User userFromDb = context.Users.FirstOrDefault(u => u.Username == user.Username);
                    Assert.NotNull(userFromDb);
                }
            }
        }

        [Fact]
        public void UpdatePasswordByUserId_ReturnsTrue_WhenAccountExists()
        {
            using (var scope = new TransactionScope())
            {
                var accountDAO = new AccountDAO();
                var userDAO = new UserDAO();
                string email = "accountTestUpdate@example.com";
                string initialPasswordHash = "test_hashed_password";
                string newPasswordHash = "new_hashed_password";

                var accountTest = new Account
                {
                    Email = email,
                    PasswordHash = initialPasswordHash
                };

                using (var context = new BevososContext())
                {
                    var user1 = new User
                    {
                        Username = "User1",
                        ProfilePictureId = 1,
                        Account = accountTest
                    };

                    context.Users.Add(user1);
                    context.SaveChanges();
                }

                bool result = accountDAO.UpdatePasswordByUserId(accountTest.UserId, newPasswordHash);

                Assert.True(result);
            }
        }

        [Fact]
        public void UpdatePasswordByUserId_ReturnsFalse_WhenAccountDoesNotExist()
        {
            using (var scope = new TransactionScope())
            {
                var accountDAO = new AccountDAO();
                int nonExistingUserId = -1;
                string newPasswordHash = "newPasswordHashed";

                bool result = accountDAO.UpdatePasswordByUserId(nonExistingUserId, newPasswordHash);

                Assert.False(result);
            }
        }

    }

}