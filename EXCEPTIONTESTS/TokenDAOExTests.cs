using DataAccess.DAO;
using DataAccess.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using Xunit;

namespace EXCEPTIONTESTS
{
    public class TokenDAOExTests
    {
        [Fact]
        public void AsignToken_ThrowsDBException_WhenDBNotAvaliable()
        {
            using (var scope = new TransactionScope())
            {
                var dao = new TokenDAO();
                Assert.Throws<DataBaseException>(() => dao.AsignToken("email@example.com"));
            }
        }

        [Fact]
        public void HasToken_ThrowsDBException_WhenDBNotAvaliable()
        {
            using (var scope = new TransactionScope())
            {
                var dao = new TokenDAO();
                Assert.Throws<DataBaseException>(() => dao.HasToken("email@example.com"));
            }
        }

        [Fact]
        public void GetToken_ThrowsDBException_WhenDBNotAvaliable()
        {
            using (var scope = new TransactionScope())
            {
                var dao = new TokenDAO();
                Assert.Throws<DataBaseException>(() => dao.GetToken("email@example.com"));
            }
        }

        [Fact]
        public void TokenIsValid_ThrowsDBException_WhenDBNotAvaliable()
        {
            using (var scope = new TransactionScope())
            {
                var dao = new TokenDAO();
                Assert.Throws<DataBaseException>(() => dao.TokenIsValid("tokenValue", "email@example.com"));
            }
        }

        [Fact]
        public void DeleteToken_ThrowsDBException_WhenDBNotAvaliable()
        {
            using (var scope = new TransactionScope())
            {
                var dao = new TokenDAO();
                Assert.Throws<DataBaseException>(() => dao.DeleteToken("tokenValue", "email@example.com"));
            }
        }
    }
}
