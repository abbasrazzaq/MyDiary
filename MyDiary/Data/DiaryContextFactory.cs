using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace MyDiary.Data
{
    public class DiaryContextFactory : IDesignTimeDbContextFactory<DiaryContext>
    {
        public DiaryContext CreateDbContext(string[] args)
        {
            var folder = Environment.SpecialFolder.LocalApplicationData;
            var path = System.IO.Path.Combine(Environment.GetFolderPath(folder), "diary.db");

            var builder = new DbContextOptionsBuilder<DiaryContext>();
            builder.UseSqlite($"Data Source={path}"); 
            return new DiaryContext(builder.Options);
        }
    }
}
