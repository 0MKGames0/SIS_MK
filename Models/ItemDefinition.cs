using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace SIS_MK.Models
{
    public class ItemDefinition
    {
        public string Id { get; set; }              // "golden_nail_puller"
        public string Name { get; set; }            // "Золотой гвоздодёр"
        public string Location { get; set; }        // "В подсобке, путь D"
        public string Scenario { get; set; }        // "Outbreak"
        public string Room { get; set; }            // "Комната #30"

        // Список id персонажей, как и было
        public List<string> AvailableFor { get; set; } = new List<string>();

        // Удобная строка для редактирования в гриде:
        // "david, shared" -> ["david","shared"]
        [JsonIgnore]
        public string AvailableForString
        {
            get
            {
                if (AvailableFor == null || AvailableFor.Count == 0)
                    return string.Empty;

                return string.Join(", ", AvailableFor);
            }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    AvailableFor = new List<string>();
                }
                else
                {
                    AvailableFor = value
                        .Split(',')
                        .Select(s => s.Trim())
                        .Where(s => s.Length > 0)
                        .ToList();
                }
            }
        }
    }
}
