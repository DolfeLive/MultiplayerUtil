using Steamworks.Data;
using Steamworks;
using System;
using UnityEngine;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using GameConsole.pcon;
using MultiplayerUtil;
using System.IO;
using System.Runtime.InteropServices;
using System.Collections;
using Clogger = MultiplayerUtil.Logger;
using System.Text.Json;
using UnityEngine.Events;

namespace MultiplayerUtil;

public class SteamManager : MonoBehaviour
{
    public static SteamManager instance;


    public float importantUpdatesASec = 64;
    public float unimportantUpdatesASec = 0.5f;

    // Runtime
    public Lobby? current_lobby;


    public SteamId selfID;
    private string playerName;
    public bool isLobbyOwner = false;
    string LobbyName;
    int maxPlayers;
    bool publicLobby;
    bool cracked;
    public Coroutine? dataLoop;

    private Serveier server;
    private Client client;

    public class ObjectUnityEvent : UnityEvent<object> {}

    public ObjectUnityEvent p2pMessageRecived = new ObjectUnityEvent();
    public UnityEvent TimeToSendImportantData = new UnityEvent();
    public UnityEvent TimeToSendUnimportantData = new UnityEvent();
    public UnityEvent StartupComplete = new UnityEvent();

    // End

    void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
        //this.gameObject.hideFlags = HideFlags.HideAndDontSave;
        instance = this;
        Callbacks();

        Command.Register();

        StartupComplete?.Invoke();
    }

    public void ReInnit(bool cracked)
    {
        if (cracked != this.cracked)
        {
            this.cracked = cracked;
            SteamClient.Init(Class1.appId);

        }
    }

    void Callbacks()
    {
        SteamMatchmaking.OnLobbyMemberJoined += (l, f) =>
        {
            Clogger.Log($"Lobby member joined: {f.Name}");
            if (f.Id != selfID)
            {
                SteamNetworking.AcceptP2PSessionWithUser(f.Id);
                server.besties.Add(f);

                l.SendChatString($":::{f.Id} Joined"); // ::: will be a hidden message marking a user joining for the host and clients to process

            }
        };
        SteamMatchmaking.OnLobbyEntered += (l) => {
            if (!String.IsNullOrEmpty(l.Owner.Name) && l.Owner.Id != selfID)
            {
                Clogger.Log($"Joined Lobby: {l.Owner.Name}");
                client.Connect(l.Owner.Id);


                foreach (var member in l.Members)
                {
                    client.connectedPeers.Add(member.Id);
                }
            }
        };

        SteamMatchmaking.OnChatMessage += (lo, fr, st) =>
        {
            Clogger.Log($"Chat message recived from {fr.Name}: {st}");

        };

        SteamMatchmaking.OnLobbyMemberLeave += (Lob, Fri) =>
        {
            if (isLobbyOwner)
            {
                server.besties.Remove(Fri);
                SteamNetworking.CloseP2PSessionWithUser(Fri.Id);
            }
            else
            {
                client.connectedPeers.Remove(Fri.Id);
                SteamNetworking.CloseP2PSessionWithUser(Fri.Id);
            }
            Clogger.Log($"Lobby member left: {Fri.Name}");
        };

        SteamMatchmaking.OnLobbyMemberDisconnected += (Lob, Fri) =>
        {
            if (isLobbyOwner)
            {
                server.besties.Remove(Fri);
                SteamNetworking.CloseP2PSessionWithUser(Fri.Id);
            }
            else
            {
                client.connectedPeers.Remove(Fri.Id);
                SteamNetworking.CloseP2PSessionWithUser(Fri.Id);
            }
            Clogger.Log($"Lobby member disconnected: {Fri.Name}");
        };

        SteamMatchmaking.OnLobbyMemberKicked += (Lob, Fri, Kicker) =>
        {
            if (isLobbyOwner)
            {
                server.besties.Remove(Fri);
                SteamNetworking.CloseP2PSessionWithUser(Fri.Id);
            }
            else
            {
                client.connectedPeers.Remove(Fri.Id);
                SteamNetworking.CloseP2PSessionWithUser(Fri.Id);
            }
            Clogger.Log($"Lobby Member kicked: {Fri.Name}, Kicker: {Kicker.Name}");
        };
    }
    
    public IEnumerator DataLoopInit()
    {
        if (dataLoop != null)
            yield break;
        
        Clogger.Log("Data Loop Init Activated");
        float interval = 1f / importantUpdatesASec;
        float unimportantInterval = 1f / unimportantUpdatesASec;

        float unimportantTimeElapsed = 0f;

        Coroutine checkloop = StartCoroutine(CheckForP2PLoop());

        while (true)
        {
            float startTime = Time.time;

            //DataSend();
            TimeToSendImportantData?.Invoke();


            if (isLobbyOwner)
            {
                unimportantTimeElapsed += Time.time - startTime;

                if (unimportantTimeElapsed >= unimportantInterval)
                {
                    TimeToSendUnimportantData?.Invoke();
                    Clogger.Log($"Lobby members: {current_lobby?.Members}");
                    current_lobby?.SetData("members", $"{current_lobby?.Members.Count()}/{maxPlayers}");
                    unimportantTimeElapsed = 0f;
                }
            }

            float elapsedTime = Time.time - startTime;

            float waitTime = Mathf.Max(0, interval - elapsedTime);
                        
            yield return new WaitForSeconds(waitTime);

            if (current_lobby == null)
            {
                Clogger.LogWarning("breaking out of DataLoopInit");
                StopCoroutine(checkloop);

                yield break;
            }
        }
    }

    private IEnumerator CheckForP2PLoop()
    {
        while (true)
        {
            object data = CheckForP2PMessages();
            p2pMessageRecived?.Invoke(data);

            yield return null;
        }
    }

    private void DataSend(object serialisedData)
    {
        try
        {
            if (current_lobby != null)
            {
                if (!isLobbyOwner)
                    client.Send(serialisedData);
                else
                    server.Send(serialisedData);
            }
            else
            {
                Clogger.Log("Current Lobby is null");
            }
        }
        catch (Exception e)
        {
            Clogger.Log($"Data Send Exception: {e}");
        }
    }

    void Update()
    {
        SteamClient.RunCallbacks();
    }
    public async void HostLobby(string LobbyName, int? maxPlayers, bool publicLobby, bool cracked, bool cheats, bool mods, (string, string) ModLobbyIDentifiers)
    {
        if (!SteamClient.IsValid)
        {
            Clogger.LogWarning("Steam client is not initialized");

            try
            {
                ReInnit(Class1.appId == 480u ? true : false);
                //SteamClient.Init(Class1.appId);
                Clogger.Log("Reinited steam");
            }
            catch (Exception e) { Clogger.LogError($"STEAM ERROR: {e}"); Clogger.LogWarning("Try launching steam if it isnt launched!"); }

            return;
        }

        if (current_lobby != null)
        {
            if (isLobbyOwner)
            {
                if (server.besties.Count > 0)
                {
                    current_lobby.Value.SetData("Owner", server.besties[0].Name);

                    if (current_lobby.Value is Lobby lobby)
                    {
                        lobby.Owner = server.besties[0];
                    }

                }

            }

            current_lobby.Value.SendChatString($":::Leaving.{selfID.Value}");
            current_lobby.Value.Leave();
            current_lobby = null;
        }


        Lobby? createdLobby = await SteamMatchmaking.CreateLobbyAsync(maxPlayers ?? 8);
        if (createdLobby == null)
        {
            Clogger.LogError("Lobby creation failed - Result is null");
            return;
        }
                
        server = new Serveier();

        this.LobbyName = LobbyName;


        if (maxPlayers <= 0) maxPlayers = 8;

        this.maxPlayers = maxPlayers ?? 8;
        this.publicLobby = publicLobby;

        isLobbyOwner = true;
        current_lobby = createdLobby;

        current_lobby?.SetJoinable(true);
        if (publicLobby)
            current_lobby?.SetPublic();
        else
            current_lobby?.SetPrivate();

        current_lobby?.SetData(ModLobbyIDentifiers.Item1, ModLobbyIDentifiers.Item2);
        current_lobby?.SetData("name", LobbyName);
        current_lobby?.SetData("cheats", cheats.ToString());
        current_lobby?.SetData("mods", mods.ToString());
        current_lobby?.SetData("members", $"1/{maxPlayers}");

        Clogger.Log($"Lobby Created, id: {current_lobby?.Id}");
    }

    // Help collected from jaket github https://github.com/xzxADIxzx/Join-and-kill-em-together/blob/main/src/Jaket/Net/LobbyController.cs
    public async void JoinLobbyWithID(ulong id)
    {
        try
        {
            Clogger.Log("Joining Lobby with ID");
            Lobby lob = new Lobby(id);

            RoomEnter result = await lob.Join();

            if (result == RoomEnter.Success)
            {
                Clogger.Log($"Lobby join Success: {result}");
                isLobbyOwner = false;
                current_lobby = lob;

                client = new Client();
            }
            else
            {
                Clogger.LogWarning($"Couldn't join the lobby. Result is {result}");
            }
        }
        catch (Exception ex)
        {
            Clogger.LogError($"An error occurred while trying to join the lobby: {ex.Message}, The error might be because steam isnt launched");
        }
    }
    
    public object CheckForP2PMessages()
    {
        byte[] buffer = new byte[64];
        uint size = (uint)buffer.Length;
        SteamId steamId = new SteamId();
        int channel = 0;
        try
        {
            while (SteamNetworking.ReadP2PPacket(buffer, ref size, ref steamId, channel))
            {
                if (steamId == selfID) continue;

                byte[] receivedData = new byte[size];
                Array.Copy(buffer, receivedData, (int)size);
                
                var dataPacket = Data.Deserialize<string>(receivedData);

                Clogger.Log($"Received P2P message from {steamId}, Data: {JsonUtility.ToJson(dataPacket)}");

                return dataPacket;
            }
        }
        catch (ArgumentException)
        {
        }
        return null;
    }

    public void InviteFriend() => SteamFriends.OpenGameInviteOverlay(SteamManager.instance.current_lobby.Value.Id);

    void OnApplicationQuit()
    {
        if (isLobbyOwner)
        {
            if (server.besties.Count > 0)
            {
                foreach (var item in server.besties)
                {
                    SteamNetworking.CloseP2PSessionWithUser(item.Id);
                }

                current_lobby?.SendChatString($"||| Setting Lobby Owner To: {server.besties[0].Name}");
                current_lobby?.SetData("Owner", server.besties[0].Name);
                current_lobby?.IsOwnedBy(server.besties[0].Id);
                
                Clogger.Log($"Setting Lobby Owner to: {server.besties[0].Name}");

            }

        }
        else
        {
            foreach (var item in client.connectedPeers)
            {
                SteamNetworking.CloseP2PSessionWithUser(item);
            }
        }
        current_lobby?.Leave();
    }
    public void SendChatMessage(string msg)
    {
        current_lobby?.SendChatString(msg);
    }
}



