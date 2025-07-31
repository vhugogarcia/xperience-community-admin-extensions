using CMS.DataEngine;

using Kentico.Xperience.Admin.Base.FormAnnotations;
using Kentico.Xperience.Admin.Base.Forms;

namespace XperienceCommunity.AdminExtensions;

public class ContentTypeTypeGeneralSelectorDataProvider
    : IGeneralSelectorDataProvider
{
    private ObjectSelectorListItem<string>? reusable;
    private ObjectSelectorListItem<string>? website;
    private ObjectSelectorListItem<string>? email;
    private ObjectSelectorListItem<string>? headless;

    private ObjectSelectorListItem<string> Reusable => reusable ??= new()
    {
        Value = ClassContentTypeType.REUSABLE,
        Text = ClassContentTypeType.REUSABLE,
        IsValid = true
    };
    private ObjectSelectorListItem<string> Website => website ??= new()
    {
        Value = ClassContentTypeType.WEBSITE,
        Text = ClassContentTypeType.WEBSITE,
        IsValid = true
    };
    private ObjectSelectorListItem<string> Email => email ??= new()
    {
        Value = ClassContentTypeType.EMAIL,
        Text = ClassContentTypeType.EMAIL,
        IsValid = true
    };
    private ObjectSelectorListItem<string> Headless => headless ??= new()
    {
        Value = ClassContentTypeType.HEADLESS,
        Text = ClassContentTypeType.HEADLESS,
        IsValid = true
    };
    private static ObjectSelectorListItem<string> InvalidItem => new() { IsValid = false };

    public Task<PagedSelectListItems<string>> GetItemsAsync(string searchTerm, int pageIndex, CancellationToken cancellationToken)
    {
        IEnumerable<ObjectSelectorListItem<string>> items =
        [
            Reusable,
            Website,
            Email,
            Headless
        ];

        if (!string.IsNullOrEmpty(searchTerm))
        {
            items = items.Where(i => i.Text.StartsWith(searchTerm, StringComparison.OrdinalIgnoreCase));
        }

        return Task.FromResult(new PagedSelectListItems<string>()
        {
            NextPageAvailable = false,
            Items = items
        });
    }

    public Task<IEnumerable<ObjectSelectorListItem<string>>> GetSelectedItemsAsync(IEnumerable<string> selectedValues, CancellationToken cancellationToken) => Task.FromResult(selectedValues?.Select(v => GetSelectedItemByValue(v)) ?? []);

    private ObjectSelectorListItem<string> GetSelectedItemByValue(string contentTypeTypeValue) => contentTypeTypeValue switch
    {
        ClassContentTypeType.REUSABLE => Reusable,
        ClassContentTypeType.WEBSITE => Website,
        ClassContentTypeType.EMAIL => Email,
        ClassContentTypeType.HEADLESS => Headless,
        _ => InvalidItem
    };
}
