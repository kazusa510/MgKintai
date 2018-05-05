using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kintai.Models.Session;

namespace Kintai.Filters
{
    public class LoginFilterAttribute : ActionFilterAttribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            // 引数はnull不可
            if (context == null) throw new ArgumentNullException(nameof(context));

            // ログイン済みならそのままAction実行
            if (context.HttpContext.Session.GetObject<Account>(Account.SESSION_KEY) is Account account && account.IsLogin) return;

            // ログインしていなければログイン画面へ遷移
            var descriptor = (ControllerActionDescriptor)context.ActionDescriptor;
            context.HttpContext.Session.SetObject<Account>(Account.SESSION_KEY, new LogoutAccount(descriptor.RouteValues));
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
