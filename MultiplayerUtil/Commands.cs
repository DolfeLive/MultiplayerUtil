using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ULTRAKILL;
using UnityEngine;
using Steamworks.Data;
using Steamworks;
using TMPro;


namespace MultiplayerUtil
{
    public class Commands
    {
        public static void Register()
        {
            GameConsole.Console.Instance.RegisterCommand(new ListLobbys());
        }
    }


    public class ListLobbys : GameConsole.ICommand
    {
        public string Name
        {
            get
            {
                return "ListLobbys";
            }
        }
        public string Description
        {
            get
            {
                return "";
            }
        }
        public string Command
        {
            get
            {
                return "LL";
            }
        }
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
