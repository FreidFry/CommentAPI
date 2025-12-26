namespace Comment.Infrastructure.Enums
{
    /// <summary>
    /// Specifies the fields by which data from the database or Redis can be sorted.
    /// </summary>
    public enum SortByEnum
    {

        /// <summary> Sort by the user's email address. </summary>
        Email,

        /// <summary> Sort by the user's name. </summary>
        UserName,

        /// <summary> Sort by the creation timestamp. </summary>
        CreateAt
    }
}
