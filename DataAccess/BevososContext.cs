using DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using System.Data.Entity.Infrastructure.Annotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Common;

namespace DataAccess
{
    public class BevososContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Account> Accounts { get; set; }
        public DbSet<Token> Tokens { get; set; }
        public DbSet<FriendRequest> FriendRequests { get; set; }
        public DbSet<Friendship> Friendships { get; set; }
        public DbSet<Blocked> BlockedList { get; set; }


        // Default constructor
        public BevososContext() : base("name=BevososContext")
        {

        }


        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .Property(u => u.Username)
                .HasColumnAnnotation(
                    IndexAnnotation.AnnotationName,
                    new IndexAnnotation(
                        new IndexAttribute("IX_UserUsername") { IsUnique = true })); //Unique

            modelBuilder.Entity<Account>()
                .Property(a => a.Email)
                .HasColumnAnnotation(
                    IndexAnnotation.AnnotationName,
                    new IndexAnnotation(
                        new IndexAttribute("IX_AccountEmail") { IsUnique = true })); //Unique

            modelBuilder.Entity<User>()
                .HasOptional(u => u.Account)
                .WithRequired(a => a.User)
                .WillCascadeOnDelete(true); // Cascadeo


            //FriendRequest relationships
            modelBuilder.Entity<FriendRequest>()
                .HasRequired(fr => fr.Requester)
                .WithMany()
                .HasForeignKey(fr => fr.RequesterId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<FriendRequest>()
                .HasRequired(fr => fr.Requestee)
                .WithMany()
                .HasForeignKey(fr => fr.RequesteeId)
                .WillCascadeOnDelete(false);

            //Friendship relationships
            modelBuilder.Entity<Friendship>()
                .HasRequired(f => f.User1)
                .WithMany()
                .HasForeignKey(f => f.User1Id)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Friendship>()
                .HasRequired(f => f.User2)
                .WithMany()
                .HasForeignKey(f => f.User2Id)
                .WillCascadeOnDelete(false);

            //Blocked relationships
            modelBuilder.Entity<Blocked>()
                .HasRequired(b => b.Blocker)
                .WithMany()
                .HasForeignKey(b => b.BlockerId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Blocked>()
                .HasRequired(b => b.Blockee)
                .WithMany()
                .HasForeignKey(b => b.BlockeeId)
                .WillCascadeOnDelete(false);

            base.OnModelCreating(modelBuilder);
        }
    }

}   
