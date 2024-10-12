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

namespace DataAccess
{
    public class BevososContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Account> Accounts { get; set; }
        public DbSet<Token> Tokens { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            // Unique constraint on User.Email
            modelBuilder.Entity<User>()
                .Property(u => u.Email)
                .HasColumnAnnotation(
                    IndexAnnotation.AnnotationName,
                    new IndexAnnotation(
                        new IndexAttribute("IX_UserEmail") { IsUnique = true }));

            // Unique constraint on Account.Email
            modelBuilder.Entity<Account>()
                .Property(a => a.Email)
                .HasColumnAnnotation(
                    IndexAnnotation.AnnotationName,
                    new IndexAnnotation(
                        new IndexAttribute("IX_AccountEmail") { IsUnique = true }));

            // Configure one-to-one relationship between User and Account
            modelBuilder.Entity<User>()
                .HasRequired(u => u.Account)
                .WithRequiredPrincipal(a => a.User);

            // Configure one-to-one relationship between Account and Token (if only one token per account)
            modelBuilder.Entity<Account>()
                .HasOptional(a => a.Token)
                .WithRequired(t => t.Account);

            base.OnModelCreating(modelBuilder);
        }
    }

}   
