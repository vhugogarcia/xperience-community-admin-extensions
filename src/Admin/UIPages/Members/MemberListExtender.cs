using Kentico.Xperience.Admin.Base.UIPages;

[assembly: PageExtender(typeof(MemberListExtender))]
namespace XperienceCommunity.AdminExtensions;

/// <summary>
/// Extends the <see cref="MemberList"/> (Members) page to add a "New member" header action
/// that opens the <see cref="MemberCreatePage"/>.
/// </summary>
public class MemberListExtender : PageExtender<MemberList>
{
    public override Task ConfigurePage()
    {
        Page.PageConfiguration.HeaderActions.AddLink<MemberCreatePage>("New member");

        return base.ConfigurePage();
    }
}
