using System;
using System.Numerics;
using Dalamud.Interface.Windowing;
using ImGuiNET;

namespace FindingFantasy.Windows;

public class ConfigWindow : Window, IDisposable
{
    private Configuration Configuration;

    public ConfigWindow(Plugin plugin) : base(
        "Here's our upcoming events",
        ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoScrollbar |
        ImGuiWindowFlags.NoScrollWithMouse)
    {
        this.Size = new Vector2(232, 232);
        this.SizeCondition = ImGuiCond.Always;

        this.Configuration = plugin.Configuration;
    }

    public void Dispose() { }

    public override void Draw()
    {
        // can't ref a property, so use a local copy
        //var configValue = this.Configuration.SomePropertyToBeSavedAndWithADefault;
        // if (ImGui.Checkbox("Random Config Bool", ref configValue))
        // {
        //    this.Configuration.SomePropertyToBeSavedAndWithADefault = configValue;
        // can save immediately on change, if you don't want to provide a "Save and Close" button
        //    this.Configuration.Save();
        ImGui.Text("This months password is Spooktober");
        ImGui.Text("Give it to your bartender");
        ImGui.Text("   For a free drink");

        ImGui.Text("Upcoming Events");
        ImGui.Indent(10);
        ImGui.Text("October 26th Monster Bash");
        ImGui.Text("November 34th Monthly Auction");

    }
    
}
