namespace Comment.Core.Interfaces
{
    public interface IApiOptions
    {
        string DbConnection { get; }
        string S2Connection { get; }
    }
}