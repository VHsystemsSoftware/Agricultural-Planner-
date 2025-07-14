using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;

namespace VHS.Client.Services
{
    public class PageTitleService : IDisposable
    {
        public event Action? OnChange;
        private string _title = "Home";
		private string _backUrl = "#";
		private string _backText = "";
		private readonly NavigationManager _navManager;

        public PageTitleService(NavigationManager navManager)
        {
            _navManager = navManager;
            _navManager.LocationChanged += OnLocationChanged;
        }

        public string Title
        {
            get => _title;
            set
            {
                if (_title != value)
                {
                    _title = value;
                    OnChange?.Invoke();
                }
            }
        }

		public string BackUrl
		{
			get => _backUrl;
			set
			{
				if (_backUrl != value)
				{
					_backUrl = value;
					OnChange?.Invoke();
				}
			}
		}

		public string BackText
		{
			get => _backText;
			set
			{
				if (_backText != value)
				{
					_backText = value;
					OnChange?.Invoke();
				}
			}
		}

		private void OnLocationChanged(object? sender, LocationChangedEventArgs e)
        { }

        public void Dispose()
        {
            _navManager.LocationChanged -= OnLocationChanged;
        }
    }
}
