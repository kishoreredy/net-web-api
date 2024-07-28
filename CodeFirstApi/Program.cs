
using CodeFirstApi.Context;
using CodeFirstApi.Context.Sso;
using CodeFirstApi.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

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

            var app = builder.Build();

            // Add middleware to the container.
            ConfigureMiddleware(app);

            app.Run();
        }

        private static void ConfigureServices(WebApplicationBuilder builder)
        {
            // Add DbContext
            ConfigureDatabase(builder);

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
            }).AddIdentity<IdentityUser, IdentityRole>().AddEntityFrameworkStores<SsoContext>();
        }

        private static void ConfigureIocContainer(WebApplicationBuilder builder)
        {
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddTransient<IRoleService, RoleService>();
            builder.Services.AddTransient<ITokenService, TokenService>();
        }
    }
}
