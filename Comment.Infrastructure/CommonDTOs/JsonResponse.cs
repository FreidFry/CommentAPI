namespace Comment.Infrastructure.CommonDTOs
{
    /// <summary>
    /// Raw data retrieved from Redis without any processing.
    /// </summary>
    /// <param name="json">The unprocessed JSON string from the cache.</param>
    public record JsonResponse(string json)
    {

    }
}
