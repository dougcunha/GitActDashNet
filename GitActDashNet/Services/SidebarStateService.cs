namespace GitActDashNet.Services;

public class SidebarStateService
{
    private bool _isCollapsed = false;
    private bool _isInitialized = false;
    
    public event Action? OnStateChanged;
    
    public bool IsCollapsed => _isCollapsed;
    
    public void ToggleSidebar()
    {
        _isCollapsed = !_isCollapsed;
        OnStateChanged?.Invoke();
    }
    
    public void SetSidebarState(bool collapsed)
    {
        if (_isCollapsed != collapsed)
        {
            _isCollapsed = collapsed;
            OnStateChanged?.Invoke();
        }
    }
    
    public void InitializeFromClient(bool collapsed)
    {
        if (!_isInitialized)
        {
            _isCollapsed = collapsed;
            _isInitialized = true;
            OnStateChanged?.Invoke();
        }
    }
}