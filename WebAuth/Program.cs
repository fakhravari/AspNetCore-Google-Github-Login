using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDataProtection()
    .SetApplicationName("Fakhravari.Ir")
    .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(builder.Environment.ContentRootPath, "keys")))
    .SetDefaultKeyLifetime(TimeSpan.FromDays(30));

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
})
.AddCookie(options =>
{
    options.Cookie.Name = "Fakhravari.Ir";
    options.LoginPath = "/Authentication/Signin";
    options.LogoutPath = "/Authentication/Logout";
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
    options.SlidingExpiration = true;
})
.AddGoogle(options =>
{
    options.ClientId = builder.Configuration["Google:ClientId"];
    options.ClientSecret = builder.Configuration["Google:ClientSecret"];
    options.CallbackPath = "/auth/google/callback";
    options.SaveTokens = true;

    // محدوده‌های دسترسی
    options.Scope.Clear();
    options.Scope.Add("openid");
    options.Scope.Add("profile");
    options.Scope.Add("email");

    // رویدادها برای دیباگ
    options.Events = new OAuthEvents
    {
        OnRedirectToAuthorizationEndpoint = context =>
        {
            context.Response.Redirect(context.RedirectUri);
            return Task.CompletedTask;
        },
        OnAccessDenied = context =>
        {
            context.Response.Redirect("/error?access_denied");
            return Task.CompletedTask;
        }
    };
});

builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.MinimumSameSitePolicy = SameSiteMode.Lax;
});


builder.Services.AddControllersWithViews();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient();
builder.Services.AddSingleton<IFileProvider>(new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot")));





builder.Services.Configure<IISServerOptions>(options => { options.MaxRequestBodySize = int.MaxValue; });
builder.Services.Configure<FormOptions>(options =>
{
    options.ValueLengthLimit = int.MaxValue;
    options.MultipartBodyLengthLimit = long.MaxValue;
    options.MultipartBoundaryLengthLimit = int.MaxValue;
    options.MultipartHeadersCountLimit = int.MaxValue;
    options.MultipartHeadersLengthLimit = int.MaxValue;
});
builder.Services.AddResponseCompression();

var app = builder.Build();

if (app.Environment.IsDevelopment())
    app.UseDeveloperExceptionPage();
else
    app.UseDeveloperExceptionPage();
//app.UseExceptionHandler("/Home/Error");
//app.UseHsts();


// ترتیب بسیار مهم است!
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// این ترتیب باید دقیقاً رعایت شود
app.UseCookiePolicy(new CookiePolicyOptions
{
    MinimumSameSitePolicy = SameSiteMode.Lax,
    Secure = CookieSecurePolicy.Always
});

app.UseAuthentication();
app.UseAuthorization();


app.MapStaticAssets();
app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}").WithStaticAssets();
app.UseRequestLocalization();
app.Run();