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


        [Theory]
        [InlineData("userWithToken@example.com", true)]
        [InlineData("userWithoutToken@example.com", false)]
        public void HasTokenTest(string email, bool expectedResult)
        {
            using (var scope = new TransactionScope())
            {
                var dao = new TokenDAO();

                // Arrange
                if (expectedResult)
                {
                    // Create a token for the email
                    using (var context = new BevososContext())
                    {
                        var token = new Token
                        {
                            Email = email,
                            TokenValue = "123456",
                            ExpiryDate = DateTime.Now.AddMinutes(15)
                        };
                        context.Tokens.Add(token);
                        context.SaveChanges();
                    }
                }

                // Act
                bool result = dao.HasToken(email);

                // Assert
                Assert.Equal(expectedResult, result);
            }
        }




        [Theory]
        [InlineData("existingUser@example.com", true)]
        [InlineData("nonExistingUser@example.com", true)]
        public void AsignTokenTest(string email, bool expectedResult)
        {
            using (var scope = new TransactionScope())
            {
                var dao = new TokenDAO();

                // Act
                int affectedRows = dao.AsignToken(email);

                // Assert
                Assert.Equal(expectedResult, affectedRows > 0);

                // Verify the token was created
                using (var context = new BevososContext())
                {
                    bool tokenExists = context.Tokens.Any(t => t.Email == email);
                    Assert.True(tokenExists);
                }
            }
        }


        [Theory]
        [InlineData("userWithToken@example.com", "432543")]
        [InlineData("userWithoutToken@example.com", "-1")]
        public void GetTokenTest(string email, string expectedToken)
        {
            using (var scope = new TransactionScope())
            {
                var dao = new TokenDAO();

                // Arrange
                if (expectedToken != null)
                {
                    using (var context = new BevososContext())
                    {
                        var token = new Token
                        {
                            Email = email,
                            TokenValue = expectedToken,
                            ExpiryDate = DateTime.Now.AddMinutes(15)
                        };
                        context.Tokens.Add(token);
                        context.SaveChanges();
                    }
                }

                // Act
                string tokenResult = dao.GetToken(email);

                // Assert
                Assert.Equal(expectedToken, tokenResult);
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
                    if (tokenValue == "validToken")
                    {
                        var token = new Token
                        {
                            Email = email,
                            TokenValue = tokenValue,
                            ExpiryDate = DateTime.Now.AddMinutes(15)
                        };
                        context.Tokens.Add(token);
                    }
                    else if (tokenValue == "expiredToken")
                    {
                        var token = new Token
                        {
                            Email = email,
                            TokenValue = tokenValue,
                            ExpiryDate = DateTime.Now.AddMinutes(-20)
                        };
                        context.Tokens.Add(token);
                    }
                    // No token added for "invalidToken"
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

                // Arrange
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
