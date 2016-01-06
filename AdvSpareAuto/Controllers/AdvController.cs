using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Caching;
using System.Web;
using System.Web.Caching;
using System.Web.Mvc;
using AdvSpareAuto.Models;
using DAL;
using WebMatrix.WebData;
using CacheItemPriority = System.Web.Caching.CacheItemPriority;

namespace AdvSpareAuto.Controllers
{
    public class ParameterBinder : IModelBinder
    {
        public string ActualParameter { get; private set; }

        public ParameterBinder(string actualParameter)
        {
            this.ActualParameter = actualParameter;
        }

        public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            object id = controllerContext.RouteData.Values[this.ActualParameter];
            return id;
        }
    }

    [AttributeUsage(AttributeTargets.Parameter)]
    public class BindParameterAttribute : CustomModelBinderAttribute
    {
        public string ActualParameter { get; private set; }

        public BindParameterAttribute(string actualParameter)
        {
            this.ActualParameter = actualParameter;
        }

        public override IModelBinder GetBinder()
        {
            return new ParameterBinder(this.ActualParameter);
        }
    }

    public class AdvController : Controller
    {

        private IAdvRepository _advRepository;
        private int pageSize = 10;

        public AdvController(IAdvRepository advRepository)
        {
            _advRepository = advRepository;
        }

        [HttpGet]
        public ActionResult PostAdv()
        {
            var model = _advRepository.Get(0);
            model.CurrentUser = _advRepository.GetUser(WebSecurity.CurrentUserId);

            if (model.CurrentUser == null)
                model.CurrentUser = new RegisterModel();
            return View(model);
        }

        [HttpPost]
        public ActionResult PostAdv(AdvModel model)
        {
            return RedirectToAction("CreateAdv");
        }

        public ActionResult Help(AdvModel model)
        {
            return View();
        }

        public ActionResult Delete(int Id)
        {
            var model = _advRepository.Get(Id);
            if (model.SellerId == WebSecurity.CurrentUserId)
            {
                _advRepository.Delete(Id);
                return View("Delete");
            }
            else
            {
                return View("Error");
            }
        }

        public ActionResult Edit(int Id)
        {
            var model = _advRepository.Get(Id);
            model.CurrentUser = _advRepository.GetUser(WebSecurity.CurrentUserId);
            if (model.SellerId == WebSecurity.CurrentUserId)
            {
                return View(model);
            }
            else
            {
                return View("Error");
            }
        }


        [HttpPost]
        public ActionResult CreateAdv(AdvModel model)
        {
            if (model.Category == 0) return View("Error", new ErrorInfo() { message = "Заполните категорию" });

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

                        model.Imgs.Add(new ImageFile() { FileBody = b, FileName = file.FileName, FileSize = b.Length, Created = DateTime.Now });
                    }
                }
                catch (Exception ex)
                {
                    return Json(new { Message = "Error in saving file" });
                }
            }
            _advRepository.Save(model);

            return View("Success");
        }

        [OutputCache(Location = System.Web.UI.OutputCacheLocation.Any, Duration = 60)]
        public ActionResult AdvDetails([BindParameter("id")] string AdvNo)
        {
            var adv = _advRepository.Get(Convert.ToInt32(AdvNo));
            _advRepository.IncreaseViewCount(Convert.ToInt32(AdvNo), adv.ViewCount);
            ViewBag.Message = string.Format("Купить {0} в {1}", adv.Name, adv.LocationName);
            return View(adv);
        }


        public ActionResult MyAdv()
        {
            List<AdvModel> adv = _advRepository.GetByUserId(WebSecurity.CurrentUserId);
            ViewBag.Message = "Мои объявления";
            return View(adv);
        }

        private void GetBooksRequestParams(out SortBy sortBy, out string keywords, out int currentPage)
        {
            sortBy = SortBy.None;

            if (Request.QueryString["SortBy"] == "По возрастанию")
            {
                sortBy = SortBy.PriceAsc;
            }

            if (Request.QueryString["SortBy"] == "По убыванию")
            {
                sortBy = SortBy.PriceDesc;
            }

            keywords = Request.QueryString["keywords"];

            if (!(int.TryParse(Request.QueryString["currentPage"], out currentPage)))
                currentPage = 1;
        }

        [HttpGet, ActionName("GetData")]
        public JsonResult GetData(String sortBy, string keywords, int currentPage, int location, string country, string category, string minPrice, string maxPrice, string type, int condition)
        {

            // GetBooksRequestParams(out SortBy sortBy, out string keywords, out int currentPage);

            var res = (IEnumerable<AdvModel>)HttpRuntime.Cache.Get(sortBy + keywords + location + country + category + minPrice + maxPrice + type + condition);

            return Json(res.ToList().Skip(pageSize * (currentPage - 1)).Take(pageSize), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, ActionName("GetPagesData")]
        public JsonResult GetPagesData(String sortBy, string keywords, int currentPage, int location, string country, string category, string minPrice, string maxPrice, string type, int condition)
        {
            AdvModel adv;
            adv = _advRepository.Get(Convert.ToInt32(0));
            AdvType atype = 0;
            if (type == "allAds")
                atype = AdvType.All;

            if (type == "businessAds")
                atype = AdvType.All;

            if (type == "personalAds")
                atype = AdvType.All;

            string OrderExpr = " ";
            if (sortBy.Contains("от большей к меньшей"))
                OrderExpr = "Order by Price Desc";

            if (sortBy.Contains("от меньшей к большей"))
                OrderExpr = "Order by Price asc";

            if (sortBy.Contains("сначала новые"))
                OrderExpr = "Order by created asc";

            if (sortBy.Contains("сначала старые"))
                OrderExpr = "Order by created desc";

            IEnumerable<AdvModel> result = new List<AdvModel>();

            result = _advRepository.GetFilteredSortedPageResult(atype, (AdvCondition)condition, keywords, pageSize, currentPage, OrderExpr).ToList();
            if (result.Count() == 0)
            {

                 
                var cat_id = adv._subCategories.Where(x => x.Name.ToUpper().Contains(keywords.ToUpper())).FirstOrDefault(); // Засплитить и поискать по всем словам

                if (cat_id != null)
                    result = _advRepository.GetFilteredSortedPageResult(atype, (AdvCondition)condition, cat_id.Category_Id, pageSize, currentPage, OrderExpr).ToList();
            }
            Array.ForEach(result.ToArray(), model =>
            {
                using (var _advContext = new AdvContext())
                {
                    model.ImgIds =
                        _advContext.Database.SqlQuery<int>("select PhotoId from dbo.advPhoto where AdvId ={0}", model.Id)
                            .ToArray();
                }

                model.LocationName = adv._locations.FirstOrDefault(x => x.CityId == model.Location).Name;
                model.CategoryName = adv._subCategories.FirstOrDefault(x => x.ID == model.Category).Name;
            });
            HttpRuntime.Cache.Add(sortBy + keywords + location + country + category + minPrice + maxPrice + type + condition, result, null, DateTime.Now + TimeSpan.FromMinutes(1), TimeSpan.Zero, CacheItemPriority.High, null);
            return Json(Enumerable.Range(0, result.Count() / pageSize + 1).ToList().Select(x => new { pagenum = x + 1 }), JsonRequestBehavior.AllowGet);
        }

        public ActionResult SaveAdv(int id)
        {
            throw new NotImplementedException();
        }
    }
}
