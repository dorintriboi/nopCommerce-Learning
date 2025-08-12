using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core.Domain.Blogs;
using Nop.Services.Localization;
using Nop.Web.Areas.Admin.Models.Blogs.Categories;

namespace Nop.Web.Areas.Admin.Factories; 

public partial class BlogCategoryModelFactory(
    ILocalizationService localizationService) : IBlogCategoryModelFactory
{
    public async Task<BlogCategorySearchModel> PrepareCategorySearchModelAsync(BlogCategorySearchModel searchModel)
    {
        ArgumentNullException.ThrowIfNull(searchModel);
        
        //prepare "published" filter (0 - all; 1 - published only; 2 - unpublished only)
        searchModel.AvailablePublishedOptions.Add(new SelectListItem
        {
            Value = "0",
            Text = await localizationService.GetResourceAsync("Admin.Catalog.Categories.List.SearchPublished.All")
        });
        searchModel.AvailablePublishedOptions.Add(new SelectListItem
        {
            Value = "1",
            Text = await localizationService.GetResourceAsync("Admin.Catalog.Categories.List.SearchPublished.PublishedOnly")
        });
        searchModel.AvailablePublishedOptions.Add(new SelectListItem
        {
            Value = "2",
            Text = await localizationService.GetResourceAsync("Admin.Catalog.Categories.List.SearchPublished.UnpublishedOnly")
        });

        //prepare page parameters
        searchModel.SetGridPageSize();

        return searchModel;
    }

    public Task<BlogCategoryListModel> PrepareCategoryListModelAsync(BlogCategorySearchModel searchModel)
    {
        throw new NotImplementedException();
    }

    public Task<BlogCategoryModel> PrepareCategoryModelAsync(BlogCategoryModel model, BlogCategory category, bool excludeProperties = false)
    {
        throw new NotImplementedException();
    }

    public Task<BlogCategoryBlogPostListModel> PrepareCategoryProductListModelAsync(BlogCategoryBlogPostSearchModel searchModel, BlogCategory category)
    {
        throw new NotImplementedException();
    }

    public Task<AddBlogToCategorySearchModel> PrepareAddProductToCategorySearchModelAsync(AddBlogToCategorySearchModel searchModel)
    {
        throw new NotImplementedException();
    }

    public Task<AddBlogToCategoryListModel> PrepareAddProductToCategoryListModelAsync(AddBlogToCategoryListModel searchModel)
    {
        throw new NotImplementedException();
    }
}