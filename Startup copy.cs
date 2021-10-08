// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Net.Http;
// using System.Threading.Tasks;
// using IdentityModel;
// using IdentityModel.Client;
// using Microsoft.AspNetCore.Authentication;
// using Microsoft.AspNetCore.Authentication.Cookies;
// using Microsoft.AspNetCore.Authentication.OpenIdConnect;
// using Microsoft.AspNetCore.Builder;
// using Microsoft.AspNetCore.Hosting;
// using Microsoft.CodeAnalysis.CSharp.Syntax;
// using Microsoft.Extensions.Configuration;
// using Microsoft.Extensions.DependencyInjection;
// using Microsoft.Extensions.Hosting;

// namespace BpoOnsite
// {
//     public class Startup
//     {
//         public Startup(IConfiguration configuration)
//         {
//             Configuration = configuration;
//         }

//         public IConfiguration Configuration { get; }

//         // This method gets called by the runtime. Use this method to add services to the container.
//         public void ConfigureServices(IServiceCollection services)
//         {
//             services.AddControllersWithViews();
//             // se agraga cognito
//             services.AddAuthentication(options =>
//             {
//                 options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
//                 options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
//                 options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
//             })
//                 .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
//                 {
//                     // middle que intercepta la validacion del token para renovarlo en caso de caducar
//                     options.Events = new CookieAuthenticationEvents
//                     {
//                         // Una vez validada la cookie de autenticación, se llama a este evento
//                         // en el vemos si el token de acceso está próximo a caducar
//                         // luego usamos el token de actualización para obtener un nuevo token de acceso y guardarlo.
//                         // Si el token de actualización no funciona por alguna razón, redirigimos a
//                         // la pantalla de inicio de sesión.
//                         OnValidatePrincipal = async cookieCtx =>
//                         {
//                             var now = DateTimeOffset.UtcNow;
//                             var expiresAt = cookieCtx.Properties.GetTokenValue("expires_at");
//                             var accessTokenExpiration = DateTimeOffset.Parse(expiresAt);
//                             var timeRemaining = accessTokenExpiration.Subtract(now);
//                             var refreshThresholdMinutes = 5;
//                             var refreshThreshold = TimeSpan.FromMinutes(refreshThresholdMinutes);

//                             if (timeRemaining < refreshThreshold)
//                             {
//                                 var refreshToken = cookieCtx.Properties.GetTokenValue("refresh_token");

//                                 if (string.IsNullOrEmpty(refreshToken))
//                                 {
//                                     cookieCtx.RejectPrincipal();
//                                     await cookieCtx.HttpContext.SignOutAsync();

//                                 }
//                                 else
//                                 {
//                                     var response = await new HttpClient().RequestRefreshTokenAsync(new RefreshTokenRequest
//                                     {
//                                         Address = Configuration["Authentication:Cognito:MetadataAddress"],
//                                         ClientId = Configuration["Authentication:Cognito:ClientId"],
//                                         ClientSecret = Configuration["Authentication:Cognito:Secret"],
//                                         RefreshToken = refreshToken
//                                     });

//                                     if (!response.IsError)
//                                     {
//                                         var expiresInSeconds = response.ExpiresIn;
//                                         var updatedExpiresAt = DateTimeOffset.UtcNow.AddSeconds(expiresInSeconds);
//                                         cookieCtx.Properties.UpdateTokenValue("expires_at", updatedExpiresAt.ToString());
//                                         cookieCtx.Properties.UpdateTokenValue("access_token", response.AccessToken);
//                                         cookieCtx.Properties.UpdateTokenValue("refresh_token", response.RefreshToken);

//                                         // Indicar al middleware de cookies que la cookie debe rehacerse (ya que la hemos actualizado)
//                                         cookieCtx.ShouldRenew = true;
//                                     }
//                                     else
//                                     {
//                                         cookieCtx.RejectPrincipal();
//                                         await cookieCtx.HttpContext.SignOutAsync();
//                                     }
//                                 }
//                             }
//                         }
//                     };
//                 })
//                 .AddOpenIdConnect(options =>
//                 {
//                     options.ResponseType = OidcConstants.ResponseTypes.Code;
//                     options.MetadataAddress = Configuration["Authentication:Cognito:MetadataAddress"];
//                     options.ClientId = Configuration["Authentication:Cognito:ClientId"];
//                     options.ClientSecret = Configuration["Authentication:Cognito:Secret"];
//                     options.SaveTokens = true;
//                     options.TokenValidationParameters.ValidateIssuer = true;
//                     options.GetClaimsFromUserInfoEndpoint = true;
//                     options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
//                     options.Authority = Configuration["Authentication:Cognito:MetadataAddress"];
//                     options.RequireHttpsMetadata = true;
//                     options.UseTokenLifetime = true;
//                 });
//         }

//         // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
//         public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
//         {
//             if (env.IsDevelopment())
//             {
//                 app.UseDeveloperExceptionPage();
//             }
//             else
//             {
//                 app.UseExceptionHandler("/Home/Error");
//             }

//             app.UseHttpsRedirection();

//             app.UseStaticFiles();


//             app.UseRouting();

//             app.UseAuthentication();

//             app.UseAuthorization();


//             app.UseEndpoints(endpoints =>
//             {
//                 endpoints.MapControllerRoute(
//                     name: "default",
//                     pattern: "{controller=Home}/{action=Index}/{id?}");
//             });
//         }
//     }
// }
