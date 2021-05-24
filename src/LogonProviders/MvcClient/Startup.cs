using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Polly;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;

namespace MvcCode
{
    public class Startup
    {
        private const string IdentityUrl = "https://localhost:8081";

        public void ConfigureServices(IServiceCollection services)
        {
            // Mette i nomi degli scope in modo compatto
            JwtSecurityTokenHandler.DefaultMapInboundClaims = false;

            services.AddControllersWithViews();

            services.AddAuthentication(options =>
                {
                    options.DefaultScheme = "cookie";
                    options.DefaultChallengeScheme = "oidc";
                })
                .AddCookie("cookie", options =>
                {
                    options.Cookie.Name = "CorsoMvc";

                    options.Events.OnSigningOut = async e => { await e.HttpContext.RevokeUserRefreshTokenAsync(); };
                })
                .AddOpenIdConnect("oidc", options =>
                {
                    options.Authority = IdentityUrl;
                    options.RequireHttpsMetadata = true;

                    options.ClientId = "client-hybrid-mvc";
                    options.ClientSecret = "2C59A271-87C0-4B7D-8583-9B7CA7954E6F";

                    // code flow + PKCE (PKCE is turned on by default)
                    options.ResponseType = "code id_token";
                    options.UsePkce = false;

                    options.Scope.Clear();
                    options.Scope.Add("openid");
                    options.Scope.Add("profile");
                    options.Scope.Add("email");
                    options.Scope.Add("offline_access");
                    options.Scope.Add("webdemo");
                    options.Scope.Add("webapi");

                    // Capire come prendere la sezione scope jwt
                    // not mapped by default
                    //options.ClaimActions.MapJsonKey(System.Security.Claims.ClaimTypes.Email, "email");
                    //options.ClaimActions.MapJsonKey("permessi", "scope");
                    //options.ClaimActions.MapJsonKey("scope", "scope", "webdemo");

                    // keeps id_token smaller
                    options.GetClaimsFromUserInfoEndpoint = true;
                    options.SaveTokens = true;

                    options.ClaimActions.DeleteClaim("s_hash");

                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        NameClaimType = "name",
                        RoleClaimType = "role",
                        RoleClaimTypeRetriever = (token, s) => s,
                        
                        
                    };
                    options.Events = new OpenIdConnectEvents
                    {
                        OnUserInformationReceived = context =>
                        {
                            string rawAccessToken = context.ProtocolMessage.AccessToken;
                            string rawIdToken = context.ProtocolMessage.IdToken;
                            var handler = new JwtSecurityTokenHandler();
                            var accessToken = handler.ReadJwtToken(rawAccessToken);
                            var idToken = handler.ReadJwtToken(rawIdToken);

                            // do something with the JWTs
                            return Task.CompletedTask;
                        }
                    };
                });

            // adds global authorization policy to require authenticated users
            services.AddAuthorization(options => { options.FallbackPolicy = options.DefaultPolicy; });

            // adds user and client access token management
            services.AddAccessTokenManagement(options =>
                {
                    // client config is inferred from OpenID Connect settings
                    // if you want to specify scopes explicitly, do it here, otherwise the scope parameter will not be sent
                    options.Client.Scope = "webapi";
                })
                .ConfigureBackchannelHttpClient()
                .AddTransientHttpErrorPolicy(policy => policy.WaitAndRetryAsync(new[]
                {
                    TimeSpan.FromSeconds(1),
                    TimeSpan.FromSeconds(2),
                    TimeSpan.FromSeconds(3)
                }));
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseDeveloperExceptionPage();
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapDefaultControllerRoute(); });
        }
    }
}