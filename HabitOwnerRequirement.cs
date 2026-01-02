using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using HabitTracker.Models.Interface;

namespace HabitTracker
{
    public class HabitOwnerRequirement : IAuthorizationRequirement
    {
        // This requirement ensures that only the owner of a habit can access it
    }

    public class HabitOwnerRequirementHandler : AuthorizationHandler<HabitOwnerRequirement>
    {
        private readonly IServiceProvider _serviceProvider;

        public HabitOwnerRequirementHandler(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            HabitOwnerRequirement requirement)
        {
            // Create a scope to resolve scoped services
            using var scope = _serviceProvider.CreateScope();
            var habitRepo = scope.ServiceProvider.GetRequiredService<IHabitRepository>();

            // Get the habit ID from the route data (GET requests)
            var routeData = context.Resource as Microsoft.AspNetCore.Routing.RouteData;
            if (routeData != null)
            {
                var habitIdValue = routeData.Values["id"]?.ToString();
                if (int.TryParse(habitIdValue, out int habitId))
                {
                    var userId = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

                    if (!string.IsNullOrEmpty(userId))
                    {
                        // Check if the user owns this habit
                        var habit = await habitRepo.GetHabitByIdAsync(habitId, userId);
                        if (habit != null)
                        {
                            context.Succeed(requirement);
                            return;
                        }
                    }
                }
            }

            // For POST requests, check the HttpContext.Items for habit ID
            // (We'll set this in the controller action)
            var httpContext = context.Resource as Microsoft.AspNetCore.Http.HttpContext;
            if (httpContext != null)
            {
                var habitId = httpContext.Items["HabitId"] as int?;
                if (habitId.HasValue)
                {
                    var userId = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

                    if (!string.IsNullOrEmpty(userId))
                    {
                        // Check if the user owns this habit
                        var habit = await habitRepo.GetHabitByIdAsync(habitId.Value, userId);
                        if (habit != null)
                        {
                            context.Succeed(requirement);
                            return;
                        }
                    }
                }
            }

            // If we get here, authorization failed
            context.Fail();
        }
    }
}
