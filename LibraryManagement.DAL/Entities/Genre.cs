
using System.ComponentModel.DataAnnotations;


namespace LibraryManagement.DAL
{
    public enum Genre
    {
        [Display(Name = "Unknown")] Unknown = 0,
        [Display(Name = "SoftwareEngineering")] SoftwareEngineering = 1,
        [Display(Name = "Mystery")] Mystery = 2,
        [Display(Name = "Thriller")] Thriller = 3,
        [Display(Name = "Romance")] Romance = 4,
        [Display(Name = "History")] History = 5,
        [Display(Name = "Drama")] Drama = 6,
       
    }
}
