using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Blogs;
using Nop.Data;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Security;

namespace Nop.Services.Blogs.Category;

public class BlogCategoryService(
    IRepository<BlogCategory> blogCategoryRepository,
    IRepository<BlogPost> blogRepository,
    ILocalizationService localizationService,
    IRepository<BlogPostBlogCategoryMapping> blogPostBlogCategoryMappingRepository,
    ICustomerService customerService,
    IWorkContext workContext,
    IAclService aclService,
    IStaticCacheManager staticCacheManager): IBlogCategoryService
{
    public async Task<bool> CanVendorAddProductsAsync(BlogCategory category, IList<BlogCategory> allCategories = null)
    {
        ArgumentNullException.ThrowIfNull(category);

        if (await workContext.GetCurrentVendorAsync() is null) // check vendors only
            return true;

        if (category.RestrictFromVendors)
            return false;

        var breadcrumb = await GetCategoryBreadCrumbAsync(category, allCategories, showHidden: true);

        return !breadcrumb.Any(c => c.RestrictFromVendors);
    }

    public async Task DeleteCategoryAsync(BlogCategory category)
    {
        await blogCategoryRepository.DeleteAsync(category);

        //reset a "Parent category" property of all child subcategories
        var subcategories = await GetAllCategoriesByParentCategoryIdAsync(category.Id, true);
        foreach (var subcategory in subcategories)
        {
            subcategory.ParentCategoryId = 0;
            await UpdateCategoryAsync(subcategory);
        }
    }

    public async Task<IList<BlogCategory>> GetAllCategoriesAsync(bool showHidden = false)
    {
        var key = staticCacheManager.PrepareKeyForDefaultCache(NopBlogsDefaults.CategoriesAllCacheKey,
            await customerService.GetCustomerRoleIdsAsync(await workContext.GetCurrentCustomerAsync()),
            showHidden);

        var categories = await staticCacheManager
            .GetAsync(key, async () => (await GetAllCategoriesAsync(string.Empty, showHidden: showHidden)).ToList());

        return categories;
    }

    public async Task<IPagedList<BlogCategory>> GetAllCategoriesAsync(string categoryName, int pageIndex = 0, int pageSize = Int32.MaxValue,
        bool showHidden = false, bool? overridePublished = null)
    {
        var unsortedCategories = await blogCategoryRepository.GetAllAsync(async query =>
        {
            if (!showHidden)
                query = query.Where(c => c.Published);
            else if (overridePublished.HasValue)
                query = query.Where(c => c.Published == overridePublished.Value);

            if (!showHidden)
            {
                //apply ACL constraints
                var customer = await workContext.GetCurrentCustomerAsync();
                query = await aclService.ApplyAcl(query, customer);
            }

            if (!string.IsNullOrWhiteSpace(categoryName))
                query = query.Where(c => c.Name.Contains(categoryName));

            return query.Where(c => !c.Deleted);
        });

        //sort categories
        var sortedCategories = SortCategoriesForTree(unsortedCategories.ToLookup(c => c.ParentCategoryId))
            .ToList();

        //paging
        return new PagedList<BlogCategory>(sortedCategories, pageIndex, pageSize);
    }
    
    protected virtual IEnumerable<BlogCategory> SortCategoriesForTree(
        ILookup<int, BlogCategory> categoriesByParentId,
        int parentId = 0,
        bool ignoreCategoriesWithoutExistingParent = false)
    {
        ArgumentNullException.ThrowIfNull(categoriesByParentId);

        var remaining = parentId > 0
            ? new HashSet<int>(0)
            : categoriesByParentId.Select(g => g.Key).ToHashSet();
        remaining.Remove(parentId);

        foreach (var cat in categoriesByParentId[parentId].OrderBy(c => c.DisplayOrder).ThenBy(c => c.Id))
        {
            yield return cat;

            remaining.Remove(cat.Id);

            foreach (var subCategory in SortCategoriesForTree(categoriesByParentId, cat.Id, true))
            {
                yield return subCategory;
                remaining.Remove(subCategory.Id);
            }
        }

        if (ignoreCategoriesWithoutExistingParent)
            yield break;

        //find categories without parent in provided category source and return them
        var orphans = remaining
            .SelectMany(id => categoriesByParentId[id])
            .OrderBy(c => c.ParentCategoryId)
            .ThenBy(c => c.DisplayOrder)
            .ThenBy(c => c.Id);

        foreach (var orphan in orphans)
            yield return orphan;
    }

    public async Task<IList<BlogCategory>> GetAllCategoriesByParentCategoryIdAsync(int parentCategoryId, bool showHidden = false)
    {
        var customer = await workContext.GetCurrentCustomerAsync();
        var customerRoleIds = await customerService.GetCustomerRoleIdsAsync(customer);

        var categories = await blogCategoryRepository.GetAllAsync(async query =>
        {
            if (!showHidden)
            {
                query = query.Where(c => c.Published);

                //apply ACL constraints
                query = await aclService.ApplyAcl(query, customerRoleIds);
            }

            query = query.Where(c => !c.Deleted && c.ParentCategoryId == parentCategoryId);

            return query.OrderBy(c => c.DisplayOrder).ThenBy(c => c.Id);
        }, cache => cache.PrepareKeyForDefaultCache(NopBlogsDefaults.CategoriesByParentCategoryCacheKey,
            parentCategoryId, showHidden, customerRoleIds));

        return categories;
    }

    public async Task<IList<BlogCategory>> GetAllCategoriesDisplayedOnHomepageAsync(bool showHidden = false)
    {
        var categories = await blogCategoryRepository.GetAllAsync(query =>
        {
            return from c in query
                orderby c.DisplayOrder, c.Id
                where c.Published &&
                      !c.Deleted &&
                      c.ShowOnHomepage
                select c;
        }, cache => cache.PrepareKeyForDefaultCache(NopBlogsDefaults.CategoriesHomepageCacheKey));

        if (showHidden)
            return categories;

        var cacheKey = staticCacheManager.PrepareKeyForDefaultCache(NopBlogsDefaults.CategoriesHomepageWithoutHiddenCacheKey,
             await customerService.GetCustomerRoleIdsAsync(await workContext.GetCurrentCustomerAsync()));

        var result = await staticCacheManager.GetAsync(cacheKey, async () =>
        {
            return await categories
                .WhereAwait(async c => await aclService.AuthorizeAsync(c))
                .ToListAsync();
        });

        return result;
    }

    public async Task<IList<int>> GetChildCategoryIdsAsync(int parentCategoryId, bool showHidden = false)
    {
        var cacheKey = staticCacheManager.PrepareKeyForDefaultCache(NopBlogsDefaults.CategoriesChildIdsCacheKey,
            parentCategoryId,
            await customerService.GetCustomerRoleIdsAsync(await workContext.GetCurrentCustomerAsync()),
            showHidden);

        return await staticCacheManager.GetAsync(cacheKey, async () =>
        {
            //little hack for performance optimization
            //there's no need to invoke "GetAllCategoriesByParentCategoryId" multiple times (extra SQL commands) to load childs
            //so we load all categories at once (we know they are cached) and process them server-side
            var lookup = await staticCacheManager.GetAsync(
                staticCacheManager.PrepareKeyForDefaultCache(NopBlogsDefaults.ChildCategoryIdLookupCacheKey, showHidden),
                async () => (await GetAllCategoriesAsync(showHidden: showHidden))
                    .ToGroupedDictionary(c => c.ParentCategoryId, x => x.Id));

            var categoryIds = new List<int>();
            if (lookup.TryGetValue(parentCategoryId, out var categories))
            {
                categoryIds.AddRange(categories);
                var childCategoryIds = categories.SelectAwait(async cId => await GetChildCategoryIdsAsync(cId, showHidden));
                // avoid allocating a new list or blocking with ToEnumerable
                await foreach (var cIds in childCategoryIds)
                    categoryIds.AddRange(cIds);
            }

            return categoryIds;
        });
    }

    public async Task<BlogCategory> GetCategoryByIdAsync(int categoryId)
    {
        return await blogCategoryRepository.GetByIdAsync(categoryId, cache => default);
    }

    public async Task<IPagedList<BlogCategory>> GetCategoriesByAppliedDiscountAsync(int? discountId = null, bool showHidden = false, int pageIndex = 0,
        int pageSize = Int32.MaxValue)
    {
        var categories = blogCategoryRepository.Table;

        if (!showHidden)
            categories = categories.Where(category => !category.Deleted);

        categories = categories.OrderBy(category => category.DisplayOrder).ThenBy(category => category.Id);

        return await categories.ToPagedListAsync(pageIndex, pageSize);
    }

    public async Task InsertCategoryAsync(BlogCategory category)
    {
        await blogCategoryRepository.InsertAsync(category);
    }

    public async Task UpdateCategoryAsync(BlogCategory category)
    {
        ArgumentNullException.ThrowIfNull(category);

        //validate category hierarchy
        var parentCategory = await GetCategoryByIdAsync(category.ParentCategoryId);
        while (parentCategory != null)
        {
            if (category.Id == parentCategory.Id)
            {
                category.ParentCategoryId = 0;
                break;
            }

            parentCategory = await GetCategoryByIdAsync(parentCategory.ParentCategoryId);
        }

        await blogCategoryRepository.UpdateAsync(category);
    }

    public async Task DeleteCategoriesAsync(IList<BlogCategory> categories)
    {
        ArgumentNullException.ThrowIfNull(categories);

        foreach (var category in categories)
            await DeleteCategoryAsync(category);
    }

    public async Task DeleteProductCategoryAsync(BlogPostBlogCategoryMapping blogCategory)
    {
        await blogPostBlogCategoryMappingRepository.DeleteAsync(blogCategory);
    }

    public async Task DeleteProductCategoriesAsync(IList<BlogPostBlogCategoryMapping> productCategories)
    {
        await blogPostBlogCategoryMappingRepository.DeleteAsync(productCategories);
    }

    public async Task<IPagedList<BlogPostBlogCategoryMapping>> GetProductCategoriesByCategoryIdAsync(int categoryId, int pageIndex = 0, int pageSize = Int32.MaxValue,
        bool showHidden = false)
    {
        if (categoryId == 0)
            return new PagedList<BlogPostBlogCategoryMapping>(new List<BlogPostBlogCategoryMapping>(), pageIndex, pageSize);

        var query = from pc in blogPostBlogCategoryMappingRepository.Table
            join p in blogRepository.Table on pc.BlogPostId equals p.Id
            where pc.CategoryId == categoryId
            orderby pc.DisplayOrder, pc.Id
            select pc;

        if (!showHidden)
        {
            var categoriesQuery = blogCategoryRepository.Table.Where(c => c.Published);

            //apply ACL constraints
            var customer = await workContext.GetCurrentCustomerAsync();
            categoriesQuery = await aclService.ApplyAcl(categoriesQuery, customer);

            query = query.Where(pc => categoriesQuery.Any(c => c.Id == pc.CategoryId));
        }

        return await query.ToPagedListAsync(pageIndex, pageSize);
    }

    public async Task<BlogPostBlogCategoryMapping> GetProductCategoryByIdAsync(int productCategoryId)
    {
        return await blogPostBlogCategoryMappingRepository.GetByIdAsync(productCategoryId, cache => default);
    }

    public async Task InsertProductCategoryAsync(BlogPostBlogCategoryMapping productCategory)
    {
        await blogPostBlogCategoryMappingRepository.InsertAsync(productCategory);
    }

    public async Task UpdateProductCategoryAsync(BlogPostBlogCategoryMapping productCategory)
    {
        await blogPostBlogCategoryMappingRepository.UpdateAsync(productCategory);
    }

    public async Task<string[]> GetNotExistingCategoriesAsync(string[] categoryIdsNames)
    {
        ArgumentNullException.ThrowIfNull(categoryIdsNames);

        var query = blogCategoryRepository.Table.Where(c => !c.Deleted);
        var queryFilter = categoryIdsNames.Distinct().ToArray();
        //filtering by name
        var filter = await query.Select(c => c.Name)
            .Where(c => queryFilter.Contains(c))
            .ToListAsync();

        queryFilter = queryFilter.Except(filter).ToArray();

        //if some names not found
        if (!queryFilter.Any())
            return queryFilter.ToArray();

        //filtering by IDs
        filter = await query.Select(c => c.Id.ToString())
            .Where(c => queryFilter.Contains(c))
            .ToListAsync();

        return queryFilter.Except(filter).ToArray();
    }

    public async Task<IDictionary<int, int[]>> GetProductCategoryIdsAsync(int[] productIds)
    {
        var query = blogPostBlogCategoryMappingRepository.Table;

        return (await query.Where(p => productIds.Contains(p.BlogPostId))
                .Select(p => new { p.BlogPostId, p.CategoryId })
                .ToListAsync())
            .GroupBy(a => a.BlogPostId)
            .ToDictionary(items => items.Key, items => items.Select(a => a.CategoryId).ToArray());
    }

    public async Task<IList<BlogCategory>> GetCategoriesByIdsAsync(int[] categoryIds)
    {
        return await blogCategoryRepository.GetByIdsAsync(categoryIds, includeDeleted: false);
    }

    public BlogPostBlogCategoryMapping FindProductCategory(IList<BlogPostBlogCategoryMapping> source, int productId, int categoryId)
    {
        return source.FirstOrDefault(pc => pc.BlogPostId == productId && pc.CategoryId == categoryId);
    }

    public async Task<string> GetFormattedBreadCrumbAsync(BlogCategory category, IList<BlogCategory> allCategories = null, string separator = ">>",
        int languageId = 0)
    {
        var result = string.Empty;

        var breadcrumb = await GetCategoryBreadCrumbAsync(category, allCategories, true);
        for (var i = 0; i <= breadcrumb.Count - 1; i++)
        {
            var categoryName = await localizationService.GetLocalizedAsync(breadcrumb[i], x => x.Name, languageId);
            result = string.IsNullOrEmpty(result) ? categoryName : $"{result} {separator} {categoryName}";
        }

        return result;
    }

    public async Task<IList<BlogCategory>> GetCategoryBreadCrumbAsync(BlogCategory category, IList<BlogCategory> allCategories = null, bool showHidden = false)
    {
        ArgumentNullException.ThrowIfNull(category);

        var breadcrumbCacheKey = staticCacheManager.PrepareKeyForDefaultCache(NopBlogsDefaults.CategoryBreadcrumbCacheKey,
            category,
            await customerService.GetCustomerRoleIdsAsync(await workContext.GetCurrentCustomerAsync()),
            await workContext.GetWorkingLanguageAsync(),
            showHidden);

        return await staticCacheManager.GetAsync(breadcrumbCacheKey, async () =>
        {
            var result = new List<BlogCategory>();

            //used to prevent circular references
            var alreadyProcessedCategoryIds = new List<int>();

            while (category != null && //not null
                   !category.Deleted && //not deleted
                   (showHidden || category.Published) && //published
                   (showHidden || await aclService.AuthorizeAsync(category)) && //ACL
                   !alreadyProcessedCategoryIds.Contains(category.Id)) //prevent circular references
            {
                result.Add(category);

                alreadyProcessedCategoryIds.Add(category.Id);

                category = allCategories != null
                    ? allCategories.FirstOrDefault(c => c.Id == category.ParentCategoryId)
                    : await GetCategoryByIdAsync(category.ParentCategoryId);
            }

            result.Reverse();

            return result;
        });
    }
}