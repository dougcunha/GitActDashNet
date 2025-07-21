namespace GitActDashNet.Services;

public sealed class SidebarStateService
{
    private bool _isInitialized;

    public event Action? OnStateChanged;

    public bool IsCollapsed { get; private set; }

    public void ToggleSidebar()
    {
        IsCollapsed = !IsCollapsed;
        OnStateChanged?.Invoke();
    }

    public void SetSidebarState(bool collapsed)
    {
        if (IsCollapsed == collapsed)
            return;

        IsCollapsed = collapsed;
        OnStateChanged?.Invoke();
    }

    public void InitializeFromClient(bool collapsed)
    {
        if (_isInitialized)
            return;

        IsCollapsed = collapsed;
        _isInitialized = true;
        OnStateChanged?.Invoke();
    }
}