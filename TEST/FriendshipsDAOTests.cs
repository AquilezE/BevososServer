using System.Collections.Generic;
using DataAccess.Models;
using DataAccess.DAO;
using DataAccess;
using System.Linq;
using System.Transactions;
using Xunit;

namespace TEST
{
    public class FriendshipsDAOTests { 

        [Fact]
        public void AddFriendship_ShouldReturnNull_WhenUsersDoNotExist()
        {
            using (TransactionScope scope = new TransactionScope())
            {
                int nonExistentUserId1 = -1;
                int nonExistentUserId2 = -2;

                FriendshipDAO friendshipDAO = new FriendshipDAO();

                Friendship result = friendshipDAO.AddFriendship(nonExistentUserId1, nonExistentUserId2);

                Assert.Null(result);

            }
        }

        [Fact]
        public void AddFriendship_ShouldReturnNull_WhenFriendshipAlreadyExists()
        {
            using (TransactionScope scope = new TransactionScope())
            {
                int user1Id, user2Id;

                using (BevososContext context = new BevososContext())
                {
                    User user1 = new User
                    {
                        Username = "User1",
                        ProfilePictureId = 1,
                        Account = new Account
                        {
                            Email = "user1@example.com",
                            PasswordHash = "hashed_password"
                        }
                    };

                    User user2 = new User
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

                    Friendship friendship = new Friendship
                    {
                        User1Id = user1Id,
                        User2Id = user2Id
                    };
                    context.Friendships.Add(friendship);
                    context.SaveChanges();
                }

                FriendshipDAO friendshipDAO = new FriendshipDAO();

                Friendship result = friendshipDAO.AddFriendship(user1Id, user2Id);

                Assert.Null(result); 

            }
        }

        [Fact]
        public void AddFriendship_ShouldReturnFriendshipIfWorks()
        {
            using (TransactionScope scope = new TransactionScope())
            {
                int user1Id, user2Id;

                using (BevososContext context = new BevososContext())
                {
                    User user1 = new User
                    {
                        Username = "User1",
                        ProfilePictureId = 1,
                        Account = new Account
                        {
                            Email = "user1@example.com",
                            PasswordHash = "hashed_password"
                        }
                    };

                    User user2 = new User
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

                    context.SaveChanges();
                }

                FriendshipDAO friendshipDAO = new FriendshipDAO();

                Friendship result = friendshipDAO.AddFriendship(user1Id, user2Id);

                Friendship expected = new Friendship
                {
                    User1Id = user1Id,
                    User2Id = user2Id
                };

                Assert.Equal(expected.User1Id, result.User1Id);
                Assert.Equal(expected.User2Id, result.User2Id);

            }
        }

        [Fact]
        public void GetFriendshipList_ShouldReturnFriends_WhenFriendshipsExist()
        {
            using (TransactionScope scope = new TransactionScope())
            {
                int userId, friendId1, friendId2;

                using (BevososContext context = new BevososContext())
                {
                    User user = new User
                    {
                        Username = "User",
                        ProfilePictureId = 1,
                        Account = new Account
                        {
                            Email = "user@example.com",
                            PasswordHash = "hashed_password"
                        }
                    };

                    User friend1 = new User
                    {
                        Username = "Friend1",
                        ProfilePictureId = 1,
                        Account = new Account
                        {
                            Email = "friend1@example.com",
                            PasswordHash = "hashed_password"
                        }
                    };

                    User friend2 = new User
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
                }

                FriendshipDAO friendshipDAO = new FriendshipDAO();

                List<User> friends = friendshipDAO.GetFriendshipList(userId);

                Assert.Equal(2, friends.Count);
                Assert.Contains(friends, f => f.UserId == friendId1);
                Assert.Contains(friends, f => f.UserId == friendId2);

            }
        }

        [Fact]
        public void RemoveFriendship_ShouldReturnFalse_WhenFriendshipDoesNotExist()
        {
            using (TransactionScope scope = new TransactionScope())
            {
                int user1Id, user2Id;

                using (BevososContext context = new BevososContext())
                {
                    User user1 = new User
                    {
                        Username = "User1",
                        ProfilePictureId = 1,
                        Account = new Account
                        {
                            Email = "user1@example.com",
                            PasswordHash = "hashed_password"
                        }
                    };

                    User user2 = new User
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

                FriendshipDAO friendshipDAO = new FriendshipDAO();

                bool result = friendshipDAO.RemoveFriendship(user1Id, user2Id);

                Assert.False(result); 
            }
        }

        [Fact]
        public void GetFriendshipList_ShouldReturnEmptyList_WhenUserHasNoFriends()
        {
            using (TransactionScope scope = new TransactionScope())
            {
                int userId;

                using (BevososContext context = new BevososContext())
                {
                
                    User user = new User
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

                FriendshipDAO friendshipDAO = new FriendshipDAO();

               
                List<User> friends = friendshipDAO.GetFriendshipList(userId);

              
                Assert.Empty(friends); 
            }
        }

        [Fact]
        public void AddFriendship_ShouldWork_WhenAddingFriendshipInReverseOrder()
        {
            using (TransactionScope scope = new TransactionScope())
            {
                int user1Id, user2Id;

                using (BevososContext context = new BevososContext())
                {
                    
                    User user1 = new User
                    {
                        Username = "User1",
                        ProfilePictureId = 1,
                        Account = new Account
                        {
                            Email = "user1@example.com",
                            PasswordHash = "hashed_password"
                        }
                    };

                    User user2 = new User
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

                FriendshipDAO friendshipDAO = new FriendshipDAO();

                
                Friendship result = friendshipDAO.AddFriendship(user2Id, user1Id); // Reverse order

                Friendship expected = new Friendship
                {
                    User1Id = user1Id,
                    User2Id = user2Id
                };

                using (BevososContext context = new BevososContext())
                {
                    bool friendshipExists = friendshipDAO.FriendshipExists(user1Id, user2Id);

                    Assert.True(friendshipExists);
                }

            }
        }

        [Fact]
        public void GetFriendshipList_ShouldReturnCorrectFriends_WhenUserIsSecondInFriendship()
        {
            using (TransactionScope scope = new TransactionScope())
            {
                int userId, friendId;

                using (BevososContext context = new BevososContext())
                {
                    User user = new User
                    {
                        Username = "User",
                        ProfilePictureId = 1,
                        Account = new Account
                        {
                            Email = "user@example.com",
                            PasswordHash = "hashed_password"
                        }
                    };

                    User friend = new User
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

                    Friendship friendship = new Friendship
                    {
                        User1Id = friendId,
                        User2Id = userId
                    };
                    context.Friendships.Add(friendship);
                    context.SaveChanges();
                }

                FriendshipDAO friendshipDAO = new FriendshipDAO();

                List<User> friends = friendshipDAO.GetFriendshipList(userId);

                Assert.Single(friends);
                Assert.Equal(friendId, friends.First().UserId);

            }
        }

        [Fact]
        public void RemoveFriendship_ShouldWork_WhenFriendshipExistsInReverseOrder()
        {
            using (TransactionScope scope = new TransactionScope())
            {
                int user1Id, user2Id;

                using (BevososContext context = new BevososContext())
                {
                    User user1 = new User
                    {
                        Username = "User1",
                        ProfilePictureId = 1,
                        Account = new Account
                        {
                            Email = "user1@example.com",
                            PasswordHash = "hashed_password"
                        }
                    };

                    User user2 = new User
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

                    Friendship friendship = new Friendship
                    {
                        User1Id = user2Id,
                        User2Id = user1Id
                    };
                    context.Friendships.Add(friendship);
                    context.SaveChanges();
                }

                FriendshipDAO friendshipDAO = new FriendshipDAO();

                bool result = friendshipDAO.RemoveFriendship(user1Id, user2Id);

                Assert.True(result);

                using (BevososContext context = new BevososContext())
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
