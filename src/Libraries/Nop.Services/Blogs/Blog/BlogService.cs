using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Blogs;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Localization;
using Nop.Data;
using Nop.Services.Localization;
using Nop.Services.Security;
using Nop.Services.Stores;

namespace Nop.Services.Blogs.Blog;

/// <summary>
/// Blog service
/// </summary>
public partial class BlogService(
    IRepository<BlogComment> blogCommentRepository,
    IRepository<BlogPost> blogPostRepository,
    IStaticCacheManager staticCacheManager,
    IWorkContext workContext,
    ILanguageService languageService,
    IAclService aclService,
    IRepository<LocalizedProperty> localizedPropertyRepository,
    IStoreMappingService storeMappingService)
    : IBlogService
{
    #region Fields

    protected readonly ILanguageService _languageService = languageService;
    protected readonly IRepository<LocalizedProperty> _localizedPropertyRepository = localizedPropertyRepository;
    protected readonly IAclService _aclService = aclService;
    protected readonly IRepository<BlogComment> _blogCommentRepository = blogCommentRepository;
    protected readonly IRepository<BlogPost> _blogPostRepository = blogPostRepository;
    protected readonly IStaticCacheManager _staticCacheManager = staticCacheManager;
    protected readonly IStoreMappingService _storeMappingService = storeMappingService;
    protected readonly IWorkContext _workContext = workContext;
    private static readonly char[] _separator = [','];

    #endregion

    #region Ctor

    #endregion

    #region Methods

    #region Blog posts

    /// <summary>
    /// Deletes a blog post
    /// </summary>
    /// <param name="blogPost">Blog post</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    public virtual async Task DeleteBlogPostAsync(BlogPost blogPost)
    {
        await _blogPostRepository.DeleteAsync(blogPost);
    }

    /// <summary>
    /// Gets a blog post
    /// </summary>
    /// <param name="blogPostId">Blog post identifier</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the blog post
    /// </returns>
    public virtual async Task<BlogPost> GetBlogPostByIdAsync(int blogPostId)
    {
        return await _blogPostRepository.GetByIdAsync(blogPostId, cache => default, useShortTermCache: true);
    }

    public virtual async Task<IList<BlogPost>> GetBlogsByIdsAsync(int[] blogIds)
    {
        return await _blogPostRepository.GetByIdsAsync(blogIds, cache => default, false);
    }

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
    public virtual async Task<IPagedList<BlogPost>> GetAllBlogPostsAsync(int storeId = 0, int languageId = 0,
        DateTime? dateFrom = null, DateTime? dateTo = null,
        int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false, string title = null)
    {
        return await _blogPostRepository.GetAllPagedAsync(async query =>
        {
            if (dateFrom.HasValue)
                query = query.Where(b => dateFrom.Value <= (b.StartDateUtc ?? b.CreatedOnUtc));

            if (dateTo.HasValue)
                query = query.Where(b => dateTo.Value >= (b.StartDateUtc ?? b.CreatedOnUtc));

            if (languageId > 0)
                query = query.Where(b => languageId == b.LanguageId);

            if (!string.IsNullOrEmpty(title))
                query = query.Where(b => b.Title.Contains(title));

            if (!showHidden || storeId > 0)
            {
                //apply store mapping constraints
                query = await _storeMappingService.ApplyStoreMapping(query, storeId);
            }

            if (!showHidden)
            {
                query = query.Where(b => !b.StartDateUtc.HasValue || b.StartDateUtc <= DateTime.UtcNow);
                query = query.Where(b => !b.EndDateUtc.HasValue || b.EndDateUtc >= DateTime.UtcNow);
            }

            query = query.OrderByDescending(b => b.StartDateUtc ?? b.CreatedOnUtc);

            return query;
        }, pageIndex, pageSize);
    }

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
    public virtual async Task<IPagedList<BlogPost>> GetAllBlogPostsByTagAsync(int storeId = 0,
        int languageId = 0, string tag = "",
        int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false)
    {
        tag = tag.Trim();

        //we load all records and only then filter them by tag
        var blogPostsAll = await GetAllBlogPostsAsync(storeId: storeId, languageId: languageId, showHidden: showHidden);
        var taggedBlogPosts = new List<BlogPost>();
        foreach (var blogPost in blogPostsAll)
        {
            var tags = await ParseTagsAsync(blogPost);
            if (!string.IsNullOrEmpty(tags.FirstOrDefault(t =>
                    t.Equals(tag, StringComparison.InvariantCultureIgnoreCase))))
                taggedBlogPosts.Add(blogPost);
        }

        //server-side paging
        var result = new PagedList<BlogPost>(taggedBlogPosts, pageIndex, pageSize);
        return result;
    }

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
    public virtual async Task<IList<BlogPostTag>> GetAllBlogPostTagsAsync(int storeId, int languageId,
        bool showHidden = false)
    {
        var cacheKey =
            _staticCacheManager.PrepareKeyForDefaultCache(NopBlogsDefaults.BlogTagsCacheKey, languageId, storeId,
                showHidden);

        var blogPostTags = await _staticCacheManager.GetAsync(cacheKey, async () =>
        {
            var rezBlogPostTags = new List<BlogPostTag>();

            var blogPosts = await GetAllBlogPostsAsync(storeId, languageId, showHidden: showHidden);

            foreach (var blogPost in blogPosts)
            {
                var tags = await ParseTagsAsync(blogPost);
                foreach (var tag in tags)
                {
                    var foundBlogPostTag = rezBlogPostTags.Find(bpt =>
                        bpt.Name.Equals(tag, StringComparison.InvariantCultureIgnoreCase));
                    if (foundBlogPostTag == null)
                    {
                        foundBlogPostTag = new BlogPostTag { Name = tag, BlogPostCount = 1 };
                        rezBlogPostTags.Add(foundBlogPostTag);
                    }
                    else
                        foundBlogPostTag.BlogPostCount++;
                }
            }

            return rezBlogPostTags;
        });

        return blogPostTags;
    }

    /// <summary>
    /// Inserts a blog post
    /// </summary>
    /// <param name="blogPost">Blog post</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    public virtual async Task InsertBlogPostAsync(BlogPost blogPost)
    {
        await _blogPostRepository.InsertAsync(blogPost);
    }

    /// <summary>
    /// Updates the blog post
    /// </summary>
    /// <param name="blogPost">Blog post</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    public virtual async Task UpdateBlogPostAsync(BlogPost blogPost)
    {
        await _blogPostRepository.UpdateAsync(blogPost);
    }

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
    public virtual async Task<IList<BlogPost>> GetPostsByDateAsync(IList<BlogPost> blogPosts, DateTime dateFrom,
        DateTime dateTo)
    {
        ArgumentNullException.ThrowIfNull(blogPosts);

        var rez = await blogPosts
            .Where(p => dateFrom.Date <= (p.StartDateUtc ?? p.CreatedOnUtc) &&
                        (p.StartDateUtc ?? p.CreatedOnUtc).Date <= dateTo)
            .ToListAsync();

        return rez;
    }

    /// <summary>
    /// Parse tags
    /// </summary>
    /// <param name="blogPost">Blog post</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the ags
    /// </returns>
    public virtual async Task<IList<string>> ParseTagsAsync(BlogPost blogPost)
    {
        ArgumentNullException.ThrowIfNull(blogPost);

        if (blogPost.Tags == null)
            return new List<string>();

        var tags = await blogPost.Tags.Split(_separator, StringSplitOptions.RemoveEmptyEntries)
            .Select(tag => tag.Trim())
            .Where(tag => !string.IsNullOrEmpty(tag))
            .ToListAsync();

        return tags;
    }

    /// <summary>
    /// Get a value indicating whether a blog post is available now (availability dates)
    /// </summary>
    /// <param name="blogPost">Blog post</param>
    /// <param name="dateTime">Datetime to check; pass null to use current date</param>
    /// <returns>Result</returns>
    public virtual bool BlogPostIsAvailable(BlogPost blogPost, DateTime? dateTime = null)
    {
        ArgumentNullException.ThrowIfNull(blogPost);

        if (blogPost.StartDateUtc.HasValue && blogPost.StartDateUtc.Value >= (dateTime ?? DateTime.UtcNow))
            return false;

        if (blogPost.EndDateUtc.HasValue && blogPost.EndDateUtc.Value <= (dateTime ?? DateTime.UtcNow))
            return false;

        return true;
    }

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
    public virtual async Task<IList<BlogComment>> GetAllCommentsAsync(int customerId = 0, int storeId = 0,
        int? blogPostId = null,
        bool? approved = null, DateTime? fromUtc = null, DateTime? toUtc = null, string commentText = null)
    {
        return await _blogCommentRepository.GetAllAsync(query =>
        {
            if (approved.HasValue)
                query = query.Where(comment => comment.IsApproved == approved);

            if (blogPostId > 0)
                query = query.Where(comment => comment.BlogPostId == blogPostId);

            if (customerId > 0)
                query = query.Where(comment => comment.CustomerId == customerId);

            if (storeId > 0)
                query = query.Where(comment => comment.StoreId == storeId);

            if (fromUtc.HasValue)
                query = query.Where(comment => fromUtc.Value <= comment.CreatedOnUtc);

            if (toUtc.HasValue)
                query = query.Where(comment => toUtc.Value >= comment.CreatedOnUtc);

            if (!string.IsNullOrEmpty(commentText))
                query = query.Where(c => c.CommentText.Contains(commentText));

            query = query.OrderBy(comment => comment.CreatedOnUtc);

            return query;
        });
    }

    /// <summary>
    /// Gets a blog comment
    /// </summary>
    /// <param name="blogCommentId">Blog comment identifier</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the blog comment
    /// </returns>
    public virtual async Task<BlogComment> GetBlogCommentByIdAsync(int blogCommentId)
    {
        return await _blogCommentRepository.GetByIdAsync(blogCommentId, cache => default, useShortTermCache: true);
    }

    /// <summary>
    /// Get blog comments by identifiers
    /// </summary>
    /// <param name="commentIds">Blog comment identifiers</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the blog comments
    /// </returns>
    public virtual async Task<IList<BlogComment>> GetBlogCommentsByIdsAsync(int[] commentIds)
    {
        return await _blogCommentRepository.GetByIdsAsync(commentIds);
    }

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
    public virtual async Task<int> GetBlogCommentsCountAsync(BlogPost blogPost, int storeId = 0,
        bool? isApproved = null)
    {
        var query = _blogCommentRepository.Table.Where(comment => comment.BlogPostId == blogPost.Id);

        if (storeId > 0)
            query = query.Where(comment => comment.StoreId == storeId);

        if (isApproved.HasValue)
            query = query.Where(comment => comment.IsApproved == isApproved.Value);

        var cacheKey = _staticCacheManager.PrepareKeyForDefaultCache(NopBlogsDefaults.BlogCommentsNumberCacheKey,
            blogPost, storeId, isApproved);

        return await _staticCacheManager.GetAsync(cacheKey, async () => await query.CountAsync());
    }

    /// <summary>
    /// Deletes a blog comment
    /// </summary>
    /// <param name="blogComment">Blog comment</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    public virtual async Task DeleteBlogCommentAsync(BlogComment blogComment)
    {
        await _blogCommentRepository.DeleteAsync(blogComment);
    }

    /// <summary>
    /// Deletes blog comments
    /// </summary>
    /// <param name="blogComments">Blog comments</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    public virtual async Task DeleteBlogCommentsAsync(IList<BlogComment> blogComments)
    {
        await _blogCommentRepository.DeleteAsync(blogComments);
    }

    /// <summary>
    /// Inserts a blog comment
    /// </summary>
    /// <param name="blogComment">Blog comment</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    public virtual async Task InsertBlogCommentAsync(BlogComment blogComment)
    {
        await _blogCommentRepository.InsertAsync(blogComment);
    }

    /// <summary>
    /// Update a blog comment
    /// </summary>
    /// <param name="blogComment">Blog comment</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    public virtual async Task UpdateBlogCommentAsync(BlogComment blogComment)
    {
        await _blogCommentRepository.UpdateAsync(blogComment);
    }

    public async Task<IPagedList<BlogPost>> SearchBlogsAsync(int pageIndex = 0, int pageSize = Int32.MaxValue,
        IList<int> categoryIds = null,
        IList<int> manufacturerIds = null, int vendorId = 0, int warehouseId = 0, bool visibleIndividuallyOnly = false,
        bool excludeFeaturedBlogs = false, decimal? priceMin = null, decimal? priceMax = null, int blogTagId = 0,
        string keywords = null, bool searchDescriptions = false, bool searchManufacturerPartNumber = true,
        bool searchSku = true, bool searchBlogTags = false, int languageId = 0,
        IList<SpecificationAttributeOption> filteredSpecOptions = null,
        ProductSortingEnum orderBy = ProductSortingEnum.Position, bool showHidden = false,
        bool? overridePublished = null)
    {
        //some databases don't support int.MaxValue
        if (pageSize == int.MaxValue)
            pageSize = int.MaxValue - 1;

        var productsQuery = _blogPostRepository.Table;

        var customer = await _workContext.GetCurrentCustomerAsync();

        if (!showHidden)
        {
            //apply ACL constraints
            productsQuery = await _aclService.ApplyAcl(productsQuery, customer);
        }

        var providerResults = new List<int>();

        if (!string.IsNullOrEmpty(keywords))
        {
            var langs = await _languageService.GetAllLanguagesAsync(showHidden: true);

            //Set a flag which will to points need to search in localized properties. If showHidden doesn't set to true should be at least two published languages.
            var searchLocalizedValue =
                languageId > 0 && langs.Count >= 2 && (showHidden || langs.Count(l => l.Published) >= 2);
            var productsByKeywords = new List<int>().AsQueryable();
            var runStandardSearch = showHidden;
            productsQuery =
                from p in productsQuery
                join pbk in productsByKeywords on p.Id equals pbk
                select p;
        }


        if (providerResults.Any() && orderBy == ProductSortingEnum.Position && !showHidden)
        {
            var sortedProducts = from p in productsQuery
                join pr in providerResults.Select((id, ind) => new { ind, id }) on p.Id equals pr.id into orderSeq
                from os in orderSeq.DefaultIfEmpty()
                orderby os == null ? int.MaxValue : os.ind
                select p;


            return await sortedProducts.ToPagedListAsync(pageIndex, pageSize);
        }

        return await productsQuery.ToPagedListAsync(pageIndex, pageSize);
    }

    #endregion

    #endregion
}