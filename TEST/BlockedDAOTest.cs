﻿using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using DataAccess;
using DataAccess.DAO;
using DataAccess.Models;
using Xunit;

namespace TEST
{

    public class BlockedDAOTest
    {

        [Fact]
        public void AddBlock_ShouldAddBlock_WhenUsersExist()
        {
            int blockerId;
            int blockeeId;

            using (var scope = new TransactionScope())
            {
                using (var context = new BevososContext())
                {
                    var blocker = new User
                    {
                        Username = "BlockerUser",
                        ProfilePictureId = 1,
                        Account = new Account
                        {
                            Email = "blocker@example.com",
                            PasswordHash = "hashed_password"
                        }
                    };

                    var blockee = new User
                    {
                        Username = "BlockeeUser",
                        ProfilePictureId = 1,
                        Account = new Account
                        {
                            Email = "blockee@example.com",
                            PasswordHash = "hashed_password"
                        }
                    };

                    context.Users.Add(blocker);
                    context.Users.Add(blockee);
                    context.SaveChanges();

                    blockerId = blocker.UserId;
                    blockeeId = blockee.UserId;
                }

                var blockService = new BlockedDAO();

                bool result = blockService.AddBlock(blockerId, blockeeId, "reasons");

                Assert.True(result);
            }
        }

        [Fact]
        public void AddBlock_ShouldReturnFalse_WhenUsersDoNotExist()
        {
            int blockerId = 999;
            int blockeeId = 1000;

            var blockService = new BlockedDAO();


            bool result = blockService.AddBlock(blockerId, blockeeId, "reasons");


            Assert.False(result);
        }

        [Fact]
        public void DeleteBlock_ShouldDeleteBlock_WhenBlockExists()
        {
            int blockerId;
            int blockeeId;

            using (var scope = new TransactionScope())
            {
                using (var context = new BevososContext())
                {
                    var blocker = new User
                    {
                        Username = "BlockerUser",
                        ProfilePictureId = 1,
                        Account = new Account
                        {
                            Email = "blocker@example.com",
                            PasswordHash = "hashed_password"
                        }
                    };

                    var blockee = new User
                    {
                        Username = "BlockeeUser",
                        ProfilePictureId = 1,
                        Account = new Account
                        {
                            Email = "blockee@example.com",
                            PasswordHash = "hashed_password"
                        }
                    };

                    context.Users.Add(blocker);
                    context.Users.Add(blockee);
                    context.SaveChanges();

                    blockerId = blocker.UserId;
                    blockeeId = blockee.UserId;

                    var block = new Blocked
                    {
                        BlockerId = blockerId,
                        BlockeeId = blockeeId
                    };
                    context.BlockedList.Add(block);
                    context.SaveChanges();
                }

                var blockService = new BlockedDAO();


                bool result = blockService.DeleteBlock(blockerId, blockeeId);

                Assert.True(result);

                using (var context = new BevososContext())
                {
                    Blocked block =
                        context.BlockedList.FirstOrDefault(b => b.BlockerId == blockerId && b.BlockeeId == blockeeId);
                    Assert.Null(block);
                }
            }
        }

        [Fact]
        public void DeleteBlock_ShouldReturnFalse_WhenBlockDoesNotExist()
        {
            using (var scope = new TransactionScope())
            {
                int blockerId;
                int blockeeId;

                using (var context = new BevososContext())
                {
                    var blocker = new User
                    {
                        Username = "BlockerUser",
                        ProfilePictureId = 1,
                        Account = new Account
                        {
                            Email = "blocker@example.com",
                            PasswordHash = "hashed_password"
                        }
                    };

                    var blockee = new User
                    {
                        Username = "BlockeeUser",
                        ProfilePictureId = 1,
                        Account = new Account
                        {
                            Email = "blockee@example.com",
                            PasswordHash = "hashed_password"
                        }
                    };

                    context.Users.Add(blocker);
                    context.Users.Add(blockee);
                    context.SaveChanges();

                    blockerId = blocker.UserId;
                    blockeeId = blockee.UserId;
                }

                var blockService = new BlockedDAO();

                bool result = blockService.DeleteBlock(blockerId, blockeeId);

                Assert.False(result);
            }
        }

        [Fact]
        public void GetBlockList_ShouldReturnBlockedUsers_WhenBlocksExist()
        {
            int blockeeId1;
            int blockeeId2;

            var blockedList = new List<User>();

            using (var scope = new TransactionScope())
            {
                int blockerId;
                using (var context = new BevososContext())
                {
                    var blocker = new User
                    {
                        Username = "BlockerUser",
                        ProfilePictureId = 1,
                        Account = new Account
                        {
                            Email = "blocker@example.com",
                            PasswordHash = "hashed_password"
                        }
                    };

                    var blockee1 = new User
                    {
                        Username = "BlockeeUser1",
                        ProfilePictureId = 1,
                        Account = new Account
                        {
                            Email = "blockee1@example.com",
                            PasswordHash = "hashed_password"
                        }
                    };

                    var blockee2 = new User
                    {
                        Username = "BlockeeUser2",
                        ProfilePictureId = 1,
                        Account = new Account
                        {
                            Email = "blockee2@example.com",
                            PasswordHash = "hashed_password"
                        }
                    };

                    context.Users.Add(blocker);
                    context.Users.Add(blockee1);
                    context.Users.Add(blockee2);
                    context.SaveChanges();

                    blockerId = blocker.UserId;
                    blockeeId1 = blockee1.UserId;
                    blockeeId2 = blockee2.UserId;

                    context.BlockedList.Add(new Blocked { BlockerId = blockerId, BlockeeId = blockeeId1 });
                    context.BlockedList.Add(new Blocked { BlockerId = blockerId, BlockeeId = blockeeId2 });
                    context.SaveChanges();
                    blockedList.Add(blockee1);
                    blockedList.Add(blockee2);
                }

                var blockService = new BlockedDAO();

                List<User> blockedUsers = blockService.GetBlockList(blockerId);

                Assert.Equal(blockedList, blockedUsers);
            }
        }

        [Fact]
        public void GetBlockList_ShouldReturnEmptyList_WhenNoBlocksExist()
        {
            using (var scope = new TransactionScope())
            {
                int blockerId;

                using (var context = new BevososContext())
                {
                    var blocker = new User
                    {
                        Username = "BlockerUser",
                        ProfilePictureId = 1,
                        Account = new Account
                        {
                            Email = "blocker@example.com",
                            PasswordHash = "hashed_password"
                        }
                    };

                    context.Users.Add(blocker);
                    context.SaveChanges();

                    blockerId = blocker.UserId;
                }

                var blockService = new BlockedDAO();

                List<User> blockedUsers = blockService.GetBlockList(blockerId);

                Assert.Empty(blockedUsers);
            }
        }

        [Fact]
        public void GetBlockList_ShouldReturnEmptyList_WhenBlockerDoesNotExist()
        {
            int blockerId = 999;

            var blockService = new BlockedDAO();

            List<User> blockedUsers = blockService.GetBlockList(blockerId);

            Assert.Empty(blockedUsers);
        }

    }

}