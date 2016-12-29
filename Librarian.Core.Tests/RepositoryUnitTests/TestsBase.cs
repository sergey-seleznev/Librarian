using System;
using System.Threading;
using System.Threading.Tasks;
using Librarian.Core.Data;
using Librarian.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Librarian.Core.Tests.RepositoryUnitTests
{
    public class TestsBase
    {
        #region Unique indices

        private static int _uniqueIndex;
        private static int GetUniqueIndex()
        {
            return Interlocked.Increment(ref _uniqueIndex);
        }

        #endregion

        #region InMemory test data container

        protected static LibrarianContext CreateTestContext()
        {
            var options = new DbContextOptionsBuilder<LibrarianContext>();
            options.UseInMemoryDatabase("LibrarianTest_" + GetUniqueIndex());
            return new LibrarianContext(options.Options);
        }

        #endregion

        #region Sample item creators

        protected static async Task<int> AddSampleShelfAsync(LibrarianRepository repository, int number = 1, int capacity = 20)
        {
            var shelf = new Shelf
            {
                Number = number,
                Description = "Fine Arts",
                Capacity = capacity
            };

            return await repository.AddShelfAsync(shelf);
        }
        
        protected static async Task<int> AddSampleBookAsync(LibrarianRepository repository, int shelfId, int position = 1, int? ageLimit = null, int? durationLimit = null)
        {
            var book = new Book
            {
                ShelfId = shelfId,
                Position = position,
                Name = "Frederick P. Brooks Jr." + GetUniqueIndex(),
                Title = "The Mythical Man-Month" + GetUniqueIndex(),
                AgeLimit = ageLimit,
                DurationLimit = durationLimit
            };

            return await repository.AddBookAsync(book);
        }
        
        protected static async Task<int> AddSampleClientAsync(LibrarianRepository repository, int age = 40, bool isUntrustworthy = false)
        {
            var client = new Client
            {
                Name = "John Walker " + GetUniqueIndex(),
                Birthdate = DateTime.Today.AddYears(-1 * age),
                IsUntrustworthy = isUntrustworthy
            };

            return await repository.AddClientAsync(client);
        }

        protected static async Task<int> AddSampleBorrowingAsync(LibrarianRepository repository, int clientId, int bookId, DateTime? date = null)
        {
            return await repository.BorrowBook(clientId, bookId, date);
        }

        #endregion
        
    }
}
