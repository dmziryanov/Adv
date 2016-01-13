using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DAL;
using WebMatrix.WebData;


namespace AdvSpareAuto.Controllers
{
    public class BlogController : Controller
    {
        private readonly IBlogRepository _blogRepository;
        private readonly IImageRepository _advRepository;

        public BlogController(IBlogRepository blogRepository, IImageRepository advRepository)
        {
            _blogRepository = blogRepository;
            _advRepository = advRepository;
        }

        //
        // GET: /Blog/

        public ActionResult Index()
        {
            return View(_blogRepository.GetAll());
        }

        public ActionResult Create()
        {
            ViewBag.Message = "Блог, Блоги, создать блог";
            var b = new BlogModel() { Blog = new Blog(), _categories = BaseRepository._categories};
            return View(b);
        }

        public ActionResult PostBlog(Blog model)
        {
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

                        model.ImageId = _advRepository.Save((new ImageFile() { FileBody = b, FileName = file.FileName, FileSize = b.Length, Created = DateTime.Now}));
                        model.UserId = WebSecurity.CurrentUserId;
                    }
                }
                catch (Exception ex)
                {
                    return Json(new { Message = "Error in saving file" });
                }
            }
            
            _blogRepository.Add(model);
            return View("Success", model);
        }

        public ActionResult Post(BlogPost model)
        {
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

                        model.MainPhotoId = _advRepository.Save((new ImageFile() { FileBody = b, FileName = file.FileName, FileSize = b.Length, Created = DateTime.Now }));
                    }
                }
                catch (Exception ex)
                {
                    return Json(new { Message = "Error in saving file" });
                }
            }
            
            
 //           model.UserId = WebSecurity.CurrentUserId;
            _blogRepository.AddPost(model);
            return RedirectToAction("BlogDetails","Blog", new { Id= model.BlogId});
        }

        public ActionResult BlogDetails(int id)
        {

            var blog = _blogRepository.Get(id);
            var m = new BlogModel() {Blog = blog };
            m._categories = AdvRepository._categories;
            m.Recent = _blogRepository.GetRecent();
                
            ViewBag.Message = "Блог " + blog.Name;
            return View(m);
        }

        public ActionResult MyBlog(int id)
        {

            var blogs = _blogRepository.GetBlogsByUserId(id);

            ViewBag.Message = "Мои Блоги";
            return View("MyBlog", blogs);
        }



        public ActionResult CreatePost(int id)
        {
            return View(new BlogPost() { BlogId = id});
        }

        public ActionResult PostComment(int PostId, string PostComment)
        {
            var model = new BlogComment()
            {
                CreatedDate = DateTime.Now,
                PostId = PostId,
                ParentId = 0,
                PostComment = PostComment,
                UserId = WebSecurity.CurrentUserId
            };

            _blogRepository.AddComment(model);
            var post = _blogRepository.GetPost(PostId); //To do get blog by post id
            var _blog = _blogRepository.Get(post.BlogId);

            var BlogComments = _blogRepository.GetComments(model.PostId);
            var BlogCommentModels =
                BlogComments.Select(x => new BlogCommentModel() { Comment = x, UserAvatarId = 10, UserName = "Tmp" }).ToList();
            var blogPostModel = new BlogPostModel() { Post = post, BlogCommentModels = BlogCommentModels, Blog = _blog };
            blogPostModel.Recent = _blogRepository.GetRecent();
            blogPostModel._categories = AdvRepository._categories;
            ViewBag.Message = blogPostModel.Blog.Name + " " + blogPostModel.Post.Topic;
            return View("BlogPost", blogPostModel);

        }


        public ActionResult BlogPost(int id)
        {
            var post = _blogRepository.GetPost(id); //To do get blog by post id
            var _blog = _blogRepository.Get(post.BlogId);
            
            var BlogComments = _blogRepository.GetComments(id);
            var BlogCommentModels =
                BlogComments.Select(x => new BlogCommentModel() { Comment = x, UserAvatarId = 10, UserName = "Tmp" }).ToList();
            var model = new BlogPostModel() { Post = post, BlogCommentModels = BlogCommentModels, Blog = _blog };
            model.Recent = _blogRepository.GetRecent();
            model._categories = AdvRepository._categories;
            return View(model);
        }

        public ActionResult UserBlog(int id)
        {
            return View("BlogDetails", new BlogModel() { UserPosts = _blogRepository.GetByUserId(id) });
        }
    }
}
