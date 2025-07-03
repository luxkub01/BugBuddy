using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BugBuddy.Models;
using BugBuddy.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using BugBuddy.Data;

namespace BugBuddy.Controllers
{
    [Authorize]
    public class BugsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        public BugsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Bugs
        public async Task<IActionResult> Index(string search, string status, string sort)
        {
            var userId = _userManager.GetUserId(User);
            var bugs = _context.Bugs.Where(b => b.UserId == userId);

            // Filter
            if (!string.IsNullOrEmpty(search))
                bugs = bugs.Where(b => b.Title.Contains(search));

            if (!string.IsNullOrEmpty(status))
                bugs = bugs.Where(b => b.Status == status);

            // Sort
            bugs = sort switch
            {
                "dateCreated" => bugs.OrderByDescending(b => b.DateCreated),
                "lastUpdated" => bugs.OrderByDescending(b => b.LastUpdated),
                "priority" => bugs.OrderBy(b =>
                     b.Priority == "Low" ? 1 :
                     b.Priority == "Medium" ? 2 :
                     b.Priority == "High" ? 3 : 4),
                _ => bugs.OrderByDescending(b => b.LastUpdated)
            };

            return View(await bugs.ToListAsync());
        }


        // GET: Bugs/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bug = await _context.Bugs
                .Include(b => b.User)
                .Include(x=>x.Notes)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (bug == null)
            {
                return NotFound();
            }

            return View(bug);
        }

        // GET: Bugs/Create
        public IActionResult Create()
        {
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id");
            return View();
        }

        // POST: Bugs/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
       
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Bug bug)
        {
            // Set the current user ID
            bug.UserId = _userManager.GetUserId(User);
            bug.DateCreated = DateTime.UtcNow;
            bug.LastUpdated = DateTime.UtcNow;

            // Remove User from validation to avoid issues
            ModelState.Remove("User");
            ModelState.Remove("UserId");

            if (ModelState.IsValid)
            {
                _context.Add(bug);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(bug);
        }

        // GET: Bugs/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bug = await _context.Bugs.FindAsync(id);
            if (bug == null)
            {
                return NotFound();
            }
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", bug.UserId);
            return View(bug);
        }

        // POST: Bugs/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,Priority,Status,DateCreated,LastUpdated,UserId")] Bug bug)
        {
            if (id != bug.Id)
            {
                return NotFound();
            }
            ModelState.Remove("User");

            if (ModelState.IsValid)
            {
                try
                {
                    bug.LastUpdated = DateTime.UtcNow;
                    _context.Update(bug);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BugExists(bug.Id))
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
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", bug.UserId);
            return View(bug);
        }

        // GET: Bugs/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bug = await _context.Bugs
                .Include(b => b.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (bug == null)
            {
                return NotFound();
            }

            return View(bug);
        }

        // POST: Bugs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var bug = await _context.Bugs.FindAsync(id);
            if (bug != null)
            {
                _context.Bugs.Remove(bug);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        [HttpGet]
        public async Task<IActionResult> GetNote(int id)
        {
            var note = await _context.Notes.FindAsync(id);
            if (note == null)
                return NotFound();

            return PartialView("_EditNotePartial", note); // if using partial
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddNote(int bugId, string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                return RedirectToAction("Details", new { id = bugId });

            var note = new Note
            {
                BugId = bugId,
                Content = content,
                CreatedAt = DateTime.UtcNow
            };

            _context.Notes.Add(note);

            var bug = await _context.Bugs.FindAsync(bugId);
            if (bug != null)
            {
                bug.LastUpdated = DateTime.UtcNow;
                _context.Update(bug);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Details", new { id = bugId });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditNote(int id, string content)
        {
            var note = await _context.Notes.FindAsync(id);
            if (note == null)
                return NotFound();

            note.Content = content;
            await _context.SaveChangesAsync();

            // update bug's LastUpdated timestamp
            var bug = await _context.Bugs.FindAsync(note.BugId);
            if (bug != null)
            {
                bug.LastUpdated = DateTime.UtcNow;
                _context.Bugs.Update(bug);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Details", new { id = note.BugId });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteNote(int id)
        {
            var note = await _context.Notes.FindAsync(id);
            if (note == null)
                return NotFound();

            int bugId = note.BugId;

            _context.Notes.Remove(note);

            var bug = await _context.Bugs.FindAsync(bugId);
            if (bug != null)
            {
                bug.LastUpdated = DateTime.UtcNow;
                _context.Bugs.Update(bug);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Details", new { id = bugId });
        }


        private bool BugExists(int id)
        {
            return _context.Bugs.Any(e => e.Id == id);
        }
    }
}
