using Employee_Directory_App.Data;
using Employee_Directory_App.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Employee_Directory_App.Controllers
{

    public class AuthenticateController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly SignInManager<AuthenticateUser> _signInManager;
        private readonly UserManager<AuthenticateUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AuthenticateController(ApplicationDbContext context, SignInManager<AuthenticateUser> signInManager, UserManager<AuthenticateUser> user, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _signInManager = signInManager;
            _userManager = user;
            _roleManager = roleManager;
        }

        //GET Login
        public IActionResult Login()
        {
            return View();
        }

        // POST Login
        [HttpPost]
        public async Task<IActionResult> Login(LoginModel loginModel)
        {
            if (!ModelState.IsValid)
            {
                return View(loginModel);
            }
            var user = await _userManager.FindByNameAsync(loginModel.Username);
            if (user != null && await _userManager.CheckPasswordAsync(user, loginModel.Password))
            {
                await _signInManager.SignOutAsync();

                var result = await _signInManager.PasswordSignInAsync(
                    user,
                    loginModel.Password,
                    isPersistent: false,
                    lockoutOnFailure: false
                    );

                if (result.Succeeded)
                {
                    return RedirectToAction("Index", "Employees");
                }
            }
            ViewBag.Error = "Invalid username or password.";
            return View(loginModel);
        }

        // GET Register
        public IActionResult Register()
        {
            return View();
        }

        // POST Register
        [HttpPost]
        public async Task<IActionResult> Register(RegisterModel registerModel)
        {
            if (!ModelState.IsValid)
            {
                return View(registerModel);
            }

          
            if (await _userManager.FindByNameAsync(registerModel.Username) != null)
            {
                ViewBag.Error = "This user already exists";
                return View(registerModel);
            }

            if (!await _roleManager.RoleExistsAsync("Employee"))
            {
                await _roleManager.CreateAsync(new IdentityRole("Employee"));
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try { 
                var employee = new Employee
                {
                    Id = Guid.NewGuid(),
                    FirstName = registerModel.FirstName,
                    LastName = registerModel.LastName,
                    Email = registerModel.Email,
                    PhoneNumber = registerModel.PhoneNumber,
                    HireDate = DateTime.Now,
                    IsActive = true
                };
           
                _context.Employees.Add(employee);
                await _context.SaveChangesAsync();

                var authenticateUser = new AuthenticateUser
                {
                    UserName = registerModel.Username,
                    Email = registerModel.Email,
                    EmployeeId = employee.Id,
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(authenticateUser, registerModel.Password);

                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                        ModelState.AddModelError("", error.Description);

                    return View(registerModel);
                
                }
                await _userManager.AddToRoleAsync(authenticateUser, "Employee");

                await transaction.CommitAsync();

                return RedirectToAction("Login");

            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                ViewBag.Error =  ex.Message;

                if (ex.InnerException != null)
                {
                    ViewBag.Error += " Inner Exception: " + ex.InnerException.Message;
                }
                return View(registerModel);
            }

         
        }


        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login");
        }
    }
}
