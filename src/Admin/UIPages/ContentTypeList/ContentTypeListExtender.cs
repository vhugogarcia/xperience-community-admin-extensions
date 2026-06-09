using CMS.Core;

using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Admin.Base.UIPages;

using XperienceCommunity.AdminExtensions;

[assembly: PageExtender(typeof(ContentTypeListExtender))]
namespace XperienceCommunity.AdminExtensions;

public class ContentTypeListExtender : PageExtender<ContentTypeList>
{
    private readonly IEventLogService eventLogService;

    public ContentTypeListExtender(IEventLogService eventLogService) => this.eventLogService = eventLogService;

    public override Task ConfigurePage()
    {
        _ = base.ConfigurePage();

        if (Page.PageConfiguration.FilterConfiguration.FormModel is null)
        {
            Page.PageConfiguration.FilterConfiguration.FormModel = new ContentTypeListMultiFilter();
        }
        else
        {
            eventLogService.LogWarning(
                nameof(ContentTypeListExtender),
                "DUPLICATE_FILTER",
                loggingPolicy: LoggingPolicy.ONLY_ONCE);
        }

        return Task.CompletedTask;
    }
}
