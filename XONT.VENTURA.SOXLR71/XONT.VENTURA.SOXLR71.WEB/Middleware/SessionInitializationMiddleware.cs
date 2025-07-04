using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System.Text.Json;
using XONT.Common.Data;
using XONT.Ventura.AppConsole;

namespace XONT.VENTURA.SOXLR71.WEB.Middlewares;

public class SessionInitializationMiddleware
{
    private readonly RequestDelegate _next;

    public SessionInitializationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Session != null && !context.Session.Keys.Contains("Main_LoginUser"))
        {
            var user = new User { UserName = "xontadmin", PowerUser = "1", BusinessUnit = "SJAP", UserLevelGroup = "USER" };

            BusinessUnit businessunit = new BusinessUnit { BusinessUnitName = "SJAP (Pvt) Ltd", AddressLine1 = "No. 146,", AddressLine2 = "Dawson Street", AddressLine3 = "Colombo 02", AddressLine4 = "", AddressLine5 = "", TelephoneNumber = "94-112251055", FaxNumber = "94 -112251090", WebAddress = "www.kansaipaintlanka.com", EmailAddress = "customercare@kansaipaintlanka.com" };

            context.Session.SetString("Theme", "Blue");
            context.Session.SetInt32("Main_Language", (int)LanguageChange.Language.English);
            context.Session.SetString("Main_UserName", "xontadmin");
            context.Session.SetString("Main_BusinessUnit", "SJAP");
            var userData = JsonConvert.SerializeObject(user);
            context.Session.SetString("Main_LoginUser", userData);
            var bUnitData = JsonConvert.SerializeObject(businessunit);
            context.Session.SetString("Main_BusinessUnitDetail", bUnitData);
        }

        await _next(context);
    }
}

