using System;
using System.Linq;
using System.Threading.Tasks;
using Librarian.Core.Data;
using Xunit;

namespace Librarian.Core.Tests.RepositoryUnitTests
{
    public class BorrowingTests : TestsBase
    {
        [Fact(DisplayName = "Non-existent book and client borrowing")]
        public async Task NonExistentBorrowingTest()
        {
            using (var context = CreateTestContext())
            {
                var repository = new LibrarianRepository(context);

                await Assert.ThrowsAnyAsync<Exception>(
                    async () => await repository.BorrowBook(clientId: 1, bookId: 1));
            }
        }

        [Fact(DisplayName = "Non-existent book borrowing")]
        public async Task NonExistentBookBorrowingTest()
        {
            using (var context = CreateTestContext())
            {
                var repository = new LibrarianRepository(context);

                int clientId = await AddSampleClientAsync(repository);

                await Assert.ThrowsAnyAsync<Exception>(
                    async () => await repository.BorrowBook(clientId, bookId: 1));
            }
        }

        [Fact(DisplayName = "Non-existent client borrowing")]
        public async Task NonExistentClientBorrowingTest()
        {
            using (var context = CreateTestContext())
            {
                var repository = new LibrarianRepository(context);

                int shelfId = await AddSampleShelfAsync(repository);
                int bookId = await AddSampleBookAsync(repository, shelfId);

                await Assert.ThrowsAnyAsync<Exception>(
                    async () => await repository.BorrowBook(clientId: 1, bookId: bookId));
            }
        }

        [Fact(DisplayName = "Book borrowing")]
        public async Task BookBorrowingTest()
        {
            using (var context = CreateTestContext())
            {
                var repository = new LibrarianRepository(context);

                int shelfId = await AddSampleShelfAsync(repository);
                int bookId = await AddSampleBookAsync(repository, shelfId);
                int clientId = await AddSampleClientAsync(repository);

                int borrowingId = await repository.BorrowBook(clientId: clientId, bookId: bookId);
                
                var borrowings = await repository.GetActiveBorrowingsAsync();
                var borrowing = await repository.GetBorrowingAsync(borrowingId);

                Assert.Equal(1, borrowings.Count());
                Assert.NotNull(borrowing);
            }
        }

        [Fact(DisplayName = "Close book borrowing")]
        public async Task CloseBorrowingTest()
        {
            using (var context = CreateTestContext())
            {
                var repository = new LibrarianRepository(context);

                int shelfId = await AddSampleShelfAsync(repository);
                int bookId = await AddSampleBookAsync(repository, shelfId);
                int clientId = await AddSampleClientAsync(repository);

                int borrowingId = await repository.BorrowBook(clientId: clientId, bookId: bookId);
                await repository.ReturnBook(borrowingId);

                var borrowings = await repository.GetActiveBorrowingsAsync();
                Assert.Equal(0, borrowings.Count());
            }
        }

        [Fact(DisplayName = "Close already closed book borrowing")]
        public async Task CloseClosedBorrowingTest()
        {
            using (var context = CreateTestContext())
            {
                var repository = new LibrarianRepository(context);

                int shelfId = await AddSampleShelfAsync(repository);
                int bookId = await AddSampleBookAsync(repository, shelfId);
                int clientId = await AddSampleClientAsync(repository);

                int borrowingId = await repository.BorrowBook(clientId: clientId, bookId: bookId);
                await repository.ReturnBook(borrowingId);

                await Assert.ThrowsAnyAsync<Exception>(
                    async () => await repository.ReturnBook(borrowingId));
            }
        }

        [Fact(DisplayName = "Close non-existing borrowing")]
        public async Task CloseNonExistingBorrowingTest()
        {
            using (var context = CreateTestContext())
            {
                var repository = new LibrarianRepository(context);
                
                await Assert.ThrowsAnyAsync<Exception>(
                    async () => await repository.ReturnBook(borrowingId: 13));
            }
        }

        [Fact(DisplayName = "Borrowed book borrowing")]
        public async Task BorrowedBookBorrowingTest()
        {
            using (var context = CreateTestContext())
            {
                var repository = new LibrarianRepository(context);

                int shelfId = await AddSampleShelfAsync(repository);
                int bookId = await AddSampleBookAsync(repository, shelfId);
                int clientId1 = await AddSampleClientAsync(repository);
                int clientId2 = await AddSampleClientAsync(repository);

                await repository.BorrowBook(clientId1, bookId);

                await Assert.ThrowsAnyAsync<Exception>(
                    async () => await repository.BorrowBook(clientId2, bookId));
            }
        }

        [Fact(DisplayName = "Borrowing book while overdue")]
        public async Task BorrowingBookWhileOverdueTest()
        {
            using (var context = CreateTestContext())
            {
                var repository = new LibrarianRepository(context);

                int shelfId = await AddSampleShelfAsync(repository);
                int bookId1 = await AddSampleBookAsync(repository, shelfId, 1, durationLimit: 1);
                int bookId2 = await AddSampleBookAsync(repository, shelfId, 2);
                int clientId = await AddSampleClientAsync(repository);

                // 3 days ago borrowed a 1 day duration limited book
                await repository.BorrowBook(clientId, bookId1, DateTime.Today.AddDays(-3));

                // try borrowing another one
                await Assert.ThrowsAnyAsync<Exception>(
                    async () => await repository.BorrowBook(clientId, bookId2));
            }
        }

        [Fact(DisplayName = "Get non-existing borrowing")]
        public async Task GetNonExistingBorrowingTest()
        {
            using (var context = CreateTestContext())
            {
                var repository = new LibrarianRepository(context);
                
                var borrowing = await repository.GetBorrowingAsync(id: 13);

                Assert.Null(borrowing);
            }
        }

        [Fact(DisplayName = "Exceeding trustworthy borrowing limit")]
        public async Task ExceedingTrustworthyBorrowingLimitTest()
        {
            using (var context = CreateTestContext())
            {
                var repository = new LibrarianRepository(context);

                int shelfId = await AddSampleShelfAsync(repository);
                int clientId = await AddSampleClientAsync(repository);
                int bookId;

                // borrow three books
                for (int i = 0; i < 3; i++)
                {
                    bookId = await AddSampleBookAsync(repository, shelfId, i + 1);
                    await repository.BorrowBook(clientId, bookId);
                }

                // try to borrow one more
                bookId = await AddSampleBookAsync(repository, shelfId, 4);
                await Assert.ThrowsAnyAsync<Exception>(
                    async () => await repository.BorrowBook(clientId, bookId));
            }
        }

        [Fact(DisplayName = "Exceeding untrustworthy borrowing limit")]
        public async Task ExceedingUntrustworthyBorrowingLimitTest()
        {
            using (var context = CreateTestContext())
            {
                var repository = new LibrarianRepository(context);

                int shelfId = await AddSampleShelfAsync(repository);
                int clientId = await AddSampleClientAsync(repository, isUntrustworthy: true);

                // borrow a book
                int bookId = await AddSampleBookAsync(repository, shelfId);
                await repository.BorrowBook(clientId, bookId);

                // try to borrow one more
                bookId = await AddSampleBookAsync(repository, shelfId, 2);
                await Assert.ThrowsAnyAsync<Exception>(
                    async () => await repository.BorrowBook(clientId, bookId));
            }
        }

        
        [Fact(DisplayName = "Exceeding age limit")]
        public async Task ExceedingAgeLimitTest()
        {
            using (var context = CreateTestContext())
            {
                var repository = new LibrarianRepository(context);

                int shelfId = await AddSampleShelfAsync(repository);
                int bookId = await AddSampleBookAsync(repository, shelfId, ageLimit: 16);
                int clientId = await AddSampleClientAsync(repository, age: 12);
                
                await Assert.ThrowsAnyAsync<Exception>(
                    async () => await repository.BorrowBook(clientId, bookId));
            }
        }

        [Fact(DisplayName = "Age limit")]
        public async Task AgeLimitTest()
        {
            using (var context = CreateTestContext())
            {
                var repository = new LibrarianRepository(context);

                int shelfId = await AddSampleShelfAsync(repository);
                int bookId = await AddSampleBookAsync(repository, shelfId, ageLimit: 16);
                int clientId = await AddSampleClientAsync(repository, age: 18);

                await repository.BorrowBook(clientId, bookId);
            }
        }

        [Fact(DisplayName = "Updating untrustworthy status")]
        public async Task UpdatingUntrustworthyStatusTest()
        {
            using (var context = CreateTestContext())
            {
                var repository = new LibrarianRepository(context);

                int shelfId = await AddSampleShelfAsync(repository);
                int clientId = await AddSampleClientAsync(repository);
                
                // borrow, overdue and return three books
                for (int i = 0; i < 3; i++)
                {
                    int bookId = await AddSampleBookAsync(repository, shelfId, i + 1, durationLimit: 3);
                    int borrowingId = await repository.BorrowBook(clientId, bookId, DateTime.Today.AddDays(-7));
                    await repository.ReturnBook(borrowingId);
                }

                var client = await repository.GetClientAsync(clientId);

                Assert.True(client.IsUntrustworthy);
            }
        }

    }
}
