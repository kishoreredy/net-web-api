
using CodeFirstApi.Context;
using CodeFirstApi.Context.Sso;
using CodeFirstApi.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace CodeFirstApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            ConfigureServices(builder);

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddSession(s =>
            {
                s.IOTimeout = TimeSpan.FromMinutes(1);
                s.IdleTimeout = TimeSpan.FromMinutes(1);
                //s.Cookie.Expiration = TimeSpan.FromMinutes(10);
                s.Cookie.HttpOnly = true;
                s.Cookie.MaxAge = TimeSpan.FromHours(1);
            });
            builder.Services.AddDistributedMemoryCache();

            var app = builder.Build();

            // Add middleware to the container.
            ConfigureMiddleware(app);

            app.Run();
        }

        private static void ConfigureServices(WebApplicationBuilder builder)
        {
            // Add DbContext
            ConfigureDatabase(builder);

            ConfigureAuthenticationAuthorization(builder);

            // Add Dependency Injection
            ConfigureIocContainer(builder);
        }

        private static void ConfigureMiddleware(WebApplication app)
        {
            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseSession();
            app.UseAuthentication();
            app.UseAuthorization();


            app.MapControllers();
        }

        private static void ConfigureDatabase(WebApplicationBuilder builder)
        {
            builder.Services.AddDbContext<CodeFirstContext>(options =>
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString("CodeFirstDb"));
            });

            builder.Services.AddDbContext<SsoContext>(options =>
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString("CodeFirstDb"));
            }).AddIdentity<IdentityUser, IdentityRole>(i =>
            {
                i.Password.RequiredLength = 3;
                i.Password.RequireUppercase = false;
                i.Password.RequireLowercase = false;
                i.Password.RequireNonAlphanumeric = false;
                i.Password.RequireDigit = false;
            }).AddEntityFrameworkStores<SsoContext>();
        }

        private static void ConfigureAuthenticationAuthorization(WebApplicationBuilder builder)
        {
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
                .AddJwtBearer(b =>
                {
                    b.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateAudience = true,
                        ValidateIssuer = true,
                        ValidateActor = true,
                        RequireExpirationTime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = builder.Configuration.GetSection("Jwt.Issuer").Value,
                        ValidAudience = builder.Configuration.GetSection("Jwt:Audience").Value,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration.GetSection("Jwt:Key").Value!))
                    };
                })
                    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, c =>
                    {
                        c.ExpireTimeSpan = TimeSpan.FromMinutes(1);
                        c.SlidingExpiration = true;
                    });
        }

        private static void ConfigureIocContainer(WebApplicationBuilder builder)
        {
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddTransient<IRoleService, RoleService>();
            builder.Services.AddTransient<ITokenService, TokenService>();
        }
    }
}
