using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace GeniView.Cloud.Models
{
    /// <summary>
    /// Class that renders MVC views to a string using the
    /// standard MVC View Engine to render the view.
    /// 
    /// Requires that ASP.NET HttpContext is present to
    /// work, but works outside of the context of MVC
    /// 
    /// Particularly useful for rendering CSHTML for emails.
    /// 
    /// Code extracted from:
    /// https://github.com/RickStrahl/WestwindToolkit/blob/master/Westwind.Web.Mvc/Utils/ViewRenderer.cs
    /// </summary>
    public class ViewRenderer
    {

        protected ControllerContext Context { get; set; }

        public ViewRenderer(ControllerContext controllerContext = null)
        {
            if (controllerContext == null)
            {
                if (HttpContext.Current != null)
                    controllerContext = CreateController<EmptyController>().ControllerContext;
                else
                    throw new InvalidOperationException(
                        "ViewRenderer must run in the context of an ASP.NET " +
                        "Application and requires HttpContext.Current to be present.");
            }
            Context = controllerContext;
        }

        public string RenderViewToString(string viewPath, object model = null)
        {
            return RenderViewToStringInternal(viewPath, model, false);
        }

        public void RenderView(string viewPath, object model, TextWriter writer)
        {
            RenderViewToWriterInternal(viewPath, writer, model, false);
        }

        public string RenderPartialViewToString(string viewPath, object model = null)
        {
            return RenderViewToStringInternal(viewPath, model, true);
        }

        public void RenderPartialView(string viewPath, object model, TextWriter writer)
        {
            RenderViewToWriterInternal(viewPath, writer, model, true);
        }


        public static string RenderView(string viewPath, object model = null,
                                        ControllerContext controllerContext = null)
        {
            ViewRenderer renderer = new ViewRenderer(controllerContext);
            return renderer.RenderViewToString(viewPath, model);
        }


        public static void RenderView(string viewPath, TextWriter writer, object model,
                                        ControllerContext controllerContext)
        {
            ViewRenderer renderer = new ViewRenderer(controllerContext);
            renderer.RenderView(viewPath, model, writer);
        }


        public static string RenderView(string viewPath, object model,
                                        ControllerContext controllerContext,
                                        out string errorMessage)
        {
            errorMessage = null;
            try
            {
                ViewRenderer renderer = new ViewRenderer(controllerContext);
                return renderer.RenderViewToString(viewPath, model);
            }
            catch (Exception ex)
            {
                errorMessage = ex.GetBaseException().Message;
            }
            return null;
        }


        public static void RenderView(string viewPath, object model, TextWriter writer,
                                        ControllerContext controllerContext,
                                        out string errorMessage)
        {
            errorMessage = null;
            try
            {
                ViewRenderer renderer = new ViewRenderer(controllerContext);
                renderer.RenderView(viewPath, model, writer);
            }
            catch (Exception ex)
            {
                errorMessage = ex.GetBaseException().Message;
            }
        }

        public static string RenderPartialView(string viewPath, object model = null,
                                                ControllerContext controllerContext = null)
        {
            ViewRenderer renderer = new ViewRenderer(controllerContext);
            return renderer.RenderPartialViewToString(viewPath, model);
        }

        public static void RenderPartialView(string viewPath, TextWriter writer, object model = null,
                                                ControllerContext controllerContext = null)
        {
            ViewRenderer renderer = new ViewRenderer(controllerContext);
            renderer.RenderPartialView(viewPath, model, writer);
        }

        protected void RenderViewToWriterInternal(string viewPath, TextWriter writer, object model = null, bool partial = false)
        {
            ViewEngineResult viewEngineResult = null;
            if (partial)
                viewEngineResult = ViewEngines.Engines.FindPartialView(Context, viewPath);
            else
                viewEngineResult = ViewEngines.Engines.FindView(Context, viewPath, null);

            if (viewEngineResult == null)
                throw new FileNotFoundException();

            // get the view and attach the model to view data
            var view = viewEngineResult.View;
            Context.Controller.ViewData.Model = model;

            var ctx = new ViewContext(Context, view,
                                        Context.Controller.ViewData,
                                        Context.Controller.TempData,
                                        writer);
            view.Render(ctx, writer);
        }

        private string RenderViewToStringInternal(string viewPath, object model,
                                                    bool partial = false)
        {
            // first find the ViewEngine for this view
            ViewEngineResult viewEngineResult = null;
            if (partial)
                viewEngineResult = ViewEngines.Engines.FindPartialView(Context, viewPath);
            else
                viewEngineResult = ViewEngines.Engines.FindView(Context, viewPath, null);

            if (viewEngineResult == null || viewEngineResult.View == null)
            {
                throw new Exception("Can't find view.");
            }

            var view = viewEngineResult.View;
            Context.Controller.ViewData.Model = model;

            string result = null;

            using (var sw = new StringWriter())
            {
                var ctx = new ViewContext(Context, view,
                                            Context.Controller.ViewData,
                                            Context.Controller.TempData,
                                            sw);
                view.Render(ctx, sw);
                result = sw.ToString();
            }

            return result;
        }


        public static T CreateController<T>(RouteData routeData = null, params object[] parameters)
                    where T : Controller, new()
        {
            T controller = (T)Activator.CreateInstance(typeof(T), parameters);
            HttpContextBase wrapper = null;
            if (HttpContext.Current != null)
                wrapper = new HttpContextWrapper(System.Web.HttpContext.Current);
            else
                throw new InvalidOperationException(
                    "Can't create Controller Context if no active HttpContext instance is available.");

            if (routeData == null)
                routeData = new RouteData();

            if (!routeData.Values.ContainsKey("controller") && !routeData.Values.ContainsKey("Controller"))
                routeData.Values.Add("controller", controller.GetType().Name.ToLower().Replace("controller", ""));

            controller.ControllerContext = new ControllerContext(wrapper, routeData, controller);
            return controller;
        }

    }

    public class EmptyController : Controller
    {
    }
}
