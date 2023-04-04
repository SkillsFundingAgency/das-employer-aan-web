using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Employer.Aan.Web.AppStart;
using SFA.DAS.Employer.Aan.Web.Filters;
using SFA.DAS.Employer.Aan.Web.Infrastructure;
using SFA.DAS.Employer.Shared.UI;

var builder = WebApplication.CreateBuilder(args);

var rootConfiguration = builder.Configuration.LoadConfiguration();

builder.Services
    .AddOptions()
    .AddLogging()
    .AddApplicationInsightsTelemetry()
    .AddHttpContextAccessor()
    .AddAuthenticationServices()
    .AddServiceRegistrations(rootConfiguration)
    .AddMaMenuConfiguration(RouteNames.SignOut, rootConfiguration["ResourceEnvironmentName"]);

builder.Services.AddHealthChecks();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(10);
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.IsEssential = true;
});

builder.Services
    .Configure<RouteOptions>(options => { options.LowercaseUrls = true; })
    .AddMvc(options =>
    {
        options.Filters.Add(new RequiredSessionModelAttribute());
        options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
    })
    .AddSessionStateTempDataProvider();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    /// app.UseHealthChecks("/health"); 
    /// app.UseStatusCodePagesWithReExecute("/error/{0}"); 
    /// app.UseExceptionHandler("/error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseCookiePolicy();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseSession();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(
        "default",
        "{controller=Home}/{action=Index}/{id?}");
});

app.Run();
