using System.Threading.Tasks;
using FluentValidation;
using Librarian.Core.Data;
using Librarian.Core.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using NUglify.Helpers;

namespace Librarian.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILibrarianRepository _repository;

        public HomeController(ILibrarianRepository repository)
        {
            _repository = repository;
        }

        // GET: Home/
        public async Task<IActionResult> Index()
        {
            return View(await _repository.GetActiveBorrowingsAsync());
        }


        // GET: Home/Borrow/
        public async Task<IActionResult> Borrow(int? clientId, int? bookId)
        {
            ViewData["ClientId"] = new SelectList(await _repository.GetClientsAsync(), "Id", "Name", clientId);
            ViewData["BookId"] = new SelectList(await _repository.GetBooksAsync(), "Id", "DisplayText", bookId);

            return View();
        }

        // POST: Home/Borrow
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Borrow([Bind("BookId, ClientId")] Borrowing borrowing)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _repository.BorrowBook(borrowing.ClientId, borrowing.BookId);
                    return RedirectToAction("Index");
                }
                catch (RepositoryOperationException ex)
                {
                    ModelState.AddModelError(ex.Key, ex.Message);
                }
                catch (ValidationException ex)
                {
                    ex.Errors.ForEach(e =>
                        ModelState.AddModelError(e.PropertyName, e.ErrorMessage));
                }
            }

            ViewData["ClientId"] = new SelectList(await _repository.GetClientsAsync(), "Id", "Name", borrowing.ClientId);
            ViewData["BookId"] = new SelectList(await _repository.GetBooksAsync(), "Id", "DisplayText", borrowing.BookId);

            return View(borrowing);
        }

        
        // GET: Home/Close/5
        public async Task<IActionResult> Close(int? id)
        {
            if (id == null) return NotFound();

            var borrowing = await _repository.GetBorrowingAsync(id.Value);
            if (borrowing == null) return NotFound();
            if (borrowing.DateReturned != null) return RedirectToAction("Index");

            return View(borrowing);
        }

        // POST: Home/Close/5
        [HttpPost, ActionName("Close")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CloseConfirmed(int id)
        {
            try
            {
                await _repository.ReturnBook(id);
            }
            catch (RepositoryOperationException)
            {
                // skip this for now
            }
            catch (ValidationException)
            {
                // skip this for now
            }

            return RedirectToAction("Index");
        }
    }
}
