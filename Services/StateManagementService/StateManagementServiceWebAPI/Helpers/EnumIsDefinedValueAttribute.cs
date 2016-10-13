// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateManagementServiceWebAPI.Helpers
{
    /// <summary>
    /// Validate that the value of the (enum) property is a defined member of the enum type
    /// </summary>
    public class EnumIsDefinedValueAttribute : ValidationAttribute
    {

        /// <summary>
        /// Perform the validation
        /// </summary>
        /// <param name="value"></param>
        /// <param name="validationContext"></param>
        /// <returns></returns>
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null)
            {
                // don't enforce value being provided... RequiredAttribute does that!
                return ValidationResult.Success;
            }
            Type valueType = value.GetType();
            if (!valueType.IsEnum)
            {
                throw new InvalidOperationException($"EnumIsDefinedValueAttribute can only be applied to properties that are enum typed ({validationContext.DisplayName})");
            }
            if(valueType.IsEnumDefined(value))
            {
                return ValidationResult.Success;
            }
            else
            {
                return new ValidationResult($"Value '{value}' is not a valid member for enum '{valueType.Name}' on {validationContext.DisplayName}");
            }
        }
    }
}

