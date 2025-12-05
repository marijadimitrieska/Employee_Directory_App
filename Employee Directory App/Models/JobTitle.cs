using System.ComponentModel.DataAnnotations;

namespace Employee_Directory_App.Models
{
    public class JobTitle
    {
        [Key]
        public Guid Id { get; set; }
        [Required]
        public string Title { get; set; }
    }
}
