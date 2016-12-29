using FluentValidation;
using Librarian.Core.Data;
using Librarian.Core.Models;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace Librarian.Core.Validators
{
    public class BookValidator : AbstractValidator<Book>
    {
        private readonly LibrarianContext _context;

        public BookValidator(LibrarianContext context)
        {
            _context = context;

            RuleFor(m => m.Position)
                .GreaterThanOrEqualTo(1)
                .WithMessage("Invalid value");

            RuleFor(m => m.Position)
                .MustAsync(FitIntoTheShelf)
                .WithMessage("Position exceeds shelf capacity");

            RuleFor(m => m.Position)
                .MustAsync(NotCollideWithOtherBook)
                .WithMessage("There's already a book there");

            RuleFor(m => m.Title)
                .MustAsync(NotBeDuplicate)
                .WithMessage("There's already such a book");

            When(m => m.Id > 0, () =>
            {
                RuleFor(m => m.AgeLimit)
                   .MustAsync(CorrespondToActiveBorrowerAge)
                   .WithMessage("The book is currently borrowed by a younger client");

                RuleFor(m => m.DurationLimit)
                    .MustAsync(CorrespondToActiveBorrowingDuration)
                    .WithMessage("The book is currently borrowed longer");
            });
        }

        public async Task ValidateDeleteAndThrowAsync(int id)
        {
            Book book = await _context.Books
               .Include(m => m.Borrowings)
               .SingleOrDefaultAsync(m => m.Id == id);

            if (book == null)
                throw new RepositoryOperationException("Book not found!");

            if (book.Borrowings.Any(m => m.DateReturned == null))
                throw new RepositoryOperationException("Can't delete the currently borrowed book!");
        }

        private async Task<bool> FitIntoTheShelf(Book model, int position, CancellationToken cancel)
        {
            var shelf = await _context.Shelves
                .SingleOrDefaultAsync(m => 
                    m.Id == model.ShelfId, cancel);

            if (shelf == null)
                return false;

            if (position > shelf.Capacity)
                return false;

            return true;
        }

        private async Task<bool> NotCollideWithOtherBook(Book model, int position, CancellationToken cancel)
        {
            var otherBook = await _context.Books
                .FirstOrDefaultAsync(m => 
                    m.ShelfId == model.ShelfId &&
                    m.Position == position &&
                    m.Id != model.Id, cancel);

            return otherBook == null;
        }

        private async Task<bool> NotBeDuplicate(Book model, string title, CancellationToken cancel)
        {
            var otherBook = await _context.Books
                .FirstOrDefaultAsync(m =>
                    m.Title == title &&
                    m.Name == model.Name &&
                    m.Id != model.Id, cancel);

            return otherBook == null;
        }

        private async Task<bool> CorrespondToActiveBorrowerAge(Book model, int? ageLimit, CancellationToken cancel)
        {
            if (ageLimit == null)
                return true;

            var activeBorrowing = await _context.Borrowings
                .Where(m => m.BookId == model.Id &&
                            m.DateReturned == null)
                .Include(m => m.Client)
                .FirstOrDefaultAsync(cancel);

            if (activeBorrowing == null)
                return true;

            return ageLimit < activeBorrowing.Client.Age;
        }

        private async Task<bool> CorrespondToActiveBorrowingDuration(Book model, int? durationLimit, CancellationToken cancel)
        {
            if (durationLimit == null)
                return true;

            var activeBorrowing = await _context.Borrowings
                .Where(m => m.BookId == model.Id &&
                            m.DateReturned == null)
                .FirstOrDefaultAsync(cancel);

            if (activeBorrowing == null)
                return true;

            return durationLimit >= activeBorrowing.Duration;
        }

    }
}
