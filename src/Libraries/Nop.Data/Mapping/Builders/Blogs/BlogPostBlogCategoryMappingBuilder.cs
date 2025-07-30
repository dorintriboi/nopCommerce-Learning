using FluentMigrator.Builders.Create.Table;
using Nop.Core.Domain.Blogs;
using Nop.Data.Extensions;

namespace Nop.Data.Mapping.Builders.Blogs;

public partial class BlogPostBlogCategoryMappingBuilder : NopEntityBuilder<BlogPostBlogCategoryMapping>
{
    #region Methods

    /// <summary>
    /// Apply entity configuration
    /// </summary>
    /// <param name="table">Create table expression builder</param>
    public override void MapEntity(CreateTableExpressionBuilder table)
    {
        table
            .WithColumn(nameof(BlogPostBlogCategoryMapping.CategoryId)).AsInt32().ForeignKey<BlogCategory>()
            .WithColumn(nameof(BlogPostBlogCategoryMapping.BlogPostId)).AsInt32().ForeignKey<BlogPost>();
    }

    #endregion
}