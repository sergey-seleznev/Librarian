using System;
using System.Linq;
using System.Threading.Tasks;
using Librarian.Core.Data;
using Xunit;

namespace Librarian.Core.Tests.RepositoryUnitTests
{
    public class ShelfTests : TestsBase
    {
        [Fact(DisplayName = "Empty shelves")]
        public async Task EmptyShelvesTest()
        {
            using (var context = CreateTestContext())
            {
                var repository = new LibrarianRepository(context);

                var shelves = await repository.GetShelvesAsync();
                Assert.Equal(0, shelves.Count());
            }
        }

        [Fact(DisplayName = "Get non-existing borrowing")]
        public async Task GetNonExistingShelfTest()
        {
            using (var context = CreateTestContext())
            {
                var repository = new LibrarianRepository(context);

                var shelf = await repository.GetShelfAsync(id: 13);

                Assert.Null(shelf);
            }
        }

        [Fact(DisplayName = "Adding a shelf")]
        public async Task AddingShelfTest()
        {
            using (var context = CreateTestContext())
            {
                var repository = new LibrarianRepository(context);

                var id = await AddSampleShelfAsync(repository);

                Assert.True(id > 0);

                var _shelf = await repository.GetShelfAsync(id);
                Assert.NotNull(_shelf);

                var shelves = (await repository.GetShelvesAsync()).ToList();
                Assert.NotNull(shelves);
                Assert.Equal(1, shelves.Count);
                Assert.Equal(id, shelves[0].Id);
            }
        }

        [Fact(DisplayName = "Shelf display text")]
        public async Task ShelfDisplayTextTest()
        {
            using (var context = CreateTestContext())
            {
                var repository = new LibrarianRepository(context);

                var id = await AddSampleShelfAsync(repository);
                var shelf = await repository.GetShelfAsync(id);
                
                Assert.Contains(shelf.Number.ToString(), shelf.DisplayText);
                Assert.Contains(shelf.Description, shelf.DisplayText);
            }
        }

        [Fact(DisplayName = "Adding a number of shelves")]
        public async Task AddingShelvesTest()
        {
            using (var context = CreateTestContext())
            {
                var repository = new LibrarianRepository(context);

                var count = 3;

                for (int i = 0; i < count; i++)
                    await AddSampleShelfAsync(repository, i + 1);
            
                var shelves = await repository.GetShelvesAsync();
                Assert.Equal(count, shelves.Count());
            }
        }
        
        [Fact(DisplayName = "Updating a shelf")]
        public async Task UpdatingShelfTest()
        {
            using (var context = CreateTestContext())
            {
                var repository = new LibrarianRepository(context);
                
                // create
                var number = 1;
                var id = await AddSampleShelfAsync(repository, number);

                // update
                var shelf = await repository.GetShelfAsync(id);
                shelf.Number++;
                await repository.UpdateShelfAsync(shelf);

                // verify
                shelf = await repository.GetShelfAsync(id);
                Assert.Equal(number + 1, shelf.Number);
            }
        }

        [Fact(DisplayName = "Duplicate shelf numbers")]
        public async Task DuplicateShelfNumberTest()
        {
            using (var context = CreateTestContext())
            {
                var repository = new LibrarianRepository(context);
                
                await AddSampleShelfAsync(repository, 13);

                await Assert.ThrowsAnyAsync<Exception>(
                    async () => await AddSampleShelfAsync(repository, 13));
            }
        }

        [Fact(DisplayName = "Deleting an empty shelf")]
        public async Task DeletingEmptyShelfTest()
        {
            using (var context = CreateTestContext())
            {
                var repository = new LibrarianRepository(context);

                int id = await AddSampleShelfAsync(repository);
                await repository.DeleteShelfAsync(id);

                var shelves = await repository.GetShelvesAsync();
                Assert.Equal(0, shelves.Count());
            }
        }

        [Fact(DisplayName = "Deleting non-existent shelf")]
        public async Task DeletingNonExistentShelfTest()
        {
            using (var context = CreateTestContext())
            {
                var repository = new LibrarianRepository(context);

                await Assert.ThrowsAsync<RepositoryOperationException>(
                    async () => await repository.DeleteShelfAsync(13));
            }
        }

        [Fact(DisplayName = "Deleting non-empty shelf")]
        public async Task DeletingNonEmptyShelfTest()
        {
            using (var context = CreateTestContext())
            {
                var repository = new LibrarianRepository(context);

                int shelfId = await AddSampleShelfAsync(repository);

                await AddSampleBookAsync(repository, shelfId);
                await AddSampleBookAsync(repository, shelfId, 2);
                await AddSampleBookAsync(repository, shelfId, 3);

                await repository.DeleteShelfAsync(shelfId);

                var shelves = await repository.GetShelvesAsync();
                var books = await repository.GetBooksAsync();

                Assert.Equal(0, shelves.Count());
                Assert.Equal(0, books.Count());
            }
        }

        [Fact(DisplayName = "Deleting non-empty shelf with a borrowed book")]
        public async Task DeletingNonEmptyBorrowedBookShelfTest()
        {
            using (var context = CreateTestContext())
            {
                var repository = new LibrarianRepository(context);

                var clientId = await AddSampleClientAsync(repository);
                var shelfId = await AddSampleShelfAsync(repository);

                await AddSampleBookAsync(repository, shelfId);
                await AddSampleBookAsync(repository, shelfId, 2);
                var bookId = await AddSampleBookAsync(repository, shelfId, 3);

                await AddSampleBorrowingAsync(repository, clientId, bookId);

                await Assert.ThrowsAsync<RepositoryOperationException>(
                    async () => await repository.DeleteShelfAsync(shelfId));
            }
        }
    }
}
