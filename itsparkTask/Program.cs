using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using itsparkTask.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Globalization;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("ItSparkTaskContextConnection") ?? throw new InvalidOperationException("Connection string 'ItSparkTaskContextConnection' not found.");

builder.Services.AddDbContext<ItSparkTaskContext>(options => options.UseSqlServer(connectionString));

//builder.Services.AddDefaultIdentity<CustomUser>(options => options.SignIn.RequireConfirmedAccount = true).AddEntityFrameworkStores<ItSparkTaskContext>();

// Add services to the container.
builder.Services.AddControllersWithViews();

// Configure authentication services.
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;

})
.AddCookie(options =>
{
    options.LoginPath = "/Account/Login"; // Path to the login page
})
.AddGoogle(GoogleDefaults.AuthenticationScheme, options =>
{
    options.ClientId = builder.Configuration.GetSection("GoogleCredentials:ClientID").Value;
    options.ClientSecret = builder.Configuration.GetSection("GoogleCredentials:ClientSecret").Value;
    options.Scope.Add(GmailService.Scope.GmailReadonly);
    options.Scope.Add(GmailService.Scope.GmailSend);
    options.SaveTokens = true;
    options.Events = new OAuthEvents
    {
        OnCreatingTicket = context =>
        {
            var accessToken = context.AccessToken;
            var identity = (ClaimsIdentity)context.Principal.Identity;
            identity.AddClaim(new Claim("access_token", accessToken));
            return Task.CompletedTask;

        }
    };
});

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new List<CultureInfo>();
    foreach (var name in new[] { "en-US", "ar-EG" })
        supportedCultures.Add(new CultureInfo(name));
    options.DefaultRequestCulture = new RequestCulture("en-US", "en-US");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
});
builder.Services.AddHttpContextAccessor();
builder.Services
    .AddMvc()
    .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix)
    .AddDataAnnotationsLocalization();

builder.Services.AddPortableObjectLocalization(options => options.ResourcesPath = "Resources");

// Configure Gmail service.
builder.Services.AddScoped<GmailService>(serviceProvider =>
{
    var httpContextAccessor = serviceProvider.GetRequiredService<IHttpContextAccessor>();
    var user = httpContextAccessor.HttpContext.User;
    var accessToken = user?.FindFirst("access_token")?.Value;

    if (!string.IsNullOrEmpty(accessToken))
    {
        var credential = GoogleCredential.FromAccessToken(accessToken);
        return new GmailService(new Google.Apis.Services.BaseClientService.Initializer
        {
            HttpClientInitializer = credential
        });
    }

    throw new InvalidOperationException("AccessToken not found.");
});

var app = builder.Build();

// Redirect HTTP requests to HTTPS.
app.UseRewriter(new RewriteOptions().AddRedirectToHttpsPermanent());



// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();


app.UseRequestLocalization(app.Services.GetService<IOptions<RequestLocalizationOptions>>().Value);

// Enable authentication and authorization.
app.UseAuthentication();
app.UseAuthorization();

// Define a default route for the application.
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
