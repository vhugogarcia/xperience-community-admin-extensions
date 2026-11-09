using System.Globalization;
using System.Net;
using System.Text;

using CMS.Core;
using CMS.DataEngine;
using CMS.LicenseProvider;
using CMS.LicenseProvider.Internal;

using Kentico.Xperience.Admin.Base.UIPages;

[assembly: PageExtender(typeof(XperienceCommunity.AdminExtensions.LicenseSettingsExtender))]
namespace XperienceCommunity.AdminExtensions;

/// <summary>
/// Extends the <see cref="SettingsDetails"/> page (Settings &gt; System &gt; License) to display
/// an "Information" callout with license details below the built-in synchronization callout.
/// The callout is hidden when no license is set or when the license data cannot be read.
/// </summary>
/// <remarks>
/// Only data exposed by the public <see cref="ILicenseRetriever"/> API is displayed:
/// key type, expiration, channel limits, validity and grace period.
/// </remarks>
public class LicenseSettingsExtender : PageExtender<SettingsDetails>
{
    /// <summary>
    /// Code name of the settings group category that renders the "Licensing" card.
    /// </summary>
    private const string LICENSING_GROUP_CATEGORY_NAME = "CMS.System.Licenses.Group";

    private const string DATE_FORMAT = "dd MMM yyyy HH:mm 'UTC'";

    /// <summary>
    /// Number of days before expiration from which the callout switches to a warning.
    /// </summary>
    private const int EXPIRATION_WARNING_DAYS = 30;

    private readonly ILicenseRetriever licenseRetriever;
    private readonly IInfoProvider<SettingsCategoryInfo> settingsCategoryInfoProvider;
    private readonly IEventLogService eventLogService;

    public LicenseSettingsExtender(
        ILicenseRetriever licenseRetriever,
        IInfoProvider<SettingsCategoryInfo> settingsCategoryInfoProvider,
        IEventLogService eventLogService)
    {
        this.licenseRetriever = licenseRetriever;
        this.settingsCategoryInfoProvider = settingsCategoryInfoProvider;
        this.eventLogService = eventLogService;
    }

    public override async Task<TemplateClientProperties> ConfigureTemplateProperties(TemplateClientProperties properties)
    {
        properties = await base.ConfigureTemplateProperties(properties);

        if (properties is not SettingsDetailTemplateClientProperties settingsProperties || settingsProperties.Categories is null)
        {
            return properties;
        }

        // The callout is purely informational. Any unexpected failure (e.g. a SaaS/cloud instance
        // returning license data in a different shape) must never break the License settings page,
        // so the callout is simply skipped in that case.
        try
        {
            var licensingGroup = await settingsCategoryInfoProvider.GetAsync(LICENSING_GROUP_CATEGORY_NAME);
            if (licensingGroup is null)
            {
                return properties;
            }

            var groupCategory = settingsProperties.Categories.FirstOrDefault(c => c.CategoryId == licensingGroup.CategoryID);
            if (groupCategory is null)
            {
                // Not the License settings page.
                return properties;
            }

            var callout = BuildLicenseDetailsCallout();
            if (callout is null)
            {
                return properties;
            }

            groupCategory.Callouts ??= [];
            groupCategory.Callouts.Add(callout);
        }
        catch (Exception ex)
        {
            // Hide the callout rather than crash the page. Logged only once to avoid flooding
            // the event log on every visit to the License settings page.
            eventLogService.LogWarning(
                nameof(LicenseSettingsExtender),
                "LICENSE_DETAILS_CALLOUT",
                ex,
                "The license details callout could not be displayed on the License settings page.",
                LoggingPolicy.ONLY_ONCE);
        }

        return properties;
    }

    /// <summary>
    /// Builds the license details callout, or returns <c>null</c> when no license is set.
    /// </summary>
    private CalloutConfiguration? BuildLicenseDetailsCallout()
    {
        var activationType = licenseRetriever.GetActivationType();
        if (activationType == ActivationType.None)
        {
            return null;
        }

        bool isValid = licenseRetriever.IsValidLicense;
        bool inGracePeriod = licenseRetriever.IsInGracePeriod();
        var expirationUtc = licenseRetriever.GetExpirationDateUtc();
        bool hasExpiration = expirationUtc != DateTime.MinValue;
        bool isExpired = hasExpiration && expirationUtc <= DateTime.UtcNow;

        // A production license key is short-lived by design (renewed during license synchronization),
        // so its expiration date is not the subscription end. The note explains this, and the
        // "expires soon" warning is only meaningful when the date is the real end of the license.
        bool showRenewalNote = activationType != ActivationType.Development && !licenseRetriever.IsActivatedByLicenseToken;
        bool expiresSoon = !showRenewalNote
            && hasExpiration
            && !isExpired
            && expirationUtc <= DateTime.UtcNow.AddDays(EXPIRATION_WARNING_DAYS);

        var rows = new List<(string Label, string Value)>
        {
            ("Status", isExpired ? "Expired" : GetStatusText(isValid, inGracePeriod)),
            ("Key type", GetActivationTypeText(activationType)),
            ("Expiration", FormatExpiration(expirationUtc)),
        };

        var limitations = licenseRetriever.GetChannelLimitations();
        if (limitations is not null)
        {
            rows.Add(("Channels", limitations.StandardChannels.ToString(CultureInfo.InvariantCulture)));
            rows.Add(("Microchannels", limitations.Microchannels.ToString(CultureInfo.InvariantCulture)));
        }

        var content = new StringBuilder();
        foreach (var (label, value) in rows)
        {
            content.Append("<strong>")
                .Append(WebUtility.HtmlEncode(label))
                .Append(":</strong> ")
                .Append(WebUtility.HtmlEncode(value))
                .Append("<br />");
        }

        if (showRenewalNote)
        {
            content.Append("<br /><em><strong>Note:</strong> The expiration date is the key's current validity (typically up to one month) ")
                .Append("and is refreshed automatically during license synchronization. ")
                .Append("It doesn't mean your subscription ends earlier.</em>");
        }

        // The admin UI has no error callout, so an expired license uses the warning style
        // with a headline to set it apart from the "expires soon" warning.
        bool isWarning = isExpired || expiresSoon || !isValid || inGracePeriod;

        return new CalloutConfiguration
        {
            Headline = isExpired ? "License expired" : null,
            Content = content.ToString(),
            ContentAsHtml = true,
            Type = isWarning ? CalloutType.FriendlyWarning : CalloutType.QuickTip,
            Placement = CalloutPlacement.OnPaper,
        };
    }

    private static string GetStatusText(bool isValid, bool inGracePeriod) => (isValid, inGracePeriod) switch
    {
        (_, true) => "In grace period",
        (true, false) => "Valid",
        _ => "Not valid",
    };

    private static string GetActivationTypeText(ActivationType activationType) => activationType switch
    {
        ActivationType.Production => "Production",
        ActivationType.Development => "Development",
        ActivationType.Trial => "Trial",
        ActivationType.NonProductionToken => "Non-production",
        _ => activationType.ToString(),
    };

    private static string FormatExpiration(DateTime expirationUtc)
    {
        if (expirationUtc == DateTime.MinValue)
        {
            return "N/A";
        }

        string formatted = expirationUtc.ToString(DATE_FORMAT, CultureInfo.InvariantCulture);
        int daysLeft = (int)Math.Floor((expirationUtc - DateTime.UtcNow).TotalDays);

        return daysLeft >= 0
            ? $"{formatted} ({daysLeft} day(s) left)"
            : $"{formatted} (expired)";
    }
}
