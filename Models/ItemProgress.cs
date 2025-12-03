namespace SIS_MK.Models
{
    public class ItemProgress
    {
        public string GameId { get; set; } = string.Empty;

        public string CharacterId { get; set; }
        public string ItemId { get; set; }
        public bool IsCollected { get; set; }
        public string Note { get; set; }
    }
}
