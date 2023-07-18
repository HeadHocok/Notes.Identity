using System.Collections.Generic;
using IdentityServer4;
using IdentityServer4.Models;
using IdentityModel;

namespace Notes.Identity
{
    public static class Configuration
    {
        public static IEnumerable<ApiScope> ApiScopes =>
            new List<ApiScope>
            {
                new ApiScope("NotesWebAPI", "Web API") //Идентификатор для ресурсов клиента которые можно использовать. Отправляется в процессе аутентификации
            };

        public static IEnumerable<IdentityResource> IdentityResources =>
            new List<IdentityResource>
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile() //Позволяет видеть утверждения о пользователе (имя, рождение и т.д)
            };

        public static IEnumerable<ApiResource> ApiResources => //Ресурсы к которым может поступать запрос
            new List<ApiResource>
            {
                new ApiResource("NotesWebAPI", "Web API", new []
                    { JwtClaimTypes.Name})
                {
                    Scopes = {"NotesWebAPI"}
                }
            };

        public static IEnumerable<Client> Clients => //Каким клиентским приложениям доступно использовать наше приложение
            new List<Client>
            {
                new Client
                {
                    ClientId = "notes-web-api",
                    ClientName = "Notes Web",
                    AllowedGrantTypes = GrantTypes.Code, //определяет некоторые параметры. Смотреть в документации.
                    RequireClientSecret = false, //нужен ли секретный ключ клиента для autorization code (Для защиты бд после взлома)
                    RequirePkce = true,
                    RedirectUris = //Куда может происходить перенаправление после аутентификации
                    {
                        "http://.../signin-oidc"
                    },
                    AllowedCorsOrigins =
                    {
                        "http://..."
                    },
                    PostLogoutRedirectUris =
                    {
                        "http:/.../signout-oidc"
                    },
                    AllowedScopes = //области доступные клиенту
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        "NotesWebAPI"
                    },
                    AllowAccessTokensViaBrowser = true //управляет передачей токена через браузер. Чтобы предотвратить утечку токена
                }
            };
    }
}