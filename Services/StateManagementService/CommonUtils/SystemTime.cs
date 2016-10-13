// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
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

