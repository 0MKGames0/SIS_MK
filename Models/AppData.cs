using System.Collections.Generic;

namespace SIS_MK.Models
{
    public class AppData
    {
        public List<CharacterDefinition> Characters { get; set; } = new();
        public List<ItemDefinition> Items { get; set; } = new();
    }
}
