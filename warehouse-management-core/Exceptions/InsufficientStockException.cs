using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace warehouse_management_core.Exceptions
{
    public class InsufficientStockException(Guid wrongId, int amount, int difference) : Exception($"Item {wrongId} is not enough in quantity {amount}, {difference} pieces is missing.") { }
}
