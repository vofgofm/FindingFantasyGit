using System;
using Dalamud.Game.Command;
using Dalamud.Game;
using Dalamud.IoC;
using Dalamud.Plugin;
using System.IO;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using FindingFantasy.Windows;
using FindingFantasy.Effects;
using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.ClientState.Statuses;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.Keys;
using Dalamud.Game.ClientState.Party;
using FFXIVClientStructs.FFXIV.Client.Game.Group;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.ClientState.Objects.Enums;
using System.Collections.Generic;



using System.Linq;


namespace FindingFantasy.Effects
{
    public class FoodMonitor : IDisposable
    {
        private readonly DalamudPluginInterface pluginInterface;

        [PluginService]
        public static IClientState ClientState { get; private set; }

        [PluginService]
        public static IPartyList PartyList { get; private set; }

        [PluginService]
        public static IChatGui Chat { get; private set; }

        [PluginService]
        public static IObjectTable ObjectTable { get; private set; }

        


        public FoodMonitor(DalamudPluginInterface pluginInterface, IChatGui chat, IPartyList partyList, IObjectTable otable, IClientState cstate)
        {
            this.pluginInterface = pluginInterface;
            Chat = chat;
            PartyList = partyList;
            ObjectTable = otable;
            ClientState = cstate;
        }

        private Dictionary<uint, DateTime> lastReported = new Dictionary<uint, DateTime>();

        public void Update()
        {



            foreach (var obj in ObjectTable)
            {
                if (obj is BattleChara battleChara)
                {
                    var wellFedStatus = battleChara.StatusList.FirstOrDefault(status => status.GameData.Name == "Well Fed");
                    if (wellFedStatus != null && wellFedStatus.Param == 184)
                    {
                        // Check if the character has been logged in the last 30 seconds
                        if (!lastReported.ContainsKey(battleChara.ObjectId) || (DateTime.Now - lastReported[battleChara.ObjectId]).TotalSeconds > 30)
                        {
                            // Update the last reported time
                            lastReported[battleChara.ObjectId] = DateTime.Now;

                            // Print the character name, status ID, and the current date/time
                            Chat.Print($"{battleChara.Name.TextValue} has Well Fed Status ID: {wellFedStatus.StatusId} at {DateTime.Now}");
                            Chat.Print($"{battleChara.Name.TextValue} Is a dirty stinking wine thief, call them out and screenshot this");
                        }
                    }
                }
            }


        }

        public void Dispose()
        {
            // Cleanup code here, if any
        }
    }
}
