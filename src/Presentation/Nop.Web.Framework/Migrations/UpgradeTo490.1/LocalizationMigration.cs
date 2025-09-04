using FluentMigrator;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Data.Migrations;
using Nop.Services.Localization;
using Nop.Web.Framework.Extensions;

namespace Nop.Web.Framework.Migrations.UpgradeTo490._1;

[NopUpdateMigration("2025-08-03 00:00:00", "4.90.1", UpdateMigrationType.Localization)]
public class LocalizationMigration: MigrationBase
{
    public override void Up()
    {
       if (!DataSettingsManager.IsDatabaseInstalled())
            return;

        //do not use DI, because it produces exception on the installation process
        var localizationService = EngineContext.Current.Resolve<ILocalizationService>();

        var (languageId, _) = this.GetLanguageData();

        #region Add or update locales

        localizationService.AddOrUpdateLocaleResource(new Dictionary<string, string>
        {
            //#4834
            ["Security.Permission.ContentManagement.BlogCategoriesCreateEditDelete"] = "Admin area. Blog categories. Create, edit, delete",
            ["Security.Permission.ContentManagement.BlogCategoriesView"] = "Admin area. Blog categories. View",
            ["Admin.ContentManagement.Blog.Categories"] = "Blog categories",
            ["Admin.Documentation.Reference.Blog.Categories"] = "Learn more about <a target=\"_blank\" href=\"{0}\">blog categories</a>",
            ["Admin.ContentManagement.BlogCategories.Fields.Name"] = "Name",
            ["Admin.ContentManagement.BlogCategories.Fields.Published"] = "Published",
            ["Admin.ContentManagement.BlogCategories.Fields.DisplayOrder"] = "Display order",
            ["Admin.ContentManagement.BlogCategories.List.ImportFromExcelTip"] = "Imported blog categories are distinguished by ID. If the ID already exists, then its corresponding category will be updated. You should not specify ID (leave 0) for new categories.",
            ["Admin.ContentManagement.BlogCategories.List.SearchCategoryName"] = "Category name",
            ["Admin.ContentManagement.BlogCategories.List.SearchPublished"] = "Published",
            ["Admin.ContentManagement.BlogCategories.List.SearchPublished.All"] = "All",
            ["Admin.ContentManagement.BlogCategories.List.SearchPublished.PublishedOnly"] = "Published only",
            ["Admin.ContentManagement.BlogCategories.List.SearchPublished.UnpublishedOnly"] = "Unpublished only"
        }, languageId);

        #endregion
    }

    public override void Down()
    {
        //add the downgrade logic if necessary 
    }
}