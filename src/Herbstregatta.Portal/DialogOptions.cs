using MudBlazor;

namespace Herbstregatta.Portal
{
    public static class GlobalDialogOptions
    {
        public static DialogOptions FullWidthDialogOptions()
        {
            return new DialogOptions
            {
                MaxWidth = MaxWidth.Medium,
                FullWidth = true
            };
        }
    }
}
