using Kintai.Models;
using Kintai.Models.Session;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace Kintai.Managers
{
    public static class LoginManager
    {
        private const string DEBUG_REFRESH_TOKEN = "microgear-develop-token";
        private const string DEBUG_EMAIL = "developer@microgear.co.jp";
        private const string DEBUG_NAME = "開発 太郎";

        private static GoogleOAuthClient googleOAuth = new GoogleOAuthClient(
            clientId: "806346248877-lkple2av52ef1m4v43tg6flg97sqrh0e.apps.googleusercontent.com",
            clientSecret: "H5sJ9N6RznP33g-V0JAJ4n5H");

        public static bool IsLogin(HttpContext context)
        {
            return (context.Session.GetObject<Account>(Account.SESSION_KEY) is Account account && account.IsLogin);
        }

        public static bool LoginByCookie(HttpContext context)
        {
            // クッキーからリフレッシュトークンが取得できるならそのままログイン
            if (context.Request.Cookies["refresh_token"] != null)
            {
#if DEBUG
                if (context.Request.Cookies["refresh_token"] == DEBUG_REFRESH_TOKEN) return ExecuteLogin(context, null, out _);
#endif

                // リフレッシュトークンを使ってアクセストークンを取得
                var token = googleOAuth.GetAccessToken(context.Request.Cookies["refresh_token"]);
                // アクセストークンを使ってログイン処理を実施
                return ExecuteLogin(context, token, out _);
            }
            else
            {
                return false;
            }
        }

        public static string GetGoogleLoginUrl(string callback)
        {
            return googleOAuth.GetAuthUrl(callback);
        }

        public static bool LoginByAuthorizationCode(HttpContext context, string code, string callback, out string message)
        {
            // アクセストークンとリフレッシュトークンを取得
            var token = googleOAuth.GetAccessToken(code, callback, out string refreshToken);
            // リフレッシュトークンはクッキーに保持
            context.Response.Cookies.Append("refresh_token", refreshToken);
            // アクセストークンを使ってログイン処理を実施
            return ExecuteLogin(context, token, out message);
        }

#if DEBUG
        public static bool DebugLogin(HttpContext context)
        {
            // DEBUG用のリフレッシュトークンはクッキーに保持
            context.Response.Cookies.Append("refresh_token", DEBUG_REFRESH_TOKEN);
            return ExecuteLogin(context, null, out _);
        }
#endif

        private static bool ExecuteLogin(HttpContext context, string accessToken, out string message)
        {
            // メールアドレス、名前を取得
            var email = accessToken != null ? googleOAuth.GetEmail(accessToken) : DEBUG_EMAIL;
            var name = GetUserName(email);

            // 名前が取得できなければログイン失敗
            if (name == null)
            {
                message = $"{email} は ユーザー登録されていないためログインできません。";
                return false;
            }

            // アカウント情報をセッションに保存
            context.Session.SetObject<Account>(Account.SESSION_KEY, new LoginAccount(email, name));

            message = $"{email} が ログインしました。";

            return true;
        }

        private static string GetUserName(string email)
        {
            // デバッグならデバッグユーザーの名前を
            if (email == DEBUG_EMAIL) return DEBUG_NAME;

            string conn = "Server=(localdb)\\mssqllocaldb;Database=DataKintai;Trusted_Connection=True;MultipleActiveResultSets=true";
            using (var connection = new SqlConnection(conn))
            using (var command = new SqlCommand("SELECT * FROM Users WHERE Email=@Email", connection))
            {
                command.Parameters.Add(new SqlParameter("Email", email));
                connection.Open();
                var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    return reader.GetString(2);
                }
            }
            return null;
        }
    }
}
