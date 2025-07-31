using CMS.DataEngine;

using Kentico.Xperience.Admin.Base.Filters;

namespace XperienceCommunity.AdminExtensions;

public class ContentTypeTypeWhereConditionBuilder : IWhereConditionBuilder
{
    public Task<IWhereCondition> Build(string columnName, object value)
    {
        if (string.IsNullOrEmpty(columnName))
        {
            throw new ArgumentException(
                $"{nameof(columnName)} cannot be a null or an empty string.");
        }

        var whereCondition = new WhereCondition();

        if (value is null || value is not IEnumerable<string> contentTypeUses)
        {
            return Task.FromResult<IWhereCondition>(whereCondition);
        }

        _ = whereCondition.WhereIn(columnName, contentTypeUses.ToArray());

        return Task.FromResult<IWhereCondition>(whereCondition);
    }
}
