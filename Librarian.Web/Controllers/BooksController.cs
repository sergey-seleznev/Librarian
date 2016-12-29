using System.Threading.Tasks;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Librarian.Core.Data;
using Librarian.Core.Models;
using NUglify.Helpers;

namespace Librarian.Controllers
{
    public class BooksController : Controller
    {
        private readonly ILibrarianRepository _repository;

        public BooksController(ILibrarianRepository repository)
        {
            _repository = repository;
        }

        // GET: Books
        public async Task<IActionResult> Index()
        {
            return View(await _repository.GetBooksAsync());
        }
        
        // GET: Books/Create
        public async Task<IActionResult> Create()
        {
            ViewData["ShelfId"] = new SelectList(await _repository.GetShelvesAsync(), "Id", "DisplayText");
            return View();
        }

        // POST: Books/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Name,ShelfId,Position,AgeLimit,DurationLimit")] Book book)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _repository.AddBookAsync(book);
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

            ViewData["ShelfId"] = new SelectList(await _repository.GetShelvesAsync(), "Id", "DisplayText");
            return View(book);
        }

        // GET: Books/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var book = await _repository.GetBookAsync(id.Value);
            if (book == null)
            {
                return NotFound();
            }

            ViewData["ShelfId"] = new SelectList(await _repository.GetShelvesAsync(), "Id", "DisplayText");
            return View(book);
        }

        // POST: Books/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Name,ShelfId,Position,AgeLimit,DurationLimit,ClientId")] Book book)
        {
            if (id != book.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    await _repository.UpdateBookAsync(book);
                    return RedirectToAction("Index");
                }
                catch (RepositoryOperationException ex)
                {
                    ModelState.AddModelError(ex.Key, ex.Message);
                }
            }

            ViewData["ShelfId"] = new SelectList(await _repository.GetShelvesAsync(), "Id", "DisplayText");
            return View(book);
        }
        
        // GET: Books/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var book = await _repository.GetBookAsync(id.Value);
            if (book == null) return NotFound();

            return View(book);
        }

        // POST: Books/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                await _repository.DeleteBookAsync(id);
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

            return await Delete(id);
        }
        
    }
}
