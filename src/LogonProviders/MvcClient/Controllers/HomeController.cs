using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;

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

        public async Task<IActionResult> Logout()
        {
            // Only remove local site cookie
            await HttpContext.SignOutAsync("cookie");
            return RedirectToAction("Index");

            // Also Logout from Identity Server SSO
            //return SignOut("cookie", "oidc");
        }
    }
}