using Accounting.BusinessLogics;
using Accounting.BusinessLogics.IBusinessLogics;
using Accounting.Helpers;
using Accounting.Middleware;
using Accounting.Models;
using Accounting.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Text.Json.Serialization;

namespace Accounting;

public class Program
{
    public static void Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

        // Add services to the container.

        builder.Services.AddControllers().AddJsonOptions(opt =>
        {
            opt.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(option =>
       {
           option.SwaggerDoc("v1", new OpenApiInfo { Title = "Gold Marketing API's", Version = "v1", Description = ".NET Core 8 Web API" });
           option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
           {
               In = ParameterLocation.Header,
               Description = "Please enter a valid token",
               Name = "Authorization",
               Type = SecuritySchemeType.ApiKey,
               BearerFormat = "JWT",
               Scheme = "Bearer"
           });
           option.AddSecurityRequirement(new OpenApiSecurityRequirement
   {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type=ReferenceType.SecurityScheme,
                    Id="Bearer"
                }
            },
            new string[]{}
        }
   });
       });
        builder.Services.AddDbContext<GAccountingDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("GAccountingDbContext")));

        builder.Services.AddAuthentication(x =>
       {
           x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
           x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
       }).AddJwtBearer(o =>
       {
           byte[] Key = Encoding.UTF8.GetBytes(builder.Configuration["JwtTokenSettings:SymmetricSecurityKey"]!);
           o.SaveToken = true;
           o.IncludeErrorDetails = true;
           o.UseSecurityTokenValidators = true;
           o.TokenValidationParameters = new TokenValidationParameters
           {
               ClockSkew = TimeSpan.Zero,
               ValidateIssuer = true,
               ValidateAudience = true,
               ValidateLifetime = true,
               ValidateIssuerSigningKey = true,
               ValidIssuer = builder.Configuration["JwtTokenSettings:Issuer"],
               ValidAudience = builder.Configuration["JwtTokenSettings:Audience"],
               LifetimeValidator = TokenLifetimeValidator.Validate,
               IssuerSigningKey = new SymmetricSecurityKey(Key)
           };
       });

        builder.Services.AddProblemDetails();
        builder.Services.AddTransient<AuthenticationService>();
        builder.Services.AddScoped<IUsers, Users>();

        WebApplication app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseMiddleware<ExceptionMiddleware>();
        app.MapControllers();

        app.Run();
    }
}
