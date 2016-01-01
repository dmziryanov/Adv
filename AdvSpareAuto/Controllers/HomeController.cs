using System;
using System.Collections.Generic;
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

        public HomeController(IAdvRepository advRepository, IReviewRepository reviewRepository, IBlogRepository blogRepository)
        {
            _advRepository = advRepository;
            _reviewRepository = reviewRepository;
            _blogRepository = blogRepository;
        }

        public ActionResult Index()
        {
            ViewBag.Message = "Объявления, блоги, отзывы, подать объявление, создать блог, оставить отзыв";
            var adv = _advRepository.Get(0);
            adv.TopReview = _reviewRepository.First();
            adv.TopBlog = _blogRepository.First();
            return View(adv);
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your app description page.";
            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";
            return View();
        }

        public ActionResult PrivateOffice()
        {
            RegisterModel m = _advRepository.GetUser(WebSecurity.CurrentUserId);
            AccountModel model = new AccountModel() { User = m };
            return View(model);
        }

        public ActionResult FAQ()
        {
            return View();
        }

        public ActionResult Terms()
        {
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
