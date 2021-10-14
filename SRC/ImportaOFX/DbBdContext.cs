using ImportaOFX.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ImportaOFX
{
    public class DbBdContext : DbContext
    {
        public DbBdContext(DbContextOptions<DbBdContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Transacao>().HasKey(table => table.Id);
        }

        public DbSet<Transacao> Transacoes { get; set; }
    }
}
