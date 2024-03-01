using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Security;

namespace ITHELPDESK.CookieExtension
{
    public static class CookieManage
    {
        public static string encryptedCookie(string value)
        {
            var cookieText = Encoding.UTF8.GetBytes(value);
            var encryptedValue = Convert.ToBase64String(MachineKey.Protect(cookieText, "ProtectCookie"));
            return encryptedValue;
        }

        public static string decryptedCookie(string value)
        {
            var bytes = Convert.FromBase64String(value);
            var output = MachineKey.Unprotect(bytes, "ProtectCookie");
            string result = Encoding.UTF8.GetString(output);
            return result;
        }
    }
}