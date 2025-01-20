using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ULTRAKILL;
using UnityEngine;
using Steamworks.Data;
using Steamworks;


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


            Debug.Log($"the zaza: {JsonUtility.ToJson(
                    getthingy.Select(_ => _.Data).ToArray()
                    )}");
        }
    }
}
