using System.Collections.Generic;
using DataAccess.Models;
using DataAccess.DAO;
using DataAccess;
using System.Linq;
using System.Transactions;
using Xunit;

namespace TEST
{
    public class FriendRequestDAOTest
    {

        [Fact]
        public void SendFriendRequest_ShouldSendRequest_WhenUsersExistAndNoExistingRequest()
        {
            using (var scope = new TransactionScope())
            {
                int requesterId, requesteeId;

                using (var context = new BevososContext())
                {
                    var requester = new User
                    {
                        Username = "RequesterUser",
                        ProfilePictureId = 1,
                        Account = new Account
                        {
                            Email = "requester@example.com",
                            PasswordHash = "hashed_password"
                        }
                    };

                    var requestee = new User
                    {
                        Username = "RequesteeUser",
                        ProfilePictureId = 1,
                        Account = new Account
                        {
                            Email = "requestee@example.com",
                            PasswordHash = "hashed_password"
                        }
                    };

                    context.Users.Add(requester);
                    context.Users.Add(requestee);
                    context.SaveChanges();

                    requesterId = requester.UserId;
                    requesteeId = requestee.UserId;
                }

                var friendService = new FriendRequestDAO();


                int result = friendService.SendFriendRequest(requesterId, requesteeId);


                Assert.NotEqual(0, result);

                using (var context = new BevososContext())
                {
                    var request = context.FriendRequests.FirstOrDefault(r => r.Id == result);

                    Assert.NotNull(request);
                }

            }
        }

        [Fact]
        public void SendFriendRequest_ShouldReturnZero_WhenRequestAlreadyExists()
        {
            using (var scope = new TransactionScope())
            {
                int requesterId, requesteeId;

                using (var context = new BevososContext())
                {
                    var requester = new User
                    {
                        Username = "RequesterUser",
                        ProfilePictureId = 1,
                        Account = new Account
                        {
                            Email = "requester@example.com",
                            PasswordHash = "hashed_password"
                        }
                    };

                    var requestee = new User
                    {
                        Username = "RequesteeUser",
                        ProfilePictureId = 1,
                        Account = new Account
                        {
                            Email = "requestee@example.com",
                            PasswordHash = "hashed_password"
                        }
                    };

                    context.Users.Add(requester);
                    context.Users.Add(requestee);
                    context.SaveChanges();

                    requesterId = requester.UserId;
                    requesteeId = requestee.UserId;

                    var existingRequest = new FriendRequest
                    {
                        RequesterId = requesterId,
                        RequesteeId = requesteeId
                    };
                    context.FriendRequests.Add(existingRequest);
                    context.SaveChanges();
                }

                var friendService = new FriendRequestDAO();

                int result = friendService.SendFriendRequest(requesterId, requesteeId);

                Assert.Equal(0, result);
            }
        }

        [Fact]
        public void AcceptFriendRequest_ShouldAcceptRequest_WhenRequestExists()
        {
            using (var scope = new TransactionScope())
            {
                int requestId;

                using (var context = new BevososContext())
                {

                    var requester = new User
                    {
                        Username = "RequesterUser",
                        ProfilePictureId = 1,
                        Account = new Account
                        {
                            Email = "requester@example.com",
                            PasswordHash = "hashed_password"
                        }
                    };

                    var requestee = new User
                    {
                        Username = "RequesteeUser",
                        ProfilePictureId = 1,
                        Account = new Account
                        {
                            Email = "requestee@example.com",
                            PasswordHash = "hashed_password"
                        }
                    };

                    context.Users.Add(requester);
                    context.Users.Add(requestee);
                    context.SaveChanges();

                    int requesterId = requester.UserId;
                    int requesteeId = requestee.UserId;


                    var friendRequest = new FriendRequest
                    {
                        RequesterId = requesterId,
                        RequesteeId = requesteeId
                    };
                    context.FriendRequests.Add(friendRequest);
                    context.SaveChanges();

                    requestId = friendRequest.Id;
                }

                var friendService = new FriendRequestDAO();


                bool result = friendService.AcceptFriendRequest(requestId);

                Assert.True(result);

            }
        }

        [Fact]
        public void AcceptFriendRequest_ShouldReturnFalse_WhenRequestDoesNotExist()
        {
            using (var scope = new TransactionScope())
            {
                var friendService = new FriendRequestDAO();

                bool result = friendService.AcceptFriendRequest(-1);

                Assert.False(result);
            }
        }

        [Fact]
        public void DeclineFriendRequest_ShouldDeleteRequest_WhenRequestExists()
        {
            using (var scope = new TransactionScope())
            {
                int requestId;

                using (var context = new BevososContext())
                {
                    var requester = new User
                    {
                        Username = "RequesterUser",
                        ProfilePictureId = 1,
                        Account = new Account
                        {
                            Email = "requester@example.com",
                            PasswordHash = "hashed_password"
                        }
                    };

                    var requestee = new User
                    {
                        Username = "RequesteeUser",
                        ProfilePictureId = 1,
                        Account = new Account
                        {
                            Email = "requestee@example.com",
                            PasswordHash = "hashed_password"
                        }
                    };

                    context.Users.Add(requester);
                    context.Users.Add(requestee);
                    context.SaveChanges();

                    int requesterId = requester.UserId;
                    int requesteeId = requestee.UserId;

                    var friendRequest = new FriendRequest
                    {
                        RequesterId = requesterId,
                        RequesteeId = requesteeId
                    };
                    context.FriendRequests.Add(friendRequest);
                    context.SaveChanges();

                    requestId = friendRequest.Id;
                }

                var friendService = new FriendRequestDAO();

                bool result = friendService.DeclineFriendRequest(requestId);

                Assert.True(result);

                using (var context = new BevososContext())
                {
                    var request = context.FriendRequests.FirstOrDefault(r => r.Id == requestId);

                    Assert.Null(request);
                }
            }
        }

        [Fact]
        public void DeclineFriendRequest_ShouldReturnFalse_WhenRequestDoesNotExist()
        {
            using (var scope = new TransactionScope())
            {
                var friendService = new FriendRequestDAO();

                bool result = friendService.DeclineFriendRequest(-1);


                Assert.False(result);

            }
        }

        [Fact]
        public void GetPendingFriendRequests_ShouldReturnRequests_WhenRequestsExist()
        {
            using (var scope = new TransactionScope())
            {
                int requesteeId;

                List<FriendRequest> pendingRequests = new List<FriendRequest>();
                using (var context = new BevososContext())
                {
                    var requestee = new User
                    {
                        Username = "RequesteeUser",
                        ProfilePictureId = 1,
                        Account = new Account
                        {
                            Email = "requestee@example.com",
                            PasswordHash = "hashed_password"
                        }
                    };

                    var requester1 = new User
                    {
                        Username = "RequesterUser1",
                        ProfilePictureId = 1,
                        Account = new Account
                        {
                            Email = "requester1@example.com",
                            PasswordHash = "hashed_password"
                        }
                    };

                    var requester2 = new User
                    {
                        Username = "RequesterUser2",
                        ProfilePictureId = 1,
                        Account = new Account
                        {
                            Email = "requester2@example.com",
                            PasswordHash = "hashed_password"
                        }
                    };

                    context.Users.Add(requestee);
                    context.Users.Add(requester1);
                    context.Users.Add(requester2);
                    context.SaveChanges();

                    requesteeId = requestee.UserId;
                    int requesterId1 = requester1.UserId;
                    int requesterId2 = requester2.UserId;

                    var friendRequest1 = new FriendRequest
                    {
                        RequesterId = requesterId1,
                        RequesteeId = requesteeId
                    };
                    var friendRequest2 = new FriendRequest
                    {
                        RequesterId = requesterId2,
                        RequesteeId = requesteeId
                    };
                    context.FriendRequests.Add(friendRequest1);
                    context.FriendRequests.Add(friendRequest2);


                    context.SaveChanges();

                    pendingRequests.Add(friendRequest1);
                    pendingRequests.Add(friendRequest2);
                }

                var friendService = new FriendRequestDAO();

                List<FriendRequest> pendingRequestsResult = friendService.GetPendingFriendRequests(requesteeId);

                Assert.Equal(pendingRequests, pendingRequestsResult);



            }
        }

        [Fact]
        public void GetPendingFriendRequests_ShouldReturnEmptyList_WhenNoRequestsExist()
        {
            using (var scope = new TransactionScope())
            {
                int requesteeId;

                using (var context = new BevososContext())
                {
                    var requestee = new User
                    {
                        Username = "RequesteeUser",
                        ProfilePictureId = 1,
                        Account = new Account
                        {
                            Email = "requestee@example.com",
                            PasswordHash = "hashed_password"
                        }
                    };

                    context.Users.Add(requestee);
                    context.SaveChanges();

                    requesteeId = requestee.UserId;
                }

                var friendService = new FriendRequestDAO();

                List<FriendRequest> pendingRequests = friendService.GetPendingFriendRequests(requesteeId);

                Assert.Empty(pendingRequests);

            }
        }
    }
}
