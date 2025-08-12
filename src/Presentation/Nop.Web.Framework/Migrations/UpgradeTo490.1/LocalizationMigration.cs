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
        }, languageId);

        #endregion
    }

    public override void Down()
    {
        //add the downgrade logic if necessary 
    }
}