using System.ComponentModel.DataAnnotations;

namespace MTGCapstone.API.Data.DTOs
{
    public class CommentDTO
    {

        public int? ParentCommentId { get; set; }

        [Required, MaxLength(250)]
        public string? Message { get; set; }
    }
}
