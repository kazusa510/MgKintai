using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Kintai.Models.Session
{
    [Serializable]
    public abstract class Account
    {
        public const string SESSION_KEY = "Account";

        public bool IsLogin => this is LoginAccount;
    }

    [Serializable]
    public class LogoutAccount : Account
    {
        //public IDictionary<string, string> RouteValues { get; }

        //public LogoutAccount(string returnContrller, string returnAction)
        //{
        //    RouteValues = new Dictionary<string, string> { { "controller", returnContrller }, { "action", returnAction } };
        //}

        //public LogoutAccount(IDictionary<string,string> routeValues)
        //{
        //    RouteValues = routeValues;
        //}
    }

    [Serializable]
    public class LoginAccount : Account
    {
        public string Email { get; }
        public string Name { get; }

        public LoginAccount(string email, string name)
        {
            Email = email;
            Name = name;
        }
    }
}
