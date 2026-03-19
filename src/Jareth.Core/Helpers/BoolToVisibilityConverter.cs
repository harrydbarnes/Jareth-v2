namespace Jareth.Core.Helpers;

/// <summary>
/// Helper class for boolean to visibility conversion logic.
/// The actual converter for WinUI 3 will be in the UI project.
/// </summary>
public static class VisibilityHelper
{
    public static bool IsVisible(bool value, bool invert = false)
    {
        return invert ? !value : value;
    }
}
