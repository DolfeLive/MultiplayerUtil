using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace MultiplayerUtil;

public static class Logger
{
    public static void Log(string message) => Log(message, EType.None);
    public static void LogWarning(string message) => Log(message, EType.None, ELogType.Warning);
    public static void LogError(string message) => Log(message, EType.None, ELogType.Error);


    public static void Log(string message, bool Client) => Log(message, Client ? EType.Client : EType.Server);
    public static void LogWarning(string message, bool Client) => Log(message, Client ? EType.Client : EType.Server, ELogType.Warning);
    public static void LogError(string message, bool Client) => Log(message, Client ? EType.Client : EType.Server, ELogType.Error);

    public static void StackTraceLog(object msg, int offset = 0)
    {
        string callingMethod = "";
        StackTrace stackTrace = new StackTrace();
        for (int i = 1 + offset; i < stackTrace.FrameCount; i++)
        {
            var method = stackTrace.GetFrame(i)?.GetMethod();
            if (method != null && method.DeclaringType != null)
            {
                callingMethod = method.DeclaringType.Name ?? "Unknown";
            }
        }

        string formattedMessage = $"[{Class1.modName}] [{callingMethod}] {msg}";

        Debug.Log(formattedMessage);
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
