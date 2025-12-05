using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.General;
using System.ComponentModel.DataAnnotations;

namespace Employee_Directory_App.Models
{
    public class Employee
    {
        [Key]
        public Guid Id { get; set; }
 
        public string? FirstName { get; set; }
    
        public string? LastName { get; set; }

        [EmailAddress]
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
  
        public DateTime HireDate { get; set; }
        public Guid? DepartmentId { get; set; }
        public Department? Department { get; set; }
        public Guid? JobTitleId { get; set; }
        public JobTitle? JobTitle { get; set; }
    
        public byte[]? ProfilePhoto { get; set; }
        public string? ImageExtension { get; set; }
        public bool IsActive { get; set; }

        public string? UserId { get; set; }
        public AuthenticateUser? User { get; set; }
    }
}
