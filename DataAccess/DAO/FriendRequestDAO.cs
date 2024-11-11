using DataAccess.Exceptions;
using DataAccess.Models;
using DataAccess.Utils;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core;
using System.Data.SqlClient;
using System.Linq;


namespace DataAccess.DAO
{
    public class FriendRequestData
    {
        public int FriendRequestId { get; set; }
        public int SenderId { get; set; }
        public string SenderName { get; set; }
        public int ProfilePictureId { get; set; }
    }
    public class FriendRequestDAO
    {
        public int SendFriendRequest(int requesterId, int requesteeId)
        {
            try
            {
                using (var context = new BevososContext())
                {
                    var requester = context.Users.FirstOrDefault(u => u.UserId == requesterId);
                    if (requester == null)
                    {
                        return 0;
                    }

                    var requestee = context.Users.FirstOrDefault(u => u.UserId == requesteeId);
                    if (requestee == null)
                    {
                        return 0;
                    }

                    bool requestExists = context.FriendRequests.Any(fr =>
                        (fr.RequesterId == requesterId && fr.RequesteeId == requestee.UserId) ||
                        (fr.RequesterId == requestee.UserId && fr.RequesteeId == requesterId));

                    if (requestExists)
                    {
                        return 0;
                    }

                    var friendRequest = new FriendRequest
                    {
                        RequesterId = requesterId,
                        RequesteeId = requestee.UserId
                    };

                    context.FriendRequests.Add(friendRequest);
                    context.SaveChanges();

                    return friendRequest.Id;
                }
            }
            catch (EntityException ex)
            {
                ExceptionManager.LogErrorException(ex);
                throw new DataBaseException(ex.Message);
            }
            catch (SqlException ex)
            {
                ExceptionManager.LogErrorException(ex);
                throw new DataBaseException(ex.Message);
            }
            catch (Exception ex)
            {
                ExceptionManager.LogFatalException(ex);
                throw new DataBaseException(ex.Message);
            }
        }

        public bool AcceptFriendRequest(int requestId)
        {
            try
            {
                using (var context = new BevososContext())
                {
                    var friendRequest = context.FriendRequests.FirstOrDefault(fr => fr.Id == requestId);
                    if (friendRequest == null)
                    {
                        return false;
                    }

                    context.FriendRequests.Remove(friendRequest);
                    context.SaveChanges();

                    return true;
                }
            }
            catch (EntityException ex)
            {
                ExceptionManager.LogErrorException(ex);
                throw new DataBaseException(ex.Message);
            }
            catch (SqlException ex)
            {
                ExceptionManager.LogErrorException(ex);
                throw new DataBaseException(ex.Message);
            }
            catch (Exception ex)
            {
                ExceptionManager.LogFatalException(ex);
                throw new DataBaseException(ex.Message);
            }
        }

        public bool DeclineFriendRequest(int requestId)
        {
            try
            {
                using (var context = new BevososContext())
                {
                    var friendRequest = context.FriendRequests.FirstOrDefault(fr => fr.Id == requestId);
                    if (friendRequest == null)
                    {
                        return false;
                    }

                    context.FriendRequests.Remove(friendRequest);
                    context.SaveChanges();

                    return true;
                }
            }
            catch (EntityException ex)
            {
                ExceptionManager.LogErrorException(ex);
                throw new DataBaseException(ex.Message);
            }
            catch (SqlException ex)
            {
                ExceptionManager.LogErrorException(ex);
                throw new DataBaseException(ex.Message);
            }
            catch (Exception ex)
            {
                ExceptionManager.LogFatalException(ex);
                throw new DataBaseException(ex.Message);
            }
        }

        public List<FriendRequest> GetPendingFriendRequests(int userId)
        {
            try
            {
                using (var context = new BevososContext())
                {
                    var pendingRequests = context.FriendRequests
                        .Where(fr => fr.RequesteeId == userId)
                        .Include(fr => fr.Requester)
                        .ToList();

                    return pendingRequests;
                }
            }
            catch (EntityException ex)
            {
                ExceptionManager.LogErrorException(ex);
                throw new DataBaseException(ex.Message);
            }
            catch (SqlException ex)
            {
                ExceptionManager.LogErrorException(ex);
                throw new DataBaseException(ex.Message);
            }
            catch (Exception ex)
            {
                ExceptionManager.LogFatalException(ex);
                throw new DataBaseException(ex.Message);
            }
        }

        public List<FriendRequestData> GetFriendRequestForUser(int currentUserId)
        {
            try
            {
                using (var context = new BevososContext())
                {
                    var friendRequests = context.FriendRequests
                        .Where(fr => fr.RequesteeId == currentUserId)
                        .Select(fr => new FriendRequestData
                        {
                            FriendRequestId = fr.Id,
                            SenderId = fr.Requester.UserId,
                            SenderName = fr.Requester.Username,
                            ProfilePictureId = fr.Requester.ProfilePictureId
                        })
                        .ToList();

                    return friendRequests;
                }
            }
            catch (EntityException ex)
            {
                ExceptionManager.LogErrorException(ex);
                throw new DataBaseException(ex.Message);
            }
            catch (SqlException ex)
            {
                ExceptionManager.LogErrorException(ex);
                throw new DataBaseException(ex.Message);
            }
            catch (Exception ex)
            {
                ExceptionManager.LogFatalException(ex);
                throw new DataBaseException(ex.Message);
            }
        }

        public bool FriendRequestExists(int requestId)
        {
            try
            {
                using (var context = new BevososContext())
                {
                    return context.FriendRequests.Any(fr => fr.Id == requestId);
                }
            }
            catch (EntityException ex)
            {
                ExceptionManager.LogErrorException(ex);
                throw new DataBaseException(ex.Message);
            }
            catch (SqlException ex)
            {
                ExceptionManager.LogErrorException(ex);
                throw new DataBaseException(ex.Message);
            }
            catch (Exception ex)
            {
                ExceptionManager.LogFatalException(ex);
                throw new DataBaseException(ex.Message);
            }
        }
    }
}
