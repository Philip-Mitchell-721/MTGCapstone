namespace MTGCapstone.API.Data.Models
{
    public class ImageUris // When a card has CardFaces, ImageUris will be null on the Card, but will be present in the CardFace
    {
        public int Id { get; set; }
        public int? CardId { get; set; }
        public int? CardFaceId { get; set; }
        public string? Small { get; set; }
        public string? Normal { get; set; }
        public string? Large { get; set; }
        public string? Png { get; set; }
        public string? ArtCrop { get; set; }
        public string? BorderCrop { get; set; }
    }

}
