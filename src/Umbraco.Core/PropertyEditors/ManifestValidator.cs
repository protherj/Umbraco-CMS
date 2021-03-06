﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Umbraco.Core.Serialization;

namespace Umbraco.Core.PropertyEditors
{
    /// <summary>
    /// Represents a validator found in a package manifest
    /// </summary>
    internal class ManifestValidator : ValidatorBase
    {
        /// <summary>
        /// The validator type name
        /// </summary>
        [JsonProperty("type", Required = Required.Always)]
        public string Type { get; set; }

        /// <summary>
        /// The configuration defined for this validator in the manifest
        /// </summary>
        /// <remarks>
        /// This is NOT the pre-value for this data type
        /// </remarks>
        [JsonProperty("config")]
        [JsonConverter(typeof(JsonToStringConverter))]
        public string Config { get; set; }

        private ValueValidator _validatorInstance;

        /// <summary>
        /// Gets the ValueValidator instance
        /// </summary>
        internal ValueValidator ValidatorInstance
        {
            get
            {
                if (_validatorInstance == null)
                {
                    var val = ValidatorsResolver.Current.GetValidator(Type);
                    if (val == null)
                    {
                        throw new InvalidOperationException("No " + typeof(ValueValidator) +  " could be found for the type name of " + Type);
                    }
                    _validatorInstance = val;
                }
                return _validatorInstance;
            }
        }

        /// <summary>
        /// Validates the object with the resolved ValueValidator found for this type
        /// </summary>
        /// <param name="value"></param>
        /// <param name="preValues">The current pre-values stored for the data type</param>
        /// <param name="editor">The property editor instance that we are validating for</param>
        /// <returns></returns>
        public override IEnumerable<ValidationResult> Validate(string value, string preValues, PropertyEditor editor)
        {
            return ValidatorInstance.Validate(value, Config, preValues, editor);
        }
    }
}