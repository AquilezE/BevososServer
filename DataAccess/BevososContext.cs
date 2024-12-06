using DataAccess.Models;
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
        public DbSet<FriendRequest> FriendRequests { get; set; }
        public DbSet<Friendship> Friendships { get; set; }
        public DbSet<Blocked> BlockedList { get; set; }
        public DbSet<Stats> Stats { get; set; }

        // Constructor por defecto
        public BevososContext() : base("name=BevososContext")
        {
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            // Configuración del índice único para Username
            modelBuilder.Entity<User>()
                .Property(u => u.Username)
                .HasColumnAnnotation(
                    IndexAnnotation.AnnotationName,
                    new IndexAnnotation(
                        new IndexAttribute("IX_UserUsername") { IsUnique = true }));

            // Configuración del índice único para Email en Account
            modelBuilder.Entity<Account>()
                .Property(a => a.Email)
                .HasColumnAnnotation(
                    IndexAnnotation.AnnotationName,
                    new IndexAnnotation(
                        new IndexAttribute("IX_AccountEmail") { IsUnique = true }));

            // Relación uno a uno entre User y Account
            modelBuilder.Entity<User>()
                .HasOptional(u => u.Account)
                .WithRequired(a => a.User)
                .WillCascadeOnDelete(true);

            // Relaciones de FriendRequest
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

            // Relaciones de Friendship
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

            // Relaciones de Blocked
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

            // Configuración de la relación uno a uno entre User y Stats
            modelBuilder.Entity<User>()
                .HasOptional(u => u.Stats)
                .WithRequired(s => s.User)
                .WillCascadeOnDelete(true);

            base.OnModelCreating(modelBuilder);
        }
    }
}