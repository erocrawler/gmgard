using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GmGard.Models.Binding
{
    public class StringSplitIntListModelBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var result = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
            List<int> value = new List<int>();
            if (result != ValueProviderResult.None)
            {
                string attemptedValue = result.Values;
                value = BlogHelper.ParseIntListFromString(attemptedValue, new List<int>());
            }
            bindingContext.Result = ModelBindingResult.Success(value);
            return Task.FromResult(0);
        }
        
    }
}
