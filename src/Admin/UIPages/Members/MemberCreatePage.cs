using CMS.Membership;

using Kentico.Membership;
using Kentico.Xperience.Admin.Base.Forms;
using Kentico.Xperience.Admin.Base.Forms.Internal;
using Kentico.Xperience.Admin.Base.UIPages;

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

[assembly: UIPage(
    parentType: typeof(MemberList),
    slug: "create",
    uiPageType: typeof(MemberCreatePage),
    name: "New member",
    templateName: TemplateNames.EDIT,
    order: UIPageOrder.NoOrder)]

namespace XperienceCommunity.AdminExtensions;

/// <summary>
/// Page of the Members application that lets an administrator register a new member.
/// </summary>
[UIPermission(SystemPermissions.CREATE)]
public class MemberCreatePage : ModelEditPage<MemberCreateModel>
{
    private const string EMAIL_IN_USE_MESSAGE = "A member with this email address or user name already exists.";
    private const string USER_NAME_IN_USE_MESSAGE = "A member with this user name or email address already exists.";
    private const string PASSWORDS_DO_NOT_MATCH_MESSAGE = "The password and its confirmation do not match.";

    private readonly IMemberInfoProvider memberInfoProvider;
    private readonly IMemberExternalLoginInfoProvider memberExternalLoginInfoProvider;
    private readonly IPageLinkGenerator pageLinkGenerator;
    private readonly IOptions<IdentityOptions> identityOptions;
    private readonly IOptions<PasswordHasherOptions> passwordHasherOptions;

    private MemberCreateModel? model;

    public MemberCreatePage(
        IFormItemCollectionProvider formItemCollectionProvider,
        IFormDataBinder formDataBinder,
        IMemberInfoProvider memberInfoProvider,
        IMemberExternalLoginInfoProvider memberExternalLoginInfoProvider,
        IPageLinkGenerator pageLinkGenerator,
        IOptions<IdentityOptions> identityOptions,
        IOptions<PasswordHasherOptions> passwordHasherOptions)
        : base(formItemCollectionProvider, formDataBinder)
    {
        this.memberInfoProvider = memberInfoProvider;
        this.memberExternalLoginInfoProvider = memberExternalLoginInfoProvider;
        this.pageLinkGenerator = pageLinkGenerator;
        this.identityOptions = identityOptions;
        this.passwordHasherOptions = passwordHasherOptions;
    }

    protected override MemberCreateModel Model => model ??= new MemberCreateModel();

    public override Task ConfigurePage()
    {
        PageConfiguration.Headline = "New member";
        PageConfiguration.SubmitConfiguration.Label = "Save";

        return base.ConfigurePage();
    }

    // The base commands require the Update permission; creating a member requires Create instead.
    [PageCommand(Permission = SystemPermissions.CREATE)]
    public override Task<ICommandResponse<FormChangeResult>> Change(FormChangeCommandArguments args) => base.Change(args);

    [PageCommand(Permission = SystemPermissions.CREATE)]
    public override Task<ICommandResponse> Submit(FormSubmissionCommandArguments args) => base.Submit(args);

    protected override async Task<ICollection<IFormItem>> GetFormItems()
    {
        var items = await base.GetFormItems();
        var components = items.OfType<IFormComponent>().ToList();

        GetComponent(components, nameof(MemberCreateModel.UserName))
            .AddValidationRule(new UniqueMemberIdentifierValidationRule(memberInfoProvider, USER_NAME_IN_USE_MESSAGE));

        GetComponent(components, nameof(MemberCreateModel.Email))
            .AddValidationRule(new UniqueMemberIdentifierValidationRule(memberInfoProvider, EMAIL_IN_USE_MESSAGE));

        GetComponent(components, nameof(MemberCreateModel.ConfirmPassword))
            .AddValidationRule(new MatchesFieldValidationRule(nameof(MemberCreateModel.Password), PASSWORDS_DO_NOT_MATCH_MESSAGE));

        // Members sign in on the live site, so the password must follow the site's ASP.NET Identity
        // password options and not the administration's password policy.
        if (GetComponent(components, nameof(MemberCreateModel.Password)) is PasswordComponent password)
        {
            var policy = identityOptions.Value.Password;

            password.Properties.RequiredLength = policy.RequiredLength;
            password.Properties.RequiredUniqueChars = policy.RequiredUniqueChars;
            password.Properties.RequireDigit = policy.RequireDigit;
            password.Properties.RequireNonAlphanumeric = policy.RequireNonAlphanumeric;
            password.Properties.RequireUppercase = policy.RequireUppercase;
            password.Properties.RequireLowercase = policy.RequireLowercase;
        }

        return items;
    }

    protected override async Task<ICommandResponse> ProcessFormData(MemberCreateModel model, ICollection<IFormItem> formItems)
    {
        string userName = model.UserName.Trim();
        string email = model.Email.Trim();

        // The validation rules already checked both values. Checking again right before the insert
        // narrows the window in which two administrators could register the same member.
        if (await UniqueMemberIdentifierValidationRule.IsInUse(memberInfoProvider, email)
            || await UniqueMemberIdentifierValidationRule.IsInUse(memberInfoProvider, userName))
        {
            return ResponseFrom(new FormSubmissionResult(FormSubmissionStatus.ValidationFailure)
            {
                Items = await GetClientFormItems(formItems)
            }).AddErrorMessage(EMAIL_IN_USE_MESSAGE);
        }

        var hasher = new PasswordHasher<ApplicationUser>(passwordHasherOptions);
        var member = new ApplicationUser
        {
            UserName = userName,
            Email = email,
            Enabled = model.Enabled,
            SecurityStamp = Guid.NewGuid().ToString()
        };

        // The hash format does not depend on the user type, so it is verified by the site's
        // UserManager<TUser> regardless of which ApplicationUser subclass the site registers.
        member.PasswordHash = hasher.HashPassword(member, model.Password);

        // Kentico's own member store, so the member is saved exactly as one registered on the live site.
        using var store = new ApplicationUserStore<ApplicationUser>(memberInfoProvider, memberExternalLoginInfoProvider);
        await store.CreateAsync(member, CancellationToken.None);

        string editPath = pageLinkGenerator.GetPath<MemberEdit>(new PageParameterValues
        {
            { typeof(MemberEditSection), member.Id }
        });

        return NavigateTo(editPath, true).AddSuccessMessage($"Member '{userName}' created.");
    }

    private static IFormComponent GetComponent(IEnumerable<IFormComponent> components, string name) =>
        components.First(component => string.Equals(component.Name, name, StringComparison.OrdinalIgnoreCase));
}
