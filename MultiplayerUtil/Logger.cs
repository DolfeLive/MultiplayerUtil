
namespace MultiplayerUtil;

public static class Logger
{
    public static void Log(string message) => Log(message, EType.None);
    public static void LogWarning(string message) => Log(message, EType.None, ELogType.Warning);
    public static void LogError(string message) => Log(message, EType.None, ELogType.Error);

    public static void Log(string message, bool Client) => Log(message, Client ? EType.Client : EType.Server);
    public static void LogWarning(string message, bool Client) => Log(message, Client ? EType.Client : EType.Server, ELogType.Warning);
    public static void LogError(string message, bool Client) => Log(message, Client ? EType.Client : EType.Server, ELogType.Error);

    public static void StackTraceLog(object msg, int limit = 0)
    public static void UselessLog(string message)
    {
#if DEBUG
        Log(message, EType.None, ELogType.Normal);
#endif
    }

    /// <summary>
    /// A Log that also shows how we got to this error
    /// </summary>
    /// <param name="msg">the error to log</param>
    /// <param name="offset">the offset for getting thr stacktrace</param>
    public static void StackTraceLog(object msg, int offset = 0)
    {
#if DEBUG
        string callingMethod = "";
        StackTrace stackTrace = new StackTrace();
        for (int i = 1 + offset; i < stackTrace.FrameCount; i++)
        {
            var frame = stackTrace.GetFrame(i);
            var method = frame?.GetMethod();
            if (method != null && method.DeclaringType != null)
            {
                var declaringType = method.DeclaringType;

                // Ignore Unity's internal methods related to coroutines and execution contexts
                if (declaringType.Namespace != null &&
                    (declaringType.Namespace.StartsWith("UnityEngine") || // Unity Engine's internal namespace
                     declaringType.Namespace.StartsWith("System.Threading") || // For ExecutionContext
                     declaringType.FullName.Contains("MoveNextRunner") || // Internal Unity state machine methods
                     declaringType.FullName.Contains("SetupCoroutine"))) // Internal coroutine setup
                {
                    continue;
                }

                // Add the method name to the stack log
                stackLog.Add($"{declaringType.Name}.{method.Name}");
            }
        }
        stackLog.Reverse();
        string stackPath = string.Join(" => ", stackLog);

        string prefix = $"[{stackPath}]";
        string formattedMessage = $"{prefix} {msg}";

        Debug.Log(formattedMessage);
#elif RELEASE
        LogError(msg.ToString());
#endif
    }

    private static void Log(string message, EType etype, ELogType eLogType = ELogType.Normal)
    {
        string callingNamespace = GetCallingNamespace();
        string formattedMessage = $"[{Class1.modName}] [{callingNamespace}]{(etype != EType.None ? (etype == EType.Client ? " [Client]" : " [Server]") : "")} {message}";

        switch (eLogType)
        {
            case ELogType.Normal:
                Debug.Log(formattedMessage);
                break;
            case ELogType.Warning:
                Debug.LogWarning(formattedMessage);
                break;
            case ELogType.Error:
                Debug.LogError(formattedMessage);
                break;
        }
    }

    private static string GetCallingNamespace()
    {
        StackTrace stackTrace = new StackTrace();
        for (int i = 3; i < stackTrace.FrameCount; i++) // Start from 3 to skip Logger's own stack frames
        {
            var method = stackTrace.GetFrame(i)?.GetMethod();
            if (method != null && method.DeclaringType != null)
            {
                return method.DeclaringType.Namespace ?? "UnknownNamespace";
            }
        }
        return "UnknownNamespace";
    }

    private enum EType
    {
        None,
        Client,
        Server
    }

    private enum ELogType
    { 
        Normal,
        Warning,
        Error
    }
}
