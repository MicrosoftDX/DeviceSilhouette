// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DeviceRichState;

namespace PersistencyProviders
{
    public interface IHistoryStorage
    {
        Task StoreStateMessageAsync(DeviceMessage stateMessage);

        Task StoreStateMessagesAsync(List<DeviceMessage> stateMessages);
    }

}

