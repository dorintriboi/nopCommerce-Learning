using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Web.Areas.Admin.Models.Blogs.Categories;

public record AddBlogToCategorySearchModel: BaseSearchModel
{
    #region Ctor

    public AddBlogToCategorySearchModel()
    {
        AvailableCategories = new List<SelectListItem>();
        AvailableManufacturers = new List<SelectListItem>();
        AvailableVendors = new List<SelectListItem>();
        AvailableBlogTypes = new List<SelectListItem>();
    }

    #endregion

    #region Properties

    [NopResourceDisplayName("Admin.ContentManagement.Blogs.List.SearchBlogName")]
    public string SearchBlogName { get; set; }

    [NopResourceDisplayName("Admin.ContentManagement.Blogs.List.SearchCategory")]
    public int SearchCategoryId { get; set; }

    [NopResourceDisplayName("Admin.ContentManagement.Blogs.List.SearchManufacturer")]
    public int SearchManufacturerId { get; set; }

    [NopResourceDisplayName("Admin.ContentManagement.Blogs.List.SearchVendor")]
    public int SearchVendorId { get; set; }

    [NopResourceDisplayName("Admin.ContentManagement.Blogs.List.SearchBlogType")]
    public int SearchBlogTypeId { get; set; }

    public IList<SelectListItem> AvailableCategories { get; set; }

    public IList<SelectListItem> AvailableManufacturers { get; set; }

    public IList<SelectListItem> AvailableVendors { get; set; }

    public IList<SelectListItem> AvailableBlogTypes { get; set; }

    #endregion
}