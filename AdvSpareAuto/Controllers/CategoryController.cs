using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DAL;

namespace AdvSpareAuto.Controllers
{
    public class CategoryController : Controller
    {

        private readonly ICategoryRepository _categoryRepository;
        private IAdvRepository _advRepository;

        public CategoryController(ICategoryRepository categoryRepository, IAdvRepository advRepository)
        {
            _categoryRepository = categoryRepository;
            _advRepository = advRepository;
        }

        public JsonResult Index()
        {
            return Json(_categoryRepository.GetAll(), JsonRequestBehavior.AllowGet);
        }

        public ActionResult Search()
        {
            var adv = _advRepository.Get(0);
            return View(adv);
        }
    }
}
