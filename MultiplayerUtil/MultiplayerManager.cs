﻿using Steamworks.Data;
using Steamworks;
using System;
using UnityEngine;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using MultiplayerUtil;
using System.IO;
using System.Collections;
using Clogger = MultiplayerUtil.Logger;
using UnityEngine.Events;
using GameConsole.pcon;

namespace MultiplayerUtil;

public class SteamManager : MonoBehaviour
{
    public static SteamManager instance;


    public float importantUpdatesASec = 32;
    public float unimportantUpdatesAMin = 3;

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

    // End

    void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
        //this.gameObject.hideFlags = HideFlags.HideAndDontSave;
        instance = this;
        SetupCallbacks();

        Command.Register();

        Callbacks.StartupComplete?.Invoke();
    }

    public void ReInnit(bool cracked)
    {
        if (cracked != this.cracked)
        {
            this.cracked = cracked;
            SteamClient.Init(Class1.appId);

        }
    }

    void SetupCallbacks()
    {
        SteamMatchmaking.OnLobbyMemberJoined += (l, f) =>
        {
            Clogger.Log($"Lobby member joined: {f.Name}");
            if (f.Id != selfID)
            {
                EstablishP2P(f);
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
                Closep2P(Fri);
            }
            else
            {
                client.connectedPeers.Remove(Fri.Id);
                Closep2P(Fri);
            }
            Clogger.Log($"Lobby member left: {Fri.Name}");
        };

        SteamMatchmaking.OnLobbyMemberDisconnected += (Lob, Fri) =>
        {
            if (isLobbyOwner)
            {
                server.besties.Remove(Fri);
                Closep2P(Fri);
            }
            else
            {
                client.connectedPeers.Remove(Fri.Id);
                Closep2P(Fri);
            }
            Clogger.Log($"Lobby member disconnected: {Fri.Name}");
        };

        SteamMatchmaking.OnLobbyMemberKicked += (Lob, Fri, Kicker) =>
        {
            if (isLobbyOwner)
            {
                server.besties.Remove(Fri);
                Closep2P(Fri);
            }
            else
            {
                client.connectedPeers.Remove(Fri.Id);
                Closep2P(Fri);
            }
            Clogger.Log($"Lobby Member kicked: {Fri.Name}, Kicker: {Kicker.Name}");
        };
    }

    public bool EstablishP2P(dynamic bestie)
    {

        switch (bestie)
        {
            case Friend friend:
                Clogger.StackTraceLog($"Establishing p2p with: {friend.Name}, {friend.Id}");
                return SteamNetworking.AcceptP2PSessionWithUser(friend.Id);

            case SteamId steamId:
                Clogger.StackTraceLog($"Establishing p2p with: {steamId}");
                return SteamNetworking.AcceptP2PSessionWithUser(steamId);

            default:
                return false;
        }
    }

    public bool Closep2P(dynamic unbestie)
    {
        switch (unbestie)
        {
            case Friend friend:
                Clogger.StackTraceLog($"DeEstablishing p2p with: {friend.Name}, {friend.Id}");
                return SteamNetworking.CloseP2PSessionWithUser(friend.Id);

            case SteamId steamId:
                Clogger.StackTraceLog($"DeEstablishing p2p with: {steamId}");
                return SteamNetworking.CloseP2PSessionWithUser(steamId);

            default:
                return false;
        }
    }

    public IEnumerator DataLoopInit()
    {
        if (dataLoop != null)
        {
            Clogger.StackTraceLog("Dataloop alr running");
            yield break;
        }

        Clogger.Log("Data Loop Init Activated");
        float interval = 1f / importantUpdatesASec;
        float unimportantInterval = 60f / unimportantUpdatesAMin;

        float unimportantTimeElapsed = 0f;

        Coroutine checkloop = StartCoroutine(CheckForP2PLoop());

        try
        {
            while (true)
            {
                Callbacks.TimeToSendImportantData?.Invoke();

                if (isLobbyOwner)
                {
                    unimportantTimeElapsed += interval;

                    if (unimportantTimeElapsed >= unimportantInterval)
                    {
                        Callbacks.TimeToSendUnimportantData?.Invoke();
                        current_lobby?.SetData("members", $"{current_lobby?.Members.Count()}/{maxPlayers}");
                        unimportantTimeElapsed = 0f;
                    }
                }

                if (current_lobby == null)
                {
                    Clogger.LogWarning("Breaking out of DataLoopInit");
                    yield break;
                }

                yield return new WaitForSeconds(interval);

            }
        }
        finally
        {
            // Ensure any cleanup tasks are performed here
            if (checkloop != null)
                StopCoroutine(checkloop);
        }
    }

    private IEnumerator CheckForP2PLoop()
    {
        while (true)
        {
            (byte[], SteamId?) data = CheckForP2PMessages();
            Callbacks.p2pMessageRecived?.Invoke(data);

            yield return null;
        }
    }

    public void DataSend(object serialisedData)
    {
        try
        {
            if (current_lobby != null)
            {
                if (isLobbyOwner)
                    server.Send(serialisedData);
                else
                    client.Send(serialisedData);
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
            server = null;
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
                current_lobby = null;
                isLobbyOwner= false;
                client = null;

                Clogger.LogWarning($"Couldn't join the lobby. Result is {result}");
            }
        }
        catch (Exception ex)
        {
            Clogger.LogError($"An error occurred while trying to join the lobby: {ex.Message}, The error might be because steam isnt launched");
        }
    }
    
    public (byte[], SteamId?) CheckForP2PMessages()
    {

        try
        {
            while (SteamNetworking.IsP2PPacketAvailable(out uint availableSize, 0))
            {
                SteamId Sender = new SteamId();

                byte[] buffer = new byte[availableSize];
                bool worked = SteamNetworking.ReadP2PPacket(buffer, ref availableSize, ref Sender, 0);

                if (worked)
                {
                    if (!Sender.IsValid)
                    {
                        Clogger.Log("Sender is null skipping");
                        continue;
                    }
                    Clogger.Log($"New p2p: {Sender}");
                    if (Sender == selfID)
                    {
                        Clogger.Log("P2p comes from self, skipping");
                        continue;
                    }

                    return (buffer, Sender);
                }
                else
                {
                    Clogger.Log($"p2p failed: {Sender}");

                    return (null, null);
                }
            }
        }
        catch (ArgumentException ae)
        {
            Clogger.LogError($"CheckForP2p Arg Exeption: {ae}");
        }
        return (null, null);
    }

    public void InviteFriend() => SteamFriends.OpenGameInviteOverlay(SteamManager.instance.current_lobby.Value.Id);

    void OnApplicationQuit() => Disconnect();
    public void Disconnect()
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


public static class Callbacks
{
    public class SenderUnityEvent : UnityEvent<(byte[], SteamId?)> { }

    /// <summary>
    ///  Actives when a p2p message is revied, the returned object IS serialized
    ///  Use Data.Deserialize
    /// </summary>
    public static SenderUnityEvent p2pMessageRecived = new SenderUnityEvent();

    /// <summary>
    /// Activates on the updateIterval
    /// Use Data.Serialize
    /// </summary>
    public static UnityEvent TimeToSendImportantData = new UnityEvent();

    /// <summary>
    /// Activated on unimportantUpdates
    /// Use Data.Serialize
    /// </summary>
    public static UnityEvent TimeToSendUnimportantData = new UnityEvent();
    
    /// <summary>
    /// Activates when SteamManager is fully set up and ready to use, make code that uses these methods after this fires
    /// </summary>
    public static UnityEvent StartupComplete = new UnityEvent();
}
