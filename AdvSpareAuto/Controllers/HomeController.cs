using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AdvSpareAuto.Filters;
using AdvSpareAuto.Models;
using DAL;
using WebMatrix.WebData;

namespace AdvSpareAuto.Controllers
{
    [InitializeSimpleMembership]
    public class HomeController : Controller
    {
        private readonly IAdvRepository _advRepository;
        private readonly IReviewRepository _reviewRepository;
        private readonly IBlogRepository _blogRepository;
        private IImageRepository _imageRepository;

        public HomeController(IAdvRepository advRepository, IReviewRepository reviewRepository, IBlogRepository blogRepository, IImageRepository imageRepository)
        {
            _advRepository = advRepository;
            _reviewRepository = reviewRepository;
            _blogRepository = blogRepository;
            _imageRepository = imageRepository;
        }

        // [OutputCache(Location = System.Web.UI.OutputCacheLocation.Any, Duration = 60)]
        public ActionResult Index()
        {
            ViewBag.Message = "Объявления, блоги, отзывы, подать объявление, создать блог, оставить отзыв";
            var adv = _advRepository.Get(0);
            adv.TopReview = _reviewRepository.First();
            adv.TopBlog = _blogRepository.First();
            ViewBag.Keywords = "объявления,отзывы,блоги,создать блог, подать объявление, объявления о продаже,объявления по продаже,продажа авто бу";
            return View(adv);
        }

        public ActionResult AdsSale()
        {
            ViewBag.Message = "Купить баннер, перетяжка, брендинг";
            ViewBag.Keywords = "объявления,отзывы,блоги,создать блог, подать объявление, объявления о продаже,объявления по продаже,продажа авто бу";
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "О нас";
            ViewBag.Keywords = "объявления,отзывы,блоги,создать блог, подать объявление, объявления о продаже,объявления по продаже,продажа авто бу";
            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Контактная информация";
            ViewBag.Keywords = "объявления,отзывы,блоги,создать блог, подать объявление, объявления о продаже,объявления по продаже,продажа авто бу";
            return View();
        }

        public ActionResult PrivateOffice()
        {
            RegisterModel m = _advRepository.GetUser(WebSecurity.CurrentUserId);
            AccountModel model = new AccountModel() { User = m };
            model.ViewCount = _advRepository.GetViewCountByUserId(m.UserId);
            model.FavoriteCount = _advRepository.GetFavoriteCountByUserId(m.UserId);
            model.AdvCount = _advRepository.GetAdvCountByUserId(m.UserId);
            var d = (DateTime?)Session["GetLoginDate"];
            model.LastLogin = d ?? DateTime.Now;
            return View(model);
        }

        public ActionResult TerminateAccount()
        {
            return View();
        }

        public ActionResult TerminateAccountSuccess(int inlineRadioOptions)
        {
            if (inlineRadioOptions == 0)
            {
                _advRepository.CloseAccount(WebSecurity.CurrentUserId, inlineRadioOptions);
            
            WebSecurity.Logout();
            return View("Success");
        }
            RegisterModel m = _advRepository.GetUser(WebSecurity.CurrentUserId);
            AccountModel model = new AccountModel() { User = m };
            model.ViewCount = _advRepository.GetViewCountByUserId(m.UserId);
            model.FavoriteCount = _advRepository.GetFavoriteCountByUserId(m.UserId);
            model.AdvCount = _advRepository.GetAdvCountByUserId(m.UserId);
            var d = (DateTime?)Session["GetLoginDate"];
            model.LastLogin = d ?? DateTime.Now;
            return View("PrivateOffice", model);
    }

        public ActionResult UpdateProfile(RegisterModel m)
        {
            RegisterModel m2 = _advRepository.GetUser(WebSecurity.CurrentUserId);
            AccountModel model = new AccountModel() { User = m };
            model.ViewCount = _advRepository.GetViewCountByUserId(m.UserId);
            model.FavoriteCount = _advRepository.GetFavoriteCountByUserId(m.UserId);
            model.AdvCount = _advRepository.GetAdvCountByUserId(m.UserId);
            var d = (DateTime?)Session["GetLoginDate"];
            model.LastLogin = d ?? DateTime.Now;
            foreach (var fileKey in Request.Files.AllKeys)
            {
                var file = Request.Files[fileKey];
                try
                {
                    if (file != null && file.InputStream.Length > 0)
                    {
                        var fileName = Path.GetFileName(file.FileName);
                        var b = new byte[file.InputStream.Length];
                        file.InputStream.Read(b, 0, (int)file.InputStream.Length);

                        var photoId = _imageRepository.Save(
                               (new ImageFile()
                               {
                                   FileBody = b,
                                   FileName = file.FileName,
                                   FileSize = b.Length,
                                   Created = DateTime.Now
                               }));
                        m.UserAvatarId = photoId;
                    }
                }
                catch (Exception ex)
                {
                    return View("PrivateOffice", model);
                }
            }
            var profile = new UserProfile()
            {
                FirstName = m.FirstName,
                LastName = m.LastName,
                UserAvatarId = m.UserAvatarId > 0 ? m.UserAvatarId : m2.UserAvatarId,
                Gender = m2.Gender,
                UserName = m.UserName,
                UserType = m.UserType,
                About = m2.About,
                HideNumber = m2.HideNumber,
                Phone = m2.Phone,
                UserId = m2.UserId
            };
            model.User.UserAvatarId = profile.UserAvatarId;
            _advRepository.SaveUser(profile);
            return View("PrivateOffice", model);
        }


        public ActionResult FAQ()
        {
            ViewBag.Keywords = "объявления,отзывы,блоги,создать блог, подать объявление, объявления о продаже,объявления по продаже,продажа авто бу";
            return View();
        }

        public ActionResult Terms()
        {
            ViewBag.Keywords = "объявления,отзывы,блоги,создать блог, подать объявление, объявления о продаже,объявления по продаже,продажа авто бу";
            return View();
        }

        public ActionResult PlaceBanner()
        {
            ViewBag.Keywords = "объявления,отзывы,блоги,создать блог, подать объявление, объявления о продаже,объявления по продаже,продажа авто бу";
            return View();
        }


        public ActionResult SendMessage(SiteMessage model)
        {
            model.Time = DateTime.Now;
            _advRepository.AddMessage(model);
            return View("SuccessMessage");
        }

    }


}
