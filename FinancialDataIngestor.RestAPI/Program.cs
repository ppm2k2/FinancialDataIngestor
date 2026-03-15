using Swashbuckle.AspNetCore.SwaggerUI;
using FundAdminRestAPI.EndPoints;
using FundAdminRestAPI.Models;
using FundAdminRestAPI.Interfaces;
using FundAdminRestAPI.BusinessLogic;
using System.Reflection;
using FundAdminRestAPI.Interfaces.BusinessLogic;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

// Configure the HTTP request pipeline.
/*if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}*/

builder.Services.AddCors( options =>
{ 
    if(builder.Environment.IsDevelopment())
    {
        options.AddDefaultPolicy(builder =>
        {
            builder.SetIsOriginAllowed(origin => new Uri(origin).Host == "localhost");
        });
    }
});


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Services.AddSwaggerGen(config =>
{
config.AddSecurityDefinition("bearerAuth", new Microsoft.OpenApi.Models.OpenApiSecurityScheme()
{
    Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
    Scheme = "bearer",
    BearerFormat = "JWT",
    Description = "JWT Authorization header"
});

config.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement{
    {
        new Microsoft.OpenApi.Models.OpenApiSecurityScheme
        {
            Reference = new Microsoft.OpenApi.Models.OpenApiReference
            {
                Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                Id="bearerAuth"
            }
        },
        new string[]{ }
    }});

    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
});

builder.Services.AddMemoryCache();



//builder.Services.AddRazorPages();

builder.Services.AddTransient<IFundAdminBL, FundAdminBL>();

var app = builder.Build();



if (!String.IsNullOrWhiteSpace(Constants.API_PATH_BASE))
{
    app.UsePathBase($"/{Constants.API_PATH_BASE.TrimStart('/')}");
}

if(app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseCors(builder => builder
    .AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader());
}

app.UseSwagger();

app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("./swagger/v1/swagger.json", "FundAdmin API V1");
    c.RoutePrefix = string.Empty;
});

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

//app.UseAuthentication();
//app.UseAuthorization();

app.MapFundServiceEndpoints();

app.Run();
