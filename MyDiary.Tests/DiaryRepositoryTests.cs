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

        [Fact]
        public async void UpdateEntryAsync_UpdatesEntryCorrectly()
        {
            var options = new DbContextOptionsBuilder<DiaryContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var context = new DiaryContext(options);

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