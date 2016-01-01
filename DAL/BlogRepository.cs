using System;
using System.Collections.Generic;
using System.Linq;

namespace DAL
{
    class BlogRepository : BaseRepository, IBlogRepository
    {
        private AdvContext _advContext;

        public BlogRepository()
        {
            
        }

        public Blog Get(int id)
        {
            using (_advContext = new AdvContext())
            {
                var res = _advContext.Database.SqlQuery<Blog>("Select * From dbo.Blog where Id = {0}", id).FirstOrDefault();
                res.Posts = _advContext.Database.SqlQuery<BlogPost>("Select * From dbo.BlogPost where BlogId = {0}", id).ToList();
                return res;
            }
        }

        public List<Blog> GetAll()
        {
            using (_advContext = new AdvContext())
            {
               return _advContext.Database.SqlQuery<Blog>("Select * From dbo.Blog").ToList();
            }
        }

        public void Add(Blog model)
        {
            using (_advContext = new AdvContext())
            {
                _advContext.Blogs.Add(model);
                _advContext.SaveChanges();
            }
        }

        public void AddPost(BlogPost model)
        {
            using (_advContext = new AdvContext())
            {
                model.LastEditDate = DateTime.Now;
                _advContext.Posts.Add(model);
                _advContext.SaveChanges();
            }
        }


        public Blog First()
        {
            using (_advContext = new AdvContext())
            {
                var res = _advContext.Database.SqlQuery<Blog>("Select * From dbo.Blog").FirstOrDefault();
                return res;
            }
        }

        public BlogPost GetPost(int id)
        {
            using (_advContext = new AdvContext())
            {
                return
                    _advContext.Database.SqlQuery<BlogPost>("Select * From dbo.BlogPost where Id = {0}", id)
                        .FirstOrDefault();
            }
        }

        public List<BlogPost> GetByUserId(int id)
        {
            using (_advContext = new AdvContext())
            {
                return _advContext.Database.SqlQuery<BlogPost>("Select * From dbo.BlogPost where UserId = {0}", id).ToList();
            }
        }

        public List<Blog> GetBlogsByUserId(int id)
        {
            using (_advContext = new AdvContext())
            {
                return _advContext.Database.SqlQuery<Blog>("Select * From dbo.Blog where UserId = {0}", id).ToList();
            }
        }

        public List<BlogComment> GetComments(int postId)
        {
            using (_advContext = new AdvContext())
            {
                return _advContext.Database.SqlQuery<BlogComment>("Select * From dbo.BlogComment where postId = {0}", postId).ToList();
            }
        }

        public IEnumerable<BlogPost> GetRecent()
        {
            using (_advContext = new AdvContext())
            {
                return _advContext.Database.SqlQuery<BlogPost>("Select TOP 10 * From dbo.BlogPost order by LastEditDate desc").ToList();
            }
        }

        public void AddComment(BlogComment model)
        {
            using (_advContext = new AdvContext())
            {
                _advContext.Comments.Add(model);
                _advContext.SaveChanges();
            }
        }
    }
}