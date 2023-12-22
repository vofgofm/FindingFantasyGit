using Dalamud.Game.Command;
using Dalamud.Game;
using Dalamud.IoC;
using Dalamud.Plugin;
using System.IO;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using FindingFantasy.Windows;
using FindingFantasy.Effects;
using Dalamud.Logging;

using FFXIVClientStructs.FFXIV.Client.System.Framework;
using System;
using FindingFantasy.Windows;

namespace FindingFantasy.Windows
{
    public sealed class Plugin : IDalamudPlugin
    {
        public string Name => "Finding Fantasy";
        private const string CommandName = "/findingfantasy";


        private DalamudPluginInterface PluginInterface { get; init; }
        private ICommandManager CommandManager { get; init; }
        private IFramework Framework { get; init; }
        public Configuration Configuration { get; init; }
        public WindowSystem WindowSystem = new("Finding Fantasy");

        private ConfigWindow ConfigWindow { get; init; }
        private MainWindow MainWindow { get; init; }

        private SwipeMenu SwipeMenu { get; init; }

       // private FoodMonitor FoodMonitorScript { get; init; }

        [PluginService]
        public static IChatGui Chat { get; private set; }

        [PluginService]
        public static IClientState ClientState { get; private set; }



        [PluginService]
        public static IPartyList PartyList { get; private set; }


        [PluginService]
        public static IObjectTable ObjectTable { get; private set; }



        public Plugin(
            [RequiredVersion("1.0")] DalamudPluginInterface pluginInterface,
            [RequiredVersion("1.0")] ICommandManager commandManager, 
            [RequiredVersion("1.0")] IFramework framework)

        {
            this.PluginInterface = pluginInterface;
            this.CommandManager = commandManager;
            this.Framework = framework;
            

            this.Configuration = this.PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            this.Configuration.Initialize(this.PluginInterface);

            // you might normally want to embed resources and load them from the manifest stream
            var imagePath = Path.Combine(PluginInterface.AssemblyLocation.Directory?.FullName!, "profile.png");
            var logoImage = this.PluginInterface.UiBuilder.LoadImage(imagePath);
            var likebuttonPath = Path.Combine(PluginInterface.AssemblyLocation.Directory?.FullName!, "like.png");
            var likeImage = this.PluginInterface.UiBuilder.LoadImage(likebuttonPath);
            var dislikebuttonPath = Path.Combine(PluginInterface.AssemblyLocation.Directory?.FullName!, "dislike.png");
            var dislikeImage = this.PluginInterface.UiBuilder.LoadImage(dislikebuttonPath);


            ConfigWindow = new ConfigWindow(this);
            MainWindow = new MainWindow(this, logoImage, pluginInterface, likeImage, dislikeImage);
            SwipeMenu = new SwipeMenu(this, logoImage);


            WindowSystem.AddWindow(ConfigWindow);
            WindowSystem.AddWindow(MainWindow);

            this.CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
            {
                HelpMessage = "A useful message to display in /xlhelp"
            });

            this.PluginInterface.UiBuilder.Draw += DrawUI;
            this.PluginInterface.UiBuilder.OpenConfigUi += DrawConfigUI;

            Framework.Update += OnFrameworkUpdate;

            Chat.Print("Plugin turned on and chat is working");

           

        }

        public void Dispose()
        {
            this.WindowSystem.RemoveAllWindows();
            
            ConfigWindow.Dispose();
            MainWindow.Dispose();
           

            this.CommandManager.RemoveHandler(CommandName);

            Framework.Update -= OnFrameworkUpdate;
        }

        private void OnCommand(string command, string args)
        {
            // in response to the slash command, just display our main ui
            MainWindow.IsOpen = true;
        }

        private void DrawUI()
        {
            this.WindowSystem.Draw();
        }

        public void DrawConfigUI()
        {
            ConfigWindow.IsOpen = true;
        }

        private void OnFrameworkUpdate(IFramework framework)
        {

            try
            {
                //Future logic using update freames
                //Chat.Print("Turn the plugin off and remove this update is working");
            }
            catch (Exception ex)
            {
                //Chat.Print("stop you just dumped like 5GB of frame errors");
            }
        }


    }
}
