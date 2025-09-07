using Nop.Core;
using Nop.Core.Domain.Blogs;
using Nop.Core.Domain.Catalog;

namespace Nop.Services.Blogs.Blog;

/// <summary>
/// Blog service interface
/// </summary>
public partial interface IBlogService
{
    #region Blog posts

    /// <summary>
    /// Deletes a blog post
    /// </summary>
    /// <param name="blogPost">Blog post</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    Task DeleteBlogPostAsync(BlogPost blogPost);

    /// <summary>
    /// Gets a blog post
    /// </summary>
    /// <param name="blogPostId">Blog post identifier</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the blog post
    /// </returns>
    Task<BlogPost> GetBlogPostByIdAsync(int blogPostId);
    
    /// <summary>
    /// Gets blogs by identifier
    /// </summary>
    /// <param name="blogIds">Blog identifiers</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the blogs
    /// </returns>
    Task<IList<BlogPost>> GetBlogsByIdsAsync(int[] blogIds);

    /// <summary>
    /// Gets all blog posts
    /// </summary>
    /// <param name="storeId">The store identifier; pass 0 to load all records</param>
    /// <param name="languageId">Language identifier; 0 if you want to get all records</param>
    /// <param name="dateFrom">Filter by created date; null if you want to get all records</param>
    /// <param name="dateTo">Filter by created date; null if you want to get all records</param>
    /// <param name="pageIndex">Page index</param>
    /// <param name="pageSize">Page size</param>
    /// <param name="showHidden">A value indicating whether to show hidden records</param>
    /// <param name="title">Filter by blog post title</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the blog posts
    /// </returns>
    Task<IPagedList<BlogPost>> GetAllBlogPostsAsync(int storeId = 0, int languageId = 0,
        DateTime? dateFrom = null, DateTime? dateTo = null,
        int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false, string title = null);

    /// <summary>
    /// Gets all blog posts
    /// </summary>
    /// <param name="storeId">The store identifier; pass 0 to load all records</param>
    /// <param name="languageId">Language identifier. 0 if you want to get all blog posts</param>
    /// <param name="tag">Tag</param>
    /// <param name="pageIndex">Page index</param>
    /// <param name="pageSize">Page size</param>
    /// <param name="showHidden">A value indicating whether to show hidden records</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the blog posts
    /// </returns>
    Task<IPagedList<BlogPost>> GetAllBlogPostsByTagAsync(int storeId = 0,
        int languageId = 0, string tag = "",
        int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false);

    /// <summary>
    /// Gets all blog post tags
    /// </summary>
    /// <param name="storeId">The store identifier; pass 0 to load all records</param>
    /// <param name="languageId">Language identifier. 0 if you want to get all blog posts</param>
    /// <param name="showHidden">A value indicating whether to show hidden records</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the blog post tags
    /// </returns>
    Task<IList<BlogPostTag>> GetAllBlogPostTagsAsync(int storeId, int languageId, bool showHidden = false);

    /// <summary>
    /// Inserts a blog post
    /// </summary>
    /// <param name="blogPost">Blog post</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    Task InsertBlogPostAsync(BlogPost blogPost);

    /// <summary>
    /// Updates the blog post
    /// </summary>
    /// <param name="blogPost">Blog post</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    Task UpdateBlogPostAsync(BlogPost blogPost);

    /// <summary>
    /// Returns all posts published between the two dates.
    /// </summary>
    /// <param name="blogPosts">Source</param>
    /// <param name="dateFrom">Date from</param>
    /// <param name="dateTo">Date to</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the filtered posts
    /// </returns>
    Task<IList<BlogPost>> GetPostsByDateAsync(IList<BlogPost> blogPosts, DateTime dateFrom, DateTime dateTo);

    /// <summary>
    /// Parse tags
    /// </summary>
    /// <param name="blogPost">Blog post</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the ags
    /// </returns>
    Task<IList<string>> ParseTagsAsync(BlogPost blogPost);

    /// <summary>
    /// Get a value indicating whether a blog post is available now (availability dates)
    /// </summary>
    /// <param name="blogPost">Blog post</param>
    /// <param name="dateTime">Datetime to check; pass null to use current date</param>
    /// <returns>Result</returns>
    bool BlogPostIsAvailable(BlogPost blogPost, DateTime? dateTime = null);

    #endregion

    #region Blog comments

    /// <summary>
    /// Gets all comments
    /// </summary>
    /// <param name="customerId">Customer identifier; 0 to load all records</param>
    /// <param name="storeId">Store identifier; pass 0 to load all records</param>
    /// <param name="blogPostId">Blog post ID; 0 or null to load all records</param>
    /// <param name="approved">A value indicating whether to content is approved; null to load all records</param> 
    /// <param name="fromUtc">Item creation from; null to load all records</param>
    /// <param name="toUtc">Item creation to; null to load all records</param>
    /// <param name="commentText">Search comment text; null to load all records</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the comments
    /// </returns>
    Task<IList<BlogComment>> GetAllCommentsAsync(int customerId = 0, int storeId = 0, int? blogPostId = null,
        bool? approved = null, DateTime? fromUtc = null, DateTime? toUtc = null, string commentText = null);

    /// <summary>
    /// Gets a blog comment
    /// </summary>
    /// <param name="blogCommentId">Blog comment identifier</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the blog comment
    /// </returns>
    Task<BlogComment> GetBlogCommentByIdAsync(int blogCommentId);

    /// <summary>
    /// Get blog comments by identifiers
    /// </summary>
    /// <param name="commentIds">Blog comment identifiers</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the blog comments
    /// </returns>
    Task<IList<BlogComment>> GetBlogCommentsByIdsAsync(int[] commentIds);

    /// <summary>
    /// Get the count of blog comments
    /// </summary>
    /// <param name="blogPost">Blog post</param>
    /// <param name="storeId">Store identifier; pass 0 to load all records</param>
    /// <param name="isApproved">A value indicating whether to count only approved or not approved comments; pass null to get number of all comments</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the number of blog comments
    /// </returns>
    Task<int> GetBlogCommentsCountAsync(BlogPost blogPost, int storeId = 0, bool? isApproved = null);

    /// <summary>
    /// Deletes a blog comment
    /// </summary>
    /// <param name="blogComment">Blog comment</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    Task DeleteBlogCommentAsync(BlogComment blogComment);

    /// <summary>
    /// Deletes blog comments
    /// </summary>
    /// <param name="blogComments">Blog comments</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    Task DeleteBlogCommentsAsync(IList<BlogComment> blogComments);

    /// <summary>
    /// Inserts a blog comment
    /// </summary>
    /// <param name="blogComment">Blog comment</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    Task InsertBlogCommentAsync(BlogComment blogComment);

    /// <summary>
    /// Update a blog comment
    /// </summary>
    /// <param name="blogComment">Blog comment</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    Task UpdateBlogCommentAsync(BlogComment blogComment);
    
     /// <summary>
    /// Search blogs
    /// </summary>
    /// <param name="pageIndex">Page index</param>
    /// <param name="pageSize">Page size</param>
    /// <param name="categoryIds">Category identifiers</param>
    /// <param name="manufacturerIds">Manufacturer identifiers</param>
    /// <param name="vendorId">Vendor identifier; 0 to load all records</param>
    /// <param name="warehouseId">Warehouse identifier; 0 to load all records</param>
    /// <param name="blogType">Blog type; 0 to load all records</param>
    /// <param name="visibleIndividuallyOnly">A values indicating whether to load only blogs marked as "visible individually"; "false" to load all records; "true" to load "visible individually" only</param>
    /// <param name="excludeFeaturedBlogs">A value indicating whether loaded blogs are marked as featured (relates only to categories and manufacturers); "false" (by default) to load all records; "true" to exclude featured blogs from results</param>
    /// <param name="priceMin">Minimum price; null to load all records</param>
    /// <param name="priceMax">Maximum price; null to load all records</param>
    /// <param name="blogTagId">Blog tag identifier; 0 to load all records</param>
    /// <param name="keywords">Keywords</param>
    /// <param name="searchDescriptions">A value indicating whether to search by a specified "keyword" in product descriptions</param>
    /// <param name="searchManufacturerPartNumber">A value indicating whether to search by a specified "keyword" in manufacturer part number</param>
    /// <param name="searchSku">A value indicating whether to search by a specified "keyword" in blog SKU</param>
    /// <param name="searchBlogTags">A value indicating whether to search by a specified "keyword" in blog tags</param>
    /// <param name="languageId">Language identifier (search for text searching)</param>
    /// <param name="filteredSpecOptions">Specification options list to filter blogs; null to load all records</param>
    /// <param name="orderBy">Order by</param>
    /// <param name="showHidden">A value indicating whether to show hidden records</param>
    /// <param name="overridePublished">
    /// null - process "Published" property according to "showHidden" parameter
    /// true - load only "Published" blogs
    /// false - load only "Unpublished" blogs
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the blogs
    /// </returns>
    Task<IPagedList<BlogPost>> SearchBlogsAsync(
        int pageIndex = 0,
        int pageSize = int.MaxValue,
        IList<int> categoryIds = null,
        IList<int> manufacturerIds = null,
        int vendorId = 0,
        int warehouseId = 0,
        bool visibleIndividuallyOnly = false,
        bool excludeFeaturedBlogs = false,
        decimal? priceMin = null,
        decimal? priceMax = null,
        int blogTagId = 0,
        string keywords = null,
        bool searchDescriptions = false,
        bool searchManufacturerPartNumber = true,
        bool searchSku = true,
        bool searchBlogTags = false,
        int languageId = 0,
        IList<SpecificationAttributeOption> filteredSpecOptions = null,
        ProductSortingEnum orderBy = ProductSortingEnum.Position,
        bool showHidden = false,
        bool? overridePublished = null);

    #endregion
}