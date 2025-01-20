using System;
using System.Collections.Generic;
using Clogger = MultiplayerUtil.Logger;
using Steamworks;
using System.Threading.Tasks;
using Steamworks.Data;
using System.Linq;

namespace MultiplayerUtil;

public class LobbyManager
{
    string LobbyName;
    int? maxPlayers;
    bool publicLobby;
    bool cracked;
    bool cheats;
    bool mods;
    (string, string) modIdentifier;
    public void SetSettings(string lobbyName, int? maxPlayers, bool publicLobby, bool cracked, bool cheats, bool mods, (string, string) modIdentifier)
    {
        this.LobbyName = lobbyName;
        this.maxPlayers = maxPlayers;
        this.publicLobby = publicLobby;
        this.cracked = cracked;
        this.cheats = cheats;
        this.mods = mods;
        this.modIdentifier = modIdentifier;
    }
    public void CreateLobby()
    {
        Clogger.Log("Creating Lobby");
        
        SteamManager.instance.HostLobby(LobbyName, maxPlayers, publicLobby, cracked, cheats, mods, modIdentifier);
    }

    public async Task<List<Lobby>> FetchLobbies((string, string) modIdentifierKVP)
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

}
