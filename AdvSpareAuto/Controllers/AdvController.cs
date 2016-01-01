using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AdvSpareAuto.Models;
using DAL;
using WebMatrix.WebData;

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

                        model.Imgs.Add(new ImageFile() { FileBody = b, FileName = file.FileName, FileSize = b.Length });
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

        public ActionResult AdvDetails([BindParameter("id")] string AdvNo)
        {
            var adv = _advRepository.Get(Convert.ToInt32(AdvNo));
            _advRepository.IncreaseViewCount(Convert.ToInt32(AdvNo), adv.ViewCount);
            ViewBag.Message = adv.Name;
            return View(adv);
        }


        public ActionResult MyAdv()
        {
            List<AdvModel> adv = _advRepository.GetByUserId(WebSecurity.CurrentUserId);
            ViewBag.Message = "Мои объявления";
            return View(adv);
        }

        private void GetBooksRequestParams(out SortBy sortBy, out string filterString, out int currentPage)
        {
            sortBy = SortBy.None;

            if (Request.QueryString["SortBy"] == "По возрастанию")
            {
                sortBy = SortBy.PriceAsc;
            }

            if (Request.QueryString["SortBy"] == "По возрастанию")
            {
                sortBy = SortBy.PriceAsc;
            }

            filterString = Request.QueryString["Filter"];

            if (!(int.TryParse(Request.QueryString["currentPage"], out currentPage)))
                currentPage = 1;
        }

        [HttpGet, ActionName("GetData")]
        public JsonResult GetData()
        {
            int currentPage;
            string filterString;

            return Json(_advRepository.GetAll(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, ActionName("GetPagesData")]
        public JsonResult GetPagesData()
        {
            int currentPage;
            string filterString = "";
            var pageCount = _advRepository.GetFilteredSortedPageCount(0, 0, filterString, pageSize);
            return Json(Enumerable.Range(0, pageCount).ToList().Select(x => new { pagenum = x + 1 }), JsonRequestBehavior.AllowGet);
        }

        public ActionResult SaveAdv(int id)
        {
            throw new NotImplementedException();
        }
    }
}
