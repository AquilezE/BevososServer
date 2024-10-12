using DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;

namespace DataAccess
{
    public class MyDbContext : DbContext
    {
        public DbSet<Account> Accounts { get; set; }
        public DbSet<User>  Users { get; set; }
        public DbSet<Token> Tokens { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            // Account to User (One-to-One)
            modelBuilder.Entity<Account>()
                .HasOptional(a => a.User)
                .WithRequired(u => u.Account);

            // Account to Token (One-to-One)
            modelBuilder.Entity<Account>()
                .HasOptional(a => a.Token)
                .WithRequired(t => t.Account);

            // Unique Index on Email
            modelBuilder.Entity<Account>()
                .Property(a => a.Email)
                .HasColumnAnnotation(
                    IndexAnnotation.AnnotationName,
                    new IndexAnnotation(new IndexAttribute { IsUnique = true }));

            base.OnModelCreating(modelBuilder);
        }
    }

}   
