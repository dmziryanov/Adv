using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web.Mvc;

namespace DAL
{
    public interface IAdvRepository
    {
        IEnumerable<AdvModel> GetAll();
        /*AdvModel GetFilteredSortedPageResult(AdvType type, AdvCondition condition, string keywords, int pageSize, double priceMin, double priceMax, int location, string country);*/

        IEnumerable<AdvModel> GetFilteredSortedPageResult(AdvType type, AdvCondition condition, string keywords, int pageSize, int currentPage, string SortBy);
        IEnumerable<AdvModel> GetFilteredSortedPageResult(AdvType type, AdvCondition condition, int cat, int pageSize, int currentPage, string SortBy);
        AdvModel Get(int id);
        void Save(AdvModel adv);
        void AddMessage(SiteMessage model);
        RegisterModel GetUser(int currentUserId);
        void SaveUser(UserProfile model);
        List<AdvModel> GetByUserId(int currentUserId);
        void Delete(int id);
        void IncreaseViewCount(int toInt32, int viewCount);
        void SaveSearch(int currentUserId, string keywords, string location);
    }

    [Table("UserProfile")]
    public class UserProfile
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserId { get; set; }
        
        [Required]
        [Display(Name = "User name")]
        public string UserName { get; set; }

        [Display(Name = "User name")]
        public string About { get; set; }

        [Display(Name = "User name")]
        public int Gender { get; set; }


        public bool HideNumber { get; set; }

        [Required]
        public string Phone { get; set; }
        [Required]
        public string LastName { get; set; }
        [Required]
        public string FirstName { get; set; }

        public int UserType { get; set; }
    }

    public class RegisterModel
    {
        [Key]
        public int UserId { get; set; }

        public int UserType { get; set; }

        [Required]
        [Display(Name = "User name")]
        public string UserName { get; set; }

        [Display(Name = "About")]
        public string About { get; set; }

        [Display(Name = "Gender")]
        public int Gender { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        public bool HideNumber { get; set; }

        [Required]
        public string Phone { get; set; }
        [Required]
        public string LastName { get; set; }
        [Required]
        public string FirstName { get; set; }

    }
}