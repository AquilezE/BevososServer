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
    }
}

