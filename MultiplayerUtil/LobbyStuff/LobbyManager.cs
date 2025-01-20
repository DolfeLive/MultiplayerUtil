using System;
using System.Collections.Generic;
using Clogger = MultiplayerUtil.Logger;
using Steamworks;
using System.Threading.Tasks;
using Steamworks.Data;
using System.Linq;

namespace MultiplayerUtil;

public static class LobbyManager
{
    static string LobbyName;
    static int? maxPlayers;
    static bool publicLobby;
    static bool cracked;
    static bool cheats;
    static bool mods;
    static (string, string) modIdentifier;
    public static void SetSettings(string lobbyName, int? maxPlayers, bool publicLobby, bool cracked, bool cheats, bool mods, (string, string) modIdentifier)
    {
        LobbyManager.LobbyName = lobbyName;
        LobbyManager.maxPlayers = maxPlayers;
        LobbyManager.publicLobby = publicLobby;
        LobbyManager.cracked = cracked;
        LobbyManager.cheats = cheats;
        LobbyManager.mods = mods;
        LobbyManager.modIdentifier = modIdentifier;
    }
    public static void CreateLobby()
    {
        Clogger.Log("Creating Lobby");
        
        SteamManager.instance.HostLobby(LobbyName, maxPlayers, publicLobby, cracked, cheats, mods, modIdentifier);
    }

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

    public static void JoinLobbyWithID(ulong id)
    {
        Clogger.Log("Joining Lobby");

        SteamManager.instance.JoinLobbyWithID(id);
    }

    public static void SendMessage(string msg)
    {
        SteamManager.instance.SendChatMessage(msg);
    }
}
