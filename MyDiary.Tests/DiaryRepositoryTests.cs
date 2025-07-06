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
        public async void GetUserPasswordHash_ReturnsCorrectHash()
        {
            var options = new DbContextOptionsBuilder<DiaryContext>()
                .UseInMemoryDatabase("Test_Diarydb")
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
                .UseInMemoryDatabase("Test_Diarydb")
                .Options;

            using (var context = new DiaryContext(options))
            {
                context.DiaryEntries.Add(new Diary
                {
                    DiaryId = 1,
                    DiaryText = "Test"
                });

                context.DiaryEntries.Add(new Diary
                {
                    DiaryId = 2,
                    DiaryText = "Test 2"
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