using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using DataAccess;
using DataAccess.DAO;
using DataAccess.Models;
using Xunit;

namespace TEST
{
    public class AccountDAOTests : IClassFixture<DatabaseFixture>
    {


        private readonly DatabaseFixture _fixture;

        public AccountDAOTests(DatabaseFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public void GetAccountByEmail_ShouldReturnAccount_WhenEmailExists()
        {
            Account result;
            AccountDAO accountDAO = new AccountDAO();

            result = accountDAO.GetAccountByEmail("accountdal_test@example.com");

            Assert.NotNull(result);
            Assert.Equal("accountdal_test@example.com", result.Email);
            Assert.Equal("AccountDALTestUser", result.User.Username);

        }

        [Theory]
        [InlineData("accountdal_test@example.com", true)]
        [InlineData("doesntExist@gmail.com", false)]
        public void EmailExistsTest(string email, bool expectedResult)
        {
            bool result;
            AccountDAO accountDAO = new AccountDAO();

            result = accountDAO.EmailExists(email);

            Assert.Equal(expectedResult, result);

        }

        [Theory]
        [InlineData("accountdal_test@example.com", "newHashedPassword")]
        public void UpdatePasswordTest(string email, string newHashedPassword)
        {
            bool result;
            AccountDAO accountDAO = new AccountDAO();

            result = accountDAO.UpdatePasswordByEmail(email, newHashedPassword);

            Assert.True(result);
        }


        [Fact]
        public void AddUserWithAccount_ShouldReturnTrue_WhenUserIsAdded()
        {
            bool result;
            AccountDAO accountDAO = new AccountDAO();
            User user = new User();
            user.Username = "newUser";
            Account account = new Account();
            account.Email = "newAccountWithUser@email.com";
            account.PasswordHash = "passwordHash";

            result = accountDAO.AddUserWithAccount(user, account);

            var accountResult = accountDAO.GetAccountByEmail(account.Email);
            var userResult = new UserDAO().GetUserByEmail(account.Email);

            Assert.Equal(userResult.UserId, accountResult.UserId);
        }

        [Fact]
        public void Test_UpdatePasswordByUserId_ReturnsTrue_WhenAccountExists()
        {
            using (var scope = new TransactionScope())
            {
                using (var context = new BevososContext())
                {
                    var user1 = new User
                    {
                        Username = "User1",
                        ProfilePictureId = 1,
                        Account = new Account
                        {
                            Email = "accountTestUpdate@example.com",
                            PasswordHash = "test_hashed_password"
                        }
                    };

                    context.Users.Add(user1);
                    context.SaveChanges();
                }
                var accountDAO = new AccountDAO();
                var accountTest = new AccountDAO().GetAccountByEmail("accountTestUpdate@example.com");

                accountTest.PasswordHash = "new_hashed_password";

                var result = accountDAO.UpdatePasswordByUserId(accountTest.UserId, accountTest.PasswordHash);

                Assert.True(result);
            }
        }

        [Fact]
        public void Test_UpdatePasswordByUserId_ReturnsFalse_WhenAccountDoesNotExist()
        {

            var accountDAO = new AccountDAO();
            var nonExistingAccount = new Account
            {
                Email = "notExistingUser@gmail.com",
                PasswordHash = "hashed_password",
                UserId = -1,
                User = new User
                {
                    UserId = -1,
                    Username = "NonExistentUser",
                    ProfilePictureId = 2,
                }
            };

            var result = accountDAO.UpdatePasswordByUserId(nonExistingAccount.UserId, "newPasswordHashed");

            Assert.False(result);
        }
    }
}

