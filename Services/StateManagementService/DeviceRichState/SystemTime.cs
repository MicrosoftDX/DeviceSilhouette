using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeviceRichState
{
    // I've put this here as the assembly is already referenced in a number of places
    // But if we end up referencing this assembly just for this type then we should move it!
    public static class SystemTime 
    {
        static SystemTime()
        {
            Reset();
        }
        public static void Reset()
        {
            UtcNow = () => DateTime.UtcNow;
        }
        public static Func<DateTime> UtcNow;
    }
}
