using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace CsInvite.SteamBot.Models
{
    class BotDbContext: DbContext
    {
        public DbSet<Friend> Friends { get; set; }
        
        public DbSet<User> User { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseMySql(@"Server=localhost;database=csinvite;uid=root;pwd=password;");
        }
    }
}
