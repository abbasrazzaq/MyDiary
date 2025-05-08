using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace MyDiary.Data
{
    internal class DiaryRepository
    {
        private readonly DiaryContext _context;

        // TODO: Dependency injection
        public DiaryRepository(/*DiaryContext context*/)
        {
            _context = new DiaryContext();
        }

        public async Task<List<DiaryEntryListItem>> GetAllEntriesAsync()
        {
            return await Task.Run(() =>
                _context.DiaryEntries
                .OrderByDescending(b => b.DiaryDate)
                .Select(x => new DiaryEntryListItem
                {
                    DiaryId = x.DiaryId, DiaryDate = x.DiaryDate
                })
                .ToList()
            );
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
    }
}
