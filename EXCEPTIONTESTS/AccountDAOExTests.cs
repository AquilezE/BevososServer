using DataAccess.Exceptions;
using DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using DataAccess.DAO;
using Xunit;

namespace EXCEPTIONTESTS
{

    public class AccountDAOExTests
    {

        [Fact]
        public void GetAccountByUserId_ThrowsDBException_WhenDBNotAvaliable()
        {
            using (var scope = new TransactionScope())
            {
                var dao = new AccountDAO();

                Assert.Throws<DataBaseException>(() => dao.GetAccountByUserId(1));
            }
        }

        [Fact]
        public void GetAccountByEmail_ThrowsDBException_WhenDBNotAvaliable()
        {
            using (var scope = new TransactionScope())
            {
                var dao = new AccountDAO();

                Assert.Throws<DataBaseException>(() => dao.GetAccountByEmail("email@email.com"));
            }
        }

        [Fact]
        public void EmailExists_ThrowsDBException_WhenDBNotAvaliable()
        {
            using (var scope = new TransactionScope())
            {
                var dao = new AccountDAO();

                Assert.Throws<DataBaseException>(() => dao.EmailExists("email@email.com"));
            }
        }

        [Fact]
        public void AddUserWithAccount_ThrowsDBException_WhenDBNotAvaliable()
        {
            using (var scope = new TransactionScope())
            {
                var dao = new AccountDAO();

                var user = new User
                {
                    Username = "TestUser",
                    ProfilePictureId = 1,
                };

                var account = new Account
                {
                    Email = "testuser@example.com",
                    PasswordHash = "hashed_password"
                };

                Assert.Throws<DataBaseException>(() => dao.AddUserWithAccount(user, account));
            }
        }

        [Fact]
        public void UpdatePasswordByEmail_ThrowsDBException_WhenDBNotAvaliable()
        {
            using (var scope = new TransactionScope())
            {
                var dao = new AccountDAO();

                Assert.Throws<DataBaseException>(() => dao.UpdatePasswordByEmail("email@email.com", "hashed_password"));
            }
        }

        [Fact]
        public void UpdatePasswordByUserId_ThrowsDBException_WhenDBNotAvaliable()
        {
            using (var scope = new TransactionScope())
            {
                var dao = new AccountDAO();

                Assert.Throws<DataBaseException>(() => dao.UpdatePasswordByUserId(1, "hashed_password"));
            }
        }

    }

}