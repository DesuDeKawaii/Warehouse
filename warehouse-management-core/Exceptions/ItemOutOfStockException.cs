using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace warehouse_management_core.Exceptions
{
    public class ItemOutOfStockException(Guid wrongId, int amount) : Exception($"Item {wrongId} can't be issued in such quantities {amount}.") { }
}
