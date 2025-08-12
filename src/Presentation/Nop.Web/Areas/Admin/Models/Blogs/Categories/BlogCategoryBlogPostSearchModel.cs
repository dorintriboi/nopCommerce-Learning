using Nop.Web.Framework.Models;

namespace Nop.Web.Areas.Admin.Models.Blogs.Categories;

public record BlogCategoryBlogPostSearchModel: BaseSearchModel
{
    #region Properties

    public int CategoryId { get; set; }

    #endregion
}