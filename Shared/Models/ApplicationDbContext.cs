using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using CsInvite.Shared.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CsInvite.Shared.Models
{
    public class ApplicationDbContext: IdentityDbContext<User>
    {
        public DbSet<Friend> Friends { get; set; }
        public DbSet<Lobby> Lobbies { get; set; }
        public DbSet<Invite> Invites { get; set; }

        private string connectionString;
        private ILoggerFactory loggerFactory;

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IConfiguration configuration, ILoggerFactory loggerFactory)
            : base(options)
        {
            connectionString = configuration.GetConnectionString("MySQL");
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.UseMySql(connectionString);
            optionsBuilder.UseLoggerFactory(loggerFactory);
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Invite>()
                .Property(i => i.Answer)
                .HasDefaultValue(Answer.None);
            builder.Entity<User>()
                .Property(u => u.Permaban)
                .HasDefaultValue(Map.None);
            builder.Entity<Invite>()
                .Property(i => i.Date)
                .HasDefaultValueSql("NOW(6)");
            builder.Entity<Lobby>()
                .Property(l => l.Created)
                .HasDefaultValueSql("NOW(6)");
            builder.Entity<Lobby>()
                .Property(l => l.LastModified)
                .HasDefaultValueSql("NOW(6)");

            builder.Entity<User>()
                .Property(u => u.UserName)
                .IsUnicode(true);
            builder.Entity<User>()
                .Property(u => u.NormalizedUserName)
                .IsUnicode(true);

            builder.Entity<User>()
                .Property(u => u.FriendsWithSteamBotIndex)
                .IsRequired(false)
                .HasDefaultValue(null);

            builder.Entity<Friend>()
                .HasOne(f => f.User)
                .WithMany(u => u.Friends)
                .HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            builder.Entity<Friend>()
                .HasOne(f => f.OtherUser)
                .WithMany(u => u.IsInFriendsListOf)
                .HasForeignKey(f => f.OtherUserId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            builder.Entity<User>()
                .HasOne(u => u.CurrentLobby)
                .WithMany(l => l.Members)
                .HasForeignKey(u => u.CurrentLobbyId)
                .OnDelete(DeleteBehavior.SetNull)
                .IsRequired(false);

            builder.Entity<Invite>()
                .HasOne(i => i.Recipient)
                .WithMany(u => u.Invites)
                .HasForeignKey(i => i.RecipientId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired(false);

            builder.Entity<Lobby>()
                .HasOne(l => l.Owner)
                .WithMany(u => u.IsOwnerOf)
                .HasForeignKey(l => l.OwnerId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            builder.Entity<Invite>()
                .HasOne(i => i.Lobby)
                .WithMany(l => l.Invites)
                .HasForeignKey(u => u.LobbyId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired(false);
        }
    }
}
