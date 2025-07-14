namespace VHS.Client.Services;

public class FireAlarmStateService
{
    public bool IsAlarmActive { get; private set; }

    public event Action? OnChange;

    public void SetAlarmState(bool isAlarmActive)
    {
        IsAlarmActive = isAlarmActive;
        NotifyStateChanged();
    }

    private void NotifyStateChanged() => OnChange?.Invoke();
}
