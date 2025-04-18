using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDataProtection().SetApplicationName("Fakhravari.Ir").PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(builder.Environment.ContentRootPath, "keys"))).SetDefaultKeyLifetime(TimeSpan.FromDays(30));
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie(options =>
{
    options.Cookie.Name = "Fakhravari.Ir";
    options.LoginPath = "/Authentication/Signin";
    options.LogoutPath = "/Authentication/Logout";

    options.Cookie.SameSite = SameSiteMode.None;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
})
.AddGoogle("Google", options =>
{
    var config = builder.Configuration.GetSection("Authentication:Google");
    options.ClientId = config["ClientId"];
    options.ClientSecret = config["ClientSecret"];
}).AddGitHub("GitHub", options =>
{
    var config = builder.Configuration.GetSection("Authentication:Github");
    options.ClientId = config["ClientId"];
    options.ClientSecret = config["ClientSecret"];
}).AddMicrosoftAccount("Microsoft", options =>
{
    var config = builder.Configuration.GetSection("Authentication:Microsoft");
    options.ClientId = config["ClientId"];
    options.ClientSecret = config["ClientSecret"];
});

builder.Services.AddControllersWithViews();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient();

builder.Services.AddSingleton<IFileProvider>(new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot")));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseRequestLocalization();
app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");
app.Run();