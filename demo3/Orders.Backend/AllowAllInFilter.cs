using Hangfire.Dashboard;

namespace Orders.Backend
{
    public class AllowAllInFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize(DashboardContext context)
        {
            return true;
        }
    }
}