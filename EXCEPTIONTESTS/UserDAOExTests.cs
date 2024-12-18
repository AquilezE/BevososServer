using DataAccess.DAO;
using DataAccess.Exceptions;
using DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using Xunit;

namespace EXCEPTIONTESTS
{

    public class UserDAOExTests
    {

        [Fact]
        public void UsernameExists_ThrowsDBException_WhenDBNotAvaliable()
        {
            using (var scope = new TransactionScope())
            {
                var dao = new UserDAO();
                Assert.Throws<DataBaseException>(() => dao.UsernameExists("testuser"));
            }
        }

        [Fact]
        public void GetUserByEmail_ThrowsDBException_WhenDBNotAvaliable()
        {
            using (var scope = new TransactionScope())
            {
                var dao = new UserDAO();
                Assert.Throws<DataBaseException>(() => dao.GetUserByEmail("email@example.com"));
            }
        }

        [Fact]
        public void GetUserById_ThrowsDBException_WhenDBNotAvaliable()
        {
            using (var scope = new TransactionScope())
            {
                var dao = new UserDAO();
                Assert.Throws<DataBaseException>(() => dao.GetUserById(1));
            }
        }

        [Fact]
        public void GetUsersByName_ThrowsDBException_WhenDBNotAvaliable()
        {
            using (var scope = new TransactionScope())
            {
                var dao = new UserDAO();
                Assert.Throws<DataBaseException>(() => dao.GetUsersByName("John", 1));
            }
        }

        [Fact]
        public void UpdateUserNames_ThrowsDBException_WhenDBNotAvaliable()
        {
            using (var scope = new TransactionScope())
            {
                var dao = new UserDAO();
                Assert.Throws<DataBaseException>(() => dao.UpdateUserNames(1, "NewUsername"));
            }
        }

        [Fact]
        public void UpdateUser_ThrowsDBException_WhenDBNotAvaliable()
        {
            using (var scope = new TransactionScope())
            {
                var dao = new UserDAO();
                var user = new User
                {
                    UserId = 1,
                    Username = "UpdatedUser",
                    ProfilePictureId = 2
                };
                Assert.Throws<DataBaseException>(() => dao.UpdateUser(user));
            }
        }

        [Fact]
        public void UserExists_ThrowsDBException_WhenDBNotAvaliable()
        {
            using (var scope = new TransactionScope())
            {
                var dao = new UserDAO();
                Assert.Throws<DataBaseException>(() => dao.UserExists(1));
            }
        }

    }

}