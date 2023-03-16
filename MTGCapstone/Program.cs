using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Build.Framework;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MTGCapstone.API.Data.Models;
using MTGCapstone.API.DbContexts;
using MTGCapstone.API.Services;
using MTGCapstone.API.Services.DomainServiceInterfaces;
using MTGCapstone.API.Services.DomainServices;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

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
    options.AddPolicy("YourDeck", policy =>
    {
        policy.RequireAuthenticatedUser();
        //policy.RequireClaim("sub", deckId );
        //TODO:ASK: Figure out how to compare "Sub" claim to the userId on the deck in the request.
    });
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
