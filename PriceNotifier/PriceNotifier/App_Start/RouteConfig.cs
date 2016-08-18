﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Security.Cryptography;

namespace PriceNotifier
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                "Login", // Route name
                "login/{providerName}", // URL with parameters
                new
                {
                    controller = "Home",
                    action = "Login",
                    providerName = UrlParameter.Optional
                });

            routes.MapRoute(
                name: "AuthRoute",
                url: "Home/Auth/{providerName}",
                defaults: new { controller = "Home", action = "Auth", providerName = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Login", id = UrlParameter.Optional }
            );

        }
    }
}
