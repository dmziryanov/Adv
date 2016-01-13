using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RmsAuto.TechDoc;
using RmsAuto.TechDoc.Entities.TecdocBase;

namespace AdvSpareAuto.Controllers
{
    public class CarController : Controller
    {
        public static List<Manufacturer> mfg;
        public static Dictionary<int, List<Model>> m = new Dictionary<int, List<Model>>();
        public static Dictionary<int, List<CarType>> ct = new Dictionary<int, List<CarType>>();
        //
        // GET: /Car/

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Brand(int? id)
        {
            if (mfg == null)
                mfg = Facade.ListManufacturers().ToList();
                
            return View(mfg);
        }

        public ActionResult Model(int id)
        {
            List<Model> n = new List<Model>();
            if (m.TryGetValue(id, out n))
                return View(n);
            else
            {
                m.Add(id, Facade.ListModels(id));
                m.TryGetValue(id, out n);
                return View(n);
            }
        }
        public ActionResult CarType(int id)
        {
            List<CarType> n = new List<CarType>();
            if (ct.TryGetValue(id, out n))
                return View(n);
            else
            {
                ct.Add(id, Facade.ListModifications(id));
                ct.TryGetValue(id, out n);
                return View(n);
            }
        }

        public ActionResult CarDescription(int id)
        {
            var n = Facade.GetModification(id, true, new List<int>());
            return View(n);

        }
    }
}
