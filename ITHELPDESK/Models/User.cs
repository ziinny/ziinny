using FarmAuthenLibrary;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace ITHELPDESK.Models
{
    public class User
    {
        public string Username { get; set; }
        public string Password { get; set; }

        public bool IsValid(string _username, string _password)
        {
            AuthenAD myad = new AuthenAD();
            myad.LdapAuthentication(ConfigurationManager.AppSettings["ConfigAuthen"]);
            Boolean result;
            bool st = false;
            try
            {
                result = myad.IsAuthenticated("spoonsugar.local", _username, _password);
                if (result)
                {
                    st = true;
                }
            }
            catch
            {
                st = false;
            }
            return st;

        }
    }
}
