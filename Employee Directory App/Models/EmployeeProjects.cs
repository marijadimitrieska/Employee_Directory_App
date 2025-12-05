using System.ComponentModel.DataAnnotations;

namespace Employee_Directory_App.Models
{
    public class EmployeeProjects
    {
        [Key]
        public Guid Id { get; set; }
        public Guid EmployeeId { get; set; }
        public Employee? Employee { get; set; }
        [Required]
        public string Title { get; set; }
    }
}
