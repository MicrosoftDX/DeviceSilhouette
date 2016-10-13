// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PersistencyProviders
{
    public interface IHistoryStorage
    {
        Task StoreStateMessage(DeviceState stateMessage);

        Task StoreStateMessages(DeviceState[] stateMessages);
    }
}

