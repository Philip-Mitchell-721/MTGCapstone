namespace MTGCapstone.API.Data.DTOs
{
    public class Card_Face
    {
        public string? name { get; set; }
        public string? _object { get; set; }
        public string? mana_cost { get; set; }
        public string? type_line { get; set; }
        public string? oracle_text { get; set; }
        public string[]? colors { get; set; }
        public string? power { get; set; }
        public string? toughness { get; set; }
        public string? flavor_text { get; set; }
        public string? artist { get; set; }
        public string? artist_id { get; set; }
        public string? illustration_id { get; set; }
        public Image_Uris? image_uris { get; set; }
        public string? flavor_name { get; set; } //This is empty in every exmple I could find?
        public string[]? color_indicator { get; set; }
    }

}
