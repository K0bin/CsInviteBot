using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using CsInvite.Shared.Models;
using Microsoft.Extensions.Configuration;

namespace CsInvite.Shared.Models
{
    public class ApplicationDbContext: IdentityDbContext<User>
    {
        public DbSet<Friend> Friends { get; set; }
        public DbSet<Lobby> Lobbies { get; set; }

        private string connectionString;

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IConfiguration configuration)
            : base(options)
        {
            connectionString = configuration.GetConnectionString("MySQL");
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseMySql(connectionString);
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
        }
    }
}
