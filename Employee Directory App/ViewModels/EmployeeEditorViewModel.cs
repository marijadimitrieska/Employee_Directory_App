using Microsoft.AspNetCore.Mvc.Rendering;

namespace Employee_Directory_App.ViewModels
{
    public class EmployeeEditorViewModel
    {
        public EmployeeGridViewModel Employee {  get; set; }
        public IEnumerable<SelectListItem> Departments { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> JobTitles { get; set; } = new List<SelectListItem>();
    }
}
