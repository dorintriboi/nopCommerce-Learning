using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core.Domain.Blogs;
using Nop.Core.Domain.Catalog;
using Nop.Services.Blogs.Category;
using Nop.Services.Localization;
using Nop.Services.Seo;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Blogs.Categories;
using Nop.Web.Framework.Factories;
using Nop.Web.Framework.Models.Extensions;

namespace Nop.Web.Areas.Admin.Factories; 

public partial class BlogCategoryModelFactory(
    ILocalizationService localizationService,
    CatalogSettings catalogSettings,
    IBaseAdminModelFactory baseAdminModelFactory,
    ILocalizedModelFactory localizedModelFactory,
    IBlogCategoryService blogCategoryService,
    IUrlRecordService urlRecordService) : IBlogCategoryModelFactory
{
    public async Task<BlogCategorySearchModel> PrepareCategorySearchModelAsync(BlogCategorySearchModel searchModel)
    {
        ArgumentNullException.ThrowIfNull(searchModel);
        
        //prepare "published" filter (0 - all; 1 - published only; 2 - unpublished only)
        searchModel.AvailablePublishedOptions.Add(new SelectListItem
        {
            Value = "0",
            Text = await localizationService.GetResourceAsync("Admin.ContentManagement.BlogCategories.List.SearchPublished.All")
        });
        searchModel.AvailablePublishedOptions.Add(new SelectListItem
        {
            Value = "1",
            Text = await localizationService.GetResourceAsync("Admin.ContentManagement.BlogCategories.List.SearchPublished.PublishedOnly")
        });
        searchModel.AvailablePublishedOptions.Add(new SelectListItem
        {
            Value = "2",
            Text = await localizationService.GetResourceAsync("Admin.ContentManagement.BlogCategories.List.SearchPublished.UnpublishedOnly")
        });

        //prepare page parameters
        searchModel.SetGridPageSize();

        return searchModel;
    }

    public async Task<BlogCategoryListModel> PrepareCategoryListModelAsync(BlogCategorySearchModel searchModel)
    {
        ArgumentNullException.ThrowIfNull(searchModel);
        //get categories
        var categories = await blogCategoryService.GetAllCategoriesAsync(categoryName: searchModel.SearchCategoryName,
            showHidden: true,
            pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize,
            overridePublished: searchModel.SearchPublishedId == 0 ? null : (searchModel.SearchPublishedId == 1));

        //prepare grid model
        var model = await new BlogCategoryListModel().PrepareToGridAsync(searchModel, categories, () =>
        {
            return categories.SelectAwait(async category =>
            {
                //fill in model values from the entity
                var categoryModel = category.ToModel<BlogCategoryModel>();

                //fill in additional values (not existing in the entity)
                categoryModel.Breadcrumb = await blogCategoryService.GetFormattedBreadCrumbAsync(category);
                categoryModel.SeName = await urlRecordService.GetSeNameAsync(category, 0, true, false);

                return categoryModel;
            });
        });

        return model;
    }

    public async Task<BlogCategoryModel> PrepareCategoryModelAsync(BlogCategoryModel model, BlogCategory category, bool excludeProperties = false)
    {
        Func<BlogCategoryLocalizedModel, int, Task> localizedModelConfiguration = null;

        if (category != null)
        {
            //fill in model values from the entity
            if (model == null)
            {
                model = category.ToModel<BlogCategoryModel>();
                model.SeName = await urlRecordService.GetSeNameAsync(category, 0, true, false);
            }

            //prepare nested search model
            PrepareCategoryBlogPostSearchModel(model.BlogCategoryBlogPostSearchModel, category);

            //define localized model configuration action
            localizedModelConfiguration = async (locale, languageId) =>
            {
                locale.Name = await localizationService.GetLocalizedAsync(category, entity => entity.Name, languageId, false, false);
                locale.Description = await localizationService.GetLocalizedAsync(category, entity => entity.Description, languageId, false, false);
                locale.MetaKeywords = await localizationService.GetLocalizedAsync(category, entity => entity.MetaKeywords, languageId, false, false);
                locale.MetaDescription = await localizationService.GetLocalizedAsync(category, entity => entity.MetaDescription, languageId, false, false);
                locale.MetaTitle = await localizationService.GetLocalizedAsync(category, entity => entity.MetaTitle, languageId, false, false);
                locale.SeName = await urlRecordService.GetSeNameAsync(category, languageId, false, false);
            };
        }

        //set default values for the new model
        if (category == null)
        {
            model.PageSize = catalogSettings.DefaultCategoryPageSize;
            model.PageSizeOptions = catalogSettings.DefaultCategoryPageSizeOptions;
            model.Published = true;
            model.IncludeInTopMenu = true;
            model.AllowCustomersToSelectPageSize = true;
        }
        
        //prepare localized models
        if (!excludeProperties)
            model.Locales = await localizedModelFactory.PrepareLocalizedModelsAsync(localizedModelConfiguration);

        //prepare available category templates
        await baseAdminModelFactory.PrepareCategoryTemplatesAsync(model.AvailableCategoryTemplates, false);

        //prepare available parent categories
        await baseAdminModelFactory.PrepareCategoriesAsync(model.AvailableCategories,
            defaultItemText: await localizationService.GetResourceAsync("Admin.ContentManagement.BlogCategories.Fields.Parent.None"));

        await baseAdminModelFactory.PreparePreTranslationSupportModelAsync(model);

        return model;
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
    
    /// <summary>
    /// Prepare category product search model
    /// </summary>
    /// <param name="searchModel">Category product search model</param>
    /// <param name="category">Category</param>
    /// <returns>Category product search model</returns>
    protected virtual BlogCategoryBlogPostSearchModel PrepareCategoryBlogPostSearchModel(BlogCategoryBlogPostSearchModel searchModel, BlogCategory category)
    {
        ArgumentNullException.ThrowIfNull(searchModel);

        ArgumentNullException.ThrowIfNull(category);

        searchModel.CategoryId = category.Id;

        //prepare page parameters
        searchModel.SetGridPageSize();

        return searchModel;
    }
}