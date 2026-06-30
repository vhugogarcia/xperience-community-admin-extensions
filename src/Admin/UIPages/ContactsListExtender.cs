using CMS.ContactManagement;
using CMS.DataEngine;

using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Admin.Base.UIPages;
using Kentico.Xperience.Admin.DigitalMarketing.UIPages;

[assembly: PageExtender(typeof(XperienceCommunity.AdminExtensions.ContactsListExtender))]
namespace XperienceCommunity.AdminExtensions;

/// <summary>
/// Extends the <see cref="ContactList"/> (Contact management &gt; Contacts) page to add a
/// "Delete all contacts" header action.
/// </summary>
public class ContactsListExtender : PageExtender<ContactList>
{
    private readonly IContactsBulkDeletionService contactsBulkDeletionService;
    private readonly IInfoProvider<ContactInfo> contactInfoProvider;

    public ContactsListExtender(
        IContactsBulkDeletionService contactsBulkDeletionService,
        IInfoProvider<ContactInfo> contactInfoProvider)
    {
        this.contactsBulkDeletionService = contactsBulkDeletionService;
        this.contactInfoProvider = contactInfoProvider;
    }

    public override Task ConfigurePage()
    {
        int count = contactInfoProvider.Get().Column("ContactID").Count;

        Page.PageConfiguration.Caption = count > 0 ? $"Contacts - {count}" : "Contacts";

        // Only offer the "Delete All" action when there is something to delete.
        if (count > 0)
        {
            Page.PageConfiguration.HeaderActions.AddCommandWithConfirmation(
                label: "Delete All",
                command: nameof(DeleteAll),
                confirmation: $"This will permanently delete all {count} contact(s). " +
                    "All activities, recalculation queues and contact group memberships " +
                    "related to these contacts will also be deleted. This action cannot be undone.",
                confirmationButton: "Delete All",
                title: "Delete all contacts",
                destructive: true);
        }

        return base.ConfigurePage();
    }

    [PageCommand]
    public async Task<ICommandResponse> DeleteAll()
    {
        await contactsBulkDeletionService.BulkDelete(new WhereCondition("1=1"));

        return Response().AddSuccessMessage("All contacts deleted. Please refresh the page to see the updated list.");
    }
}
