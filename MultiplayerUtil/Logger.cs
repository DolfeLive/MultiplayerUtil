using UnityEngine;


namespace MultiplayerUtil;

public static class Logger
{

    public static void Log(string message) => Log(message, EType.None);
    public static void LogWarning(string message) => Log(message, EType.None, ELogType.Warning);
    public static void LogError(string message) => Log(message, EType.None, ELogType.Error);


    public static void Log(string message, bool Client) => Log(message, Client ? EType.Client : EType.Server);
    public static void LogWarning(string message, bool Client) => Log(message, Client ? EType.Client : EType.Server, ELogType.Warning);
    public static void LogError(string message, bool Client) => Log(message, Client ? EType.Client : EType.Server, ELogType.Error);



    private static void Log(string message, EType etype, ELogType eLogType = ELogType.Normal)
    {
        switch (eLogType)
        { 
            case ELogType.Normal:
                Debug.Log($"[{Class1.modName}], {(etype != EType.None ? (etype == EType.Client ? "[Client]" : "[Server]") : "")} {message}");
                break;
            case ELogType.Warning:
                Debug.LogWarning($"[{Class1.modName}], {(etype != EType.None ? (etype == EType.Client ? "[Client]" : "[Server]") : "")} {message}");
                break;
            case ELogType.Error:
                Debug.LogError($"[{Class1.modName}], {(etype != EType.None ? (etype == EType.Client ? "[Client]" : "[Server]") : "")} {message}");
                break;
        }
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
