using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeviceRichState
{
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
