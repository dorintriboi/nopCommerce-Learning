using FluentMigrator;
using Nop.Core.Domain.Blogs;
using Nop.Data.Mapping;

namespace Nop.Data.Migrations.UpgradeTo490._1;

[NopSchemaMigration("2025-07-30 01:00:00", "Update BlogCategory datetime type precision")]
public class MySqlDateTimePrecisionMigration : ForwardOnlyMigration
{
    public override void Up()
    {
        var dataSettings = DataSettingsManager.LoadSettings();

        //update the types only in MySql 
        if (dataSettings.DataProvider != DataProviderType.MySql)
            return;
        
        Alter.Table(NameCompatibilityManager.GetTableName(typeof(BlogCategory)))
            .AlterColumn(NameCompatibilityManager.GetColumnName(typeof(BlogCategory), nameof(BlogCategory.CreatedOnUtc)))
            .AsCustom("datetime(6)");
        Alter.Table(NameCompatibilityManager.GetTableName(typeof(BlogCategory)))
            .AlterColumn(NameCompatibilityManager.GetColumnName(typeof(BlogCategory), nameof(BlogCategory.UpdatedOnUtc)))
            .AsCustom("datetime(6)");
    }
}