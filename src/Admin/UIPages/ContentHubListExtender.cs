using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Admin.Base.UIPages;

using Microsoft.Extensions.Configuration;

using XperienceCommunity.AdminExtensions;

[assembly: PageExtender(typeof(ContentHubListExtender))]
namespace XperienceCommunity.AdminExtensions;

/// <summary>
/// Extends the <see cref="ContentHubList"/> page to customize its configuration.
/// </summary>
public class ContentHubListExtender : PageExtender<ContentHubList>
{
    private readonly IConfiguration configuration;

    public ContentHubListExtender(IConfiguration configuration) => this.configuration = configuration;

    /// <summary>
    /// Configures the page to set the available page size options to n items per page.
    /// </summary>
    public override Task ConfigurePage()
    {
        int pageSize = configuration.GetValue("XperienceCommunityAdminExtensions:ContentHubListPageSize", 50);
        Page.PageConfiguration.PageSizes = [pageSize];
        return base.ConfigurePage();
    }
}
