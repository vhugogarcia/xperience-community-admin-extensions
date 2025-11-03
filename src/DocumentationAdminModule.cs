using CMS.Core;

[assembly: CMS.RegisterModule(typeof(DocumentationAdminModule))]

namespace XperienceCommunity.AdminExtensions;

internal class DocumentationAdminModule : AdminModule
{
    public DocumentationAdminModule()
        : base(nameof(DocumentationAdminModule))
    {
    }

    protected override void OnInit(ModuleInitParameters parameters)
    {
        base.OnInit(parameters);

        RegisterClientModule("xperiencecommunity", "admin-extensions");
    }
}
