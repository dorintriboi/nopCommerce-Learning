using FluentMigrator;
using FluentMigrator.Postgres;
using Nop.Core.Domain.Blogs;
using Nop.Data.Mapping;

namespace Nop.Data.Migrations.UpgradeTo490._1;

[NopSchemaMigration("2025-07-30 02:00:00", "AddIndexesMigration for 4.90.1")]
public class IndexesMigration: ForwardOnlyMigration
{
    /// <summary>
    /// Collect the UP migration expressions
    /// </summary>
    public override void Up()
    {
        const string blogCategoryName = "IX_BlogCategory_Name";
        if (!Schema.Table(nameof(BlogCategory)).Index(blogCategoryName).Exists())
        {
            Create.Index(blogCategoryName).OnTable(nameof(BlogCategory))
                .OnColumn(nameof(BlogCategory.Name)).Ascending()
                .WithOptions().NonClustered();
        }

        const string blogCategorySubjectToAcl = "IX_BlogCategory_SubjectToAcl";
        if (!Schema.Table(nameof(BlogCategory)).Index(blogCategorySubjectToAcl).Exists())
        {
            Create.Index(blogCategorySubjectToAcl).OnTable(nameof(BlogCategory))
                .OnColumn(nameof(BlogCategory.SubjectToAcl)).Ascending()
                .WithOptions().NonClustered();
        }

        const string blogCategoryShowOnHomepage = "IX_BlogCategory_ShowOnHomepage";
        if (!Schema.Table(nameof(BlogCategory)).Index(blogCategoryShowOnHomepage).Exists())
        {
            Create.Index(blogCategoryShowOnHomepage).OnTable(nameof(BlogCategory))
                .OnColumn(nameof(BlogCategory.ShowOnHomepage)).Ascending()
                .WithOptions().NonClustered();
        }

        const string blogCategoryPublished = "IX_BlogCategory_Published";
        if (!Schema.Table(nameof(BlogCategory)).Index(blogCategoryPublished).Exists())
        {
            Create.Index(blogCategoryPublished).OnTable(nameof(BlogCategory))
                .OnColumn(nameof(BlogCategory.Published)).Ascending()
                .WithOptions().NonClustered();
        }

        const string blogCategoryParentBlogCategoryId = "IX_BlogCategory_ParentBlogCategoryId";
        if (!Schema.Table(nameof(BlogCategory)).Index(blogCategoryParentBlogCategoryId).Exists())
        {
            Create.Index(blogCategoryParentBlogCategoryId).OnTable(nameof(BlogCategory))
                .OnColumn(nameof(BlogCategory.ParentCategoryId)).Ascending()
                .WithOptions().NonClustered();
        }

        const string blogCategoryDeleteId = "IX_BlogCategory_Delete_Id";
        if (!Schema.Table(nameof(BlogCategory)).Index(blogCategoryDeleteId).Exists())
        {
            Create.Index(blogCategoryDeleteId).OnTable(nameof(BlogCategory))
                .OnColumn(nameof(BlogCategory.Deleted)).Ascending()
                .OnColumn(nameof(BlogCategory.Id)).Ascending()
                .WithOptions().NonClustered();
        }

        const string blogCategoryLimitedToStores = "IX_BlogCategory_LimitedToStores";
        if (!Schema.Table(nameof(BlogCategory)).Index(blogCategoryLimitedToStores).Exists())
        {
            Create.Index(blogCategoryLimitedToStores).OnTable(nameof(BlogCategory))
                .OnColumn(nameof(BlogCategory.LimitedToStores)).Ascending()
                .WithOptions().NonClustered();
        }

        const string blogCategoryExtended = "IX_PCM_BlogCategoryId_Extended";
        if (!Schema.Table(nameof(BlogPostBlogCategoryMapping)).Index(blogCategoryExtended).Exists())
        {
            Create.Index(blogCategoryExtended).OnTable(NameCompatibilityManager.GetTableName(typeof(BlogPostBlogCategoryMapping)))
                .OnColumn(nameof(BlogPostBlogCategoryMapping.CategoryId)).Ascending()
                .OnColumn(nameof(BlogPostBlogCategoryMapping.IsFeaturedBlog)).Ascending()
                .WithOptions().NonClustered()
                .Include(nameof(BlogPostBlogCategoryMapping.CategoryId));
        }

        const string blogCategoryBlogCategory = "IX_PCM_BlogPost_and_BlogCategory";
        if (!Schema.Table(nameof(BlogPostBlogCategoryMapping)).Index(blogCategoryBlogCategory).Exists())
        {
            Create.Index(blogCategoryBlogCategory).OnTable(NameCompatibilityManager.GetTableName(typeof(BlogPostBlogCategoryMapping)))
                .OnColumn(nameof(BlogPostBlogCategoryMapping.CategoryId)).Ascending()
                .OnColumn(nameof(BlogPostBlogCategoryMapping.BlogPostId)).Ascending()
                .WithOptions().NonClustered();
        }

        const string blogCategoryNameDeleted = "IX_BlogCategory_Name_Deleted";
        if (!Schema.Table(nameof(BlogCategory)).Index(blogCategoryNameDeleted).Exists())
        {
            Create.Index(blogCategoryNameDeleted)
                .OnTable(nameof(BlogCategory))
                .OnColumn(nameof(BlogCategory.Name)).Ascending()
                .OnColumn(nameof(BlogCategory.Deleted)).Ascending()
                .WithOptions().NonClustered()
                .Include(nameof(BlogCategory.Id));
        }

        const string blogCategoryCategoryId = "IX_BlogPostBlogCategoryMapping_CategoryId";
        if (!Schema.Table(nameof(BlogPostBlogCategoryMapping)).Index(blogCategoryCategoryId).Exists())
        {
            Create.Index(blogCategoryCategoryId)
                .OnTable(NameCompatibilityManager.GetTableName(typeof(BlogPostBlogCategoryMapping)))
                .OnColumn(nameof(BlogPostBlogCategoryMapping.CategoryId)).Ascending()
                .WithOptions().NonClustered()
                .Include(nameof(BlogPostBlogCategoryMapping.BlogPostId));
        }
    }
}