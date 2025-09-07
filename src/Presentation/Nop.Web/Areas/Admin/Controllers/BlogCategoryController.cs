using Microsoft.AspNetCore.Mvc;
using Nop.Core.Caching;
using Nop.Core.Domain.Blogs;
using Nop.Services.Blogs.Blog;
using Nop.Services.Blogs.Category;
using Nop.Services.Catalog;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Blogs.Categories;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc;
using Nop.Web.Framework.Mvc.Filters;
using AddBlogToCategoryModel = Nop.Web.Areas.Admin.Models.Blogs.Categories.AddBlogToCategoryModel;

namespace Nop.Web.Areas.Admin.Controllers;

public partial class BlogCategoryController: BaseAdminController
{
    #region Fields

    protected readonly IBlogCategoryModelFactory _categoryModelFactory;
    protected readonly IBlogCategoryService _blogCategoryService;
    protected readonly ICustomerActivityService _customerActivityService;
    protected readonly IBlogService _blogService;
    protected readonly ILocalizationService _localizationService;
    protected readonly ILocalizedEntityService _localizedEntityService;
    protected readonly INotificationService _notificationService;
    protected readonly IPictureService _pictureService;
    protected readonly IStaticCacheManager _staticCacheManager;
    protected readonly IUrlRecordService _urlRecordService;

    #endregion

    #region Ctor

    public BlogCategoryController(
        IBlogService blogService,
        IBlogCategoryModelFactory categoryModelFactory,
        IBlogCategoryService blogCategoryService,
        ICustomerActivityService customerActivityService,
        ILocalizationService localizationService,
        ILocalizedEntityService localizedEntityService,
        INotificationService notificationService,
        IPictureService pictureService,
        IStaticCacheManager staticCacheManager,
        IUrlRecordService urlRecordService)
    {
        _categoryModelFactory = categoryModelFactory;
        _blogCategoryService = blogCategoryService;
        _customerActivityService = customerActivityService;
        _localizationService = localizationService;
        _localizedEntityService = localizedEntityService;
        _notificationService = notificationService;
        _pictureService = pictureService;
        _staticCacheManager = staticCacheManager;
        _urlRecordService = urlRecordService;
        _blogService = blogService;
    }

    #endregion

    #region Utilities

    protected virtual async Task UpdateLocalesAsync(BlogCategory category, BlogCategoryModel model)
    {
        foreach (var localized in model.Locales)
        {
            await _localizedEntityService.SaveLocalizedValueAsync(category,
                x => x.Name,
                localized.Name,
                localized.LanguageId);

            await _localizedEntityService.SaveLocalizedValueAsync(category,
                x => x.Description,
                localized.Description,
                localized.LanguageId);

            await _localizedEntityService.SaveLocalizedValueAsync(category,
                x => x.MetaKeywords,
                localized.MetaKeywords,
                localized.LanguageId);

            await _localizedEntityService.SaveLocalizedValueAsync(category,
                x => x.MetaDescription,
                localized.MetaDescription,
                localized.LanguageId);

            await _localizedEntityService.SaveLocalizedValueAsync(category,
                x => x.MetaTitle,
                localized.MetaTitle,
                localized.LanguageId);

            //search engine name
            var seName = await _urlRecordService.ValidateSeNameAsync(category, localized.SeName, localized.Name, false);
            await _urlRecordService.SaveSlugAsync(category, seName, localized.LanguageId);
        }
    }

    protected virtual async Task UpdatePictureSeoNamesAsync(BlogCategory category)
    {
        var picture = await _pictureService.GetPictureByIdAsync(category.PictureId);
        if (picture != null)
            await _pictureService.SetSeoFilenameAsync(picture.Id, await _pictureService.GetPictureSeNameAsync(category.Name));
    }

    #endregion

    public virtual IActionResult Index()
    {
        return RedirectToAction("List");
    }

    [CheckPermission(StandardPermission.ContentManagement.BLOG_CATEGORIES_VIEW)]
    public virtual async Task<IActionResult> List()
    {
        //prepare model
        var model = await _categoryModelFactory.PrepareCategorySearchModelAsync(new BlogCategorySearchModel());

        return View(model);
    }

    [HttpPost]
    [CheckPermission(StandardPermission.ContentManagement.BLOG_CATEGORIES_VIEW)]
    public virtual async Task<IActionResult> List(BlogCategorySearchModel searchModel)
    {
        //prepare model
        var model = await _categoryModelFactory.PrepareCategoryListModelAsync(searchModel);

        return Json(model);
    }
    
    [HttpPost]
    [CheckPermission(StandardPermission.ContentManagement.BLOG_CATEGORIES_CREATE_EDIT_DELETE)]
    public virtual async Task<IActionResult> DeleteSelected(ICollection<int> selectedIds)
    {
        if (selectedIds == null || !selectedIds.Any())
            return NoContent();

        var categories = await _blogCategoryService.GetCategoriesByIdsAsync(selectedIds.ToArray());

        await _blogCategoryService.DeleteCategoriesAsync(categories);

        //activity log
        var activityLogFormat = await _localizationService.GetResourceAsync("ActivityLog.DeleteCategory");
        await _customerActivityService.InsertActivitiesAsync("DeleteCategory", categories, category => string.Format(activityLogFormat, category.Name));

        return Json(new { Result = true });
    }
    
    [CheckPermission(StandardPermission.ContentManagement.BLOG_CATEGORIES_CREATE_EDIT_DELETE)]
    public virtual async Task<IActionResult> Create()
    {
        //prepare model
        var model = await _categoryModelFactory.PrepareCategoryModelAsync(new BlogCategoryModel(), null);

        return View(model);
    }
    
    [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
    [CheckPermission(StandardPermission.ContentManagement.BLOG_CATEGORIES_CREATE_EDIT_DELETE)]
    public virtual async Task<IActionResult> Create(BlogCategoryModel model, bool continueEditing)
    {
        if (ModelState.IsValid)
        {
            var category = model.ToEntity<BlogCategory>();
            category.CreatedOnUtc = DateTime.UtcNow;
            category.UpdatedOnUtc = DateTime.UtcNow;
            await _blogCategoryService.InsertCategoryAsync(category);

            //search engine name
            model.SeName = await _urlRecordService.ValidateSeNameAsync(category, model.SeName, category.Name, true);
            await _urlRecordService.SaveSlugAsync(category, model.SeName, 0);

            //locales
            await UpdateLocalesAsync(category, model);

            await _blogCategoryService.UpdateCategoryAsync(category);

            //update picture seo file name
            await UpdatePictureSeoNamesAsync(category);

            //activity log
            await _customerActivityService.InsertActivityAsync("AddNewBlogCategory",
                string.Format(await _localizationService.GetResourceAsync("ActivityLog.AddNewBlogCategory"), category.Name), category);

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.ContentManagement.Blog.Categories.Added"));

            if (!continueEditing)
                return RedirectToAction("List");

            return RedirectToAction("Edit", new { id = category.Id });
        }

        //prepare model
        model = await _categoryModelFactory.PrepareCategoryModelAsync(model, null, true);

        //if we got this far, something failed, redisplay form
        return View(model);
    }
    
    [CheckPermission(StandardPermission.ContentManagement.BLOG_CATEGORIES_VIEW)]
    public virtual async Task<IActionResult> Edit(int id)
    {
        //try to get a category with the specified id
        var category = await _blogCategoryService.GetCategoryByIdAsync(id);
        if (category == null || category.Deleted)
            return RedirectToAction("List");

        //prepare model
        var model = await _categoryModelFactory.PrepareCategoryModelAsync(null, category);

        return View(model);
    }
    
    [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
    [CheckPermission(StandardPermission.ContentManagement.BLOG_CATEGORIES_CREATE_EDIT_DELETE)]
    public virtual async Task<IActionResult> Edit(BlogCategoryModel model, bool continueEditing)
    {
        //try to get a category with the specified id
        var category = await _blogCategoryService.GetCategoryByIdAsync(model.Id);
        if (category == null || category.Deleted)
            return RedirectToAction("List");

        if (ModelState.IsValid)
        {
            var prevPictureId = category.PictureId;

            //if parent category changes, we need to clear cache for previous parent category
            if (category.ParentCategoryId != model.ParentCategoryId)
            {
                await _staticCacheManager.RemoveByPrefixAsync(NopCatalogDefaults.CategoriesByParentCategoryPrefix, category.ParentCategoryId);
                await _staticCacheManager.RemoveByPrefixAsync(NopCatalogDefaults.CategoriesChildIdsPrefix, category.ParentCategoryId);
                await _staticCacheManager.RemoveByPrefixAsync(NopCatalogDefaults.ChildCategoryIdLookupPrefix);
            }

            category = model.ToEntity(category);
            category.UpdatedOnUtc = DateTime.UtcNow;
            await _blogCategoryService.UpdateCategoryAsync(category);

            //search engine name
            model.SeName = await _urlRecordService.ValidateSeNameAsync(category, model.SeName, category.Name, true);
            await _urlRecordService.SaveSlugAsync(category, model.SeName, 0);

            //locales
            await UpdateLocalesAsync(category, model);
            
            await _blogCategoryService.UpdateCategoryAsync(category);

            //delete an old picture (if deleted or updated)
            if (prevPictureId > 0 && prevPictureId != category.PictureId)
            {
                var prevPicture = await _pictureService.GetPictureByIdAsync(prevPictureId);
                if (prevPicture != null)
                    await _pictureService.DeletePictureAsync(prevPicture);
            }

            //update picture seo file name
            await UpdatePictureSeoNamesAsync(category);

            //activity log
            await _customerActivityService.InsertActivityAsync("EditCategory",
                string.Format(await _localizationService.GetResourceAsync("ActivityLog.EditCategory"), category.Name), category);

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Catalog.Categories.Updated"));

            if (!continueEditing)
                return RedirectToAction("List");

            return RedirectToAction("Edit", new { id = category.Id });
        }

        //prepare model
        model = await _categoryModelFactory.PrepareCategoryModelAsync(model, category, true);

        //if we got this far, something failed, redisplay form
        return View(model);
    }
    
    #region Blogs

    [HttpPost]
    [CheckPermission(StandardPermission.ContentManagement.BLOG_CATEGORIES_VIEW)]
    public virtual async Task<IActionResult> BlogsList(BlogCategoryBlogPostSearchModel searchModel)
    {
        //try to get a category with the specified id
        var category = await _blogCategoryService.GetCategoryByIdAsync(searchModel.CategoryId)
            ?? throw new ArgumentException("No blog category found with the specified id");

        //prepare model
        var model = await _categoryModelFactory.PrepareCategoryBlogListModelAsync(searchModel, category);

        return Json(model);
    }

    [CheckPermission(StandardPermission.ContentManagement.BLOG_CATEGORIES_CREATE_EDIT_DELETE)]
    public virtual async Task<IActionResult> BlogUpdate(BlogCategoryBlogPostModel model)
    {
        //try to get a product category with the specified id
        var productCategory = await _blogCategoryService.GetBlogCategoryByIdAsync(model.Id)
            ?? throw new ArgumentException("No blog category mapping found with the specified id");

        //fill entity from product
        productCategory = model.ToEntity(productCategory);
        await _blogCategoryService.UpdateProductCategoryAsync(productCategory);

        return new NullJsonResult();
    }

    [CheckPermission(StandardPermission.ContentManagement.BLOG_CATEGORIES_CREATE_EDIT_DELETE)]
    public virtual async Task<IActionResult> BlogDelete(int id)
    {
        //try to get a product category with the specified id
        var productCategory = await _blogCategoryService.GetBlogCategoryByIdAsync(id)
            ?? throw new ArgumentException("No blog category mapping found with the specified id", nameof(id));

        await _blogCategoryService.DeleteBlogCategoryAsync(productCategory);

        return new NullJsonResult();
    }

    [CheckPermission(StandardPermission.ContentManagement.BLOG_CATEGORIES_CREATE_EDIT_DELETE)]
    public virtual async Task<IActionResult> BlogAddPopup(int blogCategoryId)
    {
        //prepare model
        var model = await _categoryModelFactory.PrepareAddBlogToCategorySearchModelAsync(new AddBlogToCategorySearchModel());

        return View(model);
    }

    [HttpPost]
    [CheckPermission(StandardPermission.ContentManagement.BLOG_CATEGORIES_CREATE_EDIT_DELETE)]
    public virtual async Task<IActionResult> BlogAddPopupList(AddBlogToCategorySearchModel searchModel)
    {
        //prepare model
        var model = await _categoryModelFactory.PrepareAddBlogToCategoryListModelAsync(searchModel);

        return Json(model);
    }

    [HttpPost]
    [FormValueRequired("save")]
    [CheckPermission(StandardPermission.ContentManagement.BLOG_CATEGORIES_CREATE_EDIT_DELETE)]
    public virtual async Task<IActionResult> BlogAddPopup(AddBlogToCategoryModel model)
    {
        //get selected products
        var selectedBlogs = await _blogService.GetBlogsByIdsAsync(model.SelectedBlogIds.ToArray());
        if (selectedBlogs.Any())
        {
            var existingBlogCategories = await _blogCategoryService.GetBlogCategoriesByCategoryIdAsync(model.CategoryId, showHidden: true);
            foreach (var blog in selectedBlogs)
            {
                //whether product category with such parameters already exists
                if (_blogCategoryService.FindProductCategory(existingBlogCategories, blog.Id, model.CategoryId) != null)
                    continue;

                //insert the new product category mapping
                await _blogCategoryService.InsertBlogPostBlogCategoryAsync(new BlogPostBlogCategoryMapping
                {
                    CategoryId = model.CategoryId,
                    BlogPostId = blog.Id,
                    IsFeaturedBlog = false,
                    DisplayOrder = 1
                });
            }
        }

        ViewBag.RefreshPage = true;

        return View(new AddBlogToCategorySearchModel());
    }

    #endregion
    
    [HttpPost]
    [CheckPermission(StandardPermission.ContentManagement.BLOG_CATEGORIES_CREATE_EDIT_DELETE)]
    public virtual async Task<IActionResult> Delete(int id)
    {
        //try to get a category with the specified id
        var category = await _blogCategoryService.GetCategoryByIdAsync(id);
        if (category == null)
            return RedirectToAction("List");

        await _blogCategoryService.DeleteCategoryAsync(category);

        //activity log
        await _customerActivityService.InsertActivityAsync("DeleteCategory",
            string.Format(await _localizationService.GetResourceAsync("ActivityLog.DeleteCategory"), category.Name), category);

        _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Catalog.Categories.Deleted"));

        return RedirectToAction("List");
    }
}