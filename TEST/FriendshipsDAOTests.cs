using System.Collections.Generic;
using DataAccess.Models;
using DataAccess.DAO;
using DataAccess;
using System.Linq;
using System.Transactions;
using Xunit;

namespace TEST
{
    public class FriendshipsDAOTests
    {

        [Fact]
        public void AddFriendship_ShouldReturnNull_WhenUsersDoNotExist()
        {
            using (var scope = new TransactionScope())
            {
                int nonExistentUserId1 = -1;
                int nonExistentUserId2 = -2;

                var friendshipDAO = new FriendshipDAO();

                Friendship result = friendshipDAO.AddFriendship(nonExistentUserId1, nonExistentUserId2);

                Assert.Null(result);
            }
        }

        [Fact]
        public void AddFriendship_ShouldReturnNull_WhenFriendshipAlreadyExists()
        {
            using (var scope = new TransactionScope())
            {
                int user1Id, user2Id;

                using (var context = new BevososContext())
                {
                    var user1 = new User
                    {
                        Username = "User1",
                        ProfilePictureId = 1,
                        Account = new Account
                        {
                            Email = "user1@example.com",
                            PasswordHash = "hashed_password"
                        }
                    };

                    var user2 = new User
                    {
                        Username = "User2",
                        ProfilePictureId = 1,
                        Account = new Account
                        {
                            Email = "user2@example.com",
                            PasswordHash = "hashed_password"
                        }
                    };

                    context.Users.Add(user1);
                    context.Users.Add(user2);
                    context.SaveChanges();

                    user1Id = user1.UserId;
                    user2Id = user2.UserId;

                    var friendship = new Friendship
                    {
                        User1Id = user1Id,
                        User2Id = user2Id
                    };
                    context.Friendships.Add(friendship);
                    context.SaveChanges();
                }

                var friendshipDAO = new FriendshipDAO();

                Friendship result = friendshipDAO.AddFriendship(user1Id, user2Id);

                Assert.Null(result);

            }
        }

        [Fact]
        public void AddFriendship_ShouldReturnFriendshipIfWorks()
        {
            using (var scope = new TransactionScope())
            {
                int user1Id, user2Id;


                Friendship expected; 

                using (var context = new BevososContext())
                {
                    var user1 = new User
                    {
                        Username = "User1",
                        ProfilePictureId = 1,
                        Account = new Account
                        {
                            Email = "user1@example.com",
                            PasswordHash = "hashed_password"
                        }
                    };

                    var user2 = new User
                    {
                        Username = "User2",
                        ProfilePictureId = 1,
                        Account = new Account
                        {
                            Email = "user2@example.com",
                            PasswordHash = "hashed_password"
                        }
                    };

                    context.Users.Add(user1);
                    context.Users.Add(user2);
                    context.SaveChanges();

                    user1Id = user1.UserId;
                    user2Id = user2.UserId;

                    expected = new Friendship
                    {
                        User1Id = user1Id,
                        User2Id = user2Id
                    };

                    context.SaveChanges();
                }

                var friendshipDAO = new FriendshipDAO();

                Friendship result = friendshipDAO.AddFriendship(user1Id, user2Id);


                Assert.Equal(expected, result);
            }
        }

        [Fact]
        public void GetFriendshipList_ShouldReturnFriends_WhenFriendshipsExist()
        {
            using (var scope = new TransactionScope())
            {

                var friendshipDAO = new FriendshipDAO();

                int userId, friendId1, friendId2;

                List<User> friends = new List<User>();


                using (var context = new BevososContext())
                {
                    var user = new User
                    {
                        Username = "User",
                        ProfilePictureId = 1,
                        Account = new Account
                        {
                            Email = "user@example.com",
                            PasswordHash = "hashed_password"
                        }
                    };

                    var friend1 = new User
                    {
                        Username = "Friend1",
                        ProfilePictureId = 1,
                        Account = new Account
                        {
                            Email = "friend1@example.com",
                            PasswordHash = "hashed_password"
                        }
                    };

                    var friend2 = new User
                    {
                        Username = "Friend2",
                        ProfilePictureId = 1,
                        Account = new Account
                        {
                            Email = "friend2@example.com",
                            PasswordHash = "hashed_password"
                        }
                    };

                    context.Users.Add(user);
                    context.Users.Add(friend1);
                    context.Users.Add(friend2);
                    context.SaveChanges();

                    userId = user.UserId;
                    friendId1 = friend1.UserId;
                    friendId2 = friend2.UserId;

                    context.Friendships.Add(new Friendship { User1Id = userId, User2Id = friendId1 });
                    context.Friendships.Add(new Friendship { User1Id = userId, User2Id = friendId2 });
                    context.SaveChanges();

                    friends.Add(friend1);
                    friends.Add(friend2);

                }


                List<User> friendsResults = friendshipDAO.GetFriendshipList(userId);

                Assert.Equal(friendsResults, friends);

            }
        }

        [Fact]
        public void RemoveFriendship_ShouldReturnFalse_WhenFriendshipDoesNotExist()
        {
            using (var scope = new TransactionScope())
            {
                int user1Id, user2Id;

                using (var context = new BevososContext())
                {
                    var user1 = new User
                    {
                        Username = "User1",
                        ProfilePictureId = 1,
                        Account = new Account
                        {
                            Email = "user1@example.com",
                            PasswordHash = "hashed_password"
                        }
                    };

                    var user2 = new User
                    {
                        Username = "User2",
                        ProfilePictureId = 1,
                        Account = new Account
                        {
                            Email = "user2@example.com",
                            PasswordHash = "hashed_password"
                        }
                    };

                    context.Users.Add(user1);
                    context.Users.Add(user2);
                    context.SaveChanges();

                    user1Id = user1.UserId;
                    user2Id = user2.UserId;
                }

                var friendshipDAO = new FriendshipDAO();

                bool result = friendshipDAO.RemoveFriendship(user1Id, user2Id);

                Assert.False(result);
            }
        }

        [Fact]
        public void GetFriendshipList_ShouldReturnEmptyList_WhenUserHasNoFriends()
        {
            using (var scope = new TransactionScope())
            {
                int userId;

                using (var context = new BevososContext())
                {

                    var user = new User
                    {
                        Username = "LonelyUser",
                        ProfilePictureId = 1,
                        Account = new Account
                        {
                            Email = "lonelyuser@example.com",
                            PasswordHash = "hashed_password"
                        }
                    };

                    context.Users.Add(user);
                    context.SaveChanges();

                    userId = user.UserId;
                }

                var friendshipDAO = new FriendshipDAO();


                List<User> friends = friendshipDAO.GetFriendshipList(userId);

                Assert.Empty(friends);
            }
        }

        [Fact]
        public void AddFriendship_ShouldWork_WhenAddingFriendshipInReverseOrder()
        {
            using (var scope = new TransactionScope())
            {
                int user1Id, user2Id;

                using (var context = new BevososContext())
                {

                    var user1 = new User
                    {
                        Username = "User1",
                        ProfilePictureId = 1,
                        Account = new Account
                        {
                            Email = "user1@example.com",
                            PasswordHash = "hashed_password"
                        }
                    };

                    var user2 = new User
                    {
                        Username = "User2",
                        ProfilePictureId = 1,
                        Account = new Account
                        {
                            Email = "user2@example.com",
                            PasswordHash = "hashed_password"
                        }
                    };

                    context.Users.Add(user1);
                    context.Users.Add(user2);
                    context.SaveChanges();

                    user1Id = user1.UserId;
                    user2Id = user2.UserId;
                }

                var friendshipDAO = new FriendshipDAO();


                Friendship result = friendshipDAO.AddFriendship(user2Id, user1Id);

                var expected = new Friendship
                {
                    User1Id = user1Id,
                    User2Id = user2Id
                };

                using (var context = new BevososContext())
                {
                    bool friendshipExists = friendshipDAO.FriendshipExists(user1Id, user2Id);

                    Assert.True(friendshipExists);
                }
            }
        }

        [Fact]
        public void GetFriendshipList_ShouldReturnCorrectFriends_WhenUserIsSecondInFriendship()
        {
            using (var scope = new TransactionScope())
            {
                int userId, friendId;

                using (var context = new BevososContext())
                {
                    var user = new User
                    {
                        Username = "User",
                        ProfilePictureId = 1,
                        Account = new Account
                        {
                            Email = "user@example.com",
                            PasswordHash = "hashed_password"
                        }
                    };

                    var friend = new User
                    {
                        Username = "Friend",
                        ProfilePictureId = 1,
                        Account = new Account
                        {
                            Email = "friend@example.com",
                            PasswordHash = "hashed_password"
                        }
                    };

                    context.Users.Add(user);
                    context.Users.Add(friend);
                    context.SaveChanges();

                    userId = user.UserId;
                    friendId = friend.UserId;

                    var friendship = new Friendship
                    {
                        User1Id = friendId,
                        User2Id = userId
                    };
                    context.Friendships.Add(friendship);
                    context.SaveChanges();
                }

                var friendshipDAO = new FriendshipDAO();

                List<User> friends = friendshipDAO.GetFriendshipList(userId);

                Assert.Single(friends);
                Assert.Equal(friendId, friends.First().UserId);

            }
        }

        [Fact]
        public void RemoveFriendship_ShouldWork_WhenFriendshipExistsInReverseOrder() 
        {
            using (var scope = new TransactionScope())
            {
                int user1Id, user2Id;

                using (var context = new BevososContext())
                {
                    var user1 = new User
                    {
                        Username = "User1",
                        ProfilePictureId = 1,
                        Account = new Account
                        {
                            Email = "user1@example.com",
                            PasswordHash = "hashed_password"
                        }
                    };

                    var user2 = new User
                    {
                        Username = "User2",
                        ProfilePictureId = 1,
                        Account = new Account
                        {
                            Email = "user2@example.com",
                            PasswordHash = "hashed_password"
                        }
                    };

                    context.Users.Add(user1);
                    context.Users.Add(user2);
                    context.SaveChanges();

                    user1Id = user1.UserId;
                    user2Id = user2.UserId;

                    var friendship = new Friendship
                    {
                        User1Id = user2Id,
                        User2Id = user1Id
                    };
                    context.Friendships.Add(friendship);
                    context.SaveChanges();
                }

                var friendshipDAO = new FriendshipDAO();

                bool result = friendshipDAO.RemoveFriendship(user1Id, user2Id);

                Assert.True(result);

                using (var context = new BevososContext())
                {
                    bool friendshipExists = context.Friendships.Any(f =>
                        (f.User1Id == user1Id && f.User2Id == user2Id) ||
                        (f.User1Id == user2Id && f.User2Id == user1Id));

                    Assert.False(friendshipExists);
                }

            }
        }
    

    }
}
