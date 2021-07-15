using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ClickersAPI.Helpers
{
    public class TypeBinder<T> : IModelBinder
    {


        public Task BindModelAsync(ModelBindingContext _BindingContext)
        {
            var propertyName = _BindingContext.ModelName;
            var valueProviderResult = _BindingContext.ValueProvider.GetValue(propertyName);

            if (valueProviderResult == ValueProviderResult.None) {
                return Task.CompletedTask;
            }

            try {
                var deserializedValue = JsonConvert.DeserializeObject<List<T>>(valueProviderResult.FirstValue);
                _BindingContext.Result = ModelBindingResult.Success(deserializedValue);
            }
            catch {
                _BindingContext.ModelState.TryAddModelError(propertyName, "Value is invalid for type List<int>");
            }

            return Task.CompletedTask;
        }
    }
}
