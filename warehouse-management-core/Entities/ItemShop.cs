using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace warehouse_management_core.Entities
{
    public class ItemShop : IEntity
    {
        public Id Id { get; set; }
        public int Amount { get; set; }

        public virtual Item Item { get; set; }
        public virtual Shop Shop { get; set; }
    }
}
