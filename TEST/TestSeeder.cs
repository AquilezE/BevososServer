using DataAccess.Models;
using DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace TEST
{
    public class DatabaseFixture : IDisposable
    {
        private TransactionScope _transaction;

        public DatabaseFixture()
        {
            // Start a transaction when the fixture is initialized
            _transaction = new TransactionScope(TransactionScopeOption.RequiresNew);

            // Seed the database once
            using (var context = new BevososContext())
            {
                SeedDatabase(context);
            }
        }

        private void SeedDatabase(BevososContext context)
        {
            // Ensure a clean database
            context.Tokens.RemoveRange(context.Tokens);
            context.Accounts.RemoveRange(context.Accounts);
            context.Users.RemoveRange(context.Users);

            // Seed data for AccountDAO tests
            var accountDALTestAccount = new Account
            {
                Email = "accountdal_test@example.com",
                PasswordHash = "hashed_password",
                User = new User
                {
                    Username = "AccountDALTestUser",
                    ProfilePictureId = 1
                }
            };
            context.Accounts.Add(accountDALTestAccount);

            // Seed data for TokenDAO tests
            var tokenDALTestAccount = new Account
            {
                Email = "tokendal_test@example.com",
                PasswordHash = "hashed_password",
                User = new User
                {
                    Username = "TokenTestUser",
                    ProfilePictureId = 1
                }
            };

            // Add the token-related accounts
            context.Accounts.Add(tokenDALTestAccount);

            // Add tokens for the token-related accounts
            var existingToken = new Token
            {
                Email = tokenDALTestAccount.Email,
                TokenValue = "123456",
                ExpiryDate = DateTime.Now.AddMinutes(15)
            };

            var anotherExistingToken = new Token
            {
                Email = "existingEmailToken@gmail.com",
                TokenValue = "654321",
                ExpiryDate = DateTime.Now.AddMinutes(16)
            };

            var expiredToken = new Token
            {
                Email = "expiredEmailToken@gmail.com",
                TokenValue = "789012",
                ExpiryDate = DateTime.Now.AddMinutes(-1)
            };

            context.Tokens.Add(existingToken);
            context.Tokens.Add(anotherExistingToken);
            context.Tokens.Add(expiredToken);

            // Save all changes
            context.SaveChanges();
        }

        public void Dispose()
        {
            // Rollback all changes by disposing the transaction scope
            _transaction.Dispose();
        }
    }

}
