using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static OpenWorldEventsConfigData;

namespace OpenWorldEvents.Models
{
    public class CumulativeLootTable
    {
        public List<LootItem> Items = new();
        private System.Random Random = new();
        private int TotalWeight = 0;

        public CumulativeLootTable(List<LootItem> items)
        {
            Items = items;
            TotalWeight = Items.Sum(item => item.Weight);
        }

        public LootItem GetRandomItem()
        {
            int choice = Random.Next(TotalWeight);
            int sum = 0;

            foreach (var item in Items)
            {
                sum += item.Weight;
                if (choice < sum)
                {
                    return item;
                }
            }

            return null; // Should not happen
        }
    }
}
