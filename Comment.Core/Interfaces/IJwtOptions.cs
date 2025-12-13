namespace Comment.Core.Interfaces
{
    public interface IJwtOptions
    {
        string Audience { get; }
        double ExpiresDays { get; }
        string Issuer { get; }
        string SecretKey { get; }
    }
}