// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace Silhouette.ServiceFabricUtilities
{
    /// <summary>
    /// Extensions methods for SecureString.
    /// </summary>
    public static class SecureStringExtensions
    {
        /// <summary>
        /// Gets a plaintext string value from a SecureString to use in APIs that don't accept SecureString parameters.
        /// </summary>
        /// <param name="secureString"></param>
        /// <returns></returns>
        public static string ToUnsecureString(this SecureString secureString)
        {
            if (secureString == null)
            {
                throw new ArgumentNullException();
            }

            IntPtr unmanagedString = IntPtr.Zero;

            try
            {
                unmanagedString = Marshal.SecureStringToGlobalAllocUnicode(secureString);
                return Marshal.PtrToStringUni(unmanagedString);
            }
            finally
            {
                Marshal.ZeroFreeGlobalAllocUnicode(unmanagedString);
            }
        }
    }
}

