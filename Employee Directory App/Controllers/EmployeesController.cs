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
                Employee = new Employee(),
                Departments = _context.Departments.ToList(),
                JobTitles = _context.JobTitles.ToList()
            };
            ViewData["DepartmentId"] = new SelectList(_context.Departments, "Id", "Name");
            ViewData["JobTitleId"] = new SelectList(_context.JobTitles, "Id", "Title");
            return View(vm);
        }

        [HttpPost]
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
                Employee = employee,
                Departments = _context.Departments.ToList(),
                JobTitles = _context.JobTitles.ToList()
            };

       
            return View(vm);
        }


        [HttpPost]
        public async Task<IActionResult> Edit(Guid id, EmployeeViewModel vm)
        {
            if (id != vm.Employee.Id)
            {
                return BadRequest();
            }
            var existingEntity = await _context.Employees
                .Include(e => e.Department)
                .Include(e => e.JobTitle)
                .FirstOrDefaultAsync(e=>e.Id == id);

            if(existingEntity == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            bool isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
            bool isEmployee = await _userManager.IsInRoleAsync(user, "Employee");

            if (!isAdmin && !(isEmployee && user.EmployeeId == vm.Employee.Id))
            {
                return Unauthorized();
            }

            existingEntity.Id = vm.Employee.Id;
            existingEntity.FirstName = vm.Employee.FirstName;
            existingEntity.LastName = vm.Employee.LastName;
            existingEntity.Email = vm.Employee.Email;
            existingEntity.PhoneNumber = vm.Employee.PhoneNumber;
            existingEntity.HireDate = vm.Employee.HireDate;
            existingEntity.DepartmentId = vm.Employee.DepartmentId;
            existingEntity.JobTitleId = vm.Employee.JobTitleId;
            existingEntity.IsActive = vm.Employee.IsActive;
            
            if(vm.ImageFile != null)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "profiles");
                Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = Guid.NewGuid() + Path.GetExtension(vm.ImageFile.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await vm.ImageFile.CopyToAsync(stream);
                }

                existingEntity.ProfilePhoto = "/uploads/profiles/" + uniqueFileName;


            }
            _context.Update(existingEntity);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));

            }

            //if(!ModelState.IsValid)
            //{
            //    var errors = ModelState
            //        .Where(x => x.Value.Errors.Any())
            //        .ToDictionary(
            //            kvp => kvp.Key,
            //            kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
            //        );
            //    return Json(new { success = false, errors });
            //}

            //var employee = await _context.Employees
            //    .Include(e=>e.Department)
            //    .Include(e=> e.JobTitle)
            //    .FirstOrDefaultAsync(e => e.Id == vm.Employee.Id);

            //if(employee == null)
            //{
            //    return Json(new { success = false, errors = new { Employee = new[] { "Employee not found." } } });
            //}

            //var user = await _userManager.GetUserAsync(User);
            //bool isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
            //bool isEmployee = await _userManager.IsInRoleAsync(user, "Employee");

            //if(!isAdmin && !(isEmployee && user.EmployeeId == vm.Employee.Id))
            //{
            //    return Json(new { success = false, errors = new { Employee = new[] { "Unauthorized" } } });
            //}

            //employee.FirstName = vm.Employee.FirstName;
            //employee.LastName = vm.Employee.LastName;
            //employee.Email = vm.Employee.Email;
            //employee.PhoneNumber = vm.Employee.PhoneNumber;
            //employee.HireDate = vm.Employee.HireDate;
            //employee.DepartmentId = vm.Employee.DepartmentId;
            //employee.JobTitleId = vm.Employee.JobTitleId;
            //employee.IsActive = vm.Employee.IsActive;
            
            //if(vm.ImageFile != null)
            //{
            //    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "profiles");
            //    Directory.CreateDirectory(uploadsFolder);

            //    var uniqueFileName = Guid.NewGuid() + Path.GetExtension(vm.ImageFile.FileName);
            //    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            //    using(var stream = new FileStream(filePath, FileMode.Create))
            //    {
            //        await vm.ImageFile.CopyToAsync(stream);
            //    }

            //    employee.ProfilePhoto = "/uploads/profiles/" + uniqueFileName;
            //}
            //_context.Update(employee);
            //await _context.SaveChangesAsync();

            //return Json(new { success = true, redirectUrl = Url.Action("Index", "Employees") });
     

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
            var employees = _context.Employees
                .Include(e => e.Department)
                .Include(e => e.JobTitle)
                .Select(e => new EmployeeGridViewModel
                {
                    Id = e.Id,
                    FirstName = e.FirstName,
                    LastName = e.LastName,
                    Email = e.Email,
                    PhoneNumber = e.PhoneNumber,
                    HireDate = e.HireDate,
                    DepartmentId = e.DepartmentId,
                    JobTitleId = e.JobTitleId,
                    Department = e.Department != null ? e.Department.Name : "",
                    JobTitle = e.JobTitle != null ? e.JobTitle.Title : "",
                    IsActive = e.IsActive,
                    ProfilePhoto = e.ProfilePhoto
                });
            var result = await employees.ToDataSourceResultAsync(request);

            return Json(result, new JsonSerializerOptions
            {
                PropertyNamingPolicy = null,
                ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles
            });

        }


        public IActionResult ShowImage(Guid id)
        {
            var employee = _context.Employees.Find(id);

            if (employee==null || string.IsNullOrEmpty(employee.ProfilePhoto))
            {
               
                return NotFound();

            }
            var imagePath = employee.ProfilePhoto.TrimStart('/');

            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", imagePath);

            Console.WriteLine($"Looking for image at: {filePath}");
            Console.WriteLine($"File existis: {System.IO.File.Exists(filePath)}");


            if (System.IO.File.Exists(filePath))
                {
                    var extension = Path.GetExtension(filePath).ToLower();
                    var contentType = GetContentType(extension);

                    return PhysicalFile(filePath, contentType);
                }

            var directory = Path.GetDirectoryName(filePath);
            if (Directory.Exists(directory))
            {
                Console.WriteLine($"Files in directory: {string.Join(", ", Directory.GetFiles(directory))}");
            }
            return File("/images/no-photo.png", "image/png");
        }

        private string GetContentType(string extension)
        {
            return extension switch
            {
                ".png" => "image/png",
                ".jpg" => "image/jpeg",
                ".jpeg" => "image/jpeg",
                ".gif" => "image/gif",
                ".bmp" => "image/bmp",
                ".svg" => "image/svg+xml",
                ".webp" => "image/webp",
                _ => "application/octet-stream"
            };
        }

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
                return Json(new[]{ new { name = "", size = 0, error = "No file uploaded" }});

            var allowedExtensions = new[] { ".png", ".jpg", ".jpeg", ".gif" };
            var extension = Path.GetExtension(ImageFile.FileName).ToLower();

            if (!allowedExtensions.Contains(extension))
            {
                return Json(new[] { new { name = "", size = 0, error = "Invalid file type" } });
            }

            var tempFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "temp");
            Directory.CreateDirectory(tempFolder);

            var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(ImageFile.FileName);
            var filePath = Path.Combine(tempFolder, uniqueFileName);

            using(var stream = new FileStream(filePath, FileMode.Create))
            {
                await ImageFile.CopyToAsync(stream);
            }

            return Json(new[]
            {
                new{
                    name = ImageFile.FileName,
                    size = ImageFile.Length,
                    newPhotoPath = "/uploads/temp/" + uniqueFileName,
                    uid = Guid.NewGuid().ToString()
            }
            });
        }
        [HttpPost]
        public ActionResult PdfExportSave(string contentType, string base64, string fileName)
        {
            var fileContents = Convert.FromBase64String(base64);
            return File(fileContents, contentType, fileName);
        }

        private bool EmployeeExists(Guid id)
        {
            return _context.Employees.Any(e => e.Id == id);
        }
    }
}
