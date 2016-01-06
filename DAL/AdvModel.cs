using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DAL
{
    public class AdvModel : BaseModel
    {

        public AdvModel()
        {
            this.Imgs = new List<ImageFile>();
        }

        public RegisterModel CurrentUser;

        public AdvModel[] _featuredList;

        public string topAdsVisible
        {
            get { return IsFeatured == 3 ? "visibility:visible" : "visibility:hidden"; }
        }

        public string urgentAdsVisible
        {
            get { return IsFeatured == 2 ? "visibility:visible" : "visibility:hidden"; }
        }

        public string featuredAdsVisible
        {
            get { return IsFeatured == 4 ? "visibility:visible" : "visibility:hidden"; }
        }
        
        
        public IEnumerable<CategoryCount> Popular;
        
        public int Id { get; set; }
        public string Name { get; set; }
        public string LastColumn = "last-column";

        public string Description { get; set; }
        public string AdvTypeShortName
        {
            get { return Convert.ToString(type.GetDescription()[0]); }
        }

        public string AdvTypeFullName
        {
            get { return type.GetDescription(); }
        }

        public int[] ImgIds { get; set; }

        public List<ImageFile> Imgs { get; set; }
        public double Price { get; set; }
        public string PriceLabel
        {
            get
            {
                return

                    String.Format("{0:0,0,0}", Price) + " РУБ.";
            }
        }
        public int Location { get; set; }
        public string LocationName { get; set; }
        public int IsFeatured { get; set; }
        public int? SellerId { get; set; }
        public string SellerName { get; set; }
        public string SellerDistrict { get; set; }
        

        public DateTime JoinedDate { get; set; }

        public string SellerPhone { get; set; }

        public string SellerEmail { get; set; }


        public string CategoryName { get; set; }
        
        [Range(10, 1000, ErrorMessage = "Заполните категорию")]
        public int Category { get; set; }
        public string adsurl
        {
            get { return "/Adv/AdvDetails/" + Id; }
        }

        public string firstphoto
        {
            get { return ImgIds != null && ImgIds.Length > 0 ? "/Photo/Thumb/" + ImgIds[0] : ""; }
        }
        public bool Negotiable { get; set; }
        public DateTime Created { get; set; }

        public string CreatedText
        {
            get { return Created.ToString(); }
        }
        public AdvCondition condition { get; set; }
        public AdvType type { get; set; }

        public bool? IsTrusted { get; set; }
        public Review TopReview { get; set; }
        public Blog TopBlog { get; set; }
        public int ViewCount { get; set; }
        public string KeyWords { get; set; }
    }
}