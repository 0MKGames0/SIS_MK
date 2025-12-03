namespace SIS_MK.Models
{

    public class GameDefinition
    {
        public string Id { get; set; } = string.Empty;          // "outbreak_file1"
        public string Name { get; set; } = string.Empty;        // "Resident Evil Outbreak: File #1"
        public string ItemsFile { get; set; } = "items.json";   // файл с базой предметов для этой игры
        public string LogoPath { get; set; } = string.Empty;
    }
}
