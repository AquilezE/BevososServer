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

        // Default constructor for production
        public BevososContext() : base("name=BevososContext")
        {
        }


        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            // Unique constraint on User.Username
            modelBuilder.Entity<User>()
                .Property(u => u.Username)
                .HasColumnAnnotation(
                    IndexAnnotation.AnnotationName,
                    new IndexAnnotation(
                        new IndexAttribute("IX_UserUsername") { IsUnique = true }));

            // Unique constraint on Account.Email
            modelBuilder.Entity<Account>()
                .Property(a => a.Email)
                .HasColumnAnnotation(
                    IndexAnnotation.AnnotationName,
                    new IndexAnnotation(
                        new IndexAttribute("IX_AccountEmail") { IsUnique = true }));

            // Define the one-to-one relationship between User and Account
            modelBuilder.Entity<User>()
                .HasOptional(u => u.Account)
                .WithRequired(a => a.User)
                .WillCascadeOnDelete(true);

            base.OnModelCreating(modelBuilder);
        }
    }

}   
