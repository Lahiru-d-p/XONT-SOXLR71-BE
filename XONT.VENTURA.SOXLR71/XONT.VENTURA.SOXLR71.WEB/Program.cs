
using Newtonsoft.Json;
using XONT.Common.Data;
using XONT.Ventura.AppConsole;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSystemWebAdapters();
builder.Services.AddHttpForwarder();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddApplicationInsightsTelemetry();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
builder.Services.AddControllersWithViews();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}
app.Use(async (context, next) =>
{
    if (context.Session != null && !context.Session.Keys.Contains("Theme"))
    {
        context.Session.SetString("Theme", "Blue");
        context.Session.SetInt32("Main_Language", (int)LanguageChange.Language.English);

        var user = new User { UserName = "xontadmin", PowerUser = "1", BusinessUnit = "SJAP", UserLevelGroup = "USER" };
        var userData = JsonConvert.SerializeObject(user);
        context.Session.SetString("Main_LoginUser", userData);
        context.Session.SetString("Main_UserName", "xontadmin");
        context.Session.SetString("Main_BusinessUnit", "SJAP");
    }

    await next(context);
});
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseSession();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseSystemWebAdapters();

app.MapDefaultControllerRoute();
//app.MapForwarder("/{**catch-all}", app.Configuration["ProxyTo"]).Add(static builder => ((RouteEndpointBuilder)builder).Order = int.MaxValue);

app.Run();