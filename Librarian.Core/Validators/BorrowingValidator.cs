using System.Linq;
using System.Threading.Tasks;
using Librarian.Core.Data;
using Librarian.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Librarian.Core.Validators
{
    public class BorrowingValidator
    {
        private readonly LibrarianContext _context;

        public BorrowingValidator(LibrarianContext context)
        {
            _context = context;
        }

        public async Task ValidateBookBorrowingAndThrowAsync(int clientId, int bookId)
        {
            Book book = await _context.Books
                .Include(m => m.Borrowings)
                .SingleOrDefaultAsync(m => m.Id == bookId);

            if (book == null)
                throw new RepositoryOperationException("BookId", "Book not found!");

            if (book.Borrowings.Any(m => m.DateReturned == null))
                throw new RepositoryOperationException("BookId", "The book is already borrowed!");

            Client client = await _context.Clients
                .Include(m => m.Borrowings)
                .ThenInclude(m => m.Book)
                .SingleOrDefaultAsync(m => m.Id == clientId);

            if (client == null)
                throw new RepositoryOperationException("ClientId", "Client not found!");

            if (client.Borrowings
                .Any(m => m.Duration > m.Book.DurationLimit &&
                          m.DateReturned == null))
                throw new RepositoryOperationException("ClientId", "Another borrowing is overdue!");

            int currentBorrowingsCount = client.Borrowings.Count(m => m.DateReturned == null);

            if (currentBorrowingsCount >= 3 && !client.IsUntrustworthy)
                throw new RepositoryOperationException("ClientId", "Borrowing limit exceeded!");

            if (currentBorrowingsCount >= 1 && client.IsUntrustworthy)
                throw new RepositoryOperationException("ClientId", "Untrustworthy borrowing limit exceeded!");

            if (client.Age < book.AgeLimit)
                throw new RepositoryOperationException("ClientId", "Age restriction is not satisfied!");
        }

        public async Task ValidateBookReturningAndThrowAsync(int borrowingId)
        {
            Borrowing borrowing = await _context.Borrowings
                .SingleOrDefaultAsync(m =>
                    m.Id == borrowingId);

            if (borrowing == null)
                throw new RepositoryOperationException("Borrowing not found!");

            if (borrowing.DateReturned != null)
                throw new RepositoryOperationException("Borrowing is already closed!");
        }
    }
}
