using Microsoft.SharePoint.Client;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ITHELPDESK.Controllers
{
    public class OneDriveConnectionController : Controller
    {
        private readonly string _clientId;
        private readonly string _redirectUri;
        private readonly string _resource;
        private readonly string _clientSecret;
        private readonly string _response_mode;
        private readonly string _response_type;

        public OneDriveConnectionController()
        {
            //this._clientId = "00000003-0000-0ff1-ce00-000000000000";
            //this._redirectUri = "https%3A%2F%2Fkpsugar%2Esharepoint%2Ecom%2F_forms%2Fdefault%2Easpx";
            //this._resource = Uri.EscapeDataString(resource);
            //this._clientSecret = clientSecret;
            //this._response_mode = "form_post";
            //this._response_type = "code%20id_token";
        }


        public void Authorize()
        {
            /* EXAMPLE: GET https://login.windows.net/common/oauth2/authorize
                * ?response_type=code
                * &client_id=acb81092-056e-41d6-a553-36c5bd1d4a72
                * &redirect_uri=https://mycoolwebapp.azurewebsites.net
                * &resource=https:%2f%2foutlook.office365.com%2f
                * &state=5fdfd60b-8457-4536-b20f-fcb658d19458 */

            string baseUri = "https://login.windows.net/common/oauth2/authorize";
            string authorizationUri = string.Format(baseUri
                + "?response_type=code"
                + "&client_id={0}"
                + "&redirect_uri={1}"
                + "&resource={2}"
                + "&state={3}", this._clientId, this._redirectUri, this._resource, "5fdfd60b-8457-4536-b20f-fcb658d19458");

            // Create the form
            Response.Redirect(authorizationUri);
        }



        [JsonObject(MemberSerialization.OptIn)]
        class TokenInformation
        {
            [JsonProperty(PropertyName = "access_token")]
            public string AccessToken { get; set; }

            [JsonProperty(PropertyName = "token_type")]
            public string TokenType { get; set; }

            [JsonProperty(PropertyName = "expires_in")]
            public int ExpiresIn { get; set; }

            [JsonProperty(PropertyName = "expires_on")]
            public int ExpiresOn { get; set; }

            [JsonProperty(PropertyName = "resource")]
            public string Resource { get; set; }

            [JsonProperty(PropertyName = "refresh_token")]
            public string RefreshToken { get; set; }

            [JsonProperty(PropertyName = "scope")]
            public string Scope { get; set; }

            [JsonProperty(PropertyName = "id_token")]
            public string IdToken { get; set; }
        }
    }
}