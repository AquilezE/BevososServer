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

    public class FriendRequestDAOExTests
    {

        [Fact]
        public void SendFriendRequest_ThrowsDBException_WhenDBNotAvailable()
        {
            using (var scope = new TransactionScope())
            {
                var dao = new FriendRequestDAO();

                Assert.Throws<DataBaseException>(() => dao.SendFriendRequest(1, 2));
            }
        }

        [Fact]
        public void AcceptFriendRequest_ThrowsDBException_WhenDBNotAvailable()
        {
            using (var scope = new TransactionScope())
            {
                var dao = new FriendRequestDAO();

                Assert.Throws<DataBaseException>(() => dao.AcceptFriendRequest(1));
            }
        }

        [Fact]
        public void DeclineFriendRequest_ThrowsDBException_WhenDBNotAvailable()
        {
            using (var scope = new TransactionScope())
            {
                var dao = new FriendRequestDAO();

                Assert.Throws<DataBaseException>(() => dao.DeclineFriendRequest(1));
            }
        }

        [Fact]
        public void GetPendingFriendRequests_ThrowsDBException_WhenDBNotAvailable()
        {
            using (var scope = new TransactionScope())
            {
                var dao = new FriendRequestDAO();
                int userId = 1;

                Assert.Throws<DataBaseException>(() => dao.GetPendingFriendRequests(userId));
            }
        }

        [Fact]
        public void GetFriendRequestForUser_ThrowsDBException_WhenDBNotAvailable()
        {
            using (var scope = new TransactionScope())
            {
                var dao = new FriendRequestDAO();
                int currentUserId = 1;

                Assert.Throws<DataBaseException>(() => dao.GetFriendRequestForUser(currentUserId));
            }
        }

        [Fact]
        public void FriendRequestExists_ThrowsDBException_WhenDBNotAvailable()
        {
            using (var scope = new TransactionScope())
            {
                var dao = new FriendRequestDAO();
                int requestId = 1;

                Assert.Throws<DataBaseException>(() => dao.FriendRequestExists(requestId));
            }
        }

    }

}