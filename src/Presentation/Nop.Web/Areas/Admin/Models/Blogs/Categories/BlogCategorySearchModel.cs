using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Web.Areas.Admin.Models.Blogs.Categories;

public partial record BlogCategorySearchModel : BaseSearchModel
{
    #region Properties

    [NopResourceDisplayName("Admin.ContentManagement.BlogCategories.List.SearchCategoryName")]
    public string SearchCategoryName { get; set; }

    [NopResourceDisplayName("Admin.ContentManagement.BlogCategories.List.SearchPublished")]
    public int SearchPublishedId { get; set; }

    public IList<SelectListItem> AvailablePublishedOptions { get; set; } = [];

    #endregion
}