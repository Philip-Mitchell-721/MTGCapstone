using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Build.Framework;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MTGCapstone.API.Authorization;
using MTGCapstone.API.Data.Models;
using MTGCapstone.API.DbContexts;
using MTGCapstone.API.Middleware;
using MTGCapstone.API.Services;
using MTGCapstone.API.Services.DomainServiceInterfaces;
using MTGCapstone.API.Services.DomainServices;
using System.Text;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.

//comment in changed to Adding Identity
builder.Services.AddControllers()
    .AddNewtonsoftJson(); 
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

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

builder.Services.AddDbContext<CapstoneDbContext>(dbContextOptions =>
    dbContextOptions.UseSqlServer(
        builder.Configuration["ConnectionStrings:CapstoneDbContextConnection"]));

builder.Services.AddIdentity<User, IdentityRole<int>>(opt =>
{
    opt.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<CapstoneDbContext>();


builder.Services.AddHttpClient<ScryfallClient>()
    .ConfigurePrimaryHttpMessageHandler(handler => new HttpClientHandler()
    {
        AutomaticDecompression = System.Net.DecompressionMethods.GZip
    });



builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
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

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("GetDeckForOwnerAsync", policy =>
    {
        policy.AddRequirements(new IsOwnerRequirement());
    });
});

builder.Services.AddSingleton<IAuthorizationHandler, IsOwnerAuthorizationHandler>();


WebApplication app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthentication();

app.UseMiddleware<LoggingUserScope>();

app.UseAuthorization();

app.MapControllers();

app.Run();
