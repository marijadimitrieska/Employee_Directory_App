using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Employee_Directory_App.Data;
using Employee_Directory_App.Models;
using Employee_Directory_App.ViewModels;
using Kendo.Mvc.UI;
using Kendo.Mvc.Extensions;

namespace Employee_Directory_App.Controllers
{
    public class EmployeesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EmployeesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Employees
        public async Task<IActionResult> Index()
        {
          
            return View();
        }

        // GET: Employees/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employee = await _context.Employees
                .Include(e => e.Department)
                .Include(e => e.JobTitle)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (employee == null)
            {
                return NotFound();
            }

            return View(employee);
        }

        // GET: Employees/Create
        public IActionResult Create()
        {
            var vm = new EmployeeViewModel
            {
                Employee = new Employee()
            };
            ViewData["DepartmentId"] = new SelectList(_context.Departments, "Id", "Name");
            ViewData["JobTitleId"] = new SelectList(_context.JobTitles, "Id", "Title");
            return View(vm);
        }

        // POST: Employees/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EmployeeViewModel vm)
        {
            if (!ModelState.IsValid)
            {

                ViewData["DepartmentId"] = new SelectList(_context.Departments, "Id", "Name", vm.Employee.DepartmentId);
                ViewData["JobTitleId"] = new SelectList(_context.JobTitles, "Id", "Title", vm.Employee.JobTitleId);
                return View(vm);
            }

            if (TempData["TempImage"] != null)
            {

                vm.Employee.ProfilePhoto = (byte[])TempData["TempImage"];
                vm.Employee.ImageExtension = (string)TempData["TempImageExtension"];
            }

            else if (vm.ImageFile != null)
            {
                using var memoryStream = new MemoryStream();
                await vm.ImageFile.CopyToAsync(memoryStream);
                vm.Employee.ProfilePhoto = memoryStream.ToArray();
                vm.Employee.ImageExtension = Path.GetExtension(vm.ImageFile.FileName);
            }
           
            _context.Employees.Add(vm.Employee);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
         
        }

        // GET: Employees/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employee = await _context.Employees.FindAsync(id);
            if (employee == null)
            {
                return NotFound();
            }
            var vm = new EmployeeViewModel
            {
                Employee = employee
            };

            ViewData["DepartmentId"] = new SelectList(_context.Departments, "Id", "Name", employee.DepartmentId);
            ViewData["JobTitleId"] = new SelectList(_context.JobTitles, "Id", "Title", employee.JobTitleId);
            return View(employee);
        }

        // POST: Employees/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, EmployeeViewModel vm)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (id != vm.Employee.Id)
            {
                return NotFound();
            }
            if(vm.ImageFile != null)
            {
                using var memoryStream = new MemoryStream();
                await vm.ImageFile.CopyToAsync(memoryStream);

                employee.ProfilePhoto = memoryStream.ToArray();
                employee.ImageExtension = Path.GetExtension(vm.ImageFile.FileName);


            }
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(employee);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EmployeeExists(employee.Id))
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
            ViewData["DepartmentId"] = new SelectList(_context.Departments, "Id", "Name", employee.DepartmentId);
            ViewData["JobTitleId"] = new SelectList(_context.JobTitles, "Id", "Title", employee.JobTitleId);
            return View(employee);
        }

        // GET: Employees/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employee = await _context.Employees
                .Include(e => e.Department)
                .Include(e => e.JobTitle)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (employee == null)
            {
                return NotFound();
            }

            return View(employee);
        }

        // POST: Employees/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee != null)
            {
                _context.Employees.Remove(employee);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult ReadEmployees([DataSourceRequest] DataSourceRequest request)
        {
            var employees = _context.Employees
                .Include(e => e.Department)
                .Include(e => e.JobTitle)
                .Select(e => new
                {
                    e.Id,
                    e.FirstName,
                    e.LastName,
                    e.Email,
                    e.PhoneNumber,
                    e.HireDate,
                    Department = e.Department != null ? e.Department.Name : "",
                    JobTitle = e.JobTitle != null ? e.JobTitle.Title : "",
                    PhotoUrl = e.ProfilePhoto != null ? "/Employees/ShowImage/" + e.Id : null,

                    e.IsActive
                });
            return Json(employees.ToDataSourceResult(request));
        }

        public IActionResult ShowImage(Guid id)
        {
            var employee = _context.Employees.FirstOrDefault(e => e.Id == id);

            string contentType = employee.ImageExtension?
                .ToLower() switch
            {
                ".png" => "image/png",
                ".jpg" => "image/jpeg",
                ".jpeg" => "image/jpeg",
                ".gif" => "image/gif",
                _=> "application/octet-stream"
            };
            return File(employee.ProfilePhoto, contentType);
        }

        public async Task<IActionResult> DeleteImage(Guid id)
        {
            var employee = await _context.Employees.FindAsync(id);

            if (employee == null)
            {
                return NotFound();
            }

            employee.ProfilePhoto = null;
            employee.ImageExtension = null;

            await _context.SaveChangesAsync();

            return RedirectToAction("Edit", new { id = id });
        }

        [HttpPost]
        public async Task<IActionResult> SaveTempImage(IFormFile ImageFile)
        {
            if (ImageFile == null || ImageFile.Length == 0)
                return Json(new { success = false, message = "No file upload" });

            var allowedExtensions = new[] { ".png", ".jpg", ".jpeg", ".gif" };
            var extension = Path.GetExtension(ImageFile.FileName).ToLower();

            if(!allowedExtensions.Contains(extension))
            {
                return Json(new { success = false, message = "Invalid file type" });
            }

            using var memoryStream = new MemoryStream();
            await ImageFile.CopyToAsync(memoryStream);

            TempData["TempImage"]=memoryStream.ToArray();
            TempData["TempImageExtension"] = extension;

            return Json(new { success = true, fileName = ImageFile.FileName });
        }

        private bool EmployeeExists(Guid id)
        {
            return _context.Employees.Any(e => e.Id == id);
        }
    }
}
