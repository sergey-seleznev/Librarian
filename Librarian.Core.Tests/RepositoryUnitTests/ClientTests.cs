using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Librarian.Core.Data;
using Librarian.Core.Models;
using Xunit;

namespace Librarian.Core.Tests.RepositoryUnitTests
{
    public class ClientTests : TestsBase
    {
        [Fact(DisplayName = "Empty clients")]
        public async Task EmptyClientsTest()
        {
            using (var context = CreateTestContext())
            {
                var repository = new LibrarianRepository(context);

                IEnumerable<Client> clients = await repository.GetClientsAsync();

                Assert.Equal(0, clients.Count());
            }
        }

        [Fact(DisplayName = "Get non-existing client")]
        public async Task GetNonExistingClientTest()
        {
            using (var context = CreateTestContext())
            {
                var repository = new LibrarianRepository(context);

                var client = await repository.GetClientAsync(id: 13);

                Assert.Null(client);
            }
        }

        [Fact(DisplayName = "Adding a client")]
        public async Task AddingClientTest()
        {
            using (var context = CreateTestContext())
            {
                var repository = new LibrarianRepository(context);

                int clientId = await AddSampleClientAsync(repository);

                var clients = await repository.GetClientsAsync();

                Assert.True(clientId > 0);
                Assert.Equal(1, clients.Count());
            }
        }

        [Fact(DisplayName = "Adding a number of clients")]
        public async Task AddingClientsTest()
        {
            using (var context = CreateTestContext())
            {
                var repository = new LibrarianRepository(context);
                
                int count = 3;

                for (int i = 0; i < count; i++)
                    await AddSampleClientAsync(repository);

                IEnumerable<Client> clients = await repository.GetClientsAsync();
                Assert.Equal(count, clients.Count());
            }
        }

        [Fact(DisplayName = "Updating a client")]
        public async Task UpdatingClientTest()
        {
            using (var context = CreateTestContext())
            {
                var repository = new LibrarianRepository(context);

                int age = 40;

                int clientId = await AddSampleClientAsync(repository, age);

                var client = await repository.GetClientAsync(clientId);
                client.Birthdate = client.Birthdate.AddYears(-1);
                await repository.UpdateClientAsync(client);

                // verify
                client = await repository.GetClientAsync(clientId);
                Assert.Equal(age + 1, client.Age);
            }
        }

        [Fact(DisplayName = "Duplicate client name")]
        public async Task DuplicateClientNameTest()
        {
            using (var context = CreateTestContext())
            {
                var repository = new LibrarianRepository(context);

                int clientId1 = await AddSampleClientAsync(repository);
                int clientId2 = await AddSampleClientAsync(repository);

                var client1 = await repository.GetClientAsync(clientId1);
                var client2 = await repository.GetClientAsync(clientId2);

                client2.Name = client1.Name;

                await Assert.ThrowsAnyAsync<Exception>(
                    async () => await repository.UpdateClientAsync(client2));
            }
        }

        [Fact(DisplayName = "Deleting a client")]
        public async Task DeletingClientTest()
        {
            using (var context = CreateTestContext())
            {
                var repository = new LibrarianRepository(context);
                
                int clientId = await AddSampleClientAsync(repository);

                await repository.DeleteClientAsync(clientId);

                var clients = await repository.GetClientsAsync();
                Assert.Equal(0, clients.Count());
            }
        }

        [Fact(DisplayName = "Deleting non-existing client")]
        public async Task DeletingNonExistingClientTest()
        {
            using (var context = CreateTestContext())
            {
                var repository = new LibrarianRepository(context);

                await Assert.ThrowsAnyAsync<Exception>(
                    async () => await repository.DeleteClientAsync(id: 13));
            }
        }

        [Fact(DisplayName = "Update borrowed client age limit")]
        public async Task UpdatingClientBirthdayBorrowedAgeLimitBookTest()
        {
            using (var context = CreateTestContext())
            {
                var repository = new LibrarianRepository(context);

                // 21 years old client borrowed a 18+ limit book
                int shelfId = await AddSampleShelfAsync(repository);
                int bookId = await AddSampleBookAsync(repository, shelfId, ageLimit: 18);
                int clientId = await AddSampleClientAsync(repository, age: 21);
                await AddSampleBorrowingAsync(repository, clientId, bookId);

                // update birthday, so that the age is 16 years
                var client = await repository.GetClientAsync(clientId);
                client.Birthdate = DateTime.Now.AddYears(-16);

                await Assert.ThrowsAnyAsync<Exception>(
                    async () => await repository.UpdateClientAsync(client));
            }
        }
        
        [Fact(DisplayName = "Deleting client previously borrowed a book")]
        public async Task DeletingPreviouslyBorrowingClientTest()
        {
            using (var context = CreateTestContext())
            {
                var repository = new LibrarianRepository(context);

                int shelfId = await AddSampleShelfAsync(repository);
                int bookId = await AddSampleBookAsync(repository, shelfId);
                int clientId = await AddSampleClientAsync(repository);

                int borrowingId = await AddSampleBorrowingAsync(repository, clientId, bookId);
                await repository.ReturnBook(borrowingId);

                await repository.DeleteClientAsync(clientId);

                var borrowings = await repository.GetActiveBorrowingsAsync();
                var clients = await repository.GetClientsAsync();

                Assert.Equal(0, borrowings.Count());
                Assert.Equal(0, clients.Count());
            }
        }

        [Fact(DisplayName = "Deleting client currently borrowed a book")]
        public async Task DeletingCurrenltyBorrowingClientTest()
        {
            using (var context = CreateTestContext())
            {
                var repository = new LibrarianRepository(context);

                int shelfId = await AddSampleShelfAsync(repository);
                int bookId = await AddSampleBookAsync(repository, shelfId);
                int clientId = await AddSampleClientAsync(repository);

                await AddSampleBorrowingAsync(repository, clientId, bookId);

                await Assert.ThrowsAnyAsync<Exception>(
                    async () => await repository.DeleteClientAsync(clientId));
            }
        }
    }
}
