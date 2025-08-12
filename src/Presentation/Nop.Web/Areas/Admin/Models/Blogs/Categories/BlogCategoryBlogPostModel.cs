using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Web.Areas.Admin.Models.Blogs.Categories;

public record BlogCategoryBlogPostModel: BaseNopEntityModel
{
    #region Properties

    public int CategoryId { get; set; }

    public int ProductId { get; set; }

    [NopResourceDisplayName("Admin.Catalog.Categories.Products.Fields.Product")]
    public string ProductName { get; set; }

    [NopResourceDisplayName("Admin.Catalog.Categories.Products.Fields.IsFeaturedProduct")]
    public bool IsFeaturedProduct { get; set; }

    [NopResourceDisplayName("Admin.Catalog.Categories.Products.Fields.DisplayOrder")]
    public int DisplayOrder { get; set; }

    #endregion
}