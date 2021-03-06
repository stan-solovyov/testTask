﻿using System.ComponentModel.DataAnnotations;
using OAuth2;
using OAuth2.Client;
using System.Linq;
using System.Web.Mvc;
using Domain.EF;
using Domain.Entities;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Web;
using PriceNotifier.AuthFilter;
using PriceNotifier.Infrostructure;
using PriceNotifier.ViewModels;
using User = Domain.Entities.User;

namespace PriceNotifier.Controllers
{
    public class HomeController : BaseMVCController
    {
        private readonly AuthorizationRoot _authorizationRoot;
        private readonly UserContext _db;

        public string GetHashString(string s)
        {
            //переводим строку в байт-массим  
            byte[] bytes = Encoding.Unicode.GetBytes(s);

            //создаем объект для получения средст шифрования  
            MD5CryptoServiceProvider csp = new MD5CryptoServiceProvider();

            //вычисляем хеш-представление в байтах  
            byte[] byteHash = csp.ComputeHash(bytes);
            string hash = string.Empty;

            //формируем одну цельную строку из массива  
            foreach (byte b in byteHash)
            {
                hash += $"{b:x2}";
            }

            return hash;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HomeController"/> class.
        /// </summary>
        /// <param name="authorizationRoot">The authorization manager.</param>
        /// <param name="db"></param>
        public HomeController(AuthorizationRoot authorizationRoot, UserContext db)
        {
            _authorizationRoot = authorizationRoot;
            _db = db;
        }

        public ActionResult Index()
        {
            if (Request.Cookies.AllKeys.Contains("Token"))
            {
                var token = ControllerContext.HttpContext.Request.Cookies["Token"].Value;
                var user = _db.Users.FirstOrDefault(c => c.Token == token);
                if (user != null)
                {
                    var roles = user.UserRoles.Select(c => c.Role.Name).ToArray();
                    System.Web.HttpContext.Current.User = new GenericPrincipal(new GenericIdentity(user.Username), roles);
                }

                if (token != null)
                {
                    ViewData["token"] = token;
                }
                if (!string.IsNullOrEmpty(user?.Email))
                {
                    return View();
                }
            }
            return RedirectToAction("Login");
        }


        [CookieAuthorize]
        [HttpGet]
        public ActionResult Email()
        {
            var userId = GetCurrentUserId(Request);
            var user = _db.Users.FirstOrDefault(c => c.Id == userId);
            return View(user);
        }

        [CookieAuthorize]
        [HttpPost]
        public ActionResult Email(string email)
        {
            var foo = new EmailAddressAttribute();
            var userId = GetCurrentUserId(Request);
            var user = _db.Users.FirstOrDefault(c => c.Id == userId);
            if (foo.IsValid(email))
            {
                if (user != null)
                {
                    user.Email = email;
                }

                _db.SaveChanges();
                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError("Invalid e-mail", "Email should not be empty.");
            return View(user);
        }

        /// <summary>
        /// Renders home page with login link.
        /// </summary>
        public ActionResult Login()
        {
            var model = _authorizationRoot.Clients.Select(client => new LoginInfoModel
            {
                ProviderName = client.Name,
                LoginLinkUri = client.GetLoginLinkUri()
            });
            return View(model);
        }

        /// <summary>
        /// Renders information received from authentication service.
        /// </summary>
        public ActionResult Auth(string providerName)
        {
            var a = GetClient(providerName).GetUserInfo(Request.QueryString);

            User user = new User
            {
                SocialNetworkName = a.ProviderName,
                Username = a.FirstName,
                SocialNetworkUserId = a.Id,
                Token = "",
                Email = null
            };

            if (_db.Users != null)
            {
                var userid = _db.Users.FirstOrDefault(c => c.SocialNetworkUserId == user.SocialNetworkUserId);

                if (userid == null)
                {
                    _db.Users.Add(user);
                    _db.SaveChanges();
                    var roleId = _db.Roles.FirstOrDefault(c=>c.Name == "User").Id;
                    user.UserRoles.Add(new UserRole { RoleId = roleId, UserId = user.Id });
                }
                else
                {
                    userid.SocialNetworkUserId = user.SocialNetworkUserId;
                    userid.SocialNetworkName = user.SocialNetworkName;
                    userid.Username = user.Username;
                    _db.SaveChanges();
                }
            }

            var userFound = _db.Users.FirstOrDefault(c => c.SocialNetworkUserId == user.SocialNetworkUserId);
            if (userFound != null)
            {
                userFound.Token = GetHashString(user.Id + user.SocialNetworkName + user.SocialNetworkUserId);
                _db.SaveChanges();
                HttpCookie cookie = new HttpCookie("Token");
                cookie.Value = userFound.Token;
                ControllerContext.HttpContext.Response.Cookies.Add(cookie);
                if (string.IsNullOrEmpty(userFound.Email))
                {
                    return RedirectToAction("Email", "Home");
                }

                return RedirectToAction("Index", "Home");
            }
            return RedirectToAction("Login", "Home");
        }
        private IClient GetClient(string providerName)
        {
            return _authorizationRoot.Clients.First(c => c.Name == providerName);
        }
    }
}
