using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Threading.Tasks;

namespace HabitTracker
{
    public class CustomClaimsPrincipalFactory : UserClaimsPrincipalFactory<IdentityUser>
    {
        public CustomClaimsPrincipalFactory(
            UserManager<IdentityUser> userManager,
            IOptions<IdentityOptions> optionsAccessor)
            : base(userManager, optionsAccessor)
        {
        }

        protected override async Task<ClaimsIdentity> GenerateClaimsAsync(IdentityUser user)
        {
            var identity = await base.GenerateClaimsAsync(user);
            var roles = await UserManager.GetRolesAsync(user);

            // Add role claims
            foreach (var role in roles)
            {
                identity.AddClaim(new Claim(ClaimTypes.Role, role));
            }

            // Add custom claims based on roles
            if (roles.Contains("Admin"))
            {
                identity.AddClaim(new Claim("Permission", "CanManageUsers"));
                identity.AddClaim(new Claim("Permission", "CanViewReports"));
                identity.AddClaim(new Claim("Permission", "CanDeleteHabits"));
            }

            // Add user type claim
            identity.AddClaim(new Claim("UserType", roles.Contains("Admin") ? "Administrator" : "RegularUser"));

            // Add habit management permissions
            identity.AddClaim(new Claim("Permission", "CanCreateHabits"));
            identity.AddClaim(new Claim("Permission", "CanEditOwnHabits"));
            identity.AddClaim(new Claim("Permission", "CanDeleteOwnHabits"));

            return identity;
        }
    }
}
