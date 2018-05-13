using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Kintai.Models
{
    public class GoogleOAuthClient
    {
        public string ClientId { get; private set; } = "";
        public string ClientSecret { get; private set; } = "";

        public GoogleOAuthClient(string clientId, string clientSecret)
        {
            ClientId = clientId;
            ClientSecret = clientSecret;
        }

        /// <summary>
        /// グーグル認証画面のURLを取得
        /// </summary>
        /// <param name="callback">コールバック</param>
        /// <returns>URL</returns>
        public string GetAuthUrl(string callback)
        {
            string scope = Uri.EscapeDataString("https://www.googleapis.com/auth/userinfo.email");
            return $"https://accounts.google.com/o/oauth2/auth?scope={scope}&redirect_uri={callback}&response_type=code&client_id={ClientId}&approval_prompt=force&access_type=offline";
        }

        public string GetAccessToken(string authorizationCode, string callback, out string refreshToken)
        {
            // 送信データ
            byte[] postDataBytes = Encoding.UTF8.GetBytes(
                $"code={authorizationCode}&client_id={ClientId}&client_secret={ClientSecret}&redirect_uri={callback}&grant_type=authorization_code");

            // トークンを取得
            var tokenObj = GetToken(postDataBytes);

            // リフレッシュトークンはoutパラメータに、アクセストークンは戻り値に設定
            refreshToken = tokenObj["refresh_token"].ToString();
            return tokenObj["access_token"].ToString();
        }

        public string GetAccessToken(string refreshToken)
        {
            // 送信データ
            byte[] postDataBytes = Encoding.UTF8.GetBytes(
                $"refresh_token={refreshToken}&client_id={ClientId}&client_secret={ClientSecret}&grant_type=refresh_token");

            // トークンを取得
            var tokenObj = GetToken(postDataBytes);

            // アクセストークンを戻り値に設定
            return tokenObj["access_token"].ToString();
        }

        public string GetEmail(string accessToken)
        {
            // アクセストークンを使いメールアドレスを取得
            var uri = new Uri("https://www.googleapis.com/oauth2/v2/userinfo?access_token=" + accessToken);
            var req = (HttpWebRequest)WebRequest.Create(uri);

            // レスポンスの取得 
            var res = req.GetResponse();
            var resStream = res.GetResponseStream();
            var sr = new StreamReader(resStream, Encoding.UTF8);
            var jsonStr = sr.ReadToEnd();
            sr.Close();
            var jObject = JsonConvert.DeserializeObject(jsonStr) as JObject;
            return jObject["email"].ToString();
        }


        private JObject GetToken(byte[] postData)
        {
            // 送信先の設定 
            Uri uri = new Uri("https://accounts.google.com/o/oauth2/token");
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(uri);
            req.Method = "POST";
            req.ContentType = "application/x-www-form-urlencoded";
            req.ContentLength = postData.Length;
            req.AllowAutoRedirect = false;

            // リクエストの送信 
            System.IO.Stream reqStream = req.GetRequestStream();
            reqStream.Write(postData, 0, postData.Length);
            reqStream.Close();

            // レスポンスの取得 
            System.Net.WebResponse res = req.GetResponse();
            System.IO.Stream resStream = res.GetResponseStream();
            System.IO.StreamReader sr = new System.IO.StreamReader(resStream, System.Text.Encoding.GetEncoding("UTF-8"));
            string jsonStr = sr.ReadToEnd();
            sr.Close();

            // レスポンスからトークンを取得
            return JsonConvert.DeserializeObject(jsonStr) as JObject;
        }
    }
}
