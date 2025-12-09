using Employee_Directory_App.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Employee_Directory_App.Data.Seeder
{
    public static class DBInitializer
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            using var contex = serviceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = serviceProvider.GetRequiredService<UserManager<AuthenticateUser>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            string[] roleNames = { "Admin", "Employee" };

            foreach (var role in roleNames)
            {
                if(!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            var department = await contex.Departments.FirstOrDefaultAsync(d => d.Name == "Administration");
            if(department  == null)
            {
                department = new Department
                {
                    Id = Guid.NewGuid(),
                    Name = "Administration"
                };

                contex.Departments.Add(department);
                await contex.SaveChangesAsync();
            }

            var jobTitle = await contex.JobTitles.FirstOrDefaultAsync(j => j.Title == "System Administrator");
            if (jobTitle == null) {
                jobTitle = new JobTitle
                {
                    Id= Guid.NewGuid(),
                    Title = "System Administrator"
                };
                contex.JobTitles.Add(jobTitle);
                await contex.SaveChangesAsync();
            }
            var adminEmail = "admin@test.com";

            var adminUser = await userManager.FindByNameAsync(adminEmail);

            if (adminUser == null)
            {
                var adminEmployee = new Employee
                {
                    Id = Guid.NewGuid(),
                    FirstName = "Admin",
                    LastName = "User",
                    Email = adminEmail,
                    PhoneNumber = "222222222",
                    HireDate = DateTime.Now,
                    DepartmentId = department.Id,
                    JobTitleId = jobTitle.Id,
                    IsActive = true
                };

                contex.Employees.Add(adminEmployee);
                await contex.SaveChangesAsync();

                adminUser = new AuthenticateUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true,
                    EmployeeId = adminEmployee.Id
                };

                var result = await userManager.CreateAsync(adminUser, "Admin@123");

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");

                    adminUser.EmployeeId = adminEmployee.Id;
                    await contex.SaveChangesAsync();
                }
            }
        }
    }
}
