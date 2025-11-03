[assembly: UIPage(
    parentType: typeof(WebPageLayout),
    slug: "documentation",
    uiPageType: typeof(DocumentationTab),
    name: "Docs",
    templateName: "@xperiencecommunity/admin-extensions/DocumentationTab",
    order: 20000,
    Icon = Icons.BookOpened)]

namespace XperienceCommunity.AdminExtensions;

/// <summary>
/// Provides a documentation tab for content types that displays markdown content.
/// </summary>
/// <remarks>
/// This implementation is currently commented out pending correct parent type identification.
/// The tab functionality will be activated once the correct Xperience admin parent type is determined.
/// </remarks>
public sealed class DocumentationTab : WebPageBase<DocumentationTabProperties>
{
    private readonly IContentQueryExecutor contentQueryExecutor;
    private readonly IHtmlLocalizer<SharedResources> htmlLocalizer;

    public DocumentationTab(
        IWebPageManagerFactory webPageManagerFactory,
        IAuthenticatedUserAccessor authenticatedUserAccessor,
        IPageLinkGenerator pageLinkGenerator,
        IContentQueryExecutor contentQueryExecutor,
        IHtmlLocalizer<SharedResources> htmlLocalizer)
        : base(authenticatedUserAccessor, webPageManagerFactory, pageLinkGenerator)
    {
        this.contentQueryExecutor = contentQueryExecutor;
        this.htmlLocalizer = htmlLocalizer;
    }

    public override async Task<DocumentationTabProperties> ConfigureTemplateProperties(DocumentationTabProperties properties)
    {
        var builder = new ContentItemQueryBuilder()
            .ForContentTypes(query => query.ForWebsite([WebPageIdentifier.WebPageItemID]))
            .InLanguage(WebPageIdentifier.LanguageName);

        var webPages = await contentQueryExecutor.GetWebPageResult(builder, container => container);
        var currentPageItem = webPages.FirstOrDefault();

        // If the current page doesn't exist, we can't show the documentation
        if (currentPageItem is null)
        {
            properties.PageAvailability = PageAvailabilityStatus.NotAvailable;
            return properties;
        }

        // Get the content type name from the content item
        string contentTypeName = currentPageItem.ContentTypeName;
        properties.ContentTypeName = contentTypeName;

        // Retrieve the markdown content from the localizer using the pattern: XperienceCommunity.AdminExtensions.DocumentationTab.[CONTENTYPENAME]
        string localizationKey = $"XperienceCommunity.AdminExtensions.DocumentationTab.{contentTypeName}";
        string markdownContent = htmlLocalizer.GetHtmlStringOrDefault(localizationKey, new HtmlString("")).ToString();

        // Validate if the markdown content is empty or whitespace
        if (string.IsNullOrWhiteSpace(markdownContent))
        {
            properties.PageAvailability = PageAvailabilityStatus.NotAvailable;
            return properties;
        }

        // Convert markdown to HTML
        properties.HtmlContent = ConvertMarkdownToHtml(markdownContent);
        properties.PageAvailability = PageAvailabilityStatus.Available;

        return properties;
    }

    /// <summary>
    /// Converts markdown string to HTML.
    /// </summary>
    /// <param name="markdownContent">The markdown content to convert.</param>
    /// <returns>The HTML representation of the markdown content.</returns>
    private static string ConvertMarkdownToHtml(string markdownContent)
    {
        if (string.IsNullOrWhiteSpace(markdownContent))
        {
            return string.Empty;
        }

        try
        {
            // Parse markdown to HTML using Markdig
            var pipeline = new MarkdownPipelineBuilder()
                .UseAdvancedExtensions()
                .Build();

            string htmlContent = Markdown.ToHtml(markdownContent, pipeline);

            return htmlContent;
        }
        catch
        {
            return string.Empty;
        }
    }
}

/// <summary>
/// Client properties for the Documentation tab.
/// </summary>
public sealed class DocumentationTabProperties : WebPagePropertiesTabClientProperties
{
    /// <summary>
    /// Indicates whether the page is available for display.
    /// </summary>
    public PageAvailabilityStatus PageAvailability { get; set; }

    /// <summary>
    /// The HTML content rendered from markdown.
    /// </summary>
    public string? HtmlContent { get; set; }

    /// <summary>
    /// The content type name.
    /// </summary>
    public string? ContentTypeName { get; set; }

    /// <summary>
    /// Error message if documentation cannot be loaded.
    /// </summary>
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Represents the availability status of a page.
/// </summary>
public enum PageAvailabilityStatus
{
    Available,
    NotAvailable
}
