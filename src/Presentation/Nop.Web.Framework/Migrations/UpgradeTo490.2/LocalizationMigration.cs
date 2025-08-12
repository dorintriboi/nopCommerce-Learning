using FluentMigrator;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Data.Migrations;
using Nop.Services.Localization;
using Nop.Web.Framework.Extensions;

namespace Nop.Web.Framework.Migrations.UpgradeTo490._2;

[NopUpdateMigration("2025-08-06 00:00:00", "4.90.2", UpdateMigrationType.Localization)]
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
            ["Admin.ContentManagement.Blog.Categories"] = "Blog categories"
        }, languageId);

        #endregion
    }

    public override void Down()
    {
        //add the downgrade logic if necessary 
    }
}