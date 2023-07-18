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

            using (var scope = host.Services.CreateScope()) //������� �� ���� ������
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

                        services.AddDbContext<AuthDbContext>(options => //����� � ��
                        {
                            options.UseSqlite(connectionString);
                        });

                        services.AddIdentity<AppUser, IdentityRole>(config => //������������ ���������� � ��������
                        {
                            config.Password.RequiredLength = 4; //������ ������� 4 �������
                            config.Password.RequireDigit = false; //������ ��������� ����
                            config.Password.RequireNonAlphanumeric = false; //����
                            config.Password.RequireUppercase = false; //�� ��������-�������� ��������
                        })
                        .AddEntityFrameworkStores<AuthDbContext>() //��������� �������� �� ��� ��������� � identity
                        .AddDefaultTokenProviders(); //��������� ����� ���������� ��� ��������� � ���������� ������ �������

                        services.AddIdentityServer() //��������� ������������
                            .AddAspNetIdentity<AppUser>() //���������� AppUser � ������������
                            .AddInMemoryApiResources(Configuration.ApiResources) //��������� � ������ ������ �������� (Identity) ��� �������������� � �����������
                            .AddInMemoryIdentityResources(Configuration.IdentityResources) //������, ���������� ������ ������������ �������� Identity.
                            .AddInMemoryApiScopes(Configuration.ApiScopes) //��������� � ������ ������ ��������(scopes) ��� �������������� � ����������� API.
                            .AddInMemoryClients(Configuration.Clients) //��������� � ������ ������ �������� ��� �������������� � �����������.
                            .AddDeveloperSigningCredential(); //��������� ������� � ������� �������������� � �����������. ��������� ������.

                        services.ConfigureApplicationCookie(config =>
                        {
                            config.Cookie.Name = "Notes.Identity.Cookie"; //��� ����
                            config.LoginPath = "/Auth/Login"; //���� ��� ������
                            config.LogoutPath = "/Auth/Logout"; //���� ��� ������� ������������
                        });

                        services.AddControllersWithViews();
                    });

                    //����������� ���-����������. �� ���������� ��������������� ������� ��� ��������� ������������ ����������.
                    webBuilder.Configure(app =>
                    {
                        var env = app.ApplicationServices.GetRequiredService<IWebHostEnvironment>();
                        if (env.IsDevelopment()) //�������� �� ������� ������������
                        {
                            app.UseDeveloperExceptionPage(); //����������� ������������� �� ��� ����������� �������� � ����������� �� ������ � ������ ������������� ����������.
                        }

                        app.UseStaticFiles(new StaticFileOptions //���-�� � ������������ ������� 
                        {
                            FileProvider = new PhysicalFileProvider(
                                Path.Combine(env.ContentRootPath, "Styles")),
                            RequestPath = "/styles"
                        });

                        app.UseRouting(); //��������� ������������� �� ��� �������������� � ������� IdentityServer.
                        app.UseIdentityServer(); //��������� �������� ����� ��� GET-�������� (OAuth � OpenID Connect (OIDC)) �� �������� ���� ����������. 
                        app.UseEndpoints(endpoints =>
                        {
                            endpoints.MapDefaultControllerRoute();
                        }); //���� ����� ���������� ���������� ��� GET-�������� �� �������� ���� ����������.
                    });
                });
    }
}