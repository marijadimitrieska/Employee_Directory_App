using Microsoft.AspNetCore.Identity;

namespace Employee_Directory_App.Models
{
    public class AuthenticateUser : IdentityUser
    {
        public Guid? EmployeeId { get; set; }
        public Employee? Employee { get; set; }
    }
}
