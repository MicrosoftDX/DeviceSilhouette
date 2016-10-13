// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonUtils;

namespace Silhouette.ServiceFabricUtilities
{
    public static class ServiceFabricConfiguration
    {
        public static ServiceFabricConfigurationSection GetConfigurationSection<TContext>(this TContext context, string name)
            where TContext : ServiceContext
        {
            // TODO - error handling...
            var configurationPackage = context.CodePackageActivationContext.GetConfigurationPackageObject("Config");
            var section = configurationPackage.Settings.Sections[name];
            return new ServiceFabricConfigurationSection(section);

           
          




        }
    }
}

