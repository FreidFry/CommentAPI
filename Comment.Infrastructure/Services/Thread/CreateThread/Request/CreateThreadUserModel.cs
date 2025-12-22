namespace Comment.Infrastructure.Services.Thread.CreateThread.Request
{
    public record CreateThreadUserModel
    {
        public Guid Id { get; set; }
        public string UserName { get; set; }
        public bool IsBanned { get; set; }
        public bool IsDeleted { get; set; }

        public CreateThreadUserModel(Guid Id, string UserName, bool IsBanned, bool IsDeleted)
        {
            this.Id = Id;
            this.UserName = UserName;
            this.IsBanned = IsBanned;
            this.IsDeleted = IsDeleted;
        }
        public CreateThreadUserModel()
        {
            
        }

    }
}



