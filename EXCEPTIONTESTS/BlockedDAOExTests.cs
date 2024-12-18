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
    public class BlockedDAOExTests
    {
        [Fact]
        public void AddBlock_ThrowsDBException_WhenDBNotAvaliable()
        {
            using (var scope = new TransactionScope())
            {
                var dao = new BlockedDAO();
                int blockerId = 1;
                int blockeeId = 2;

                Assert.Throws<DataBaseException>(() => dao.AddBlock(blockerId, blockeeId));
            }
        }

        [Fact]
        public void DeleteBlock_ThrowsDBException_WhenDBNotAvaliable()
        {
            using (var scope = new TransactionScope())
            {
                var dao = new BlockedDAO();
                int blockerId = 1;
                int blockeeId = 2;

                Assert.Throws<DataBaseException>(() => dao.DeleteBlock(blockerId, blockeeId));
            }
        }

        [Fact]
        public void GetBlockList_ThrowsDBException_WhenDBNotAvaliable()
        {
            using (var scope = new TransactionScope())
            {
                var dao = new BlockedDAO();
                int blockerId = 1;

               Assert.Throws<DataBaseException>(() => dao.GetBlockList(blockerId));
            }
        }

        [Fact]
        public void GetBlockedListForUser_ThrowsDBException_WhenDBNotAvaliable()
        {
            using (var scope = new TransactionScope())
            {
                var dao = new BlockedDAO();
                int currentUserId = 1;

               Assert.Throws<DataBaseException>(() => dao.GetBlockedListForUser(currentUserId));
            }
        }
    }
}
