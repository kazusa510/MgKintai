using Kintai.Models;
using Kintai.Models.Session;
using Microsoft.AspNetCore.Http;
using System.Data.SqlClient;

namespace Kintai.Managers
{
    public static class LoginManager
    {
        private const string DEBUG_REFRESH_TOKEN = "microgear-develop-token";
        private const string DEBUG_EMAIL = "developer@microgear.co.jp";
        private const string DEBUG_NAME = "開発 太郎";

        private static GoogleOAuthClient googleOAuth = new GoogleOAuthClient(
            clientId: "999999999999-XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX.apps.googleusercontent.com",
            clientSecret: "YYYYYYYYYYYYYY-ZZZZZZZZZ");

        /// <summary>
        /// ログイン済みかどうか
        /// </summary>
        /// <param name="httpContext">HttpContext</param>
        /// <returns></returns>
        public static bool IsLogin(HttpContext httpContext)
        {
            return (httpContext.Session.GetObject<Account>(Account.SESSION_KEY) is Account account && account.IsLogin);
        }

        /// <summary>
        /// GoogleOAuth認証画面へのURLを取得
        /// </summary>
        /// <param name="callback">認証後のコールバックURL</param>
        /// <returns></returns>
        public static string GetGoogleLoginUrl(string callback)
        {
            return googleOAuth.GetAuthUrl(callback);
        }

        /// <summary>
        /// GoogleOAuth認証コードを元にログインする
        /// </summary>
        /// <param name="httpContext">HttpContext</param>
        /// <param name="authorizationCode">認証コード</param>
        /// <param name="callback">コールバックURL</param>
        /// <param name="message">ログイン結果メッセージ</param>
        /// <returns>ログイン成否</returns>
        public static bool LoginByAuthorizationCode(HttpContext httpContext, string authorizationCode, string callback, out string message)
        {
            // アクセストークンとリフレッシュトークンを取得
            var token = googleOAuth.GetAccessToken(authorizationCode, callback, out string refreshToken);
            // リフレッシュトークンはクッキーに保持
            httpContext.Response.Cookies.Append("refresh_token", refreshToken);
            // アクセストークンを使ってログイン処理を実施
            return ExecuteLogin(httpContext, token, out message);
        }

        /// <summary>
        /// クッキー情報を元にログインする
        /// </summary>
        /// <param name="httpContext">HttpContext</param>
        /// <returns>ログイン成否</returns>
        public static bool LoginByCookie(HttpContext httpContext)
        {
            // クッキーからリフレッシュトークンが取得できるならそのままログイン
            if (httpContext.Request.Cookies["refresh_token"] != null)
            {
#if DEBUG
                if (httpContext.Request.Cookies["refresh_token"] == DEBUG_REFRESH_TOKEN) return ExecuteLogin(httpContext, null, out _);
#endif

                // リフレッシュトークンを使ってアクセストークンを取得
                var token = googleOAuth.GetAccessToken(httpContext.Request.Cookies["refresh_token"]);
                // アクセストークンを使ってログイン処理を実施
                return ExecuteLogin(httpContext, token, out _);
            }
            else
            {
                return false;
            }
        }

#if DEBUG
        /// <summary>
        /// デバッグモードログイン
        /// </summary>
        /// <param name="httpContext">HttpContext</param>
        /// <returns></returns>
        public static bool DebugLogin(HttpContext httpContext)
        {
            // DEBUG用のリフレッシュトークンはクッキーに保持
            httpContext.Response.Cookies.Append("refresh_token", DEBUG_REFRESH_TOKEN);
            return ExecuteLogin(httpContext, null, out _);
        }
#endif

        private static bool ExecuteLogin(HttpContext httpContext, string accessToken, out string message)
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
            httpContext.Session.SetObject<Account>(Account.SESSION_KEY, new LoginAccount(email, name));

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
