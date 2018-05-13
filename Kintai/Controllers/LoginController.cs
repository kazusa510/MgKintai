using Kintai.Managers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Kintai.Controllers
{
    public class LoginController : Controller
    {
        public string CallbackUrl => $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}{Url.Action("Callback", "Login")}";

        [HttpGet]
        public IActionResult Index()
        {
            // ログイン済み、もしくはクッキーの情報でログイン可能ならデフォルト画面へ遷移
            if (LoginManager.IsLogin(HttpContext) || LoginManager.LoginByCookie(HttpContext))
                return RedirectToAction("Index", "Home");

            // ログインできなければログインを促す画面を表示
            ViewData["Title"] = "ログイン";
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Login")]
        public IActionResult Post()
        {
#if DEBUG
            LoginManager.DebugLogin(HttpContext);
            return RedirectToAction("Index", "Home");
#else
            // GoogleのOAuth2.0認証画面にリダイレクト
            return Redirect(LoginManager.GetGoogleLoginUrl(CallbackUrl));
#endif
        }

        [HttpGet]
        public IActionResult Callback(string code)
        {
            // GoogleのOAuth認証画面からのリダイレクト処理
            if (LoginManager.LoginByAuthorizationCode(HttpContext, code, CallbackUrl, out string message))
            {
                return RedirectToAction("Index", "Home");
            }
            else
            {
                TempData["message"] = message;
                return RedirectToAction("Index");
            }
        }
    }
}
