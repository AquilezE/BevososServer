using DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.DAO
{
    public class BlockedDAO
    {
        public bool AddBlock(int blockerId, int blockeeId)
        {
            using (var context = new BevososContext()) 
            {
                try
                {
                    var blocker = context.Users.FirstOrDefault(u => u.UserId == blockerId);
                    var blockee = context.Users.FirstOrDefault(u => u.UserId == blockeeId);

                    if (blocker != null && blockee != null)
                    {
                        var block = new Blocked
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
                catch (Exception ex)
                {

                    return false;
                }
            }
        }

        public bool DeleteBlock(int blockerId, int blockeeId)
        {
            using (var context = new BevososContext())
            {
                try
                {
                    var block = context.BlockedList.FirstOrDefault(b => b.BlockerId == blockerId && b.BlockeeId == blockeeId);

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
                catch (Exception ex)
                {
                    return false;
                }
            }
        }
       
        public List<User> GetBlockList(int blockerId)
        {
            using (var context = new BevososContext())
            {
                var blockedUsers = context.BlockedList
                                          .Where(b => b.BlockerId == blockerId)
                                          .Select(b => b.Blockee) 
                                          .ToList();

                return blockedUsers;
            }
        }
    }
}
