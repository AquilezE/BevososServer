using System;
using System.Linq;
using System.Transactions;
using DataAccess;
using DataAccess.DAO;
using DataAccess.Models;
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
                        Email = "userWithToken@example.com",
                        TokenValue = "123456",
                        ExpiryDate = DateTime.Now.AddMinutes(15)
                    };
                    context.Tokens.Add(token);
                    context.SaveChanges();
                }

                bool result = dao.HasToken("userWithToken@example.com");

                Assert.True(result);
            }
        }

        [Fact]
        public void HasToken_ReturnsFalse_WhenUserDoesNotHaveToken()
        {
            using (var scope = new TransactionScope())
            {
                var dao = new TokenDAO();

                bool result = dao.HasToken("userWithoutToken@example.com");

                Assert.False(result);
            }
        }

        [Fact]
        public void AsignToken_AsignsAndReturnsOne()
        {
            using (var scope = new TransactionScope())
            {
                var dao = new TokenDAO();

                int result = dao.AsignToken("userWithToken@example.com");

                Assert.Equal(1, result);


                using (var context = new BevososContext())
                {
                    bool tokenExists = context.Tokens.Any(t => t.Email == "userWithToken@example.com");
                    Assert.True(tokenExists);
                }
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
                        Email = "userWithToken@example.com",
                        TokenValue = "123456",
                        ExpiryDate = DateTime.Now.AddMinutes(15)
                    };
                    context.Tokens.Add(token);
                    context.SaveChanges();
                }

                string tokenResult = dao.GetToken("userWithToken@example.com");

                Assert.Equal("123456", tokenResult);
            }
        }

        [Fact]
        public void GetToken_ReturnsMinusOne_WhenUserHasNoToken()
        {
            using (var scope = new TransactionScope())
            {
                var dao = new TokenDAO();
                string tokenResult = dao.GetToken("userWithoutToken@example.com");

                Assert.Equal("-1", tokenResult);
            }
        }



        [Theory]
        [InlineData("validToken", "user@example.com", true)]
        [InlineData("invalidToken", "user@example.com", false)]
        [InlineData("expiredToken", "user@example.com", false)]
        public void TokenIsValidTest(string tokenValue, string email, bool expectedResult)
        {
            using (var scope = new TransactionScope())
            {
                var dao = new TokenDAO();

                using (var context = new BevososContext())
                {
                    switch (tokenValue)
                    {
                        case "validToken":
                        {
                            var token = new Token
                            {
                                Email = email,
                                TokenValue = tokenValue,
                                ExpiryDate = DateTime.Now.AddMinutes(15)
                            };
                            context.Tokens.Add(token);
                            break;
                        }
                        case "expiredToken":
                        {
                            var token = new Token
                            {
                                Email = email,
                                TokenValue = tokenValue,
                                ExpiryDate = DateTime.Now.AddMinutes(-20)
                            };
                            context.Tokens.Add(token);
                            break;
                        }
                    }

                    context.SaveChanges();
                }

                bool result = dao.TokenIsValid(tokenValue, email);

                Assert.Equal(expectedResult, result);
            }
        }



        [Theory]
        [InlineData("validToken", "user@example.com", true)]
        [InlineData("nonExistingToken", "user@example.com", false)]
        [InlineData("validToken", "wrongUser@example.com", false)]
        public void DeleteTokenTest(string tokenValue, string email, bool expectedResult)
        {
            using (var scope = new TransactionScope())
            {
                var dao = new TokenDAO();

                using (var context = new BevososContext())
                {
                    if (tokenValue == "validToken" && email == "user@example.com")
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
                }

                bool wasDeleted = dao.DeleteToken(tokenValue, email);
                Assert.Equal(expectedResult, wasDeleted);

            }
        }
    }
}
