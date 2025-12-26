namespace Comment.Infrastructure.CommonDTOs
{
    /// <summary>
    /// A model for binding a consumer to a handler.
    /// </summary>
    /// <param name="Message">The message content or data payload.</param>
    /// <param name="StatusCode">The operation status code.</param>
    public record StatusCodeResponse(string Message, int StatusCode)
    {
    }
}
