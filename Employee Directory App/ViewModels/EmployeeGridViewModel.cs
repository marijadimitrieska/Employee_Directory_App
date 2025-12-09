using System.Text.Json.Serialization;

namespace Employee_Directory_App.ViewModels
{
    public class EmployeeGridViewModel
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime HireDate { get; set; }
        public string Department { get; set; }
        public string JobTitle { get; set; }
        public bool IsActive { get; set; }
      
        public string? ProfilePhoto { get; set; }
        //public string ImageExtension { get; set; }
        //public string ProfilePhotoString => ProfilePhoto != null && !string.IsNullOrEmpty(ImageExtension)
        //    ? $"data:image/{ImageExtension.Replace(".", "")};base64,{Convert.ToBase64String(ProfilePhoto)}" : null;
    }


}
