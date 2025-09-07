using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Models.Translation;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Web.Areas.Admin.Models.Blogs.Categories;

public partial record BlogCategoryModel: BaseNopEntityModel, IAclSupportedModel,
    ITranslationSupportedModel, ILocalizedModel<BlogCategoryLocalizedModel>
{
    #region Ctor

    public BlogCategoryModel()
    {
        if (PageSize < 1)
        {
            PageSize = 5;
        }

        Locales = new List<BlogCategoryLocalizedModel>();
        AvailableCategoryTemplates = new List<SelectListItem>();
        AvailableCategories = new List<SelectListItem>();

        SelectedCustomerRoleIds = new List<int>();
        AvailableCustomerRoles = new List<SelectListItem>();

        BlogCategoryBlogPostSearchModel = new BlogCategoryBlogPostSearchModel();
    }

    #endregion

    #region Properties

    [NopResourceDisplayName("Admin.Catalog.Categories.Fields.Name")]
    public string Name { get; set; }

    [NopResourceDisplayName("Admin.Catalog.Categories.Fields.Description")]
    public string Description { get; set; }

    [NopResourceDisplayName("Admin.Catalog.Categories.Fields.CategoryTemplate")]
    public int CategoryTemplateId { get; set; }
    public IList<SelectListItem> AvailableCategoryTemplates { get; set; }

    [NopResourceDisplayName("Admin.Catalog.Categories.Fields.MetaKeywords")]
    public string MetaKeywords { get; set; }

    [NopResourceDisplayName("Admin.Catalog.Categories.Fields.MetaDescription")]
    public string MetaDescription { get; set; }

    [NopResourceDisplayName("Admin.Catalog.Categories.Fields.MetaTitle")]
    public string MetaTitle { get; set; }

    [NopResourceDisplayName("Admin.Catalog.Categories.Fields.SeName")]
    public string SeName { get; set; }

    [NopResourceDisplayName("Admin.Catalog.Categories.Fields.Parent")]
    public int ParentCategoryId { get; set; }

    [UIHint("Picture")]
    [NopResourceDisplayName("Admin.Catalog.Categories.Fields.Picture")]
    public int PictureId { get; set; }

    [NopResourceDisplayName("Admin.Catalog.Categories.Fields.PageSize")]
    public int PageSize { get; set; }

    [NopResourceDisplayName("Admin.Catalog.Categories.Fields.AllowCustomersToSelectPageSize")]
    public bool AllowCustomersToSelectPageSize { get; set; }

    [NopResourceDisplayName("Admin.Catalog.Categories.Fields.PageSizeOptions")]
    public string PageSizeOptions { get; set; }

    [NopResourceDisplayName("Admin.Catalog.Categories.Fields.ShowOnHomepage")]
    public bool ShowOnHomepage { get; set; }

    [NopResourceDisplayName("Admin.Catalog.Categories.Fields.IncludeInTopMenu")]
    public bool IncludeInTopMenu { get; set; }

    [NopResourceDisplayName("Admin.Catalog.Categories.Fields.Published")]
    public bool Published { get; set; }

    [NopResourceDisplayName("Admin.Catalog.Categories.Fields.Deleted")]
    public bool Deleted { get; set; }

    [NopResourceDisplayName("Admin.Catalog.Categories.Fields.DisplayOrder")]
    public int DisplayOrder { get; set; }

    [NopResourceDisplayName("Admin.Catalog.Categories.Fields.RestrictFromVendors")]
    public bool RestrictFromVendors { get; set; }

    public IList<BlogCategoryLocalizedModel> Locales { get; set; }

    public string Breadcrumb { get; set; }

    public IList<int> SelectedCustomerRoleIds { get; set; }
    public IList<SelectListItem> AvailableCustomerRoles { get; set; }

    public IList<SelectListItem> AvailableCategories { get; set; }

    public BlogCategoryBlogPostSearchModel BlogCategoryBlogPostSearchModel { get; set; }

    public bool PreTranslationAvailable { get; set; }

    #endregion
}

public partial record BlogCategoryLocalizedModel : ILocalizedLocaleModel
{
    public int LanguageId { get; set; }

    [NopResourceDisplayName("Admin.Catalog.Categories.Fields.Name")]
    public string Name { get; set; }

    [NopResourceDisplayName("Admin.Catalog.Categories.Fields.Description")]
    public string Description { get; set; }

    [NopResourceDisplayName("Admin.Catalog.Categories.Fields.MetaKeywords")]
    public string MetaKeywords { get; set; }

    [NopResourceDisplayName("Admin.Catalog.Categories.Fields.MetaDescription")]
    public string MetaDescription { get; set; }

    [NopResourceDisplayName("Admin.Catalog.Categories.Fields.MetaTitle")]
    public string MetaTitle { get; set; }

    [NopResourceDisplayName("Admin.Catalog.Categories.Fields.SeName")]
    public string SeName { get; set; }
}