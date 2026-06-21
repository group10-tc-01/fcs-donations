using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Fcs.Donations.WebApi.Filters;

[ExcludeFromCodeCoverage]
public sealed class TrimStringsActionFilter : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        foreach (var argument in context.ActionArguments.Values)
        {
            if (argument is not null)
            {
                TrimAllStrings(argument);
            }
        }
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
    }

    private static void TrimAllStrings(object value)
    {
        var properties = value.GetType()
            .GetProperties()
            .Where(property => property.PropertyType == typeof(string) && property.CanWrite);

        foreach (var property in properties)
        {
            if (property.GetValue(value) is string text && !string.IsNullOrEmpty(text))
            {
                property.SetValue(value, text.Trim());
            }
        }
    }
}
