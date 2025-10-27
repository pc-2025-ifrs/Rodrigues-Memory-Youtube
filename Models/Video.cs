namespace YouTube.Models
{

    public record class Video
    {
        public required Guid Id { get; set; }

        public required string Title { get; set; }

        public string? Description { get; set; }

        public int DurationInSeconds { get; set; }

        public int Views { get; set; } = 0;

        public DateTime PublishedAt { get; set; } = DateTime.UtcNow;

        public required Account Account { get; set; }

        public List<Comment> Comments { get; } = [];

        public void AddComment(Account author, string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                throw new ArgumentException("Comentário não pode ser vazio");

            var comment = new Comment
            {
                Id = Guid.NewGuid(),
                Text = text,
                Video = this,
                Author = author,
                CreatedAt = DateTime.UtcNow
            };

            this.Comments.Add(comment);
            author.Comments.Add(comment);
        }

        public void IncrementView()
        {
            this.Views++;
        }
    }
}