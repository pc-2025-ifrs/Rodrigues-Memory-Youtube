namespace YouTube.Models
{

    public record class Comment
    {
        public required Guid Id { get; set; }

        public required string Text { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int Likes { get; set; } = 0;

        public int Dislikes { get; set; } = 0;

        public required Video Video { get; set; }

        public required Account Author { get; set; }

        public bool IsEdited { get; set; } = false;

        public void AddLike()
        {
            this.Likes++;
        }

        public void AddDislike()
        {
            this.Dislikes++;
        }

        public void Edit(string newText)
        {
            if (string.IsNullOrWhiteSpace(newText))
                throw new ArgumentException("Texto não pode ser vazio");

            this.Text = newText;
            this.IsEdited = true;
        }
    }
}