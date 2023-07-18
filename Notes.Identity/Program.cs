using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Notes.Identity.Data;
using Notes.Identity.Models;

namespace Notes.Identity
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();

            using (var scope = host.Services.CreateScope()) //Создана ли база данных
            {
                var serviceProvider = scope.ServiceProvider;

                try
                {
                    var context = serviceProvider.GetRequiredService<AuthDbContext>();
                    DbInitializer.Initialize(context);
                }
                catch (Exception exception)
                {
                    var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
                    logger.LogError(exception, "An error occurred while app initialization");
                }
            }

            host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.ConfigureServices((hostContext, services) =>
                    {
                        var connectionString = hostContext.Configuration.GetValue<string>("DbConnection");

                        services.AddDbContext<AuthDbContext>(options => //связь с бд
                        {
                            options.UseSqlite(connectionString);
                        });

                        services.AddIdentity<AppUser, IdentityRole>(config => //конфигурация требований у запросов
                        {
                            config.Password.RequiredLength = 4; //длинна минимум 4 символа
                            config.Password.RequireDigit = false; //отмена заглавных букв
                            config.Password.RequireNonAlphanumeric = false; //цифр
                            config.Password.RequireUppercase = false; //не буквенно-цифровых символов
                        })
                        .AddEntityFrameworkStores<AuthDbContext>() //добавляем контекст бд как хранилище к identity
                        .AddDefaultTokenProviders(); //дефолтный токен провайдера для получения и обновления токена доступа

                        services.AddIdentityServer() //настройка конфигурации
                            .AddAspNetIdentity<AppUser>() //добавление AppUser в конфигурацию
                            .AddInMemoryApiResources(Configuration.ApiResources) //добавляет в память список ресурсов (Identity) для аутентификации и авторизации
                            .AddInMemoryIdentityResources(Configuration.IdentityResources) //объект, содержащий список определенных ресурсов Identity.
                            .AddInMemoryApiScopes(Configuration.ApiScopes) //добавляет в память список областей(scopes) для аутентификации и авторизации API.
                            .AddInMemoryClients(Configuration.Clients) //добавляет в ПАМЯТЬ список клиентов для аутентификации и авторизации.
                            .AddDeveloperSigningCredential(); //добавляет подпись к токенам аутентификации и авторизации. Генерация ключей.

                        services.ConfigureApplicationCookie(config =>
                        {
                            config.Cookie.Name = "Notes.Identity.Cookie"; //имя куки
                            config.LoginPath = "/Auth/Login"; //путь для логина
                            config.LogoutPath = "/Auth/Logout"; //путь для логаута пользователя
                        });

                        services.AddControllersWithViews();
                    });

                    //настраивает веб-приложение. Он использует предоставленный делегат для настройки конфигурации приложения.
                    webBuilder.Configure(app =>
                    {
                        var env = app.ApplicationServices.GetRequiredService<IWebHostEnvironment>();
                        if (env.IsDevelopment()) //Является ли режимом разработчика
                        {
                            app.UseDeveloperExceptionPage(); //добавляется промежуточное ПО для отображения страницы с информацией об ошибке в случае возникновения исключения.
                        }

                        app.UseStaticFiles(new StaticFileOptions //что-то с отображением страниц 
                        {
                            FileProvider = new PhysicalFileProvider(
                                Path.Combine(env.ContentRootPath, "Styles")),
                            RequestPath = "/styles"
                        });

                        app.UseRouting(); //добавляет промежуточное по для аутентификации с помощью IdentityServer.
                        app.UseIdentityServer(); //добавляет конечную точку для GET-запросов (OAuth и OpenID Connect (OIDC)) на корневой путь приложения. 
                        app.UseEndpoints(endpoints =>
                        {
                            endpoints.MapDefaultControllerRoute();
                        }); //Этот метод определяет обработчик для GET-запросов на корневой путь приложения.
                    });
                });
    }
}