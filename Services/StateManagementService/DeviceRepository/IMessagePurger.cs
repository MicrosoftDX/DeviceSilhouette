// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
using DeviceRichState;
using System.Collections.Generic;

namespace DeviceRepository
{
    public interface IMessagePurger
    {
        List<DeviceMessage> GetPurgableMessages(List<DeviceMessage> messages);
    }
}
