using CMS.Membership;

using Kentico.Xperience.Admin.Base.Forms;

namespace XperienceCommunity.AdminExtensions;

/// <summary>
/// Fails when the value is already used by another member, either as an email address or as a user name.
/// </summary>
/// <remarks>
/// Both columns are checked for both fields, so that one member's user name cannot be set to another
/// member's email address, or vice versa.
/// </remarks>
internal sealed class UniqueMemberIdentifierValidationRule : ValidationRule<string>
{
    private readonly IMemberInfoProvider memberInfoProvider;
    private readonly string errorMessage;

    public UniqueMemberIdentifierValidationRule(IMemberInfoProvider memberInfoProvider, string errorMessage)
    {
        this.memberInfoProvider = memberInfoProvider;
        this.errorMessage = errorMessage;
    }

    protected override string DefaultErrorMessage => errorMessage;

    public override async Task<ValidationResult> Validate(string value, IFormFieldValueProvider formFieldValueProvider)
    {
        // An empty value is reported by the required rule.
        if (string.IsNullOrWhiteSpace(value))
        {
            return await ValidationResult.SuccessResult();
        }

        return await IsInUse(memberInfoProvider, value.Trim())
            ? await ValidationResult.FailResult(errorMessage)
            : await ValidationResult.SuccessResult();
    }

    internal static async Task<bool> IsInUse(IMemberInfoProvider memberInfoProvider, string value)
    {
        var members = await memberInfoProvider.Get()
            .WhereEquals(nameof(MemberInfo.MemberEmail), value)
            .Or()
            .WhereEquals(nameof(MemberInfo.MemberName), value)
            .TopN(1)
            .Column(nameof(MemberInfo.MemberID))
            .GetEnumerableTypedResultAsync();

        return members.Any();
    }
}

/// <summary>
/// Fails when the value differs from the value of another field of the same form.
/// </summary>
internal sealed class MatchesFieldValidationRule : ValidationRule<string>
{
    private readonly string otherFieldName;
    private readonly string errorMessage;

    public MatchesFieldValidationRule(string otherFieldName, string errorMessage)
    {
        this.otherFieldName = otherFieldName;
        this.errorMessage = errorMessage;
    }

    protected override string DefaultErrorMessage => errorMessage;

    public override Task<ValidationResult> Validate(string value, IFormFieldValueProvider formFieldValueProvider)
    {
        // An empty value is reported by the required rule.
        if (string.IsNullOrEmpty(value))
        {
            return ValidationResult.SuccessResult();
        }

        formFieldValueProvider.TryGet(otherFieldName, out string? otherValue);

        return string.Equals(value, otherValue, StringComparison.Ordinal)
            ? ValidationResult.SuccessResult()
            : ValidationResult.FailResult(errorMessage);
    }
}
