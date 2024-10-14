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
    public class AccountDALTests : IClassFixture<DatabaseFixture>
    {


        private readonly DatabaseFixture _fixture;

        public AccountDALTests(DatabaseFixture fixture)
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
        public void AddAccount_ShouldAddAccountToDatabase()
        {
            // Arrange
            AccountDAO accountDAO = new AccountDAO();
            Account account = new Account
            {
                Email = "newaccount@example.com",
                PasswordHash = "hashed_password",
                User = new User
                {
                    Username = "NewDalUser",
                    ProfilePictureId = 2
                }
            };

            // Act
            accountDAO.AddAccount(account);

            // Assert
            using (var context = new BevososContext())
            {
                Account addedAccount = context.Accounts.FirstOrDefault(a => a.Email == "newaccount@example.com");
                Assert.NotNull(addedAccount);
                Assert.Equal("newaccount@example.com", addedAccount.Email);
                Assert.Equal("NewDalUser", addedAccount.User.Username);
            }
        }
    }
}

