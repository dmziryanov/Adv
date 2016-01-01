using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Migrations;
using System.Data.Entity.Validation;
using System.Linq;
using System.Text;
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

        public int GetFilteredSortedPageCount(AdvType type, AdvCondition condition, string filterString, int pageSize)
        {
            using (_advContext = new AdvContext())
            {
                _tmpList = _advContext.Database.SqlQuery<AdvModel>("select * from dbo.adv").ToList();
            }

            return _tmpList.Count() / pageSize + 1;
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

            using (_advContext = new AdvContext())
            {
                model._featuredList =
                    _advContext.Database.SqlQuery<AdvModel>("select * from dbo.adv where IsFeatured = 4").ToArray();
            }

            using (_advContext = new AdvContext())
            {
                model.Popular =
                    _advContext.Database.SqlQuery<CategoryCount>(
                        "select b.Name, count(*) as Count from dbo.adv a, dbo.SubCategory b where a.Category = b.Id Group By b.Name")
                        .ToArray();
            }



            Array.ForEach(model._featuredList, a =>
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

        public void Save(AdvModel advModel)
        {
            //TODO: сделать адв как часть AdvModel;
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
                    SellerId = advModel.SellerId
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
    }

    public abstract class BaseRepository
    {
        public AdvContext _advContext;
        public CommonContext _commonContext;
        public static IEnumerable<AdvModel> _tmpList;
        public static IEnumerable<Location> _locations;
        public static IEnumerable<Category> _subCategories;
        public static IEnumerable<Category> _categories;
        public static IEnumerable<RegisterModel> _users;

        public BaseRepository()
        {

            using (_advContext = new AdvContext())
            {
                _subCategories = _advContext.Database.SqlQuery<Category>("select * from dbo.SubCategory").ToList();
                _categories = _advContext.Database.SqlQuery<Category>("select * from dbo.Category").ToList();
                _users = _advContext.Database.SqlQuery<RegisterModel>("select * from dbo.UserProfile a, [dbo].[webpages_Membership] b where a.UserId = b.UserId").ToList();
            }

            using (_commonContext = new CommonContext())
            {
                _locations = _commonContext.Database.SqlQuery<Location>("select * from dbo.Cities").ToList();
            }

        }
    }
}