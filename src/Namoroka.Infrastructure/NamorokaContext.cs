using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Namoroka.Infrastructure
{
    public class NamorokaContext : DbContext
    {
        public DbSet<Server> Servers { get; set; }
        public DbSet<Rank> Ranks { get; set; }
        public DbSet<AutoRole> AutoRoles { get; set; }

        private const string _connectionString = "server=localhost;user=root;database=namorokadb;port=3306;Connect Timeout=5;";
        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseMySql(_connectionString, new MySqlServerVersion(new Version(1, 0, 0)));
    }
    
    public class Server
    {
        public ulong Id { get; set; }
        public string Prefix { get; set; }
    }

    public class Rank
    {
        public int Id { get; set; }
        public ulong RoleId { get; set; }
        public ulong ServerId { get; set; }
    }
    
    public class AutoRole
    {
        public int Id { get; set; }
        public ulong RoleId { get; set; }
        public ulong ServerId { get; set; }
        
    }
}