﻿using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Linq;
using System.Threading.Tasks;
using GovUkDesignSystem.Attributes.DataBinding;
using System;

namespace GovUkDesignSystem.ModelBinders
{
    /// <summary>
    /// This model binder can be used to replace the default MVC model binder for a required int property. It will add
    /// validation messages to the model state inline with the GovUk Design System guidelines.
    /// This binder must be used alongside a GovUkDataBindingIntErrorTextAttribute attribute.
    /// </summary>
    public class GovUkMandatoryIntBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var errorText = bindingContext.ModelMetadata.ValidatorMetadata.OfType<GovUkDataBindingIntErrorTextAttribute>().SingleOrDefault();
            if (errorText == null)
            {
                throw new Exception("When using the GovUkMandatoryIntBinder you must also provide a GovUkDataBindingIntErrorTextAttribute attribute and ensure that you register GovUkDataBindingErrorTextProvider in your application's Startup.ConfigureServices method.");
            }
            var modelName = bindingContext.ModelName;

            var valueProviderResult = bindingContext.ValueProvider.GetValue(modelName);

            // Ensure that a value was sent to us in the request
            if (valueProviderResult == ValueProviderResult.None)
            {
                bindingContext.ModelState.TryAddModelError(modelName, errorText.ErrorMessageIfMissing);
                return Task.CompletedTask;
            }

            if (valueProviderResult.Length > 1)
            {
                throw new ArgumentException(
                    $"This property should only be able to send 1 value at a time, " +
                    $"but we just received [{valueProviderResult.Length}] values [{String.Join(", ", valueProviderResult.ToArray())}] " +
                    $"for property [{modelName}] on type [{bindingContext.ModelMetadata.ContainerType.FullName}]"
                );
            }

            bindingContext.ModelState.SetModelValue(modelName, valueProviderResult);

            var value = valueProviderResult.FirstValue;

            // Ensure that the value we have isn't empty
            if (string.IsNullOrEmpty(value))
            {
                bindingContext.ModelState.TryAddModelError(modelName, errorText.ErrorMessageIfMissing);
                return Task.CompletedTask;
            }

            // Ensure that the value is a number
            if (!double.TryParse(value, out _))
            {
                bindingContext.ModelState.TryAddModelError(modelName, $"{errorText.NameAtStartOfSentence} must be a number");
                return Task.CompletedTask;
            }

            //Ensure that the value is an integer
            if (!int.TryParse(value, out var intValue))
            {
                bindingContext.ModelState.TryAddModelError(modelName, $"{errorText.NameAtStartOfSentence} must be a whole number");
                return Task.CompletedTask;
            }

            bindingContext.Result = ModelBindingResult.Success(intValue);
            return Task.CompletedTask;
        }
    }
}