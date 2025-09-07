using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Web.Areas.Admin.Models.Blogs.Categories;

public record BlogCategoryBlogPostModel: BaseNopEntityModel
{
    #region Properties

    public int CategoryId { get; set; }

    public int BlogPostId { get; set; }

    [NopResourceDisplayName("Admin.ContentManagement.BlogCategories.BlogPost.Fields.Blog")]
    public string BlogPostName { get; set; }

    [NopResourceDisplayName("Admin.ContentManagement.BlogCategories.BlogPost.Fields.IsFeaturedBlog")]
    public bool IsFeaturedBlog { get; set; }

    [NopResourceDisplayName("Admin.ContentManagement.BlogCategories.BlogPost.Fields.DisplayOrder")]
    public int DisplayOrder { get; set; }

    #endregion
}