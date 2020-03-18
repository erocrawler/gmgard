using HybridModelBinding;
using HybridModelBinding.ModelBinding;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static HybridModelBinding.Source;

namespace GmGard.Filters
{
    public class DefaultHybridModelBinderProvider : HybridModelBinderProvider
    {
        public DefaultHybridModelBinderProvider(
            IList<IInputFormatter> formatters,
            IHttpRequestStreamReaderFactory readerFactory)
            : base(
                 new HybridBindingSource(),
                 new DefaultHybridModelBinder(formatters, readerFactory))
        { }
    }

    public class DefaultHybridModelBinder : HybridModelBinder
    {
        public DefaultHybridModelBinder(
            IList<IInputFormatter> formatters,
            IHttpRequestStreamReaderFactory readerFactory)
            : base(Strategy.FirstInWins)
        {
            base
                .AddModelBinder(Body, new BodyModelBinder(formatters, readerFactory))
                .AddValueProviderFactory(Form, new FormValueProviderFactory())
                .AddValueProviderFactory(Route, new RouteValueProviderFactory())
                .AddValueProviderFactory(QueryString, new QueryStringValueProviderFactory())
                .AddValueProviderFactory(Header, new HeaderValueProviderFactory());
        }
    }
}
