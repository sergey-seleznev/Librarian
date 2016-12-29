using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using Librarian.Core.Models;
using Librarian.Core.Validators;
using Microsoft.EntityFrameworkCore;

namespace Librarian.Core.Data
{
    public class LibrarianRepository : ILibrarianRepository
    {
        private readonly LibrarianContext _context;

        public LibrarianRepository(LibrarianContext context)
        {
            _context = context;
        }
     
        #region Books

        public async Task<IEnumerable<Book>> GetBooksAsync()
        {
            return await _context.Books
                .Include(m => m.Shelf)
                .OrderBy(m => m.Name)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Book> GetBookAsync(int id)
        {
            return await _context.Books
                .Include(m => m.Shelf)
                .Include(m => m.Borrowings)
                .SingleOrDefaultAsync(m => m.Id == id);
        }

        public async Task<int> AddBookAsync(Book book)
        {
            var validator = new BookValidator(_context);
            await validator.ValidateAndThrowAsync(book);
            
            _context.Books.Add(book);

            await _context.SaveChangesAsync();

            return book.Id;
        }

        public async Task UpdateBookAsync(Book book)
        {
            var validator = new BookValidator(_context);
            await validator.ValidateAndThrowAsync(book);
            
            _context.Update(book);

            await _context.SaveChangesAsync();
        }

        public async Task DeleteBookAsync(int id)
        {
            var validator = new BookValidator(_context);
            await validator.ValidateDeleteAndThrowAsync(id);
            
            Book book = await _context.Books
               .Include(m => m.Borrowings)
               .SingleOrDefaultAsync(m => m.Id == id);

            _context.Borrowings.RemoveRange(book.Borrowings);
            _context.Books.Remove(book);
            
            await _context.SaveChangesAsync();
        }

        #endregion

        #region Shelves

        public async Task<IEnumerable<Shelf>> GetShelvesAsync()
        {
            return await _context.Shelves
                .OrderBy(m => m.Number)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Shelf> GetShelfAsync(int id)
        {
            return await _context.Shelves
                .SingleOrDefaultAsync(m => m.Id == id);
        }

        public async Task<int> AddShelfAsync(Shelf shelf)
        {
            var validator = new ShelfValidator(_context);
            await validator.ValidateAndThrowAsync(shelf);

            _context.Shelves.Add(shelf);

            await _context.SaveChangesAsync();

            return shelf.Id;
        }

        public async Task UpdateShelfAsync(Shelf shelf)
        {
            var validator = new ShelfValidator(_context);
            await validator.ValidateAndThrowAsync(shelf);
            
            _context.Update(shelf);

            await _context.SaveChangesAsync();
        }

        public async Task DeleteShelfAsync(int id)
        {
            var validator = new ShelfValidator(_context);
            await validator.ValidateDeleteAndThrowAsync(id);
            
            Shelf shelf = await _context.Shelves
                .Include(m => m.Books)
                .ThenInclude(m => m.Borrowings)
                .SingleOrDefaultAsync(m => m.Id == id);
            
            _context.Borrowings.RemoveRange(shelf.Books.SelectMany(b => b.Borrowings));
            _context.Books.RemoveRange(shelf.Books);
            _context.Shelves.Remove(shelf);

            await _context.SaveChangesAsync();
        }

        #endregion

        #region Clients

        public async Task<IEnumerable<Client>> GetClientsAsync()
        {
            return await _context.Clients
                .OrderBy(m => m.Name)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Client> GetClientAsync(int id)
        {
            return await _context.Clients
                .SingleOrDefaultAsync(m => m.Id == id);
        }

        public async Task<int> AddClientAsync(Client client)
        {
            var validator = new ClientValidator(_context);
            await validator.ValidateAndThrowAsync(client);

            _context.Clients.Add(client);
            await _context.SaveChangesAsync();

            return client.Id;
        }

        public async Task UpdateClientAsync(Client client)
        {
            var validator = new ClientValidator(_context);
            await validator.ValidateAndThrowAsync(client);
            
            _context.Update(client);

            await _context.SaveChangesAsync();
        }

        public async Task DeleteClientAsync(int id)
        {
            var validator = new ClientValidator(_context);
            await validator.ValidateDeleteAndThrowAsync(id);
            
            Client client = await _context.Clients
                .Include(m => m.Borrowings)
                .SingleOrDefaultAsync(m => m.Id == id);
            
            _context.Borrowings.RemoveRange(client.Borrowings);
            _context.Clients.Remove(client);

            await _context.SaveChangesAsync();
        }

        #endregion

        #region Borrowings

        public async Task<IEnumerable<Borrowing>> GetActiveBorrowingsAsync()
        {
            return await _context.Borrowings
                .Include(m => m.Book)
                .Include(m => m.Client)
                .Where(m => m.DateReturned == null)
                .OrderBy(m => m.DateBorrowed)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Borrowing> GetBorrowingAsync(int id)
        {
            return await _context.Borrowings
                .Include(m => m.Book)
                .Include(m => m.Client)
                .SingleOrDefaultAsync(m => m.Id == id);
        }

        public async Task<int> BorrowBook(int clientId, int bookId, DateTime? date = null)
        {
            var validator = new BorrowingValidator(_context);
            await validator.ValidateBookBorrowingAndThrowAsync(clientId, bookId);

            Borrowing borrowing = new Borrowing
            {
                BookId = bookId,
                ClientId = clientId,
                DateBorrowed = date ?? DateTime.Now,
                DateReturned = null
            };

            _context.Borrowings.Add(borrowing);

            await _context.SaveChangesAsync();

            return borrowing.Id;
        }

        public async Task ReturnBook(int borrowingId)
        {
            var validator = new BorrowingValidator(_context);
            await validator.ValidateBookReturningAndThrowAsync(borrowingId);

            Borrowing borrowing = await _context.Borrowings
                .Include(m => m.Book)
                .Include(m => m.Client)
                .ThenInclude(m => m.Borrowings)
                .SingleOrDefaultAsync(m =>
                    m.Id == borrowingId);
            
            borrowing.DateReturned = DateTime.Now;
            borrowing.IsOverdue = borrowing.Duration > borrowing.Book.DurationLimit;

            UpdateTrustworthyStatus(borrowing.Client);

            await _context.SaveChangesAsync();
        }

        private void UpdateTrustworthyStatus(Client client)
        {
            if (!client.IsUntrustworthy)
            {
                int overdueCount = client.Borrowings.Count(b => b.IsOverdue == true);
                if (overdueCount >= 3)
                {
                    client.IsUntrustworthy = true;
                }
            }
        }

        #endregion
    }
}
