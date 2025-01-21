using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ULTRAKILL;
using UnityEngine;
using Steamworks.Data;
using Steamworks;
using TMPro;
using GameConsole;
using Clogger = MultiplayerUtil.Logger;

namespace MultiplayerUtil;

public class Command
{
    public static void Register()
    {
        var nestedTypes = typeof(Commands).GetNestedTypes();

        GameConsole.Console.Instance.RegisterCommands(nestedTypes.Where(_ => typeof(ICommand).IsAssignableFrom(_)).
            Select(_ => Activator.CreateInstance(_) as GameConsole.ICommand).
            Where(_ =>_!=null)
            .ToList()
        );
    }
}
public class Commands
{
    /// <summary>
    /// Invite friend
    /// </summary>
    public class InviteFriend : GameConsole.ICommand
    {
        public string Name => "InviteFriend";

        public string Description => "";

        public string Command => "InviteFriend";

        public async void Execute(GameConsole.Console con, string[] args)
        {
            MultiplayerUtil.LobbyManager.InviteFriend();
        }
    }

    /// <summary>
    /// Send Message
    /// </summary>
    public class SM : GameConsole.ICommand 
    {
        public string Name => "SendMessage";

        public string Description => "";

        public string Command => "SM";

        public async void Execute(GameConsole.Console con, string[] args)
        {
            MultiplayerUtil.LobbyManager.SendMessage(string.Join(" ",  args));
        }
    }

    /// <summary>
    /// Join Lobby
    /// </summary>
    public class JoinLobby : GameConsole.ICommand
    {
        public string Name => "JoinLobby";

        public string Description => "";

        public string Command => "JL";

        public async void Execute(GameConsole.Console con, string[] args)
        {
            MultiplayerUtil.LobbyManager.JoinLobbyWithID(ulong.Parse(args[0]));
        }
    }

    /// <summary>
    /// Create Lobby
    /// </summary>
    public class CreateLobby : GameConsole.ICommand
    {
        public string Name => "CreateLobby";
        
        public string Description => "";

        public string Command => "CL";
         
        public async void Execute(GameConsole.Console con, string[] args)
        {
            MultiplayerUtil.LobbyManager.CreateLobby("GAHHHHHHHHHHH", 3, true, true, false, false, ("Idk", "idk"));
        }
    }

    /// <summary>
    /// List Lobbies
    /// </summary>
    public class ListLobbys : GameConsole.ICommand
    {
        public string Name => "ListLobbys";

        public string Description => "";
        
        public string Command => "LL";
        
        public async void Execute(GameConsole.Console con, string[] args)
        {
            Clogger.Log("Retriving all open lobbies");
            List<Lobby> getthingy = getthingy = await MultiplayerUtil.LobbyManager.FetchLobbies(("Idk", "idk"));


            foreach (Lobby lob in getthingy)
            {
                Clogger.Log("-------------------");

                Clogger.Log($"Lobby name: {lob.Data.Where(kvp => kvp.Key == "name" && !string.IsNullOrEmpty(kvp.Value))
                             .Select(kvp => kvp.Value)
                             .FirstOrDefault()} ");

                Clogger.Log($"Members: {lob.Data.Where(kvp => kvp.Key == "members" && !string.IsNullOrEmpty(kvp.Value))
                             .Select(kvp => kvp.Value)
                             .FirstOrDefault()} ");

                Clogger.Log($"Id: {lob.Id}");
                Clogger.Log($"Owner:{lob.Data.Where(kvp => kvp.Key == "Owner" && !string.IsNullOrEmpty(kvp.Value))
                            .Select(kvp => kvp.Value)
                            .FirstOrDefault()}");
            }
        }
    }

    /// <summary>
    /// Disconect from current lobby
    /// </summary>
    public class Disconect : GameConsole.ICommand
    {
        public string Name => "Disconect";

        public string Description => "";

        public string Command => "DC";

        public async void Execute(GameConsole.Console con, string[] args)
        {
            MultiplayerUtil.LobbyManager.Disconnect();
        }
    }

}

