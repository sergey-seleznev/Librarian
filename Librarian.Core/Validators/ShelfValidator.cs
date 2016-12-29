using System.Threading.Tasks;
using FluentValidation;
using Librarian.Core.Data;
using Librarian.Core.Models;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Threading;

namespace Librarian.Core.Validators
{
    public class ShelfValidator : AbstractValidator<Shelf>
    {
        private readonly LibrarianContext _context;

        public ShelfValidator(LibrarianContext context)
        {
            _context = context;
            
            RuleFor(m => m.Number)
                .GreaterThanOrEqualTo(1)
                .WithMessage("Invalid value");

            RuleFor(m => m.Capacity)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Invalid value");

            RuleFor(m => m.Description)
                .NotEmpty()
                .WithMessage("Invalid value");
            
            RuleFor(m => m.Number)
                .MustAsync(NotDuplicateOtherShelfNumber)
                .WithMessage("Such number already exists");

            When(m => m.Id > 0, () =>
            {
                RuleFor(m => m.Capacity)
                    .MustAsync(HoldAllExistingBooks)
                    .WithMessage("An existing book position exceeds the capacity");
            });
        }

        public async Task ValidateDeleteAndThrowAsync(int id)
        {
            Shelf shelf = await _context.Shelves
                .Include(m => m.Books)
                .ThenInclude(m => m.Borrowings)
                .SingleOrDefaultAsync(m => m.Id == id);

            if (shelf == null)
                throw new RepositoryOperationException("Shelf not found!");

            if (shelf.Books.Any(book => 
                    book.Borrowings.Any(b => 
                        b.DateReturned == null)))
                throw new RepositoryOperationException("Shelf contains currently borrowed books!");
        }

        private async Task<bool> NotDuplicateOtherShelfNumber(Shelf model, int number, CancellationToken cancel)
        {
            return !(await _context.Shelves
                .AnyAsync(m => 
                    m.Number == number && 
                    m.Id != model.Id, 
                        cancel));
        }

        private async Task<bool> HoldAllExistingBooks(Shelf model, int capacity, CancellationToken cancel)
        {
            int maxPosition = await _context.Books
                .Where(m => m.ShelfId == model.Id)
                .Select(m => m.Position)
                .DefaultIfEmpty(0)
                .MaxAsync(cancel);

            return capacity >= maxPosition;
        }
    }
}
