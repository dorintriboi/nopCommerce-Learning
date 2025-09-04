using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Web.Areas.Admin.Models.Blogs.Categories;

public record BlogCategoryBlogPostModel: BaseNopEntityModel
{
    #region Properties

    public int CategoryId { get; set; }

    public int BlogPostId { get; set; }

    [NopResourceDisplayName("Admin.ContentManagement.BlogCategories.BlogPosts.Fields.Product")]
    public string BlogPostName { get; set; }

    [NopResourceDisplayName("Admin.ContentManagement.BlogCategories.BlogPosts.Fields.IsFeaturedProduct")]
    public bool IsFeaturedProduct { get; set; }

    [NopResourceDisplayName("Admin.ContentManagement.BlogCategories.BlogPosts.Fields.DisplayOrder")]
    public int DisplayOrder { get; set; }

    #endregion
}