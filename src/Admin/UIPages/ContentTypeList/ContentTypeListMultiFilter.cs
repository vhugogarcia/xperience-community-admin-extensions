using CMS.ContentEngine;

using Kentico.Xperience.Admin.Base.Filters;
using Kentico.Xperience.Admin.Base.FormAnnotations;

namespace XperienceCommunity.AdminExtensions;

public class ContentTypeListMultiFilter
{
    [GeneralSelectorComponent(
        dataProviderType: typeof(ContentTypeTypeGeneralSelectorDataProvider),
        Label = "Content Type Uses",
        Placeholder = "Any"
    )]
    [FilterCondition(
        BuilderType = typeof(ContentTypeTypeWhereConditionBuilder),
        ColumnName = nameof(ContentTypeInfo.ClassContentTypeType)
    )]
    public IEnumerable<string>? ClassContentTypeTypes { get; set; }
}
