namespace Nop.Core.Domain.Blogs;

public class BlogPostBlogCategoryMapping: BaseEntity
{
    /// <summary>
    /// Gets or sets the blog post identifier
    /// </summary>
    public int BlogPostId { get; set; }

    /// <summary>
    /// Gets or sets the category identifier
    /// </summary>
    public int CategoryId { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the product is featured
    /// </summary>
    public bool IsFeaturedBlog { get; set; }

    /// <summary>
    /// Gets or sets the display order
    /// </summary>
    public int DisplayOrder { get; set; }
}