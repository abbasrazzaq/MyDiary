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


        public string DbPath { get; }

        public DiaryContext(DbContextOptions<DiaryContext> options) : base(options)
        {
            //var folder = Environment.SpecialFolder.LocalApplicationData;
            //var path = Environment.GetFolderPath(folder);

            //DbPath = System.IO.Path.Join(path, "diary.db");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();
        }

    }
}
