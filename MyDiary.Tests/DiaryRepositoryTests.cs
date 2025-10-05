using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using MyDiary.Data;

namespace MyDiary.Tests
{
    public class DiaryRepositoryTests
    {
        /*
         * TODO Tests for
         *   - GetPagedEntriesAsync with search filer
         * */

        [Fact]
        public async void GetPagedEntriesAsync_ReturnsCorrectEntries()
        {
            var options = new DbContextOptionsBuilder<DiaryContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            using var context = new DiaryContext(options);
            var diaryEntries = new List<Diary>
            {
                new Diary { DiaryId = 1, DiaryDate = DateTime.Today,                DiaryText = "Entry 1", DiaryTextPlain = "Entry 1" },
                new Diary { DiaryId = 2, DiaryDate = DateTime.Today.AddDays(-1),    DiaryText = "Entry 2", DiaryTextPlain = "Entry 2" },
                new Diary { DiaryId = 3, DiaryDate = DateTime.Today.AddDays(-2),    DiaryText = "Entry 3", DiaryTextPlain = "Entry 3" },
                new Diary { DiaryId = 4, DiaryDate = DateTime.Today.AddDays(-3),    DiaryText = "Entry 4", DiaryTextPlain = "Entry 4" },
            };
            context.DiaryEntries.AddRange(diaryEntries);
            await context.SaveChangesAsync();

            var repository = new DiaryRepository(context);

            // Act
            var page1 = await repository.GetPagedEntriesAsync(0, 2, string.Empty);
            var page2 = await repository.GetPagedEntriesAsync(1, 2, string.Empty);

            // Assert
            Assert.Equal(diaryEntries.GetRange(0, 2).Select(e => e.DiaryId), page1.Items.Select(e => e.DiaryId));
            Assert.Equal(diaryEntries.GetRange(2, 2).Select(e => e.DiaryId), page2.Items.Select(e => e.DiaryId));
        }

        [Fact]
        public async void UpdateEntryAsync_UpdatesEntryCorrectly()
        {
            var options = new DbContextOptionsBuilder<DiaryContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            using var context = new DiaryContext(options);

            var diary = new Diary
            {
                DiaryId = 1,
                DiaryDate = DateTime.Today,
                DiaryText = "Original XAML",
                DiaryTextPlain = "Original Plain"
            };
            context.DiaryEntries.Add(diary);
            await context.SaveChangesAsync();

            var repository = new DiaryRepository(context);

            // Act
            string newPlainText = "Updated Plain";
            string newXamlText = "Updated XAML";
            await repository.UpdateEntryAsync(diary.DiaryId, newPlainText, newXamlText);

            // Assert
            var updateDiary = await context.DiaryEntries.FirstOrDefaultAsync(d => d.DiaryId == diary.DiaryId);
            Assert.NotNull(updateDiary);
            Assert.Equal(newPlainText, updateDiary.DiaryTextPlain);
            Assert.Equal(newXamlText, updateDiary.DiaryText);
        }

        [Fact]
        public async void GetUserPasswordHash_ReturnsCorrectHash()
        {
            var options = new DbContextOptionsBuilder<DiaryContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            using (var context = new DiaryContext(options))
            {
                context.Users.Add(new User
                {
                    Username = "testuser",
                    PasswordHash = "hashedpassword"
                });
                context.SaveChanges();
            }

            using(var context = new DiaryContext(options))
            {
                var repository = new DiaryRepository(context);

                var result = await repository.GetUserPasswordHash("testuser");

                Assert.Equal("hashedpassword", result);
            }
        }

        [Fact]
        public async void DeleteEntryAsync_DeletesCorrectEntry()
        {
            var options = new DbContextOptionsBuilder<DiaryContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            using (var context = new DiaryContext(options))
            {
                context.DiaryEntries.Add(new Diary
                {
                    DiaryId = 1,
                    DiaryText = "Test",
                    DiaryTextPlain = "Test"
                });

                context.DiaryEntries.Add(new Diary
                {
                    DiaryId = 2,
                    DiaryText = "Test 2",
                    DiaryTextPlain = "Test 2"
                });

                context.SaveChanges();
            }

            using(var context = new DiaryContext(options))
            {
                var repository = new DiaryRepository(context);

                await repository.DeleteEntryAsync(2);

                Assert.Equal(1, context.DiaryEntries.Count());

                Assert.Equal(1, context.DiaryEntries.First().DiaryId);
            }
        }
    }
}