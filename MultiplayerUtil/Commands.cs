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


namespace MultiplayerUtil
{
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
        public class JoinLobby : GameConsole.ICommand
        {
            public string Name => "JoinLobby";

            public string Description => "";

            public string Command => "JoinLobby";

            public async void Execute(GameConsole.Console con, string[] args)
            {
                MultiplayerUtil.LobbyManager.JoinLobbyWithID(ulong.Parse(args[0]));
            }
        }
        public class CreateLobby : GameConsole.ICommand
        {
            public string Name => "CreateLobby";
            
            public string Description => "";

            public string Command => "CreateLobby";
             
            public async void Execute(GameConsole.Console con, string[] args)
            {
                MultiplayerUtil.LobbyManager.SetSettings("GAHHHHHHHHHHH", 3, true, true, false, false, ("Idk", "idk"));
                MultiplayerUtil.LobbyManager.CreateLobby();
            }
        }

        public class ListLobbys : GameConsole.ICommand
        {
            public string Name => "ListLobbys";

            public string Description => "";
            
            public string Command => "LL";
            
            public async void Execute(GameConsole.Console con, string[] args)
            {
                List<Lobby> getthingy = getthingy = await MultiplayerUtil.LobbyManager.FetchLobbies(("Idk", "idk"));


                foreach (Lobby lob in getthingy)
                {
                    Debug.Log("-------------------");

                    Debug.Log($"Lobby name: {lob.Data.Where(kvp => kvp.Key == "name" && !string.IsNullOrEmpty(kvp.Value))
                                 .Select(kvp => kvp.Value)
                                 .FirstOrDefault()} ");

                    Debug.Log($"Members: {lob.Data.Where(kvp => kvp.Key == "members" && !string.IsNullOrEmpty(kvp.Value))
                                 .Select(kvp => kvp.Value)
                                 .FirstOrDefault()} ");

                    Debug.Log($"Id: {lob.Id}");
                }
            }
        }
    }
   
}
