using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Castle.Core.Resource;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Castle.Windsor.Configuration.Interpreters;
using Ioc_Windsor;
using WebMatrix.WebData;

namespace AdvSpareAuto
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class AdvApplication : System.Web.HttpApplication, IContainerAccessor
    {
        private static IWindsorContainer _container;
        public IWindsorContainer Container
        {
            get { return _container; }
        }

        public AdvApplication()
        {

        }
        protected void Application_Start()
        {
           // WebSecurity.InitializeDatabaseConnection("DefaultConnection", "System.Data.SQlClient", "UserId", "UserName", true);
            var filename = "assembly://AdvSpareAuto/windsor.config.xml";
            IResource resource = new AssemblyResource(filename);
            _container = new WindsorContainer(new XmlInterpreter(resource));


            //add all of the controllers to the container
            foreach (var type in Assembly.GetExecutingAssembly().GetTypes())
            {
                if (typeof(IController).IsAssignableFrom(type))
                {
                    _container.Register(Component.For(type).LifestylePerWebRequest());
                }
            }

            ControllerBuilder.Current.SetControllerFactory(new WindsorControllerFactory());
            AreaRegistration.RegisterAllAreas();

            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            AuthConfig.RegisterAuth();
        }
    }
}