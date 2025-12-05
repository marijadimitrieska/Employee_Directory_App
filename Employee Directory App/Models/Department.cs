using System.ComponentModel.DataAnnotations;

namespace Employee_Directory_App.Models
{
    public class Department
    {
        [Key]
        public Guid Id { get; set; }
        [Required]
        public string Name { get; set; }
    }
}
