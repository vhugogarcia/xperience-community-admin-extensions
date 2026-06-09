using CMS.DataEngine;
using CMS.EventLog;

using Kentico.Xperience.Admin.Base.UIPages;

[assembly: PageExtender(typeof(EventLogExtender))]
namespace XperienceCommunity.AdminExtensions;

public class EventLogExtender : PageExtender<EventLogList>
{
    private readonly IInfoProvider<EventLogInfo> eventLogInfoProvider;

    public EventLogExtender(IInfoProvider<EventLogInfo> eventLogInfoProvider) => this.eventLogInfoProvider = eventLogInfoProvider;

    public override Task ConfigurePage()
    {
        Page.PageConfiguration.HeaderActions.AddCommand("Clear", nameof(Clear));

        return base.ConfigurePage();
    }

    [PageCommand]
    public async Task<ICommandResponse> Clear()
    {
        IWhereCondition where = new WhereCondition("1=1");
        eventLogInfoProvider.BulkDelete(where);

        return Response().UseCommand("LoadData").AddSuccessMessage("Event log cleared.");
    }
}
