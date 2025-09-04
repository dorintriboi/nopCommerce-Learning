using Nop.Core.Caching;

namespace Nop.Services.Blogs;

/// <summary>
/// Represents default values related to blogs services
/// </summary>
public static partial class NopBlogsDefaults
{
    #region Caching defaults

    /// <summary>
    /// Key for number of blog comments
    /// </summary>
    /// <remarks>
    /// {0} : blog post ID
    /// {1} : store ID
    /// {2} : are only approved comments?
    /// </remarks>
    public static CacheKey BlogCommentsNumberCacheKey => new("Nop.blogcomment.number.{0}-{1}-{2}");

    /// <summary>
    /// Gets a key pattern to clear cache
    /// </summary>
    /// <remarks>
    /// {0} : blog post ID
    /// </remarks>
    public static string BlogCommentsNumberPrefix => "Nop.blogcomment.number.{0}";

    /// <summary>
    /// Key for blog tag list model
    /// </summary>
    /// <remarks>
    /// {0} : language ID
    /// {1} : current store ID
    /// {2} : show hidden?
    /// </remarks>
    public static CacheKey BlogTagsCacheKey => new("Nop.blogpost.tags.{0}-{1}-{2}");

    /// <summary>
    /// Gets a key pattern to clear cache
    /// </summary>
    public static string BlogTagsPrefix => "Nop.blogpost.tags.";

    #endregion

    #region Category defaults

    public static CacheKey CategoriesAllCacheKey => new("Nop.blogcategory.all.{0}-{1}");
    public static CacheKey CategoriesByParentCategoryCacheKey => new("Nop.blogcategory.byparent.{0}-{1}-{2}");
    public static CacheKey CategoriesHomepageWithoutHiddenCacheKey => new("Nop.blogcategory.homepage.withouthidden-{0}-{1}");
    public static CacheKey CategoriesHomepageCacheKey => new("Nop.blogcategory.homepage.");
    public static CacheKey CategoriesChildIdsCacheKey => new("Nop.blogcategory.childids.{0}-{1}-{2}");
    public static CacheKey ChildCategoryIdLookupCacheKey => new("Nop.childblogcategoryidlookup.bystore.{0}-{1}");
    public static CacheKey CategoryBreadcrumbCacheKey => new("Nop.category.breadcrumb.{0}-{1}-{2}-{3}-{4}");

    #endregion
    
    
    #region Blog Category defaults
    public static CacheKey BlogCategoryBreadcrumbCacheKey => new("Nop.category.breadcrumb.{0}-{1}-{2}-{3}");

    #endregion
}