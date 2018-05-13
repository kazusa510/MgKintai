using Kintai.Managers;
using Kintai.Models.Session;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;

namespace Kintai.Filters
{
    public class LoginFilterAttribute : ActionFilterAttribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            // 引数はnull不可
            if (context == null) throw new ArgumentNullException(nameof(context));

            // ログイン済みならそのままAction実行
            if (LoginManager.IsLogin(context.HttpContext)) return;

            // クッキー情報からログインできるならログインしてAction実行
            if (LoginManager.LoginByCookie(context.HttpContext)) return;

            // ログインできなければログイン画面へ
            context.Result = new RedirectToActionResult("Index", "Login", null);
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);

            // ユーザー名を取得してViewBagに詰める
            var account = context.HttpContext.Session.GetObject<Account>(Account.SESSION_KEY) as LoginAccount;
            ((Controller)context.Controller).ViewBag.UserName = account?.Name ?? account?.Email;
        }
    }
}
