namespace MTGCapstone.API.Data.Models
{
    public class Comment
    {
        public int Id { get; set; }
        public int? ParentCommentId { get; set; }
        public int UserId { get; set; }
        public int? DeckId { get; set; }

        public DateTime PostedAt { get; set; }
        public string? Message { get; set; }

        public Comment? ParentComment { get; set; }
        public User? User { get; set; }
        public Deck? Deck { get; set; }
        public List<Comment> Replies { get; set; } = new List<Comment>();
    }
}
