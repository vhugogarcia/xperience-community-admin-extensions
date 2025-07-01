using CMS.EventLog;

using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Admin.Base.Authentication;
using Kentico.Xperience.Admin.Base.UIPages;

using Microsoft.AspNetCore.Http;

using XperienceCommunity.AdminExtensions;

[assembly: PageExtender(typeof(EventLogExtender))]
namespace XperienceCommunity.AdminExtensions;

public class EventLogExtender : PageExtender<EventLogList>
{
    private readonly IHttpContextAccessor httpContextAccessor;
    private readonly IAuthenticatedUserAccessor authenticatedUserAccessor;

    public EventLogExtender(IHttpContextAccessor httpContextAccessor,
                            IAuthenticatedUserAccessor authenticatedUserAccessor)
    {
        this.httpContextAccessor = httpContextAccessor;
        this.authenticatedUserAccessor = authenticatedUserAccessor;
    }

    public override Task ConfigurePage()
    {
        Page.PageConfiguration.HeaderActions.AddCommand("Clear", "Clear");

        return base.ConfigurePage();
    }

    [PageCommand]
    public async Task<ICommandResponse> Clear()
    {
        var user = await authenticatedUserAccessor.Get();
        var httpContext = httpContextAccessor.HttpContext;

        if (httpContext?.Connection?.RemoteIpAddress == null)
        {
            return Response().AddErrorMessage("Unable to clear event log due to missing IP address.");
        }

        EventLogHelper.ClearEventLog(user.UserID, user.UserName, httpContext.Connection.RemoteIpAddress.ToString());
        return Response().UseCommand("LoadData").AddSuccessMessage("Event log cleared.");
    }
}
