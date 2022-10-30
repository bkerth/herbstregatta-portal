using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Diagnostics;

namespace Herbstregatta.Manager.Pages
{
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    [IgnoreAntiforgeryToken]
    public class ErrorModel : PageModel
    {
        public string RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

        private readonly ILogger<ErrorModel> _logger;

#pragma warning disable CS8618 // Non-nullable property 'RequestId' must contain a non-null value when exiting constructor. Consider declaring the property as nullable.
        public ErrorModel(ILogger<ErrorModel> logger)
#pragma warning restore CS8618 // Non-nullable property 'RequestId' must contain a non-null value when exiting constructor. Consider declaring the property as nullable.
        {
            _logger = logger;
        }

        public void OnGet()
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        }
    }
}
