using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Transactions;
using DataAccess;
using DataAccess.DAO;
using DataAccess.Models;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Xunit;

namespace TEST
{

    public class TokenDAOTests
    {

        [Fact]
        public void HasToken_ReturnsTrue_WhenUserHasToken()
        {
            using (var scope = new TransactionScope())
            {
                var dao = new TokenDAO();

                using (var context = new BevososContext())
                {
                    var token = new Token
                    {
                        Email = "emailWithToken@example.com",
                        TokenValue = "123456",
                        ExpiryDate = DateTime.Now.AddMinutes(15)
                    };
                    context.Tokens.Add(token);
                    context.SaveChanges();
                }

                bool result = dao.HasToken("emailWithToken@example.com");

                Assert.True(result);
            }
        }

        [Fact]
        public void HasToken_ReturnsFalse_WhenUserDoesNotHaveToken()
        {
            using (var scope = new TransactionScope())
            {
                var dao = new TokenDAO();

                bool result = dao.HasToken("emailWithoutToken@example.com");

                Assert.False(result);
            }
        }

        [Fact]
        public void AsignToken_ReturnsOne_WhenTokenAsigned()
        {
            using (var scope = new TransactionScope())
            {
                var dao = new TokenDAO();

                int result = dao.AsignToken("emailWithToken@example.com");

                Assert.Equal(1, result);
            }
        }

        [Fact]
        public void GetToken_ReturnsToken_WhenUserHasToken()
        {
            using (var scope = new TransactionScope())
            {
                var dao = new TokenDAO();

                using (var context = new BevososContext())
                {
                    var token = new Token
                    {
                        Email = "emailWithToken@example.com",
                        TokenValue = "123456",
                        ExpiryDate = DateTime.Now.AddMinutes(15)
                    };
                    context.Tokens.Add(token);
                    context.SaveChanges();
                }

                string tokenResult = dao.GetToken("emailWithToken@example.com");

                Assert.Equal("123456", tokenResult);
            }
        }

        [Fact]
        public void GetToken_ReturnsMinusOneString_WhenUserHasNoToken()
        {
            using (var scope = new TransactionScope())
            {
                var dao = new TokenDAO();
                string tokenResult = dao.GetToken("emailWithoutToken@example.com");

                Assert.Equal("-1", tokenResult);
            }
        }

        [Fact]
        public void TokenIsValid_ReturnsTrue_WhenUserHasValidToken()
        {
            string email = "email@example.com";
            string tokenValue = "123456";

            var dao = new TokenDAO();
            using (var scope = new TransactionScope())
            {
                using (var context = new BevososContext())
                {
                    var token = new Token
                    {
                        Email = email,
                        TokenValue = tokenValue,
                        ExpiryDate = DateTime.Now.AddMinutes(15)
                    };
                    context.Tokens.Add(token);
                    context.SaveChanges();
                }

                bool result = dao.TokenIsValid(tokenValue, email);

                Assert.True(result);
            }
        }

        [Fact]
        public void TokenIsValid_ReturnsFalse_WhenUserHasExpiredToken()
        {
            string email = "email@example.com";
            string tokenValue = "expiredToken";

            var dao = new TokenDAO();
            using (var scope = new TransactionScope())
            {
                using (var context = new BevososContext())
                {
                    var token = new Token
                    {
                        Email = email,
                        TokenValue = tokenValue,
                        ExpiryDate = DateTime.Now.AddMinutes(-20)
                    };
                    context.Tokens.Add(token);
                }

                bool result = dao.TokenIsValid(tokenValue, email);

                Assert.False(result);
            }
        }

        [Fact]
        public void TokenIsValid_ReturnsFalse_WhenUserHasInvalidToken()
        {
            string email = "email@example.com";
            string tokenValue = "invalidToken";

            var dao = new TokenDAO();
            bool result = dao.TokenIsValid(tokenValue, email);

            Assert.False(result);
        }

        [Fact]
        public void DeleteToken_ReturnsTrue_WhenUserHasToken()
        {
            string tokenValue = "validToken";
            string email = "email@example.com";
            var dao = new TokenDAO();

            using (var scope = new TransactionScope())
            {
                using (var context = new BevososContext())
                {
                    var token = new Token
                    {
                        Email = email,
                        TokenValue = tokenValue,
                        ExpiryDate = DateTime.Now.AddMinutes(15)
                    };

                    context.Tokens.Add(token);
                    context.SaveChanges();
                }

                bool result = dao.DeleteToken(tokenValue, email);
                Assert.True(result);
            }
        }

        [Fact]
        public void DeleteToken_ReturnsFalse_WhenUserHasNoToken()
        {
            string tokenValue = "validToken";
            string email = "email@example.com";


            var dao = new TokenDAO();
            bool result = dao.DeleteToken(tokenValue, email);

            Assert.False(result);
        }

    }

}