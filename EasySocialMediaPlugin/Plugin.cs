using Dalamud.Bindings.ImGui;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.Command;
using Dalamud.Game.Gui.ContextMenu;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using Dalamud.Utility;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Component.GUI;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace EasySocialMediaPlugin;

public sealed class Plugin : IDalamudPlugin
{
    private const string CommandName = "/social";
    private static readonly TimeSpan LookupTimeout = TimeSpan.FromSeconds(10);

    [PluginService] internal static ICommandManager CommandManager { get; private set; } = null!;
    [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] internal static IContextMenu ContextMenu { get; private set; } = null!;
    [PluginService] internal static IFramework Framework { get; private set; } = null!;
    [PluginService] internal static IGameGui GameGui { get; private set; } = null!;
    [PluginService] internal static IObjectTable ObjectTable { get; private set; } = null!;
    [PluginService] internal static ITargetManager TargetManager { get; private set; } = null!;
    [PluginService] internal static IChatGui ChatGui { get; private set; } = null!;
    [PluginService] internal static IPluginLog Log { get; private set; } = null!;

    private PendingLookup? pendingLookup;

    public Plugin()
    {
        CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
        {
            HelpMessage = "Open the targeted player's advertised F-list profile.",
        });

        ContextMenu.OnMenuOpened += OnMenuOpened;
        Framework.Update += OnFrameworkUpdate;
        PluginInterface.UiBuilder.OpenMainUi += ShowUsage;
        PluginInterface.UiBuilder.OpenConfigUi += ShowUsage;
        PluginInterface.UiBuilder.Draw += DrawProfileButtons;
    }

    public void Dispose()
    {
        PluginInterface.UiBuilder.Draw -= DrawProfileButtons;
        PluginInterface.UiBuilder.OpenConfigUi -= ShowUsage;
        PluginInterface.UiBuilder.OpenMainUi -= ShowUsage;
        Framework.Update -= OnFrameworkUpdate;
        ContextMenu.OnMenuOpened -= OnMenuOpened;
        CommandManager.RemoveHandler(CommandName);
        pendingLookup = null;
    }

    private void OnCommand(string command, string arguments)
    {
        if (TargetManager.Target is not IPlayerCharacter player)
        {
            ChatGui.PrintError("Easy Social Media: Target a nearby player first.");
            return;
        }

        BeginLookup(player.EntityId, player.Name.TextValue);
    }

    private void ShowUsage()
    {
        ChatGui.Print("Easy Social Media: Target a nearby player and use /social, or right-click them and choose Open F-list Profile.");
    }

    private void OnMenuOpened(IMenuOpenedArgs args)
    {
        if (args.MenuType != ContextMenuType.Default ||
            args.Target is not MenuTargetDefault target ||
            target.TargetObject is not IPlayerCharacter player)
        {
            return;
        }

        var entityId = player.EntityId;
        var playerName = player.Name.TextValue;

        args.AddMenuItem(new MenuItem
        {
            Name = "Open F-list Profile",
            PrefixChar = 'F',
            Priority = 50,
            OnClicked = _ => BeginLookup(entityId, playerName),
        });
    }

    private unsafe void BeginLookup(uint entityId, string playerName)
    {
        var inspectAgent = AgentInspect.Instance();
        if (inspectAgent == null)
        {
            ChatGui.PrintError("Easy Social Media: The character inspection service is unavailable.");
            return;
        }

        pendingLookup = new PendingLookup(entityId, playerName, DateTime.UtcNow);
        inspectAgent->ExamineCharacter(entityId);
        ChatGui.Print($"Easy Social Media: Checking {playerName}'s search comment...");
    }

    private unsafe void OnFrameworkUpdate(IFramework framework)
    {
        if (pendingLookup is not { } pending)
        {
            return;
        }

        if (DateTime.UtcNow - pending.StartedAt > LookupTimeout)
        {
            pendingLookup = null;
            ChatGui.PrintError($"Easy Social Media: Timed out reading {pending.PlayerName}'s search comment.");
            return;
        }

        var inspectAgent = AgentInspect.Instance();
        if (inspectAgent == null || inspectAgent->CurrentEntityId != pending.EntityId)
        {
            return;
        }

        if (inspectAgent->FetchSearchCommentStatus == 3)
        {
            pendingLookup = null;
            ChatGui.PrintError($"Easy Social Media: Could not read {pending.PlayerName}'s search comment.");
            return;
        }

        if (inspectAgent->FetchSearchCommentStatus != 2)
        {
            return;
        }

        var searchComment = inspectAgent->SearchComment.ToString();
        pendingLookup = null;

        if (!FListProfileResolver.TryResolve(searchComment, pending.PlayerName, out var url))
        {
            ChatGui.PrintError($"Easy Social Media: {pending.PlayerName}'s search comment does not contain /c/, /c, or c/<name>.");
            return;
        }

        OpenUrl(url);
    }

    private unsafe void DrawProfileButtons()
    {
        DrawAdventurePlateButton();
        DrawInspectButton();
    }

    private unsafe void DrawAdventurePlateButton()
    {
        var addon = GameGui.GetAddonByName<AtkUnitBase>("CharaCard");
        var agent = AgentCharaCard.Instance();
        if (!IsAddonVisible(addon) || agent == null || agent->Data == null)
        {
            return;
        }

        var playerName = agent->Data->Name.ToString();
        var searchComment = agent->Data->SearchComment.ToString();
        DrawLinkButtons(addon, "CharaCard", searchComment, playerName);
    }

    private unsafe void DrawInspectButton()
    {
        var addon = GameGui.GetAddonByName<AtkUnitBase>("CharacterInspect");
        var agent = AgentInspect.Instance();
        if (!IsAddonVisible(addon) || agent == null)
        {
            return;
        }

        var searchComment = agent->SearchComment.ToString();
        if (string.IsNullOrEmpty(searchComment))
        {
            return;
        }

        if (ObjectTable.SearchByEntityId(agent->CurrentEntityId) is not IPlayerCharacter player)
        {
            return;
        }

        DrawLinkButtons(addon, "CharacterInspect", searchComment, player.Name.TextValue);
    }

    private unsafe void DrawLinkButtons(AtkUnitBase* addon, string id, string searchComment, string playerName)
    {
        var links = new List<(string Label, string Url)>();

        if (FListProfileResolver.TryResolve(searchComment, playerName, out var flistUrl))
        {
            links.Add(("F-list ↗", flistUrl));
        }

        if (TwitterResolver.TryResolve(searchComment, out var twitterUrl))
        {
            links.Add(("X ↗", twitterUrl));
        }

        if (links.Count > 0)
        {
            DrawAnchoredButtons(addon, id, links);
        }
    }

    private static unsafe bool IsAddonVisible(AtkUnitBase* addon) =>
        addon != null && addon->IsReady && addon->IsVisible;

    private unsafe void DrawAnchoredButtons(AtkUnitBase* addon, string id, List<(string Label, string Url)> links)
    {
        const float estimatedWidth = 78f;
        const float gap = 6f;

        var x = addon->X + addon->GetScaledWidth(true) + gap;
        var y = addon->Y + 36f * addon->GetScale();
        if (x + estimatedWidth > ImGui.GetIO().DisplaySize.X)
        {
            x = addon->X - estimatedWidth - gap;
        }

        ImGui.SetNextWindowPos(new Vector2(x, y), ImGuiCond.Always);
        ImGui.SetNextWindowBgAlpha(0.92f);

        const ImGuiWindowFlags flags =
            ImGuiWindowFlags.NoDecoration |
            ImGuiWindowFlags.AlwaysAutoResize |
            ImGuiWindowFlags.NoSavedSettings |
            ImGuiWindowFlags.NoFocusOnAppearing |
            ImGuiWindowFlags.NoNav;

        if (ImGui.Begin($"##EasySocialMediaPlugin-{id}", flags))
        {
            foreach (var (label, url) in links)
            {
                if (ImGui.Button(label))
                {
                    OpenUrl(url);
                }
            }
        }

        ImGui.End();
    }

    private void OpenUrl(string url)
    {
        try
        {
            ChatGui.Print($"Easy Social Media: Opening {url}");
            Util.OpenLink(url);
        }
        catch (Exception exception)
        {
            Log.Error(exception, "Failed to open social media URL {Url}", url);
            ChatGui.PrintError("Easy Social Media: Windows could not open the profile URL.");
        }
    }

    private sealed record PendingLookup(uint EntityId, string PlayerName, DateTime StartedAt);
}
