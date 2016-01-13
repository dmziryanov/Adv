using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Migrations;
using System.Data.Entity.Validation;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace DAL
{
    public class AdvRepository : BaseRepository, IAdvRepository
    {


        public AdvRepository()
        {

        }

        public IEnumerable<AdvModel> GetAll()
        {
            Array.ForEach(_tmpList.ToArray(), model =>
            {
                using (_advContext = new AdvContext())
                {
                    model.ImgIds =
                        _advContext.Database.SqlQuery<int>("select PhotoId from dbo.advPhoto where AdvId ={0}", model.Id)
                            .ToArray();
                }
                model.LocationName = _locations.FirstOrDefault(x => x.CityId == model.Location).Name;
                model.CategoryName = _subCategories.FirstOrDefault(x => x.ID == model.Category).Name;
            });

            return _tmpList;
        }

        public IEnumerable<AdvModel> GetFilteredSortedPageResult(AdvType type, AdvCondition condition, string keywords, string country, int location, int pageSize, int currentPage, string OrderExpr, double minPrice, double maxPrice)
        {
            if (string.IsNullOrEmpty(OrderExpr.Trim()))
                OrderExpr = "Order by Name";

            var queryExprCount = "Select MAX(k) From (select ROW_NUMBER() OVER (PARTITION BY 1 " + OrderExpr + " ) k from dbo.Adv a where type > -1 ";
            if (type > 0)
                queryExprCount += "and type = " + (int)type;

            if (condition > 0)
                queryExprCount += "and condition = " + (int)condition;

            if (location > 0)
                queryExprCount += " and location = " + location;

            if (minPrice < maxPrice)
                queryExprCount += " and Price >=" + minPrice;

            if (maxPrice > 0)
                queryExprCount += " and Price <= " + maxPrice;

            int cid = 0;
            if (int.TryParse(country, out cid))
                if (cid > 0)
                    queryExprCount += " and country = " + cid + " ";

            if (!string.IsNullOrEmpty(keywords))
            {
                var arr = keywords.Split(' ', ';', ',', '/');
                foreach (var VARIABLE in arr)
                {
                    queryExprCount += " and name like '%" + VARIABLE +"%' ";    
                }
                
            }

            queryExprCount += ") b";

            var queryExpr = "Select * From (select ROW_NUMBER() OVER (PARTITION BY 1 " + OrderExpr + " ) k, a.* from dbo.Adv a where type > -1 ";
            if (type > 0)
                queryExpr += "and type = " + (int)type;

            if (condition > 0)
                queryExpr += "and condition = " + (int)condition;

            if (location > 0)
                queryExpr += " and location = " + location;

            if (minPrice < maxPrice)
                queryExpr += " and Price >=" + minPrice;

            if (maxPrice > 0)
                queryExpr += " and Price <= " + maxPrice;

          
            if (int.TryParse(country, out cid))
                if (cid > 0)
                    queryExpr += "and country = " + cid + " ";

            if (!string.IsNullOrEmpty(keywords))
            {
                var arr = keywords.Split(' ', ';', ',', '/');
                foreach (var VARIABLE in arr)
                {
                    queryExpr += "and name like '%" + VARIABLE + "%' ";
                }
            }
            queryExpr += String.Format(") b WHERE k >= {0} and k < {1}", (currentPage - 1) * pageSize, currentPage * pageSize);

            using (_advContext = new AdvContext())
            {
                var i = _advContext.Database.SqlQuery<long?>(queryExprCount).FirstOrDefault();
                var col = _advContext.Database.SqlQuery<AdvModel>(queryExpr).ToList();
                if (col.FirstOrDefault() != null)
                    col.FirstOrDefault().CurrentSearchPageCount = i.HasValue ? i.Value : 0;
                return col;
            }
        }

        public IEnumerable<AdvModel> GetFilteredSortedPageResult(AdvType type, AdvCondition condition, List<int> cat, string country, int location, int pageSize, int currentPage, string OrderExpr, double minPrice, double maxPrice)
        {

            if (string.IsNullOrEmpty(OrderExpr.Trim()))
                OrderExpr = "Order by Name";

            var queryExprCount = "Select MAX(k) From (select ROW_NUMBER() OVER (PARTITION BY 1 " + OrderExpr + " ) k from dbo.Adv a where type > -1 ";
            if (type > 0)
                queryExprCount += " and type = " + (int)type;

            if (condition > 0)
                queryExprCount += " and condition = " + (int)condition;

            if (minPrice < maxPrice)
                queryExprCount += " and Price >=" + minPrice;

            if (maxPrice > 0)
                queryExprCount += " and Price <= " + maxPrice;

            if (location > 0)
                queryExprCount += " and location = " + location;

            if (cat != null && cat.Count > 0)
            {
                queryExprCount += " and subcategory = ANY  (";
                
                foreach (var VARIABLE in cat)
                {
                    queryExprCount += "select " + VARIABLE + " UNION ALL ";
                }
                
                queryExprCount += "SELECT 0) ";
            }

           

            int cid = 0;
            if (int.TryParse(country, out cid))
                if (cid > 0)
                    queryExprCount += "and country = " + cid ;
            
            queryExprCount += ") b";

            var queryExpr = "Select * From (select ROW_NUMBER() OVER (PARTITION BY 1 " + OrderExpr + " ) k, a.* from dbo.Adv a where type > -1 ";
            if (type > 0)
                queryExpr += " and type = " + (int)type;

            if (condition > 0)
                queryExpr += " and condition = " + (int)condition;

            if (location > 0)
                queryExpr += " and location = " + location;

            if (minPrice < maxPrice)
                queryExpr += " and Price >=" + minPrice;

            if (maxPrice > 0)
                queryExpr += " and Price <= " + maxPrice;

            if (cat != null && cat.Count > 0)
            {
                queryExpr += "and subcategory = ANY  (";
                foreach (var VARIABLE in cat)
                {
                    queryExpr += "select " + VARIABLE + " UNION ALL ";
                }
                queryExpr += "SELECT 0) ";
            }

            
            
                if (cid > 0)
                    queryExpr += " and country = " + cid + " ";

                queryExpr += String.Format(") b WHERE k >= {0} and k < {1}", (currentPage - 1) * pageSize, currentPage * pageSize);

            using (_advContext = new AdvContext())
            {
                var i = _advContext.Database.SqlQuery<long?>(queryExprCount).FirstOrDefault();
                var col = _advContext.Database.SqlQuery<AdvModel>(queryExpr).ToList();
                if (col.FirstOrDefault() != null)
                    col.FirstOrDefault().CurrentSearchPageCount = i.HasValue ? i.Value : 0;
                return col;
            }
        }

        public IEnumerable<AdvModel> GetFilteredSortedPageResult(AdvType type, AdvCondition condition, int cat, string keywords, string country, int location, int pageSize, int currentPage, string OrderExpr, double minPrice, double maxPrice)
        {
            if (string.IsNullOrEmpty(OrderExpr.Trim()))
                OrderExpr = "Order by Name";

            var queryExprCount = "Select MAX(k) From (select ROW_NUMBER() OVER (PARTITION BY 1 " + OrderExpr + " ) k from dbo.Adv a where type > -1 ";
            if (type > 0)
                queryExprCount += " and type = " + (int)type;

            if (condition > 0)
                queryExprCount += " and condition = " + (int)condition;

            if (location > 0)
                queryExprCount += " and location = " + location;

            if (minPrice < maxPrice)
                queryExprCount += " and Price >=" + minPrice;

            if (maxPrice > 0)
                queryExprCount += " and Price <= " + maxPrice;

            int cid = 0;
            if (int.TryParse(country, out cid))
                if (cid > 0)
                    queryExprCount += " and country = " + cid + " ";

            if (!string.IsNullOrEmpty(keywords))
                queryExprCount += string.Format(" and name like {0} ", "'%" + keywords + "%'");

            queryExprCount += String.Format("and subcategory = " + cat + ") b");

            
            var queryExpr = "Select * From (select ROW_NUMBER() OVER (PARTITION BY 1 " + OrderExpr + " ) k, a.* from dbo.Adv a where type > -1 ";
            if (type > 0)
                queryExpr += " and type = " + (int)type;

            if (condition > 0)
                queryExpr += " and condition = " + (int)condition;

            if (location > 0)
                queryExpr += " and location = " + location;

            if (minPrice < maxPrice)
                queryExpr += " and Price >=" + minPrice;

            if (maxPrice > 0)
                queryExpr += " and Price <= " + maxPrice;

            cid = 0;
            if (int.TryParse(country, out cid))
                if (cid > 0)
                    queryExpr += " and country = " + cid + " ";

            if (!string.IsNullOrEmpty(keywords))
                queryExpr += string.Format(" and name like {0} ", "'%" + keywords + "%'");

            queryExpr += String.Format(" and subcategory = " + cat + ") b WHERE k >= {0} and k < {1}", (currentPage - 1) * pageSize, currentPage * pageSize);

            
            using (_advContext = new AdvContext())
            {
                var i = _advContext.Database.SqlQuery<long?>(queryExprCount).FirstOrDefault();
                var col = _advContext.Database.SqlQuery<AdvModel>(queryExpr).ToList();
                if (col.FirstOrDefault() != null) 
                    col.FirstOrDefault().CurrentSearchPageCount = i.HasValue ? i.Value : 0;
                return col;
            }
        }

        public AdvModel Get(int id)
        {
            var model = new AdvModel();
            if (id != 0)
            {
                using (_advContext = new AdvContext())
                {
                    model = _advContext.Database.SqlQuery<AdvModel>("select * from dbo.adv where id = {0}", id).FirstOrDefault();
                    model.ImgIds =
                        _advContext.Database.SqlQuery<int>("select PhotoId from dbo.advPhoto where AdvId = {0}",
                            model.Id).ToArray();
                }
                model.LocationName = _locations.FirstOrDefault(x => x.CityId == model.Location).Name;
                model.CategoryName = _subCategories.FirstOrDefault(x => x.ID == model.Category).Name;
            }

            model._subCategories = _subCategories;
            model._categories = _categories;
            model._locations = _locations.OrderBy(x => x.Name);
            model._countries = _countries.OrderBy(x => x.CountryName);

            model._simalarList = GetSimilar(model);
            

            using (_advContext = new AdvContext())
            {
                Random rnd = new Random();
                model._featuredList = _advContext.Database.SqlQuery<AdvModel>("select * from dbo.adv where IsFeatured = 4").ToArray().OrderBy(x => rnd.Next()).ToArray();
            }

            using (_advContext = new AdvContext())
            {
                model.Popular =
                    _advContext.Database.SqlQuery<CategoryCount>(
                        "select b.id, b.Name, count(*) as Count from dbo.adv a, dbo.SubCategory b where a.Category = b.Id Group By b.id, b.Name")
                        .ToArray();
            }


            Array.ForEach(model._featuredList.Concat(model._simalarList).ToArray(), a =>
                {
                    using (_advContext = new AdvContext())
                    {
                        a.ImgIds =
                            _advContext.Database.SqlQuery<int>("select PhotoId from dbo.advPhoto where AdvId ={0}", a.Id)
                                .ToArray();
                    }
                    a.LocationName = _locations.FirstOrDefault(x => x.CityId == a.Location).Name;
                    a.CategoryName = _subCategories.FirstOrDefault(x => x.ID == a.Category).Name;
                });

            return model;
        }

        public IEnumerable<AdvModel> GetSimilar(AdvModel sample)
        {
            using (_advContext = new AdvContext())
            {
                var deltap = sample.Price*0.1;
                return _advContext.Database.SqlQuery<AdvModel>("select * from dbo.adv where category = {0} and price > ({2} - {1}) and price < ({2} + {1}) and id <> {3}", sample.SubCategory, deltap, sample.Price, sample.Id).ToList();
            }
        }

        public CarAdvModel GetCar(int id)
        {
            var model = new CarAdvModel();
            using (_advContext = new AdvContext())
            {
                using (_advContext = new AdvContext())
                {
                    model = _advContext.Database.SqlQuery<CarAdvModel>("select * from dbo.adv, dbo.caradv where caradv.id = adv.id and adv.id = {0}", id).FirstOrDefault();
                    if (model != null)
                    model.ImgIds =
                        _advContext.Database.SqlQuery<int>("select PhotoId from dbo.advPhoto where AdvId = {0}",
                            model.Id).ToArray();
                 
                }
                if (model != null)
                {
                    model.LocationName = _locations.FirstOrDefault(x => x.CityId == model.Location).Name;
                    model.CategoryName = _subCategories.FirstOrDefault(x => x.ID == model.Category).Name;
                }

            }
            return model;
        }

        public void Save(AdvModel advModel)
        {
            //TODO: ������� ��� ��� ����� AdvModel;
            using (var advContext = new AdvContext())
            {
                var a = new Adv()
                {
                    Category = advModel.Category,
                    Description = advModel.Description,
                    Condition = (int)advModel.condition,
                    Price = advModel.Price,
                    Location = advModel.Location,
                    Name = advModel.Name,
                    Negotiable = advModel.Negotiable,
                    type = (int)advModel.type,
                    SubCategory = advModel.Category,
                    SellerEmail = advModel.SellerEmail,
                    SellerPhone = advModel.SellerPhone,
                    SellerName = advModel.SellerName,
                    SellerDistrict = advModel.SellerDistrict,
                    IsFeatured = advModel.IsFeatured,
                    SellerId = advModel.SellerId,
                    Country = advModel.Country,
                    Currency = advModel.Currency
                };
                advContext.ImageFiles.AddOrUpdate(advModel.Imgs.ToArray());
                advContext.SaveChanges();
                advContext.advs.AddOrUpdate(a);
                advContext.SaveChanges();
                advContext.AdvPhotos.AddOrUpdate(
                    advModel.Imgs.Select(x => new AdvPhoto() { AdvId = a.Id, PhotoId = x.FileID }).ToArray());
                advContext.SaveChanges();
            }
        }

        public void AddMessage(SiteMessage model)
        {
            using (var advContext = new AdvContext())
            {
                try
                {
                    advContext.SiteMessages.AddOrUpdate(model);
                    advContext.SaveChanges();
                }

                catch (DbEntityValidationException ex)
                {
                    StringBuilder sb = new StringBuilder();

                    foreach (var failure in ex.EntityValidationErrors)
                    {
                        sb.AppendFormat("{0} failed validation\n", failure.Entry.Entity.GetType());
                        foreach (var error in failure.ValidationErrors)
                        {
                            sb.AppendFormat("- {0} : {1}", error.PropertyName, error.ErrorMessage);
                            sb.AppendLine();
                        }
                    }

                    throw new DbEntityValidationException(
                        "Entity Validation Failed - errors follow:\n" +
                        sb.ToString(), ex
                        ); // Add the original exception as the innerException
                }

            }
        }

        public RegisterModel GetUser(int currentUserId)
        {
            using (_advContext = new AdvContext())
            {
                return _advContext.Database.SqlQuery<RegisterModel>("select * from dbo.UserProfile a, [dbo].[webpages_Membership] b where a.UserId = b.UserId and a.UserId = {0}", currentUserId).FirstOrDefault();
            }
        }

        public void SaveUser(UserProfile model)
        {
            using (var _ctx = new UsersContext())
            {
                try
                {
                    _ctx.Database.ExecuteSqlCommand("Update dbo.UserProfile SET About = {1}, Gender={2}, HideNumber={3}, Phone={4}, LastName={5}, FirstName={6}, UserType = {7}  where UserName =  {0}",
                        model.UserName, model.About, model.Gender, model.HideNumber, model.Phone, model.LastName, model.FirstName, model.UserType);
                    _ctx.SaveChanges();
                }
                catch (DbEntityValidationException ex)
                {
                    StringBuilder sb = new StringBuilder();

                    foreach (var failure in ex.EntityValidationErrors)
                    {
                        sb.AppendFormat("{0} failed validation\n", failure.Entry.Entity.GetType());
                        foreach (var error in failure.ValidationErrors)
                        {
                            sb.AppendFormat("- {0} : {1}", error.PropertyName, error.ErrorMessage);
                            sb.AppendLine();
                        }
                    }

                    throw new DbEntityValidationException(
                        "Entity Validation Failed - errors follow:\n" +
                        sb.ToString(), ex
                        ); // Add the original exception as the innerException
                }


            }
        }

        public List<AdvModel> GetByUserId(int currentUserId)
        {
            using (var _ctx = new UsersContext())
            {
                var advs = _ctx.Database.SqlQuery<AdvModel>("select * from dbo.adv where SellerId = {0}", currentUserId).ToList();
                Array.ForEach(advs.ToArray(), model =>
                {
                    using (_advContext = new AdvContext())
                    {
                        model.ImgIds =
                            _advContext.Database.SqlQuery<int>("select PhotoId from dbo.advPhoto where AdvId = {0}",
                                model.Id).ToArray();
                    }
                });
                return advs;
            }
        }

        public void Delete(int id)
        {
            using (var _ctx = new UsersContext())
            {
                _ctx.Database.ExecuteSqlCommand("delete from dbo.adv where Id = {0}", id);
            }
        }

        public void IncreaseViewCount(int toInt32, int viewCount)
        {
            using (var _ctx = new UsersContext())
            {
                _ctx.Database.ExecuteSqlCommand("update dbo.adv set ViewCount={1} where Id = {0}", toInt32, viewCount + 1);
            }
        }

        public void SaveSearch(int currentUserId, string keywords, string location)
        {
            using (var _ctx = new AdvContext())
            {
                _ctx.Database.ExecuteSqlCommand("insert into UserSearch (currentUserId,keywords, location) values ({0},{1},{2})", currentUserId, keywords, location);
            }
        }

        public void SaveCar(CarAdvModel advModel)
        {
            //TODO: ������� ��� ��� ����� AdvModel;
            using (var advContext = new AdvContext())
            {
                var a = new CarAdv()
                {
                    Category = advModel.Category,
                    Description = advModel.Description,
                    Condition = (int)advModel.condition,
                    Price = advModel.Price,
                    Location = advModel.Location,
                    Name = advModel.Name,
                    Negotiable = advModel.Negotiable,
                    type = (int)advModel.type,
                    SubCategory = advModel.Category,
                    SellerEmail = advModel.SellerEmail,
                    SellerPhone = advModel.SellerPhone,
                    SellerName = advModel.SellerName,
                    SellerDistrict = advModel.SellerDistrict,
                    IsFeatured = advModel.IsFeatured,
                    SellerId = advModel.SellerId,
                    Country = advModel.Country,
                    MileAge = advModel.MileAge,
                    PTS = advModel.PTS,
                    Year = advModel.Year,
                    customs = advModel.customs,
                    guarantee = advModel.guarantee,
                    VIN = advModel.VIN,
                    Currency = advModel.Currency,
                    Brand = advModel.Brand,
                    CarModel = advModel.CarModel,
                    CarType = advModel.CarType
                };
                advContext.ImageFiles.AddOrUpdate(advModel.Imgs.ToArray());
                advContext.SaveChanges();
                advContext.caradvs.AddOrUpdate(a);
                advContext.SaveChanges();
                advContext.AdvPhotos.AddOrUpdate(
                    advModel.Imgs.Select(x => new AdvPhoto() { AdvId = a.Id, PhotoId = x.FileID }).ToArray());
                advContext.SaveChanges();
            }
        }

        public IEnumerable<AdvModel> GetFilteredSortedPageResult(AdvType atype, AdvCondition advCondition, int location, int category, string keywords,
            int pageSize, int currentPage, string OrderExpr)
        {
            var queryExpr = "select * from dbo.Adv where type > -1 ";
            if (atype > 0)
                queryExpr += " and type = " + (int)atype;

            if (advCondition > 0)
                queryExpr += " and condition = " + (int)advCondition;

            queryExpr += " and subcategory = " + category;

            if (!string.IsNullOrEmpty(keywords))
                queryExpr += string.Format(" and name like {0} ", "'%" + keywords + "%'");

            using (_advContext = new AdvContext())
            {
                return _advContext.Database.SqlQuery<AdvModel>(queryExpr + OrderExpr).ToList();
            }
        }
    }

    public abstract class BaseRepository
    {
        public AdvContext _advContext;
        public CommonContext _commonContext;
        public static IEnumerable<AdvModel> _tmpList;
        public static IEnumerable<Location> _locations;
        public static IEnumerable<Country> _countries;
        public static IEnumerable<Category> _subCategories;
        public static IEnumerable<Category> _categories;
        public static IEnumerable<RegisterModel> _users;

        public BaseRepository()
        {
            using (_advContext = new AdvContext())
            {
                _subCategories = _advContext.Database.SqlQuery<Category>("select * from dbo.SubCategory").ToList();
                _categories = _advContext.Database.SqlQuery<Category>("select a.Id, a.Name, IconName, Count(NULLIF(c.id, 0)) as Count from dbo.Category a LEFT JOIN dbo.SubCategory b ON a.id = b.category_id LEFT JOIN dbo.Adv c ON b.id = c.subcategory group by a.Id, a.Name, IconName Order by a.Name").ToList();
                _users = _advContext.Database.SqlQuery<RegisterModel>("select * from dbo.UserProfile a, [dbo].[webpages_Membership] b where a.UserId = b.UserId").ToList();
            }

            using (_commonContext = new CommonContext())
            {
                _locations = _commonContext.Database.SqlQuery<Location>("select * from dbo.Cities").ToList();
                _countries = _commonContext.Database.SqlQuery<Country>("select * from dbo.Country").ToList();

            }

        }
    }
}