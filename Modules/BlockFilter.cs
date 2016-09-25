using System;
using System.Linq;
using System.Web;
using DAL;

namespace AdvSpareAuto.Modules
{
    using System;
    using System.Web;
    using System.Collections;

    public class BlockIPModule : IHttpModule
    {
        public String ModuleName
        {
            get { return "BlockIPModule"; }
        }

        // In the Init function, register for HttpApplication 
        // events by adding your handlers.
        public void Init(HttpApplication application)
        {
            application.BeginRequest += (new EventHandler(this.Application_BeginRequest));
            application.EndRequest += (new EventHandler(this.Application_EndRequest));
        }

        // Your BeginRequest event handler.
        private void Application_BeginRequest(Object source, EventArgs e)
        {
            HttpApplication application = (HttpApplication) source;
            HttpContext context = application.Context;
            
            var IP = context.Request.ServerVariables["REMOTE_ADDR"];
            if (
                (new[]
                {
                    104, 131, 132, 138, 140, 143, 148, 154, 158, 159, 167, 168, 170, 177, 186, 187, 189, 190, 192, 200, 201, 204, 207
                    
                }).Contains(int.Parse(IP.Split('.')[0])) && !context.Request.UrlReferrer.AbsolutePath.Contains("adv.spare-auto.com"))
            {
                using (var _advContext = new AdvContext())
                {

                    _advContext.Database.SqlQuery<AdvModel>("insert into dbo.SiteMessage (text) values ({0})", IP)
                        .FirstOrDefault();

                }

                context.Response.Redirect(@"http://adv.spare-auto.com/home/terms");

            }
        }

        // Your EndRequest event handler.
        private void Application_EndRequest(Object source, EventArgs e)
        {
            HttpApplication application = (HttpApplication)source;
            HttpContext context = application.Context;
            var IP = context.Request.ServerVariables["REMOTE_ADDR"];
            if (
                (new[]
                {
                    104, 131, 132, 138, 140, 143, 148, 154, 158, 159, 167, 168, 170, 177, 186, 187, 189, 190, 192, 200, 201, 204, 207
                    
                }).Contains(int.Parse(IP.Split('.')[0])))
            {
                using (var _advContext = new AdvContext())
                {

                    _advContext.Database.SqlQuery<AdvModel>("insert into dbo.SiteMessage (text) values ({0})", IP)
                        .FirstOrDefault();

                }
                context.Response.Clear();
                context.Response.Write("<hr><h1><font color=red>Becouse of investigating bot traffic, visitors from this country are temporary not allowed to this site</font></h1>");
            }
        }

        public void Dispose()
        {
        }
    }
}

