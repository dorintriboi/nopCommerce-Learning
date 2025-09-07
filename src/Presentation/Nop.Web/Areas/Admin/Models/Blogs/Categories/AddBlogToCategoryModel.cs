using Nop.Web.Framework.Models;

namespace Nop.Web.Areas.Admin.Models.Blogs.Categories;

/// <summary>
/// Represents a product model to add to the category
/// </summary>
public partial record AddBlogToCategoryModel : BaseNopModel
{
    #region Ctor

    public AddBlogToCategoryModel()
    {
        SelectedBlogIds = new List<int>();
    }
    #endregion

    #region Properties

    public int CategoryId { get; set; }

    public IList<int> SelectedBlogIds { get; set; }

    #endregion
}