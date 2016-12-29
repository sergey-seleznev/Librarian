using System;
using System.Threading.Tasks;
using FluentValidation;
using Librarian.Core.Data;
using Librarian.Core.Models;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Threading;

namespace Librarian.Core.Validators
{
    public class ClientValidator : AbstractValidator<Client>
    {
        private readonly LibrarianContext _context;

        public ClientValidator(LibrarianContext context)
        {
            _context = context;
            
            RuleFor(m => m.Name)
                .MustAsync(NotDuplicateOtherClientName)
                .WithMessage("Such a client already exists");

            When(m => m.Id > 0, () =>
            {
                RuleFor(m => m.Birthdate)
                    .MustAsync(ConformCurrentBorrowingsAgeLimits)
                    .WithMessage("Age not satisfies current borrowed book limit");
            });
        }

        public async Task ValidateDeleteAndThrowAsync(int id)
        {
            Client client = await _context.Clients
                .Include(m => m.Borrowings)
                .SingleOrDefaultAsync(m => m.Id == id);

            if (client == null)
                throw new RepositoryOperationException("Client not found!");

            if (client.Borrowings.Any(m => m.DateReturned == null))
                throw new RepositoryOperationException("Client has active borrowings!");
        }

        private async Task<bool> NotDuplicateOtherClientName(Client model, string name, CancellationToken cancel)
        {
            return !(await _context.Clients
                .AnyAsync(m => 
                    m.Name == name && 
                    m.Id != model.Id,
                        cancel));
        }

        private async Task<bool> ConformCurrentBorrowingsAgeLimits(Client model, DateTime birthday, CancellationToken cancel)
        {
            int? minimumBorrowedBookAgeLimit = await _context.Borrowings
                    .Where(m => m.ClientId == model.Id &&
                                m.DateReturned == null)
                    .Select(m => m.Book.AgeLimit)
                    .DefaultIfEmpty(0)
                    .MinAsync(cancel);

            return model.Age >= minimumBorrowedBookAgeLimit;
        }
    }
}
