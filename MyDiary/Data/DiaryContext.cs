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

        public string DbPath { get; }

        public DiaryContext()
        {
            var folder = Environment.SpecialFolder.LocalApplicationData;
            var path = Environment.GetFolderPath(folder);

            DbPath = System.IO.Path.Join(path, "diary.db");
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseSqlite($"Data Source={DbPath}");

    }

    public class Diary
    {
        public int DiaryId { get; set; }
        public DateTime DiaryDate { get; set; }
        public required string DiaryText { get; set; }
    }
}
