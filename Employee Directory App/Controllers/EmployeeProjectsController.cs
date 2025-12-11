using Employee_Directory_App.Data;
using Employee_Directory_App.Models;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Employee_Directory_App.Controllers
{
    public class EmployeeProjectsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EmployeeProjectsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: EmployeeProjects
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.EmployeesProjects.Include(e => e.Employee);
            return View(await applicationDbContext.ToListAsync());
        }

        public async Task<IActionResult> ReadEmployeeProjects([DataSourceRequest] DataSourceRequest request)
        {
            var employeeProjects = _context.EmployeesProjects
                .Include(p => p.Employee)
                .Select(p => new EmployeeProjects
                {
                    Id = p.Id,
                    EmployeeId = p.EmployeeId,
                    EmployeeFullName = p.Employee != null ? p.Employee.FirstName + " " + p.Employee.LastName : "",
                    Title = p.Title
                });

            var result = await employeeProjects.ToDataSourceResultAsync(request);

            return Json(result, new JsonSerializerOptions
            {
                PropertyNamingPolicy = null,
                ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles
            });
        }
        // GET: EmployeeProjects/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employeeProjects = await _context.EmployeesProjects
                .Include(e => e.Employee)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (employeeProjects == null)
            {
                return NotFound();
            }

            return View(employeeProjects);
        }

        // GET: EmployeeProjects/Create
        public IActionResult Create()
        {
            ViewData["EmployeeId"] = new SelectList(_context.Employees, "Id", "Email");
            return View();
        }

        // POST: EmployeeProjects/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,EmployeeId,Title")] EmployeeProjects employeeProjects)
        {
            if (ModelState.IsValid)
            {
                employeeProjects.Id = Guid.NewGuid();
                _context.Add(employeeProjects);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["EmployeeId"] = new SelectList(_context.Employees, "Id", "Email", employeeProjects.EmployeeId);
            return View(employeeProjects);
        }

        // GET: EmployeeProjects/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employeeProjects = await _context.EmployeesProjects.FindAsync(id);
            if (employeeProjects == null)
            {
                return NotFound();
            }
            ViewData["EmployeeId"] = new SelectList(_context.Employees, "Id", "Email", employeeProjects.EmployeeId);
            return View(employeeProjects);
        }

        // POST: EmployeeProjects/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,EmployeeId,Title")] EmployeeProjects employeeProjects)
        {
            if (id != employeeProjects.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(employeeProjects);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EmployeeProjectsExists(employeeProjects.Id))
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
            ViewData["EmployeeId"] = new SelectList(_context.Employees, "Id", "Email", employeeProjects.EmployeeId);
            return View(employeeProjects);
        }

        // GET: EmployeeProjects/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employeeProjects = await _context.EmployeesProjects
                .Include(e => e.Employee)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (employeeProjects == null)
            {
                return NotFound();
            }

            return View(employeeProjects);
        }

        // POST: EmployeeProjects/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var employeeProjects = await _context.EmployeesProjects.FindAsync(id);
            if (employeeProjects != null)
            {
                _context.EmployeesProjects.Remove(employeeProjects);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool EmployeeProjectsExists(Guid id)
        {
            return _context.EmployeesProjects.Any(e => e.Id == id);
        }
    }
}
