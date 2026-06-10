using System;

namespace BackupCR.Services
{
    public class NavigationService
    {
        public event Action<string>? OnNavigateRequested;

        public void RequestNavigation(string page)
        {
            OnNavigateRequested?.Invoke(page);
        }
    }
}
