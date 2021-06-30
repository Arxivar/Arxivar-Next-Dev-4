using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;

namespace MvcCode.Controllers
{
    public class HomeController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public HomeController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [AllowAnonymous]
        public IActionResult Index() => View();

        [Authorize]
        public IActionResult Secure() => View();

        public IActionResult Logout() => SignOut("cookie", "oidc");
    }
}