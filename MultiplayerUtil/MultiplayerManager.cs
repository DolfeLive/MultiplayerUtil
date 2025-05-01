 
namespace MultiplayerUtil;

public class SteamManager : MonoBehaviour
{
    public static SteamManager instance;


    public const float importantUpdatesASec = 33.3f;
    public const float unimportantUpdatesAMin = 6;

    // Runtime
    public Lobby? current_lobby;

    public List<SteamId> BannedSteamIds = new();
    public List<SteamId> BlockedSteamIds = new();


    public SteamId selfID;
    private string playerName;
    public bool isLobbyOwner = false;
    string LobbyName;
    int maxPlayers;
    bool publicLobby;
    bool cracked;
    public Coroutine? dataLoop;

    public Server.Serveier server;
    private Client.Client client;
    private bool CheckForP2P = false;

    // End

    public static bool SelfP2PSafeguards = false;

    void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
        //this.gameObject.hideFlags = HideFlags.HideAndDontSave;
        instance = this;
#if DEBUG
        Command.Register();
#endif
        Callbacks.StartupComplete?.Invoke();

        Callbacks.p2pMessageRecived.AddListener(_ =>
        {
            var (data, sender) = _; // (byte[], SteamId?)

            if (data == null || !sender.Value.IsValid)
            {
                Debug.LogError($"Received invalid P2P message: data or sender is null. data:{data == null}, Sender:{sender.Value}");
                return;
            }
            if (SelfP2PSafeguards)
                if (!sender.HasValue || sender.Value == LobbyManager.selfID) // Check if sender isnt null or if the sender is yourself
                {
                    Debug.Log($"Failed at reciving, why: {(sender.HasValue ? "Has value" : "Does not have value")} {(data.Length > 0 ? string.Join("", data) : "")}, Sender: {(sender.HasValue ? sender.Value.ToString() : "null")}");
                    return;
                }

            ObserveManager.OnMessageRecived(data, sender);
        });

        Debug.unityLogger.filterLogType = LogType.Log | LogType.Warning | LogType.Error | LogType.Exception | LogType.Assert;
        this.selfID = SteamClient.SteamId;
        
        SetupCallbacks();
    }

    public void ReInit(bool cracked)
    {
        if (cracked != this.cracked)
        {
            this.cracked = cracked;
            SteamClient.Shutdown();
            SteamClient.Init(_MultiplayerUtil.appId);
            this.selfID = SteamClient.SteamId;
        }
    }
    
    void SetupCallbacks()
    {
#if DEBUG
        Steamworks.Dispatch.OnDebugCallback = (type, str, server) =>
        {
            Clogger.Log($"[Callback {type} {(server ? "server" : "client")}] {str}");
        };
#endif
        Steamworks.Dispatch.OnException = (e) =>
        {
            Clogger.LogError($"Exception: {e.Message}, {e.StackTrace}");
        };

        //Steamworks.SteamUtils.OnSteamShutdown

        SteamMatchmaking.OnLobbyCreated += (result, lobby) =>
        {
            Clogger.Log($"Lobby Created, result: {result}, lobby: {lobby.Id}");
            Callbacks.OnLobbyCreated.Invoke(lobby);
        };
        
        SteamNetworking.OnP2PSessionRequest += (id) =>
        {
            Clogger.Log($"P2P requested from: {id}");
            Callbacks.OnP2PSessionRequest.Invoke(id);
        };

        SteamNetworking.OnP2PConnectionFailed += (id, sessionError) =>
        {
            Clogger.Log($"P2P Connection failed, id: {id}, error: {sessionError}");
            Callbacks.OnP2PConnectionFailed.Invoke(id, sessionError);
        };

        SteamMatchmaking.OnLobbyMemberJoined += (l, f) =>
        {
            Clogger.Log($"Lobby member joined: {f.Name}");
            if (f.Id != selfID && !BannedSteamIds.Contains(f.Id))
            {
                bool p2pEstablished = EstablishP2P(f);
                server.besties.Add(f);

                if (p2pEstablished == false)
                {
                    Clogger.LogWarning($"Falied to establish p2p with: {f.Name}");
                }

                l.SendChatString($":::{f.Id} Joined"); // ::: will be a hidden message marking a user joining for the host and clients to process
                Callbacks.OnLobbyMemberJoined.Invoke(l, f);
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
                    SteamManager.instance.EstablishP2P(member.Id);
                }

                Callbacks.OnLobbyEntered.Invoke(l);
            }
        };

        SteamMatchmaking.OnChatMessage += (lo, fr, st) =>
        {
            if (BlockedSteamIds.Contains(fr.Id)) return;

            Clogger.Log($"Chat message received from {fr.Name}: {st}");
            Callbacks.OnChatMessageRecived.Invoke(lo, fr, st);

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
            Callbacks.OnLobbyMemberLeave.Invoke(Lob, Fri);
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
            Callbacks.OnLobbyMemberLeave.Invoke(Lob, Fri);
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
            Callbacks.OnLobbyMemberLeave.Invoke(Lob, Fri);
            Clogger.Log($"Lobby Member kicked: {Fri.Name}, Kicker: {Kicker.Name}");
        };

        SteamMatchmaking.OnLobbyMemberBanned += (Lob, Banne, Kicker) =>
        {
            if (isLobbyOwner)
            {
                server.besties.Remove(Banne);
                Closep2P(Banne);
            }
            else
            {
                client.connectedPeers.Remove(Banne.Id);
                Closep2P(Banne);
            }
            Callbacks.OnLobbyMemberLeave.Invoke(Lob, Banne);
            Callbacks.OnLobbyMemberBanned.Invoke(Banne);
            Clogger.Log($"Lobby Member Banned: {Banne.Name}, Banner: {Kicker.Name}");
        };

    }

    public bool EstablishP2P(dynamic bestie)
    {
        string HelloP2P = "Hello!§";
        bool Result = false;
        switch (bestie)
        {
            case Friend friend:
                if (friend.Id.Value == selfID.Value)
                {
                    Clogger.Log("Skippng establishing p2p with self");
                    return true;
                }
                Clogger.StackTraceLog($"Establishing p2p with: {friend.Name}, {friend.Id}");
                Result = SteamNetworking.SendP2PPacket(friend.Id, Data.Serialize(HelloP2P));
                //Result = SteamNetworking.AcceptP2PSessionWithUser(friend.Id);
                return Result;
                break;
            case SteamId steamId:
                if (SelfP2PSafeguards)
                    if (steamId.Value == selfID.Value)
                    {
                        Clogger.Log("Skippng establishing p2p with self");
                        return true;
                    }
                Clogger.StackTraceLog($"Establishing p2p with: {steamId}");
                Result = SteamNetworking.SendP2PPacket(steamId, Data.Serialize(HelloP2P));
                //Result = SteamNetworking.AcceptP2PSessionWithUser(steamId);
                return Result;
                break;
            default:
                Clogger.LogError("Error with establishing p2p");
                return false;
                break;
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
                Clogger.LogError("Error with closing p2p");
                return false;
        }
    }

    public IEnumerator DataLoopInit()
    {
        if (dataLoop != null)
        {
            Clogger.StackTraceLog("Dataloop alr running", 0); 
            yield break;
        }

        Clogger.Log("Data Loop Init Activated");
        float interval = 1f / importantUpdatesASec;
        float unimportantInterval = 60f / unimportantUpdatesAMin;

        float unimportantTimeElapsed = 0f;
        
        CheckForP2P = true;

        yield return new WaitForSecondsRealtime(0.1f);

        try
        {
            while (true)
            {
                if (current_lobby == null)
                {

                    yield return new WaitForSeconds(5f);
                    if (current_lobby == null)
                    {
                        Clogger.LogWarning("Breaking out of DataLoopInit");
                        yield break;
                    }
                    print("everything was fine");

                }

                Callbacks.TimeToSendImportantData?.Invoke();

                if (isLobbyOwner)
                {
                    unimportantTimeElapsed += interval;

                    if (unimportantTimeElapsed >= unimportantInterval)
                    {
                        Clogger.UselessLog("TimeToSendUnimportantData invoked");
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
            CheckForP2P = false;
            Clogger.Log("DataLoopInit ending");
        }
    }
        
    public void DataSend(object data)
    {
        try
        {
            NetworkWrapper wrapper = new()
            {
                ClassType = data.GetType().AssemblyQualifiedName,
                ClassData = Data.Serialize(data)
            };

            byte[] serializedData = Data.Serialize(wrapper);

            if (current_lobby != null)
            {
                if (isLobbyOwner)
                    server.Send(serializedData);
                else
                    client.Send(serializedData);
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

        if (CheckForP2P)
        {
            (byte[], SteamId?) data = CheckForP2PMessages();
            if (data != (null, null))
            {
                Clogger.Log("P2P Message recived");
                
                if (Data.TryDeserialize<string>(data.Item1, out string result) && result == "Hello!§")
                {
                    Clogger.Log("The p2p message was just a init for a p2p");
                    return;
                }
                
                Callbacks.p2pMessageRecived.Invoke(data);
            }
        }
    }
    public async void HostLobby(string LobbyName, int? maxPlayers, bool publicLobby, bool cracked, bool cheats, bool mods, (string, string) ModLobbyIDentifiers)
    {
        if (!SteamClient.IsValid)
        {
            Clogger.LogWarning("Steam client is not initialized");

            try
            {
                ReInit(_MultiplayerUtil.appId == 480u ? true : false);
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
                    current_lobby?.SetData("Owner", server.besties[0].Name);

                    if (current_lobby.Value is Lobby lobby)
                    {
                        lobby.Owner = server.besties[0];
                    }

                }

            }

            current_lobby?.SendChatString($":::Leaving.{selfID.Value}");
            current_lobby?.Leave();
            current_lobby = null;
        }


        Lobby? createdLobby = await SteamMatchmaking.CreateLobbyAsync(maxPlayers ?? 8);
        if (createdLobby == null)
        {
            Clogger.LogError("Lobby creation failed - Result is null");
            return;
        }
                
        server = new Server.Serveier();

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
        current_lobby?.SetData("Owner", SteamClient.Name);

        Clogger.Log($"Lobby Created, id: {current_lobby?.Id}, {isLobbyOwner}");
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

                client = new Client.Client();
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
                    if (SelfP2PSafeguards)
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
                current_lobby?.SetData("members", server.besties.Count.ToString());
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
        current_lobby = null;
        server = null;
        client = null;
    }

   
    public void SendChatMessage(string msg)
    {
        current_lobby?.SendChatString(msg);
    }

    ~SteamManager()
    {
        Disconnect();
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

    /// <summary>
    /// Fires when a member joins the lobby (uttil automatically sets up p2p)
    /// </summary>
    public static UnityEvent<Lobby, Friend> OnLobbyMemberJoined = new UnityEvent<Lobby, Friend>();

    public static UnityEvent<Lobby, Friend> OnLobbyMemberLeave = new UnityEvent<Lobby, Friend>();
    
    public static UnityEvent<Lobby, Friend, string> OnChatMessageRecived = new UnityEvent<Lobby, Friend, string>();
    
    public static UnityEvent<SteamId> OnP2PSessionRequest = new UnityEvent<SteamId>();
    
    public static UnityEvent<SteamId, P2PSessionError> OnP2PConnectionFailed = new UnityEvent<SteamId, P2PSessionError>();
    
    public static UnityEvent<Friend> OnLobbyMemberBanned = new UnityEvent<Friend>();
    
    public static UnityEvent<Lobby> OnLobbyEntered = new UnityEvent<Lobby>();
    
    public static UnityEvent<Lobby> OnLobbyCreated = new UnityEvent<Lobby>();

}

// Wrapper so i can handle multiple classes
[System.Serializable]
public class NetworkWrapper
{
    public string ClassType { get; set; }
    public byte[] ClassData { get; set; }
}

// this system will allow users to subscribe with their class to notifications of when the specific class they are looking for is detected

public static class ObserveManager
{

    public static Dictionary<Type, Callbacks.SenderUnityEvent> subscribedEvents = new();

    public static void SubscribeToType(Type classType, out Callbacks.
        SenderUnityEvent whenDetected)
    {
        Callbacks.SenderUnityEvent whenDetectedAction = new();

        subscribedEvents.Add(classType, whenDetectedAction);

        whenDetected = whenDetectedAction;
    }

    public static void OnMessageRecived(byte[] message, SteamId? sender)
    {
        NetworkWrapper recivedData = null;
        try
        {
            recivedData = Data.Deserialize<NetworkWrapper>(message);
        }
        catch (InvalidCastException e)
        {
            Logger.LogWarning($"Failed to cast p2p message, sender: {sender}, message len: {message.Length}");
            return;
        }

        Clogger.Log($"Recived p2p message, sender: {sender}, type: {recivedData.ClassType}, data: {recivedData.ClassData}");

        Type type = Type.GetType(recivedData.ClassType);
        if (type != null && ObserveManager.subscribedEvents.TryGetValue(type, out Callbacks.SenderUnityEvent notifier))
        {
            notifier.Invoke((recivedData.ClassData, sender));
        }
    }

}