﻿using CodeNames.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CodeNames.Data
{
    public class AppDbContext : IdentityDbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        
        }

        public DbSet<GameRoom> GameRooms { get; set; }

        public DbSet<LiveGameSession> LiveGameSession { get; set; }
    }
}
