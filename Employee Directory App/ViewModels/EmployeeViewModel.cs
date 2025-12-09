using Employee_Directory_App.Models;

namespace Employee_Directory_App.ViewModels
{
    public class EmployeeViewModel
    {
        public Employee Employee { get; set; }
        public IEnumerable<Department> Departments { get; set; }
        public IEnumerable<JobTitle> JobTitles { get; set; }
        public string Password { get; set; }
        public IFormFile? ImageFile { get; set; }
    }
}
