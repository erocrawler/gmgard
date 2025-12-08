using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using GmGard.Models;
using static OpenIddict.Abstractions.OpenIddictConstants;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.IdentityModel.Tokens;
using Amazon.Runtime.Internal.Util;
using Microsoft.Extensions.Logging;

namespace GmGard.Controllers
{
    public class AuthorizationController : Controller
    {
        private readonly IOpenIddictApplicationManager _applicationManager;
        private readonly IOpenIddictAuthorizationManager _authorizationManager;
        private readonly IOpenIddictScopeManager _scopeManager;
        private readonly SignInManager<UserProfile> _signInManager;
        private readonly UserManager<UserProfile> _userManager;
        private readonly ILogger<AuthorizationController> _logger;

        public AuthorizationController(
            IOpenIddictApplicationManager applicationManager,
            IOpenIddictAuthorizationManager authorizationManager,
            IOpenIddictScopeManager scopeManager,
            SignInManager<UserProfile> signInManager,
            UserManager<UserProfile> userManager,
            ILogger<AuthorizationController> logger)
        {
            _applicationManager = applicationManager;
            _authorizationManager = authorizationManager;
            _scopeManager = scopeManager;
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
        }

        [HttpGet("~/connect/authorize")]
        [HttpPost("~/connect/authorize")]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Authorize()
        {
            _logger.LogInformation("=== AUTHORIZE ACTION HIT ===");
            _logger.LogInformation($"Request Path: {Request.Path}");
            _logger.LogInformation($"Request Query: {Request.QueryString}");
            
            var request = HttpContext.GetOpenIddictServerRequest();
            if (request == null)
            {
                _logger.LogError("OpenIddict request is NULL");
                throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");
            }
            
            _logger.LogInformation($"Client ID: {request.ClientId}");
            _logger.LogInformation($"Response Type: {request.ResponseType}");
            _logger.LogInformation($"Scopes: {request.Scope}");
            _logger.LogInformation($"Redirect URI: {request.RedirectUri}");

            // Retrieve the user principal stored in the authentication cookie.
            var result = await HttpContext.AuthenticateAsync(IdentityConstants.ApplicationScheme);

            // If the user principal can't be extracted, redirect to the login page.
            if (!result.Succeeded)
            {
                var queryString = Request.HasFormContentType 
                    ? QueryString.Create(Request.Form.Select(x => new KeyValuePair<string, string>(x.Key, x.Value)))
                    : Request.QueryString;
                    
                return Challenge(
                    authenticationSchemes: IdentityConstants.ApplicationScheme,
                    properties: new AuthenticationProperties
                    {
                        RedirectUri = Request.PathBase + Request.Path + queryString
                    });
            }

            // Retrieve the profile of the logged in user.
            var user = await _userManager.GetUserAsync(result.Principal) ??
                throw new InvalidOperationException("The user details cannot be retrieved.");

            // Retrieve the application details from the database.
            var application = await _applicationManager.FindByClientIdAsync(request.ClientId) ??
                throw new InvalidOperationException("Details concerning the calling client application cannot be found.");

            // Retrieve the permanent authorizations associated with the user and the calling client application.
            var authorizations = await _authorizationManager.FindAsync(
                subject: await _userManager.GetUserIdAsync(user),
                client: await _applicationManager.GetIdAsync(application),
                status: Statuses.Valid,
                type: AuthorizationTypes.Permanent,
                scopes: request.GetScopes()).ToListAsync();

            switch (await _applicationManager.GetConsentTypeAsync(application))
            {
                // If the consent is external (e.g. when authorizations are granted by a sysadmin),
                // immediately return an error if no authorization can be found in the database.
                case ConsentTypes.External when !authorizations.Any():
                    return Forbid(
                        authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                        properties: new AuthenticationProperties(new Dictionary<string, string>
                        {
                            [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.ConsentRequired,
                            [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] =
                                "The logged in user is not allowed to access this client application."
                        }));

                // If the consent is implicit or if an authorization was found,
                // return an authorization response without displaying the consent form.
                case ConsentTypes.Implicit:
                case ConsentTypes.External when authorizations.Any():
                case ConsentTypes.Explicit when authorizations.Any() && string.IsNullOrEmpty(request.Prompt) || request.Prompt != "consent":
                    var principal = await _signInManager.CreateUserPrincipalAsync(user);
                    
                    // Ensure the subject claim is set (required by OpenIddict)
                    if (!principal.HasClaim(claim => claim.Type == Claims.Subject))
                    {
                        var userId = await _userManager.GetUserIdAsync(user);
                        ((ClaimsIdentity)principal.Identity).AddClaim(new Claim(Claims.Subject, userId));
                    }
                    
                    // Add additional claims for userinfo endpoint
                    var identity = (ClaimsIdentity)principal.Identity;
                    
                    _logger.LogInformation("Adding claims for user: {Username}, NickName: {NickName}, Email: {Email}", 
                        user.UserName, user.NickName, user.Email);
                    
                    // Always add these claims (OpenIddict will handle duplicates)
                    identity.AddClaim(new Claim(Claims.PreferredUsername, user.UserName));
                    identity.AddClaim(new Claim(Claims.Name, user.UserName));
                    identity.AddClaim(new Claim("nickname", user.NickName ?? user.UserName));
                    identity.AddClaim(new Claim("points", user.Points.ToString()));
                    identity.AddClaim(new Claim("level", user.Level.ToString()));
                    
                    // Add email if available
                    if (!string.IsNullOrEmpty(user.Email))
                    {
                        identity.AddClaim(new Claim(Claims.Email, user.Email));
                    }

                    // Note: in this sample, the granted scopes match the requested scope
                    // but you may want to allow the user to uncheck specific scopes.
                    // For that, simply restrict the list of scopes before calling SetScopes.
                    principal.SetScopes(request.GetScopes());
                    principal.SetResources(await _scopeManager.ListResourcesAsync(principal.GetScopes()).ToListAsync());

                    // Automatically create a permanent authorization to avoid requiring explicit consent
                    // for future authorization or token requests containing the same scopes.
                    var authorization = authorizations.LastOrDefault();
                    if (authorization == null)
                    {
                        authorization = await _authorizationManager.CreateAsync(
                            principal: principal,
                            subject: await _userManager.GetUserIdAsync(user),
                            client: await _applicationManager.GetIdAsync(application),
                            type: AuthorizationTypes.Permanent,
                            scopes: principal.GetScopes());
                    }

                    principal.SetAuthorizationId(await _authorizationManager.GetIdAsync(authorization));

                    foreach (var claim in principal.Claims)
                    {
                        claim.SetDestinations(GetDestinations(claim, principal));
                    }
                    
                    _logger.LogInformation("Authorization principal claims: {@Claims}", principal.Claims.Select(c => new { c.Type, c.Value }));

                    return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

                // At this point, no authorization was found in the database and an error must be returned
                // if the client application specified prompt=none in the authorization request.
                case ConsentTypes.Explicit when request.Prompt == "none":
                case ConsentTypes.Systematic when request.Prompt == "none":
                    return Forbid(
                        authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                        properties: new AuthenticationProperties(new Dictionary<string, string>
                        {
                            [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.ConsentRequired,
                            [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] =
                                "Interactive user consent is required."
                        }));

                // In every other case, render the consent form.
                default:
                    return View("Authorize", new
                    {
                        ApplicationName = await _applicationManager.GetDisplayNameAsync(application),
                        Scope = request.Scope
                    });
            }
        }

        [Authorize, FormValueRequired("submit.Accept")]
        [HttpPost("~/connect/authorize"), ValidateAntiForgeryToken]
        public async Task<IActionResult> Accept()
        {
            var request = HttpContext.GetOpenIddictServerRequest() ??
                throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");

            // Retrieve the profile of the logged in user.
            var user = await _userManager.GetUserAsync(User) ??
                throw new InvalidOperationException("The user details cannot be retrieved.");

            // Retrieve the application details from the database.
            var application = await _applicationManager.FindByClientIdAsync(request.ClientId) ??
                throw new InvalidOperationException("Details concerning the calling client application cannot be found.");

            // Retrieve the permanent authorizations associated with the user and the calling client application.
            var authorizations = await _authorizationManager.FindAsync(
                subject: await _userManager.GetUserIdAsync(user),
                client: await _applicationManager.GetIdAsync(application),
                status: Statuses.Valid,
                type: AuthorizationTypes.Permanent,
                scopes: request.GetScopes()).ToListAsync();

            // Note: the same check is already made in the other action but is repeated
            // here to ensure a malicious user can't abuse this POST-only endpoint and
            // force it to return a valid response without the external authorization.
            if (!authorizations.Any() && await _applicationManager.HasConsentTypeAsync(application, ConsentTypes.External))
            {
                return Forbid(
                    authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                    properties: new AuthenticationProperties(new Dictionary<string, string>
                    {
                        [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.ConsentRequired,
                        [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] =
                            "The logged in user is not allowed to access this client application."
                    }));
            }

            var principal = await _signInManager.CreateUserPrincipalAsync(user);
            
            // Add additional claims for userinfo endpoint
            var identity = (ClaimsIdentity)principal.Identity;
            
            _logger.LogInformation("Adding claims for user: {Username}, NickName: {NickName}, Email: {Email}", 
                user.UserName, user.NickName, user.Email);
            
            // Always add these claims (OpenIddict will handle duplicates)
            identity.AddClaim(new Claim(Claims.PreferredUsername, user.UserName));
            identity.AddClaim(new Claim(Claims.Name, user.UserName));
            identity.AddClaim(new Claim("nickname", user.NickName ?? user.UserName));
            identity.AddClaim(new Claim("points", user.Points.ToString()));
            identity.AddClaim(new Claim("level", user.Level.ToString()));
            
            // Add email if available
            if (!string.IsNullOrEmpty(user.Email))
            {
                identity.AddClaim(new Claim(Claims.Email, user.Email));
            }

            // Note: in this sample, the granted scopes match the requested scope
            // but you may want to allow the user to uncheck specific scopes.
            // For that, simply restrict the list of scopes before calling SetScopes.
            principal.SetScopes(request.GetScopes());
            principal.SetResources(await _scopeManager.ListResourcesAsync(principal.GetScopes()).ToListAsync());

            // Automatically create a permanent authorization to avoid requiring explicit consent
            // for future authorization or token requests containing the same scopes.
            var authorization = authorizations.LastOrDefault();
            if (authorization == null)
            {
                authorization = await _authorizationManager.CreateAsync(
                    principal: principal,
                    subject: await _userManager.GetUserIdAsync(user),
                    client: await _applicationManager.GetIdAsync(application),
                    type: AuthorizationTypes.Permanent,
                    scopes: principal.GetScopes());
            }

            principal.SetAuthorizationId(await _authorizationManager.GetIdAsync(authorization));

            foreach (var claim in principal.Claims)
            {
                claim.SetDestinations(GetDestinations(claim, principal));
            }

            // Returning a SignInResult will ask OpenIddict to issue the appropriate access/identity tokens.
            return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        [Authorize, FormValueRequired("submit.Deny")]
        [HttpPost("~/connect/authorize"), ValidateAntiForgeryToken]
        public IActionResult Deny()
        {
            // Notify OpenIddict that the authorization grant has been denied by the resource owner
            // to redirect the user agent to the client application using the appropriate response_mode.
            return Forbid(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        [HttpPost("~/connect/token")]
        public async Task<IActionResult> Exchange()
        {
            var request = HttpContext.GetOpenIddictServerRequest() ??
                throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");

            ClaimsPrincipal claimsPrincipal;

            if (request.IsAuthorizationCodeGrantType() || request.IsRefreshTokenGrantType())
            {
                // Retrieve the claims principal stored in the authorization code/refresh token.
                claimsPrincipal = (await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme)).Principal;
            }
            else if (request.IsClientCredentialsGrantType())
            {
                // Note: the client credentials are automatically validated by OpenIddict:
                // if client_id or client_secret are invalid, this action won't be invoked.

                var application = await _applicationManager.FindByClientIdAsync(request.ClientId);
                if (application == null)
                {
                    return Forbid(
                        authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                        properties: new AuthenticationProperties(new Dictionary<string, string>
                        {
                            [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidClient,
                            [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] =
                                "The client application was not found in the database."
                        }));
                }

                // Create a new ClaimsIdentity containing the claims that
                // will be used to create an id_token, a token or a code.
                var identity = new ClaimsIdentity(
                    TokenValidationParameters.DefaultAuthenticationType);

                // Use the client_id as the subject identifier.
                identity.AddClaim(new Claim(Claims.Subject, await _applicationManager.GetClientIdAsync(application) ?? string.Empty)
                    .SetDestinations(Destinations.AccessToken, Destinations.IdentityToken));

                identity.AddClaim(new Claim(Claims.Name, await _applicationManager.GetDisplayNameAsync(application) ?? string.Empty)
                    .SetDestinations(Destinations.AccessToken, Destinations.IdentityToken));

                claimsPrincipal = new ClaimsPrincipal(identity);

                claimsPrincipal.SetScopes(request.GetScopes());
                claimsPrincipal.SetResources(await _scopeManager.ListResourcesAsync(claimsPrincipal.GetScopes()).ToListAsync());
            }
            else
            {
                return Forbid(
                    authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                    properties: new AuthenticationProperties(new Dictionary<string, string>
                    {
                        [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.UnsupportedGrantType,
                        [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] =
                            "The specified grant type is not supported."
                    }));
            }

            // Returning a SignInResult will ask OpenIddict to issue the appropriate access/identity tokens.
            return SignIn(claimsPrincipal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        [HttpGet("~/connect/userinfo")]
        [HttpPost("~/connect/userinfo")]
        [Authorize(AuthenticationSchemes = OpenIddictServerAspNetCoreDefaults.AuthenticationScheme)]
        public async Task<IActionResult> Userinfo()
        {
            _logger.LogInformation("=== USERINFO ACTION HIT ===");
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge(
                    authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                    properties: new AuthenticationProperties(new Dictionary<string, string>
                    {
                        [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidToken,
                        [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] =
                            "The specified access token is bound to an account that no longer exists."
                    }));
            }

            var claims = new Dictionary<string, object>(StringComparer.Ordinal)
            {
                // Note: the "sub" claim is a mandatory claim and must be included in the JSON response.
                [Claims.Subject] = await _userManager.GetUserIdAsync(user),
                // Always include username for compatibility
                [Claims.PreferredUsername] = await _userManager.GetUserNameAsync(user),
                ["nickname"] = user.NickName ?? user.UserName
            };

            if (User.HasScope(Scopes.Email))
            {
                claims[Claims.Email] = await _userManager.GetEmailAsync(user);
                claims[Claims.EmailVerified] = await _userManager.IsEmailConfirmedAsync(user);
            }

            if (User.HasScope(Scopes.Profile))
            {
                claims[Claims.Name] = await _userManager.GetUserNameAsync(user);
                
                // Add custom claims
                claims["points"] = user.Points;
                claims["level"] = user.Level;
            }

            if (User.HasScope(Scopes.Roles))
            {
                claims[Claims.Role] = await _userManager.GetRolesAsync(user);
            }

            // Note: the complete list of standard claims supported by the OpenID Connect specification
            // can be found here: http://openid.net/specs/openid-connect-core-1_0.html#StandardClaims
            _logger.LogInformation("Userinfo claims: {@Claims}", claims);
            return Ok(claims);
        }

        [HttpGet("~/connect/logout")]
        public async Task<IActionResult> Logout()
        {
            // Ask ASP.NET Core Identity to delete the local and external cookies created
            // when the user agent is redirected from the external identity provider
            // after a successful authentication flow (e.g Google or Facebook).
            await _signInManager.SignOutAsync();

            // Returning a SignOutResult will ask OpenIddict to redirect the user agent
            // to the post_logout_redirect_uri specified by the client application or to
            // the RedirectUri specified in the authentication properties if none was set.
            return SignOut(
                authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                properties: new AuthenticationProperties
                {
                    RedirectUri = "/"
                });
        }

        private IEnumerable<string> GetDestinations(Claim claim, ClaimsPrincipal principal)
        {
            // Note: by default, claims are NOT automatically included in the access and identity tokens.
            // To allow OpenIddict to serialize them, you must attach them a destination, that specifies
            // whether they should be included in access tokens, in identity tokens or in both.

            switch (claim.Type)
            {
                case Claims.Name:
                case Claims.PreferredUsername:
                    yield return Destinations.AccessToken;

                    if (principal.HasScope(Scopes.Profile))
                        yield return Destinations.IdentityToken;

                    yield break;

                case Claims.Email:
                    yield return Destinations.AccessToken;

                    if (principal.HasScope(Scopes.Email))
                        yield return Destinations.IdentityToken;

                    yield break;

                case Claims.Role:
                    yield return Destinations.AccessToken;

                    if (principal.HasScope(Scopes.Roles))
                        yield return Destinations.IdentityToken;

                    yield break;
                
                // Custom claims
                case "nickname":
                case "points":
                case "level":
                    yield return Destinations.AccessToken;
                    
                    if (principal.HasScope(Scopes.Profile))
                        yield return Destinations.IdentityToken;
                    
                    yield break;

                // Never include the security stamp in the access and identity tokens, as it's a secret value.
                case "AspNet.Identity.SecurityStamp": yield break;

                default:
                    yield return Destinations.AccessToken;
                    yield break;
            }
        }
    }

    public class FormValueRequiredAttribute : ActionMethodSelectorAttribute
    {
        private readonly string _name;

        public FormValueRequiredAttribute(string name)
        {
            _name = name;
        }

        public override bool IsValidForRequest(RouteContext routeContext, ActionDescriptor action)
        {
            if (string.Equals(routeContext.HttpContext.Request.Method, "GET", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(routeContext.HttpContext.Request.Method, "HEAD", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(routeContext.HttpContext.Request.Method, "DELETE", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(routeContext.HttpContext.Request.Method, "TRACE", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (string.IsNullOrEmpty(routeContext.HttpContext.Request.ContentType) ||
                !routeContext.HttpContext.Request.ContentType.StartsWith("application/x-www-form-urlencoded", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            return !string.IsNullOrEmpty(routeContext.HttpContext.Request.Form[_name]);
        }
    }
}
