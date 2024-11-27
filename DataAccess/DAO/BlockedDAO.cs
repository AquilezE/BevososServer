using DataAccess.Exceptions;
using DataAccess.Models;
using DataAccess.Utils;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core;
using System.Data.SqlClient;
using System.Linq;


namespace DataAccess.DAO
{
    public class BlockedData
    {
        public int BlockId { get; set; }
        public int BlockedId { get; set; }
        public string BlockerUsername { get; set; }
        public int ProfilePictureId { get; set; }
    }

    public class BlockedDAO
    {
        public bool AddBlock(int blockerId, int blockeeId)
        {
            return ExceptionHelper.ExecuteWithExceptionHandling(() =>
            {
                using (BevososContext context = new BevososContext())
                {
                    User blocker = context.Users.FirstOrDefault(u => u.UserId == blockerId);
                    User blockee = context.Users.FirstOrDefault(u => u.UserId == blockeeId);

                    if (blocker != null && blockee != null)
                    {
                        Blocked block = new Blocked
                        {
                            Blocker = blocker,
                            Blockee = blockee
                        };

                        context.BlockedList.Add(block);
                        context.SaveChanges();

                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            });
        }

        public bool DeleteBlock(int blockerId, int blockeeId)
        {
            return ExceptionHelper.ExecuteWithExceptionHandling(() =>
            {
                using (BevososContext context = new BevososContext())
                {
                    Blocked block = context.BlockedList.FirstOrDefault(b => b.BlockerId == blockerId && b.BlockeeId == blockeeId);

                    if (block != null)
                    {
                        context.BlockedList.Remove(block);
                        context.SaveChanges();

                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            });
        }

        public List<User> GetBlockList(int blockerId)
        {
            return ExceptionHelper.ExecuteWithExceptionHandling(() =>
            {
                using (BevososContext context = new BevososContext())
                {
                    List<User> blockedUsers = context.BlockedList
                                              .Where(b => b.BlockerId == blockerId)
                                              .Select(b => b.Blockee)
                                              .ToList();

                    return blockedUsers;
                }
            });
        }

        public List<BlockedData> GetBlockedListForUser(int currentUserId)
        {
            return ExceptionHelper.ExecuteWithExceptionHandling(() =>
            {
                using (BevososContext context = new BevososContext())
                {
                    List<BlockedData> blockedUsers = context.BlockedList
                        .Where(b => b.BlockerId == currentUserId)
                        .Select(b => new BlockedData
                        {
                            BlockId = b.Id,
                            BlockedId = b.Blockee.UserId,
                            BlockerUsername = b.Blockee.Username,
                            ProfilePictureId = b.Blockee.ProfilePictureId
                        })
                        .ToList();

                    return blockedUsers;
                }
            });
        }
    }
}