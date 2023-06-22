using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Build.Framework;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using MTGCapstone.API.Data.Models;
using MTGCapstone.API.DbContexts;
using MTGCapstone.API.Middleware;
using MTGCapstone.API.Services;
using MTGCapstone.API.Services.DomainServiceInterfaces;
using MTGCapstone.API.Services.DomainServices;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Text.Json.Serialization;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.

//comment in changed to Adding Identity
builder.Services.AddControllers()
    .AddNewtonsoftJson();

//JsonConvert.DefaultSettings = () => new JsonSerializerSettings
//{
//    Formatting = Formatting.Indented,
//    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
//};
//.AddJsonOptions(options =>
//{
//    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
//}); 

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddTransient<IScryfallApiService, ScryfallApiService>();
builder.Services.AddScoped<ICardService, CardService>();
builder.Services.AddScoped<IDeckService, DeckService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddTransient<IPropertyMappingService, PropertyMappingService>();

//This is if you want to add additional ModelState Validation and/or return a 422 UnprocessableEntity instead of 400.
//builder.Services.Configure<ApiBehaviorOptions>(options => options.SuppressModelStateInvalidFilter = true);

//builder.Services
//        .AddFluentEmail("fromemail@test.test")
//        //.AddRazorRenderer()
//        .AddSmtpSender("localhost", 25);

string myAllowSpecificOrigins = "allowMyFrontEnd";
string allowAnyOrigins = "allowAnyOrigins";
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: myAllowSpecificOrigins,
                      policy =>
                      {
                          policy.WithOrigins("http://127.0.0.1:5500")
                            .AllowAnyHeader();
                          //TODO: ASK: remove this .AllowAnyHeader()?
                      });
    options.AddPolicy(name: allowAnyOrigins,
                      policy =>
                      {
                          policy.AllowAnyOrigin()
                           .AllowAnyHeader()
                           .AllowAnyMethod();
                      });
});

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

builder.Services.AddDbContext<CapstoneDbContext>(dbContextOptions =>
    dbContextOptions.UseSqlServer(
        builder.Configuration["ConnectionStrings:CapstoneDbContextConnection"]));

builder.Services.AddIdentity<User, IdentityRole<int>>(opt =>
{
    opt.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<CapstoneDbContext>()
.AddDefaultTokenProviders();

builder.Services.AddHttpClient<ScryfallClient>()
    .ConfigurePrimaryHttpMessageHandler(handler => new HttpClientHandler()
    {
        AutomaticDecompression = System.Net.DecompressionMethods.GZip
    });



builder.Services.AddAuthentication(opt =>
    {
        opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.SaveToken = true;
        options.TokenValidationParameters = new()
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Authentication:Issuer"],
            ValidAudience = builder.Configuration["Authentication:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.ASCII.GetBytes(builder.Configuration["Authentication:SecretForKey"]))
        };
    });



builder.Services.AddAuthorization();




WebApplication app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseCors(allowAnyOrigins);

JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
app.UseAuthentication();

//app.UseMiddleware<LoggingUserScope>();

app.UseAuthorization();

app.MapControllers();

app.Run();
