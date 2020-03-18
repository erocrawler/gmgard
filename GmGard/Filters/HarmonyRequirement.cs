using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using GmGard.Models;
using System.Threading.Tasks;

namespace GmGard.Filters
{
    public class HarmonyHandler : AuthorizationHandler<HarmonyRequirement>
    {
        private HarmonySettingsModel _harmonySettings;

        public HarmonyHandler(IOptions<AppSettingsModel> harmonySettings)
        {
            _harmonySettings = harmonySettings.Value.HarmonySettings ?? new HarmonySettingsModel();
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, HarmonyRequirement requirement)
        {
            if (_harmonySettings.Harmony != true || context.User.Identity.IsAuthenticated)
            {
                context.Succeed(requirement);
            }
            else
            {
                context.Fail();
            }
            return Task.FromResult(0);
        }
    }

    public class HarmonyRequirement : IAuthorizationRequirement { }
}