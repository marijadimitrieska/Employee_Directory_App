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
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Telerik.SvgIcons;
using System.Text.Json;

namespace Employee_Directory_App.Controllers
{
    public class EmployeesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AuthenticateUser> _userManager;
        public EmployeesController(ApplicationDbContext context, UserManager<AuthenticateUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Employees
        public async Task<IActionResult> Index()
        {
          var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToAction("Login", "Authenticate");
            }
            if (await _userManager.IsInRoleAsync(user, "Admin"))
            {
                return View();
            }

            return RedirectToAction("Details", new { id = user.EmployeeId });
        }

        // GET: Employees/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);

            if(await _userManager.IsInRoleAsync(user, "Employee") && user.EmployeeId != id)
            {
                return Unauthorized();
               
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
        [Authorize(Roles ="Admin")]
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
        [Authorize(Roles ="Admin")]
        public async Task<IActionResult> Create(EmployeeViewModel vm)
        {
            if (!ModelState.IsValid)
            {

                ViewData["DepartmentId"] = new SelectList(_context.Departments, "Id", "Name", vm.Employee.DepartmentId);
                ViewData["JobTitleId"] = new SelectList(_context.JobTitles, "Id", "Title", vm.Employee.JobTitleId);
                return View(vm);
            }

   
           if (vm.ImageFile != null)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "profiles");
                Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(vm.ImageFile.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);


                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await vm.ImageFile.CopyToAsync(stream);
                }
                vm.Employee.ProfilePhoto = "/uploads/profiles/" + uniqueFileName;

            }
           
            _context.Employees.Add(vm.Employee);
            await _context.SaveChangesAsync();

            var newUser = new AuthenticateUser
            {
                UserName = vm.Employee.Email,
                Email = vm.Employee.Email,
                EmployeeId = vm.Employee.Id,
                EmailConfirmed = true
            };

            var createUserResult = await _userManager.CreateAsync(newUser, vm.Password);

            if (!createUserResult.Succeeded)
            {
                foreach (var error in createUserResult.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
                return View(vm);
            }
            await _userManager.AddToRoleAsync(newUser, "Employee");
            return RedirectToAction(nameof(Index));
         
        }

        // GET: Employees/Edit/5

        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);

            if (await _userManager.IsInRoleAsync(user, "Employee") && user.EmployeeId != id)
            {
                return Unauthorized();
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

            return View(vm);
        }

        // POST: Employees/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        public async Task<IActionResult> Update([DataSourceRequest] DataSourceRequest request, EmployeeGridViewModel vm)
        {
            if(vm == null)
            {
                return Json(ModelState.ToDataSourceResult());
            }

            var employee = await _context.Employees
                .Include(e=>e.Department)
                .Include(e=> e.JobTitle)
                .FirstOrDefaultAsync(e => e.Id == vm.Id);

            if(employee == null)
            {
                return Json(ModelState.ToDataSourceResult());
            }

            var user = await _userManager.GetUserAsync(User);
            bool isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
            bool isEmployee = await _userManager.IsInRoleAsync(user, "Employee");

            if(!isAdmin && !(isEmployee && user.EmployeeId == vm.Id))
            {
                return Json(new { error = true, message = "Unauthorized" });
            }

            employee.FirstName = vm.FirstName;
            employee.LastName = vm.LastName;
            employee.Email = vm.Email;
            employee.PhoneNumber = vm.PhoneNumber;
            employee.HireDate = vm.HireDate;
            employee.IsActive = vm.IsActive;
            employee.ProfilePhoto = vm.ProfilePhoto;
            //if (employee == null)
            //{
            //    return Json(ModelState.ToDataSourceResult());
            //}
            //if(vm.ImageFile != null)
            //{
            //    if (!string.IsNullOrEmpty(employee.ProfilePhoto))
            //    {
            //        var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", employee.ProfilePhoto.TrimStart('/'));
            //        if (System.IO.File.Exists(oldFilePath))
            //        {
            //            System.IO.File.Delete(oldFilePath);
            //        }
            //    }
            //    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "profiles");
            //    Directory.CreateDirectory(uploadsFolder);

            //    var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(vm.ImageFile.FileName);
            //    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            //    using (var stream = new FileStream(filePath, FileMode.Create))
            //    {
            //        await vm.ImageFile.CopyToAsync(stream);
            //    }
            //    employee.ProfilePhoto = "/uploads/profiles/" + uniqueFileName; 

            //}
            //employee.PhoneNumber = vm.Employee.PhoneNumber;
            //employee.Email = vm.Employee.Email;

            if (ModelState.IsValid)
            {

                _context.Update(employee);
                await _context.SaveChangesAsync();

                //catch (DbUpdateConcurrencyException)
                //{
                //    if (!EmployeeExists(employee.Id))
                //    {
                //        return NotFound();
                //    }
                //    else
                //    {
                //        throw;
                //    }
                //}
            }

            var resultVm = new EmployeeGridViewModel
            {
                Id = employee.Id,
                FirstName = vm.FirstName,
                LastName = vm.LastName,
                Email = vm.Email,
                PhoneNumber = vm.PhoneNumber,
                HireDate = vm.HireDate,
                Department = vm.Department,
                JobTitle = vm.JobTitle,
                IsActive = vm.IsActive,
                ProfilePhoto = vm.ProfilePhoto
            };
            return Json(new[] { resultVm }.ToDataSourceResult(request, ModelState));
     
        }

        // GET: Employees/Delete/5
        [Authorize(Roles = "Admin")]
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
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Destroy([DataSourceRequest] DataSourceRequest request, EmployeeGridViewModel vm)
        {
            if (vm != null)
            {
                var entity = await _context.Employees.FindAsync(vm.Id);

                if (entity != null)
                {
                    _context.Employees.Remove(entity);
                    await _context.SaveChangesAsync();
                }
            }
            return Json(new[] {vm}.ToDataSourceResult(request, ModelState));
        }

        public async Task<IActionResult> ReadEmployees([DataSourceRequest] DataSourceRequest request)
        {
            var user = await _userManager.GetUserAsync(User);
            bool isAdmin = await _userManager.IsInRoleAsync(user, "Admin");

            IQueryable<Employee> employees;

            try
            {
                if (isAdmin)
                {
                    employees = _context.Employees
                        .Include(e => e.Department)
                        .Include(e => e.JobTitle);
                }
                else
                {
                    employees = _context.Employees
                        .Where(e => e.Id == user.EmployeeId)
                        .Include(e => e.Department)
                        .Include(e => e.JobTitle);

                }
                var dto = await employees
                    .Select(e => new EmployeeGridViewModel
                    {
                        Id = e.Id,
                        FirstName = e.FirstName,
                        LastName = e.LastName,
                        Email = e.Email,
                        PhoneNumber = e.PhoneNumber,
                        HireDate = e.HireDate,
                        Department = e.Department != null ? e.Department.Name : null,
                        JobTitle = e.JobTitle != null ? e.JobTitle.Title : null,
                        IsActive = e.IsActive,
                       ProfilePhoto = e.ProfilePhoto
                    }).ToListAsync();

                var result = dto.ToDataSourceResult(request);
                return Json(result, new JsonSerializerOptions
                {
                        PropertyNamingPolicy = null
                });
            }
            catch (Exception ex)
            {
                return Json(new {error = true, message = ex.Message });
            }
        }

        //public IActionResult ShowImage(Guid id)
        //{
        //    var employee = _context.Employees.FirstOrDefault(e => e.Id == id);

        //    if(employee?.ProfilePhoto == null)
        //    {
        //        return File("~/images/no-image.png", "image/png");
        //    }
        //    string contentType = employee.ImageExtension?
        //        .ToLower() switch
        //    {
        //        ".png" => "image/png",
        //        ".jpg" => "image/jpeg",
        //        ".jpeg" => "image/jpeg",
        //        ".gif" => "image/gif",
        //        _ => "application/octet-stream"
        //    };
        //    return File(employee.ProfilePhoto, contentType);
        //}

        //public async Task<IActionResult> DeleteImage(Guid id)
        //{
        //    var employee = await _context.Employees.FindAsync(id);

        //    if (employee == null)
        //    {
        //        return NotFound();
        //    }

        //    employee.ProfilePhoto = null;
        //    employee.ImageExtension = null;

        //    await _context.SaveChangesAsync();

        //    return RedirectToAction("Edit", new { id = id });
        //}

        [HttpPost]
        public async Task<IActionResult> SaveTempImage(IFormFile ImageFile)
        {
            if (ImageFile == null || ImageFile.Length == 0)
                return Json(new { success = false, message = "No file upload" });

            var allowedExtensions = new[] { ".png", ".jpg", ".jpeg", ".gif" };
            var extension = Path.GetExtension(ImageFile.FileName).ToLower();

            if (!allowedExtensions.Contains(extension))
            {
                return Json(new { success = false, message = "Invalid file type" });
            }

            using var memoryStream = new MemoryStream();
            await ImageFile.CopyToAsync(memoryStream);

            TempData["TempImage"] = memoryStream.ToArray();
            TempData["TempImageExtension"] = extension;

            return Json(new { success = true });
        }

        //private bool EmployeeExists(Guid id)
        //{
        //    return _context.Employees.Any(e => e.Id == id);
        //}
    }
}
