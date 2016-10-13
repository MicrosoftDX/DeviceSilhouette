// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateProcessorService
{
  
    public class Latitude 
    {     
        public string Xaxis { get; set; }    
        public string Yaxis { get; set; }
        public string Zaxis { get; set; }

        public Latitude(string x, string y, string z)
        {
            this.Xaxis = x;
            this.Yaxis = y;
            this.Zaxis = z;
        }
    }
}

