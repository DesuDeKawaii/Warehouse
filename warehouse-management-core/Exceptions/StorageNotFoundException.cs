using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace warehouse_management_core.Exceptions
{
    public class StorageNotFoundException(Guid wrongId) : Exception($"Storage {wrongId} is not found.") { }

}
