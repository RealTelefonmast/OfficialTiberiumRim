namespace TeleCore.Logging;

public interface ILoggerProvider
{
    ModLogger Log { get; }
}
