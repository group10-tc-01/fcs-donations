using Fcs.Donations.WebApi.Filters;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;

namespace Fcs.Donations.IntegratedTests.WebApi;

public sealed class TrimStringsActionFilterTests
{
    [Fact]
    public void Given_WritableStringProperty_When_ActionExecutes_Then_ShouldTrimValue()
    {
        var request = new RequestModel { Description = "  donation note  " };
        var actionContext = new ActionContext(
            new DefaultHttpContext(),
            new RouteData(),
            new ActionDescriptor());
        var context = new ActionExecutingContext(
            actionContext,
            [],
            new Dictionary<string, object?> { ["request"] = request },
            new object());
        var filter = new TrimStringsActionFilter();

        filter.OnActionExecuting(context);

        request.Description.Should().Be("donation note");
    }

    private sealed class RequestModel
    {
        public string Description { get; set; } = string.Empty;
    }
}
