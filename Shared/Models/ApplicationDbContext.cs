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

            /*
            builder.Entity<User>()
                .HasMany(u => u.Friends)
                .WithOne(f => f.User);

            builder.Entity<User>()
                .HasMany(u => u.IsInFriendsListOf)
                .WithOne(f => f.OtherUser);*/

            builder.Entity<Friend>()
                .HasOne(f => f.User)
                .WithMany(u => u.Friends)
                .HasForeignKey(f => f.UserId)
                .HasPrincipalKey(u => u.Id)
                .IsRequired();

            builder.Entity<Friend>()
                .HasOne(f => f.OtherUser)
                .WithMany(u => u.IsInFriendsListOf)
                .HasForeignKey(f => f.OtherUserId)
                .HasPrincipalKey(u => u.Id)
                .IsRequired();
        }
    }
}
