using Kentico.Xperience.Admin.Base.FormAnnotations;

namespace XperienceCommunity.AdminExtensions;

/// <summary>
/// Form model of the <see cref="MemberCreatePage"/>.
/// </summary>
public class MemberCreateModel
{
    [TextInputComponent(Label = "User name", Order = 1)]
    [RequiredValidationRule]
    [MaxLengthValidationRule(100)]
    public string UserName { get; set; } = string.Empty;

    [TextInputComponent(Label = "Email address", Order = 2)]
    [RequiredValidationRule]
    [EmailValidationRule]
    [MaxLengthValidationRule(254)]
    public string Email { get; set; } = string.Empty;

    // The password policy is taken from the site's ASP.NET Identity options in MemberCreatePage.GetFormItems.
    [PasswordComponent(Label = "Password", Order = 3)]
    [RequiredValidationRule]
    [MaxLengthValidationRule(100)]
    public string Password { get; set; } = string.Empty;

    [PasswordComponent(Label = "Confirm password", IgnorePasswordPolicy = true, Order = 4)]
    [RequiredValidationRule]
    [MaxLengthValidationRule(100)]
    public string ConfirmPassword { get; set; } = string.Empty;

    [CheckBoxComponent(
        Label = "Enabled",
        ExplanationText = "Enabled members can sign in right away. No email is sent to the member.",
        Order = 5)]
    public bool Enabled { get; set; } = true;
}
