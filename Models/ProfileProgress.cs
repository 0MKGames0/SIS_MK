using System.Collections.Generic;

namespace SIS_MK.Models
{
    public class ProfileProgress
    {
        public string Name { get; set; } = "Default";
        public List<ItemProgress> Items { get; set; } = new();
    }
}
