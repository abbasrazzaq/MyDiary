using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace MyDiary.Data
{

    public class PagedResult<T>
    {
        public List<T> Items { get; set; } = new();
        public int TotalPages { get; set; }
        public int TotalCount { get; set; }
    }

    public class DiaryRepository
    {
        private readonly DiaryContext _context;

        public DiaryRepository(DiaryContext context)
        {
            _context = context;
        }

        public async Task<PagedResult<DiaryEntryListItem>> GetPagedEntriesAsync(int pageIndex, int pageSize)
        {
            var query = _context.DiaryEntries.OrderByDescending(e => e.DiaryDate);

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var items = await query
                .Skip(pageIndex * pageSize)
                .Take(pageSize)
                .Select(x => new DiaryEntryListItem
                {
                    DiaryId = x.DiaryId,
                    DiaryDate = x.DiaryDate,
                    DiaryText = x.DiaryText
                })
                .ToListAsync();

            return new PagedResult<DiaryEntryListItem>
            {
                Items = items,
                TotalPages = totalPages,
                TotalCount = totalCount
            };
        }

        public async Task<Diary> GetEntryByIdAsync(int id)
        {
            return await Task.Run(() =>
                _context.DiaryEntries.FirstOrDefault(x => x.DiaryId == id)
                );
        }

        public async Task AddEntryAsync(Diary entry)
        {
            _context.DiaryEntries.Add(entry);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateEntryAsync(int diaryId, string diaryEntryTextXaml)
        {
            var entry =  await _context.DiaryEntries.FirstOrDefaultAsync(d => d.DiaryId == diaryId);
            if (entry != null)
            {
                entry.DiaryText = diaryEntryTextXaml;
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteEntryAsync(int id)
        {
            var entry = await _context.DiaryEntries.FirstOrDefaultAsync(d => d.DiaryId == id);
            if(entry != null)
            {
                _context.DiaryEntries.Remove(entry);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<string> GetUserPasswordHash(string username)
        {
            string passwordHash = null;
            var user = await _context.Users.FirstOrDefaultAsync(d => d.Username == username);
            if (user != null)
            {
                passwordHash = user.PasswordHash;
            }

            return passwordHash;
        }

    }
}
