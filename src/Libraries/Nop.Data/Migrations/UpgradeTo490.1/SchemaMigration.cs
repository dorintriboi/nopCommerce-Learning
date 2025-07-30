using FluentMigrator;
using Nop.Core.Domain.Blogs;
using Nop.Data.Extensions;

namespace Nop.Data.Migrations.UpgradeTo490._1;

[NopSchemaMigration("2025-07-30 00:00:00", "Add Blog Category Table")]
public class SchemaMigration : Migration
{
    public override void Up()
    {
        if (!Schema.Table(nameof(BlogCategory)).Exists())
            Create.TableFor<BlogCategory>();
        
        if (!Schema.Table(nameof(BlogPostBlogCategoryMapping)).Exists())
            Create.TableFor<BlogPostBlogCategoryMapping>();
        
        var categoryTableName = nameof(BlogCategory);
        var restrictFromVendorsColumnName = nameof(BlogCategory.RestrictFromVendors);

        if (!Schema.Table(categoryTableName).Column(restrictFromVendorsColumnName).Exists())
        {
            Alter.Table(categoryTableName)
                .AddColumn(restrictFromVendorsColumnName)
                .AsBoolean()
                .NotNullable()
                .WithDefaultValue(false);
        }
    }

    public override void Down()
    {
        throw new NotImplementedException();
    }
}