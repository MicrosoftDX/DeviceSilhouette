﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Fabric.Description;

namespace Silhouette.ServiceFabricUtilities
{
    public class ServiceFabricConfigurationSection
    {
        private ConfigurationSection _section;

        public ServiceFabricConfigurationSection(ConfigurationSection section)
        {
            _section = section;
        }

        public string this[string key]
        {
            get { return GetValueAndDecryptIfRequired(key); }
        }

        private string GetValueAndDecryptIfRequired(string key)
        {
            // TODO error handling
            var parameter = _section.Parameters[key];
            if (parameter.IsEncrypted)
            {
                var secureValue = parameter.DecryptValue();
                return secureValue.ToUnsecureString();
            }
            else
            {
                return parameter.Value;
            }
        }
    }
}
