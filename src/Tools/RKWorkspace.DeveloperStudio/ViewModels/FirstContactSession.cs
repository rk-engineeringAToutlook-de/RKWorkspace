namespace RKWorkspace.DeveloperStudio.ViewModels;

internal sealed class FirstContactSession
{
    private const string LeftLocation = "Left";
    private const string RightLocation = "Right";
    private DateTime _startedAtUtc;
    private DateTime? _firstGripAtUtc;
    private DateTime? _completedAtUtc;
    private bool _isGrabbed;
    private bool _isOverTarget;
    private bool _isCompleted;
    private string _objectLocation = LeftLocation;
    private int _failedAttempts;
    private int _cancellations;
    private int _unnecessaryClicks;

    public FirstContactSession()
    {
        Reset();
    }

    public event EventHandler? Changed;

    public void Reset()
    {
        _startedAtUtc = DateTime.UtcNow;
        _firstGripAtUtc = null;
        _completedAtUtc = null;
        _isGrabbed = false;
        _isOverTarget = false;
        _isCompleted = false;
        _objectLocation = LeftLocation;
        _failedAttempts = 0;
        _cancellations = 0;
        _unnecessaryClicks = 0;
        NotifyChanged();
    }

    public FirstContactSnapshot GetSnapshot()
    {
        return new FirstContactSnapshot
        {
            StartedAtUtc = _startedAtUtc,
            ObservedAtUtc = DateTime.UtcNow,
            FirstGripAtUtc = _firstGripAtUtc,
            CompletedAtUtc = _completedAtUtc,
            IsGrabbed = _isGrabbed,
            IsOverTarget = _isOverTarget,
            IsCompleted = _isCompleted,
            ObjectLocation = _objectLocation,
            FailedAttempts = _failedAttempts,
            Cancellations = _cancellations,
            UnnecessaryClicks = _unnecessaryClicks
        };
    }

    public bool RecordGrip()
    {
        if (_isCompleted)
        {
            return false;
        }

        _firstGripAtUtc ??= DateTime.UtcNow;
        _isGrabbed = true;
        _isOverTarget = false;
        NotifyChanged();
        return true;
    }

    public void RecordHoverTarget(bool overTarget)
    {
        if (!_isGrabbed || _isOverTarget == overTarget)
        {
            return;
        }

        _isOverTarget = overTarget;
        NotifyChanged();
    }

    public bool RecordPlace(bool overTarget)
    {
        if (!_isGrabbed)
        {
            return false;
        }

        _isGrabbed = false;
        _isOverTarget = false;
        if (overTarget)
        {
            _isCompleted = true;
            _objectLocation = RightLocation;
            _completedAtUtc ??= DateTime.UtcNow;
            NotifyChanged();
            return true;
        }

        _failedAttempts++;
        _cancellations++;
        _objectLocation = LeftLocation;
        NotifyChanged();
        return false;
    }

    public void RecordUnnecessaryClick()
    {
        if (_isCompleted)
        {
            return;
        }

        _unnecessaryClicks++;
        NotifyChanged();
    }

    public void RecordCancellation()
    {
        if (!_isGrabbed)
        {
            return;
        }

        _isGrabbed = false;
        _isOverTarget = false;
        _cancellations++;
        NotifyChanged();
    }

    private void NotifyChanged()
    {
        Changed?.Invoke(this, EventArgs.Empty);
    }
}
