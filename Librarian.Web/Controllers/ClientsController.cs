using System.Threading.Tasks;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Librarian.Core.Data;
using Librarian.Core.Models;
using NUglify.Helpers;

namespace Librarian.Controllers
{
    public class ClientsController : Controller
    {
        private readonly ILibrarianRepository _repository;

        public ClientsController(ILibrarianRepository repository)
        {
            _repository = repository;    
        }

        // GET: Clients
        public async Task<IActionResult> Index()
        {
            return View(await _repository.GetClientsAsync());
        }
        
        // GET: Clients/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Clients/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Birthdate,IsUntrustworthy")] Client client)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _repository.AddClientAsync(client);
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

            return View(client);
        }

        // GET: Clients/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var client = await _repository.GetClientAsync(id.Value);
            if (client == null) return NotFound();

            return View(client);
        }

        // POST: Clients/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Birthdate,IsUntrustworthy")] Client client)
        {
            if (id != client.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    await _repository.UpdateClientAsync(client);
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
            
            return View(client);
        }

        // GET: Clients/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var client = await _repository.GetClientAsync(id.Value);
            if (client == null) return NotFound();

            return View(client);
        }

        // POST: Clients/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                await _repository.DeleteClientAsync(id);
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
