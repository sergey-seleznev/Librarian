using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Librarian.Core.Data;
using Librarian.Core.Models;
using Xunit;

namespace Librarian.Core.Tests.RepositoryUnitTests
{
    public class BookTests : TestsBase
    {
        [Fact(DisplayName = "Empty books")]
        public async Task EmptyBooksTest()
        {
            using (var context = CreateTestContext())
            {
                var repository = new LibrarianRepository(context);

                IEnumerable<Book> books = await repository.GetBooksAsync();

                Assert.Equal(0, books.Count());
            }
        }

        [Fact(DisplayName = "Get non-existing book")]
        public async Task GetNonExistingBookTest()
        {
            using (var context = CreateTestContext())
            {
                var repository = new LibrarianRepository(context);

                var book = await repository.GetBookAsync(id: 13);

                Assert.Null(book);
            }
        }

        [Fact(DisplayName = "Adding a book")]
        public async Task AddingBookTest()
        {
            using (var context = CreateTestContext())
            {
                var repository = new LibrarianRepository(context);

                int shelfId = await AddSampleShelfAsync(repository);
                int bookId = await AddSampleBookAsync(repository, shelfId);

                var books = await repository.GetBooksAsync();

                Assert.True(bookId > 0);
                Assert.Equal(1, books.Count());
            }
        }

        [Fact(DisplayName = "Adding a book to a non-existent shelf")]
        public async Task AddingBookToNonExistentShelfTest()
        {
            using (var context = CreateTestContext())
            {
                var repository = new LibrarianRepository(context);

                await Assert.ThrowsAnyAsync<Exception>(
                    async () => await AddSampleBookAsync(repository, shelfId: 13));
            }
        }

        [Fact(DisplayName = "Adding a book to a smaller shelf")]
        public async Task AddingBookToASmallerShelfTest()
        {
            using (var context = CreateTestContext())
            {
                var repository = new LibrarianRepository(context);

                int shelfId = await AddSampleShelfAsync(repository, capacity: 10);

                await Assert.ThrowsAnyAsync<Exception>(
                    async () => await AddSampleBookAsync(repository, shelfId, position: 13));
            }
        }

        [Fact(DisplayName = "Book display text")]
        public async Task BookDisplayTextTest()
        {
            using (var context = CreateTestContext())
            {
                var repository = new LibrarianRepository(context);

                var shelfId = await AddSampleShelfAsync(repository);
                var bookId = await AddSampleBookAsync(repository, shelfId);
                var book = await repository.GetBookAsync(bookId);

                Assert.Contains(book.Name, book.DisplayText);
                Assert.Contains(book.Title, book.DisplayText);
            }
        }

        [Fact(DisplayName = "Distinct book identifier code")]
        public void BookIdentifierCodeTest()
        {
            var book = new Book { Position = 1 };
            Assert.Equal("-1", book.IdentifierCode);
        }

        [Fact(DisplayName = "Repository book identifier code")]
        public async Task RepositoryBookIdentifierCodeTest()
        {
            using (var context = CreateTestContext())
            {
                var repository = new LibrarianRepository(context);

                var shelfId = await AddSampleShelfAsync(repository);
                var bookId = await AddSampleBookAsync(repository, shelfId);

                var shelf = await repository.GetShelfAsync(shelfId);
                var book = await repository.GetBookAsync(bookId);

                var expectedIdCode = shelf.Number + "-" + book.Position;

                Assert.Equal(expectedIdCode, book.IdentifierCode);
            }
        }

        [Fact(DisplayName = "Adding a number of books")]
        public async Task AddingBooksTest()
        {
            using (var context = CreateTestContext())
            {
                var repository = new LibrarianRepository(context);

                int shelfId = await AddSampleShelfAsync(repository);

                int count = 3;

                for (int i = 0; i < count; i++)
                    await AddSampleBookAsync(repository, shelfId, i + 1);

                IEnumerable<Book> books = await repository.GetBooksAsync();
                Assert.Equal(count, books.Count());
            }
        }

        [Fact(DisplayName = "Updating a book")]
        public async Task UpdatingBookTest()
        {
            using (var context = CreateTestContext())
            {
                var repository = new LibrarianRepository(context);

                int position = 1;

                int shelfId = await AddSampleShelfAsync(repository);
                int bookId = await AddSampleBookAsync(repository, shelfId, position);

                var book = await repository.GetBookAsync(bookId);
                book.Position++;
                await repository.UpdateBookAsync(book);

                // verify
                book = await repository.GetBookAsync(bookId);
                Assert.Equal(position + 1, book.Position);
            }
        }

        [Fact(DisplayName = "Duplicate book location")]
        public async Task DuplicateBookLocationTest()
        {
            using (var context = CreateTestContext())
            {
                var repository = new LibrarianRepository(context);
                
                int shelfId = await AddSampleShelfAsync(repository);
                await AddSampleBookAsync(repository, shelfId, position: 13);
                
                await Assert.ThrowsAnyAsync<Exception>(
                    async () => await AddSampleBookAsync(repository, shelfId, position: 13));
            }
        }

        [Fact(DisplayName = "Duplicate book position on different shelves")]
        public async Task DuplicatePositionDifferentShelvesTest()
        {
            using (var context = CreateTestContext())
            {
                var repository = new LibrarianRepository(context);
                
                int shelfId1 = await AddSampleShelfAsync(repository);
                int shelfId2 = await AddSampleShelfAsync(repository, 2);

                await AddSampleBookAsync(repository, shelfId1, position: 13);
                await AddSampleBookAsync(repository, shelfId2, position: 13);
            }
        }

        [Fact(DisplayName = "Deleting a book")]
        public async Task DeletingBookTest()
        {
            using (var context = CreateTestContext())
            {
                var repository = new LibrarianRepository(context);

                int shelfId = await AddSampleShelfAsync(repository);
                int bookId = await AddSampleBookAsync(repository, shelfId);
                
                await repository.DeleteBookAsync(bookId);
                
                var books = await repository.GetBooksAsync();
                Assert.Equal(0, books.Count());
            }
        }

        [Fact(DisplayName = "Deleting non-existing book")]
        public async Task DeletingNonExistingBookTest()
        {
            using (var context = CreateTestContext())
            {
                var repository = new LibrarianRepository(context);

                await Assert.ThrowsAnyAsync<Exception>(
                    async () => await repository.DeleteBookAsync(id: 13));
            }
        }

        [Fact(DisplayName = "Update book age limit")]
        public async Task UpdatingBookAgeLimitTest()
        {
            using (var context = CreateTestContext())
            {
                var repository = new LibrarianRepository(context);

                int shelfId = await AddSampleShelfAsync(repository);
                int bookId = await AddSampleBookAsync(repository, shelfId, ageLimit: 21);
                
                // update age limit to 18 years
                var book = await repository.GetBookAsync(bookId);
                book.AgeLimit = 18;

                await repository.UpdateBookAsync(book);
            }
        }

        [Fact(DisplayName = "Update borrowed book age limit exceeded")]
        public async Task UpdatingBorrowedBookAgeLimitExceededTest()
        {
            using (var context = CreateTestContext())
            {
                var repository = new LibrarianRepository(context);

                // 10 years old client borrowed a book
                int shelfId = await AddSampleShelfAsync(repository);
                int bookId = await AddSampleBookAsync(repository, shelfId);
                int clientId = await AddSampleClientAsync(repository, 10);
                await AddSampleBorrowingAsync(repository, clientId, bookId);

                // update age limit to 18 years
                var book = await repository.GetBookAsync(bookId);
                book.AgeLimit = 18;

                await Assert.ThrowsAnyAsync<Exception>(
                    async () => await repository.UpdateBookAsync(book));
            }
        }

        [Fact(DisplayName = "Update borrowed book age limit")]
        public async Task UpdatingBorrowedBookAgeLimitTest()
        {
            using (var context = CreateTestContext())
            {
                var repository = new LibrarianRepository(context);

                // 21 years old client borrowed a book
                int shelfId = await AddSampleShelfAsync(repository);
                int bookId = await AddSampleBookAsync(repository, shelfId);
                int clientId = await AddSampleClientAsync(repository, 21);
                await AddSampleBorrowingAsync(repository, clientId, bookId);

                // update age limit to 18 years
                var book = await repository.GetBookAsync(bookId);
                book.AgeLimit = 18;

                await repository.UpdateBookAsync(book);
            }
        }

        [Fact(DisplayName = "Update book duration limit")]
        public async Task UpdatingBookDurationLimitTest()
        {
            using (var context = CreateTestContext())
            {
                var repository = new LibrarianRepository(context);
                
                int shelfId = await AddSampleShelfAsync(repository);
                int bookId = await AddSampleBookAsync(repository, shelfId, durationLimit: 3);
                
                // update duration limit to 1 day
                var book = await repository.GetBookAsync(bookId);
                book.DurationLimit = 1;

                await repository.UpdateBookAsync(book);
            }
        }

        [Fact(DisplayName = "Update borrowed book duration limit exceeded")]
        public async Task UpdatingBorrowedBookDurationLimitExceededTest()
        {
            using (var context = CreateTestContext())
            {
                var repository = new LibrarianRepository(context);

                // a client borrowed a book 3 days ago
                int shelfId = await AddSampleShelfAsync(repository);
                int bookId = await AddSampleBookAsync(repository, shelfId);
                int clientId = await AddSampleClientAsync(repository);
                await AddSampleBorrowingAsync(repository, clientId, bookId, DateTime.Today.AddDays(-3));

                // update duration limit to 1 day
                var book = await repository.GetBookAsync(bookId);
                book.DurationLimit = 1;
                
                await Assert.ThrowsAnyAsync<Exception>(
                    async () => await repository.UpdateBookAsync(book));
            }
        }

        [Fact(DisplayName = "Update borrowed book duration limit")]
        public async Task UpdatingBorrowedBookDurationLimitTest()
        {
            using (var context = CreateTestContext())
            {
                var repository = new LibrarianRepository(context);

                // a client borrowed a book 3 days ago
                int shelfId = await AddSampleShelfAsync(repository);
                int bookId = await AddSampleBookAsync(repository, shelfId);
                int clientId = await AddSampleClientAsync(repository);
                await AddSampleBorrowingAsync(repository, clientId, bookId, DateTime.Today.AddDays(-3));

                // update duration limit to 7 day
                var book = await repository.GetBookAsync(bookId);
                book.DurationLimit = 7;

                await repository.UpdateBookAsync(book);
            }
        }

        [Fact(DisplayName = "Deleting previously borrowed book")]
        public async Task DeletingPreviouslyBorrowedBookTest()
        {
            using (var context = CreateTestContext())
            {
                var repository = new LibrarianRepository(context);

                int shelfId = await AddSampleShelfAsync(repository);
                int bookId = await AddSampleBookAsync(repository, shelfId);
                int clientId = await AddSampleClientAsync(repository);

                int borrowingId = await AddSampleBorrowingAsync(repository, clientId, bookId);
                await repository.ReturnBook(borrowingId);
                
                await repository.DeleteBookAsync(bookId);

                var borrowings = await repository.GetActiveBorrowingsAsync();
                var books = await repository.GetBooksAsync();

                Assert.Equal(0, borrowings.Count());
                Assert.Equal(0, books.Count());
            }
        }

        [Fact(DisplayName = "Deleting currently borrowed book")]
        public async Task DeletingCurrenltyBorrowedBookTest()
        {
            using (var context = CreateTestContext())
            {
                var repository = new LibrarianRepository(context);

                int shelfId = await AddSampleShelfAsync(repository);
                int bookId = await AddSampleBookAsync(repository, shelfId);
                int clientId = await AddSampleClientAsync(repository);

                await AddSampleBorrowingAsync(repository, clientId, bookId);

                await Assert.ThrowsAnyAsync<Exception>(
                    async () => await repository.DeleteBookAsync(bookId));
            }
        }
    }
}
