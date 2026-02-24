using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NoteList.Data;
using NoteList.Models;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Linq;

namespace NoteList.Controllers
{
    [Authorize]
    public class NotesController : Controller
    {
        private readonly AppDbContext _context;

        public NotesController(AppDbContext context)
        {
            _context = context;
        }

        private int GetCurrentUserId()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(userIdStr, out int userId))
            {
                return userId;
            }
            return 0; // Return 0 or throw an exception if the user ID is invalid/missing
        }

        // GET: Notes
        public async Task<IActionResult> Index()
        {
            var userId = GetCurrentUserId();
            var notes = await _context.Notes
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
            return View(notes);
        }

        // GET: Notes/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Notes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Content")] Note note)
        {
            if (ModelState.IsValid)
            {
                note.UserId = GetCurrentUserId();
                note.CreatedAt = System.DateTime.Now;
                _context.Add(note);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Note created successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(note);
        }

        // GET: Notes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userId = GetCurrentUserId();
            var note = await _context.Notes
                .FirstOrDefaultAsync(m => m.Id == id && m.UserId == userId);

            if (note == null)
            {
                return NotFound();
            }
            return View(note);
        }

        // POST: Notes/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Content")] Note note)
        {
            if (id != note.Id)
            {
                return NotFound();
            }

            var userId = GetCurrentUserId();

            if (ModelState.IsValid)
            {
                try
                {
                    var existingNote = await _context.Notes.FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId);
                    if (existingNote == null)
                    {
                        return NotFound();
                    }

                    existingNote.Title = note.Title;
                    existingNote.Content = note.Content;
                    existingNote.UpdatedAt = System.DateTime.Now;

                    _context.Update(existingNote);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Note updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!NoteExists(note.Id, userId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(note);
        }

        // GET: Notes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userId = GetCurrentUserId();
            var note = await _context.Notes
                .FirstOrDefaultAsync(m => m.Id == id && m.UserId == userId);

            if (note == null)
            {
                return NotFound();
            }

            return View(note);
        }

        // POST: Notes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = GetCurrentUserId();
            var note = await _context.Notes.FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId);

            if (note != null)
            {
                _context.Notes.Remove(note);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Note deleted successfully!";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool NoteExists(int id, int userId)
        {
            return _context.Notes.Any(e => e.Id == id && e.UserId == userId);
        }
    }
}
