using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Employer.Aan.Web.AppStart;
using SFA.DAS.Employer.Aan.Web.Filters;
using SFA.DAS.Employer.Aan.Web.Infrastructure;
using SFA.DAS.Employer.Aan.Web.Validators;
using SFA.DAS.Employer.Shared.UI;

var builder = WebApplication.CreateBuilder(args);

var rootConfiguration = builder.Configuration.LoadConfiguration();

builder.Services
    .AddOptions()
    .AddLogging()
    .AddApplicationInsightsTelemetry()
    .AddHttpContextAccessor()
    .AddServiceRegistrations(rootConfiguration)
    .AddAuthenticationServices(rootConfiguration)
    .AddSession(rootConfiguration)
    .AddValidatorsFromAssembly(typeof(RegionsSubmitModelValidator).Assembly)
    .AddMaMenuConfiguration(RouteNames.SignOut, rootConfiguration["ResourceEnvironmentName"]);

builder.Services.AddHealthChecks();

builder.Services
    .Configure<RouteOptions>(options => { options.LowercaseUrls = true; })
    .AddMvc(options =>
    {
        options.Filters.Add<RequiresExistingMemberAttribute>();
        options.Filters.Add<RequiredSessionModelAttribute>();
        options.Filters.Add<AutoValidateAntiforgeryTokenAttribute>();
    })
    .AddSessionStateTempDataProvider();

#if DEBUG
builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();
#endif

if (!builder.Environment.IsDevelopment())
{
    builder.Services.AddDataProtection(rootConfiguration);
}

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    /// app.UseStatusCodePagesWithReExecute("/error/{0}"); 
    /// app.UseExceptionHandler("/error");
    app.UseHsts();
}

app
    .UseHealthChecks("/ping")
    .UseHttpsRedirection()
    .UseStaticFiles()
    .UseCookiePolicy()
    .UseRouting()
    .UseAuthentication()
    .UseAuthorization()
    .UseSession()
    .UseEndpoints(endpoints =>
    {
        endpoints.MapControllerRoute(
            "default",
            "{controller=Home}/{action=Index}/{id?}");
    });

app.Run();
