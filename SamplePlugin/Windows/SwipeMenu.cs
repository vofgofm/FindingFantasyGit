using System;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Interface.Internal;
using Dalamud.Interface.Windowing;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using ImGuiNET;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Game.UI;

namespace FindingFantasy.Windows
{
    public class SwipeMenu : Window, IDisposable
    {
        private IDalamudTextureWrap GoatImage;
        private Plugin Plugin;
        private object ClientState;

        public SwipeMenu(Plugin plugin, IDalamudTextureWrap goatImage) : base(
            "Finding Fantasy", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
        {
            this.SizeConstraints = new WindowSizeConstraints
            {
                MinimumSize = new Vector2(375, 660),
                MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
            };

            this.GoatImage = goatImage;
            this.Plugin = plugin;

        }

        public void Dispose()
        {
            this.GoatImage.Dispose();
        }

        public override void Draw()
        {
            //ImGui.Text($"The random config bool is {this.Plugin.Configuration.SomePropertyToBeSavedAndWithADefault}");

            // if (ImGui.Button("Show Calendar"))
            // {
            //     this.Plugin.DrawConfigUI();
            // }

            ImGui.Spacing();

            //ImGui.Text("This months password is Spooktober");
            ImGui.Indent(55);
            ImGui.Image(this.GoatImage.ImGuiHandle, new Vector2(this.GoatImage.Width, this.GoatImage.Height));
            ImGui.Unindent(55);
            ImGui.Dummy(new Vector2(0, 20.0f));
            ImGui.Text("Name:");
            ImGui.Text("Hroth Daddy");
            ImGui.Dummy(new Vector2(0, 20.0f));
            ImGui.Text("About:");
            ImGui.PushTextWrapPos(ImGui.GetCursorPosX() + 375.0f);
            ImGui.Text("Poly couple looking for a third for our Hrothtub. If you can't handle me at my grey parse you don't deserve me at my clear");
            ImGui.Dummy(new Vector2(0, 60.0f));


            if (ImGui.Button("Dislike"))
            {
                this.Plugin.DrawConfigUI();
            }

            ImGui.SameLine(0.0f, 150.0f); // 20.0f is the spacing between the buttons

            if (ImGui.Button("Like"))
            {
                this.Plugin.DrawConfigUI();
            }
        }
    }
}
