using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace MyDiary.Data
{
    public class DiaryContext : DbContext
    {
        public DbSet<Diary> DiaryEntries { get; set; }

        public DbSet<User> Users { get; set; }

        public DiaryContext(DbContextOptions<DiaryContext> options) : base(options)
        { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();
        }

    }
}
