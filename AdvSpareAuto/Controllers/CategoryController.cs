using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DAL;
using WebMatrix.WebData;

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

        [OutputCache(Location = System.Web.UI.OutputCacheLocation.Any, Duration = 60)]
        public ActionResult Search(string keywords, string location)
        {
            //В этом методе только заполняется модель и берется первое объявление, основной список заполняется асинхронно в  CategoryController
            _advRepository.SaveSearch(WebSecurity.CurrentUserId, keywords, location);
            var adv = _advRepository.Get(0);
            adv.KeyWords = keywords;
            if (!string.IsNullOrEmpty(location))
            {
                if (adv._locations.Where(x => x.Name.Contains(location)).FirstOrDefault() != null)
                    adv.LocationName = adv._locations.Where(x => x.Name.Contains(location)).FirstOrDefault().Name;
                else
                {
                    int i = 0;
                    if (int.TryParse(location, out i))
                    {
                        if (adv._locations.Where(x => x.CityId == i).FirstOrDefault() != null)
                            adv.LocationName =
                                adv._locations.Where(x => x.CityId == Convert.ToInt32(location)).FirstOrDefault().Name;
                    }
                    else
                    {
                        adv.LocationName = "Такого города нет базе данных, выберите другой город, ближайший к вам";
                    }
                }
            }
            
                

            return View(adv);
        }
    }
}
