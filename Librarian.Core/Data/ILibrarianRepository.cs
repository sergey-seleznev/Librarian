using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Librarian.Core.Models;

namespace Librarian.Core.Data
{
    public interface ILibrarianRepository
    {
        #region Books

        Task<IEnumerable<Book>> GetBooksAsync();

        Task<Book> GetBookAsync(int id);

        Task<int> AddBookAsync(Book book);

        Task UpdateBookAsync(Book book);

        Task DeleteBookAsync(int id);

        #endregion

        #region Shelves

        Task<IEnumerable<Shelf>> GetShelvesAsync();

        Task<Shelf> GetShelfAsync(int id);

        Task<int> AddShelfAsync(Shelf shelf);

        Task UpdateShelfAsync(Shelf shelf);

        Task DeleteShelfAsync(int id);

        #endregion

        #region Clients

        Task<IEnumerable<Client>> GetClientsAsync();

        Task<Client> GetClientAsync(int id);

        Task<int> AddClientAsync(Client client);

        Task UpdateClientAsync(Client client);

        Task DeleteClientAsync(int id);

        #endregion

        #region Borrowings

        Task<IEnumerable<Borrowing>> GetActiveBorrowingsAsync();

        Task<Borrowing> GetBorrowingAsync(int id);
        
        Task<int> BorrowBook(int clientId, int bookId, DateTime? date = null);

        Task ReturnBook(int borrowingId);

        #endregion
    }
}
