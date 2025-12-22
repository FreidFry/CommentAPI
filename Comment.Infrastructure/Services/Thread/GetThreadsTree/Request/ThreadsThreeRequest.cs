namespace Comment.Infrastructure.Services.Thread.GetThreadsTree.Request
{
    /// <summary>
    /// Represents the parameters used to request a paginated list of threads, including an optional starting point and
    /// a maximum number of results.
    /// </summary>
    /// <param name="After">The date and time after which threads should be retrieved. If null, retrieval starts from the most recent
    /// threads.</param>
    /// <param name="Limit">The maximum number of threads to return. Must be a non-negative integer.</param>
    public record ThreadsThreeRequest(DateTime? After, int Limit);
}
