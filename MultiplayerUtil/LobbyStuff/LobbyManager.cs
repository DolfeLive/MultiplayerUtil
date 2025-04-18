﻿ 
namespace MultiplayerUtil;

public static class LobbyManager
{
    public static bool isLobbyOwner => SteamManager.instance.isLobbyOwner;
    public static Lobby? current_lobby => SteamManager.instance.current_lobby;
    public static SteamId selfID => SteamManager.instance.selfID;

    /// <summary>
    /// How many important updates are sent a second, approx 30fps
    /// </summary>
    public static float importantUpdatesASec
    {
        get
        {
            return SteamManager.importantUpdatesASec;
        }
    }

    /// <summary>
    /// How many unimportant updates are sent a min, approx 1 every 10 seconds
    /// </summary>
    public static float unimportantUpdatesAMin
    {
        get
        {
            return SteamManager.unimportantUpdatesAMin;
        }
    }

    private static void restartLoop()
    { 
        if (SteamManager.instance.dataLoop != null)
        {
            SteamManager.instance.StopCoroutine(SteamManager.instance.dataLoop);
            SteamManager.instance.dataLoop = SteamManager.instance.StartCoroutine(SteamManager.instance.DataLoopInit());
        }
    }

    /// <summary>
    /// Creates a lobby with the set settings
    /// </summary>
    /// <param name="lobbyName">The name of the lobby.</param>
    /// <param name="maxPlayers">The maximum number of players allowed in the lobby. If null defaults to 8.</param>
    /// <param name="publicLobby">Indicates whether the lobby is public or private.</param>
    /// <param name="cracked">Indicates if the server will run be joinable by cracked clients or offical.</param>
    /// <param name="cheats">Indicates whether cheats are enabled in the lobby.</param>
    /// <param name="mods">Indicates whether mods are enabled in the lobby.</param>
    /// <param name="modIdentifier">The identifier your mod uses when making a lobby</param>
    public static void CreateLobby(string lobbyName, int? maxPlayers, bool publicLobby, bool cracked, bool cheats, bool mods, (string, string) modIdentifier)
    {
        Clogger.Log("Creating Lobby");
        SteamManager.instance.HostLobby(lobbyName, maxPlayers, publicLobby, cracked, cheats, mods, modIdentifier);
    }

    /// <summary>
    /// returns a list of all lobbies matching your mods lobby identifier
    /// </summary>
    /// <param name="modIdentifierKVP">The identifier your mod uses when making a lobby</param>
    public static async Task<List<Lobby>> FetchLobbies((string, string) modIdentifierKVP)
    {
        List<Lobby> foundLobbies = new List<Lobby>();
        try
        {
            var lobbyList = await SteamMatchmaking.LobbyList.RequestAsync();

            if (lobbyList != null)
            {
                foundLobbies = lobbyList
                    .Where(lobby => lobby.Data.Any(data =>
                        data.Key == modIdentifierKVP.Item1 && data.Value == modIdentifierKVP.Item2))
                    .ToList();
            }
        }
        catch (Exception e)
        {
            Clogger.LogError($"Lobby finding exeption: {e}");
        }

        Clogger.Log($"Found Lobbies: {foundLobbies.Count}");
        return foundLobbies;
    }

    /// <summary>
    /// Joins a lobby with the specified ulong id
    /// </summary>
    /// <param name="id">The lobby id</param>
    public static void JoinLobbyWithID(ulong id)
    {
        Clogger.Log("Joining Lobby");
        
        SteamManager.instance.JoinLobbyWithID(id);
    }

    /// <summary>
    /// Send a chat message
    /// </summary>
    /// <param name="msg">Sends a chat message to the current lobby</param>
    public static void SendMessage(string msg) => SteamManager.instance.SendChatMessage(msg);


    /// <summary>
    /// Send data to connected p2p players
    /// </summary>
    /// <param name="data">The class object to be sent, no need to Serialize, that is done automatically</param>
    public static void SendData(object data)
    {
        SteamManager.instance.DataSend(data);
    }

    /// <summary>
    /// Opens steam invite friend menu
    /// </summary>
    public static void InviteFriend() => SteamManager.instance.InviteFriend();

    /// <summary>
    /// Ditto
    /// </summary>
    public static void Disconnect() => SteamManager.instance.Disconnect();

    /// <summary>
    /// Reinnits steam for use when switing between cracked or not
    /// </summary>
    /// <param name="cracked">if it should innit with appid 480 or ultrakill</param>
    public static void ReInnitSteamClient(bool cracked)
    {
        SteamManager.instance.ReInit(cracked);
    }
}
