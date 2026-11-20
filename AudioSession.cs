namespace VoxBridge;

internal enum AudioMode
{
    Speaking,
    Listening
}

internal sealed class AudioSession
{
    public AudioMode? ActiveMode { get; private set; }
    public bool IsStopping { get; private set; }

    public bool TryStart(AudioMode mode)
    {
        if (ActiveMode is not null)
            return false;

        ActiveMode = mode;
        return true;
    }

    public bool TryStop(AudioMode mode)
    {
        if (ActiveMode != mode || IsStopping)
            return false;

        // Tamamlanma olayı gelene kadar diğer işlem de başlatılamaz.
        IsStopping = true;
        return true;
    }

    public void Complete(AudioMode mode)
    {
        if (ActiveMode != mode)
            return;

        ActiveMode = null;
        IsStopping = false;
    }
}
