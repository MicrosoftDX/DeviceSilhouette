using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
