namespace YouTube.Models
{
    public record class Account
    {
        public Guid Id { get; set; }

        public required string Username { get; set; }

        public required string Email { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public List<Video> Videos { get; } = [];

        public List<Comment> Comments { get; } = [];

        public void PublishVideo(string title, string description = "")
        {
            var video = new Video
            {
                Id = Guid.NewGuid(),
                Title = title,
                Description = description,
                Account = this,
                PublishedAt = DateTime.UtcNow
            };
            this.Videos.Add(video);
        }
    }
}