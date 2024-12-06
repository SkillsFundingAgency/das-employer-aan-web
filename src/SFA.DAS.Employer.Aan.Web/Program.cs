using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.ApprenticeAan.Web.Validators.MemberProfile;
using SFA.DAS.Employer.Aan.Web.AppStart;
using SFA.DAS.Employer.Aan.Web.Filters;
using SFA.DAS.Employer.Aan.Web.Infrastructure;
using SFA.DAS.Employer.Aan.Web.Validators;
using SFA.DAS.Employer.Shared.UI;
using SFA.DAS.Validation.Mvc.Extensions;
using SharpGrip.FluentValidation.AutoValidation.Mvc.Enums;
using SharpGrip.FluentValidation.AutoValidation.Mvc.Extensions;

var builder = WebApplication.CreateBuilder(args);

var rootConfiguration = builder.Configuration.LoadConfiguration(builder.Services);

builder.Services
    .AddOptions()
    .AddLogging()
    .AddApplicationInsightsTelemetry()
    .AddHttpContextAccessor()
    .AddServiceRegistrations(rootConfiguration)
    .AddAuthenticationServices(rootConfiguration)
    .AddSession(rootConfiguration)
    .AddValidatorsFromAssembly(typeof(RegionsSubmitModelValidator).Assembly)
    .AddValidatorsFromAssembly(typeof(ConnectWithMemberSubmitModelValidator).Assembly)
    .AddMaMenuConfiguration(RouteNames.SignOut, rootConfiguration["ResourceEnvironmentName"]);

//builder.Services.AddValidatorsFromAssemblyContaining<ReceiveNotificationsSubmitModelValidator>();
//builder.Services.AddFluentValidationAutoValidation(options =>
//{
//    options.ValidationStrategy = ValidationStrategy.Annotations;
//});

builder.Services.AddHealthChecks();

builder.Services
    .Configure<RouteOptions>(options => { options.LowercaseUrls = false; })
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
    app.UseExceptionHandler("/error");
    app.UseHsts();
}

app.UseContentSecurityPolicy();

app.Use(async (context, next) =>
{
    if (context.Response.Headers.ContainsKey("X-Frame-Options"))
    {
        context.Response.Headers.Remove("X-Frame-Options");
    }

    context.Response.Headers!.Append("X-Frame-Options", "SAMEORIGIN");

    await next();

    if (context.Response.StatusCode == 404 && !context.Response.HasStarted)
    {
        //Re-execute the request so the user gets the error page
        var originalPath = context.Request.Path.Value;
        context.Items["originalPath"] = originalPath;
        context.Request.Path = "/error/404";
        await next();
    }
});

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
