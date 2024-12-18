using DataAccess.DAO;
using DataAccess.Exceptions;
using System.Transactions;
using Xunit;

namespace EXCEPTIONTESTS
{

    public class FriendshipDAOExTests
    {

        [Fact]
        public void AddFriendship_ThrowsDBException_WhenDBNotAvaliable()
        {
            using (var scope = new TransactionScope())
            {
                var dao = new FriendshipDAO();
                Assert.Throws<DataBaseException>(() => dao.AddFriendship(1, 2));
            }
        }

        [Fact]
        public void FriendshipExists_ThrowsDBException_WhenDBNotAvaliable()
        {
            using (var scope = new TransactionScope())
            {
                var dao = new FriendshipDAO();
                Assert.Throws<DataBaseException>(() => dao.FriendshipExists(1, 2));
            }
        }

        [Fact]
        public void RemoveFriendship_ThrowsDBException_WhenDBNotAvaliable()
        {
            using (var scope = new TransactionScope())
            {
                var dao = new FriendshipDAO();
                Assert.Throws<DataBaseException>(() => dao.RemoveFriendship(1, 2));
            }
        }

        [Fact]
        public void GetFriendshipList_ThrowsDBException_WhenDBNotAvaliable()
        {
            using (var scope = new TransactionScope())
            {
                var dao = new FriendshipDAO();
                Assert.Throws<DataBaseException>(() => dao.GetFriendshipList(1));
            }
        }

        [Fact]
        public void GetFriendsForUser_ThrowsDBException_WhenDBNotAvaliable()
        {
            using (var scope = new TransactionScope())
            {
                var dao = new FriendshipDAO();
                Assert.Throws<DataBaseException>(() => dao.GetFriendsForUser(1));
            }
        }

    }

}