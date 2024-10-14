using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using DataAccess;
using DataAccess.DAO;
using DataAccess.Models;
using Xunit;

namespace TEST
{
    public class TokenDAOTests : IClassFixture<DatabaseFixture>
    {
        private readonly DatabaseFixture _fixture;


        public TokenDAOTests(DatabaseFixture fixture)
        {
            _fixture = fixture;
        }


        [Theory]
        [InlineData("tokendal_test@example.com", true)]
        [InlineData("existingEmailToken@gmail.com", true)]
        [InlineData("notExistentEmail@gmail.com", false)]
        public void HasTokenTest(string email, bool expectedResult)
        {
            // Arrange
            var dao = new TokenDAO();

            // Act
            bool result = dao.HasToken(email);

            // Assert
            Assert.Equal(expectedResult, result);
        }



        [Theory]
        [InlineData("notExistentEmail@gmail.com", true)]
        [InlineData("tokendal_test@example.com", true)]
        public void AsignTokenTest(string email, bool expectedResult)
        {
            // Arrange
            var dao = new TokenDAO();

            // Act
            int affectedRows = dao.AsignToken(email);
            
            // Assert
            Assert.Equal(expectedResult, affectedRows > 0);
        }

        [Theory]
        [InlineData("tokendal_test@example.com", "123456")]
        [InlineData("existingEmailToken@gmail.com", "654321")]
        [InlineData("notExistingEmailToken@gmail.com","-1")]
        public void GetTokenTest(string email, string expectedToken)
        {
            // Arrange
            var dao = new TokenDAO();

            // Act
            string token = dao.GetToken(email);


            Assert.Equal(expectedToken, token);
        }



        [Theory]
        [InlineData("123456", "tokendal_test@example.com", true)]
        [InlineData("654321", "tokendal_test@example.com", false)]
        [InlineData("789012", "expiredEmailToken@gmail.com",false)]
        public void TokenIsValidTest(string token, string email, bool expectedResult)
        {
            // Arrange
            var dao = new TokenDAO();

            // Act
            bool result = dao.TokenIsValid(token, email);

            // Assert
            Assert.Equal(expectedResult, result);
        }


        [Theory]
        [InlineData("123456", "tokendal_test@example.com", true)]
        [InlineData("", "notExistentEmail@gmail.com", false)]
        [InlineData("invalidToken", "existingEmailToken@gmail.com", false)]
        public void DeleteTokenTest(string token, string email, bool expectedResult)
        {
            // Arrange
            var dao = new TokenDAO();
            bool wasDeleted = dao.DeleteToken(token, email);
            Assert.Equal(expectedResult, wasDeleted);
        }

        


    }
}
