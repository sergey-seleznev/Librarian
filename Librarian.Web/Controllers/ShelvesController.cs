using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Librarian.Core.Data;
using Librarian.Core.Models;
using NUglify.Helpers;

namespace Librarian.Controllers
{
    public class ShelvesController : Controller
    {
        private readonly ILibrarianRepository _repository;

        public ShelvesController(ILibrarianRepository repository)
        {
            _repository = repository;    
        }

        // GET: Shelves
        public async Task<IActionResult> Index()
        {
            return View(await _repository.GetShelvesAsync());
        }
        
        // GET: Shelves/Create
        public async Task<IActionResult> Create()
        {
            var shelves = await _repository.GetShelvesAsync();

            var nextShelfNumber = shelves
                .Select(m => m.Number)
                .DefaultIfEmpty(0)
                .Max() + 1;
            
            var model = new Shelf { Number = nextShelfNumber };
            return View(model);
        }

        // POST: Shelves/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Number,Description,Capacity")] Shelf shelf)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _repository.AddShelfAsync(shelf);
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

            return View(shelf);
        }

        // GET: Shelves/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var shelf = await _repository.GetShelfAsync(id.Value);
            if (shelf == null) return NotFound();

            return View(shelf);
        }

        // POST: Shelves/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Number,Description,Capacity")] Shelf shelf)
        {
            if (id != shelf.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    await _repository.UpdateShelfAsync(shelf);
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

            return View(shelf);
        }

        // GET: Shelves/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var shelf = await _repository.GetShelfAsync(id.Value);
            if (shelf == null) return NotFound();

            return View(shelf);
        }

        // POST: Shelves/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                await _repository.DeleteShelfAsync(id);
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
