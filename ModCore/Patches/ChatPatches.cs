using HarmonyLib;
using ProjectM;
using ProjectM.Network;
using ModCore.Services;
using System;
using static ModCore.Frameworks.CommandFramework.CommandFramework;
using ModCore.Events;
using VampireCommandFramework;

namespace ModCore.Patches;

/// <summary>
/// Prefix on VCF's CommandRegistry.Handle.
/// If the command belongs to our ETSMods framework, execute it and return
/// CommandResult.Success so VCF skips its "command not found" message.
/// If it's not ours, let VCF handle it normally.
/// Special case: .help — let VCF run first (Postfix appends our list).
/// </summary>
[HarmonyPatch(typeof(CommandRegistry), nameof(CommandRegistry.Handle))]
public static class VcfCommandRegistryPatch
{
    public static bool Prefix(ICommandContext ctx, string input, ref CommandResult __result)
    {
        if (ctx is not ChatCommandContext chatCtx) return true;

        bool isHelp = input.TrimStart('.').Equals("help", StringComparison.OrdinalIgnoreCase);
        if (isHelp) return true; // let VCF handle .help first, Postfix appends ours

        // Check if our framework has this command (without executing)
        var stripped = input.StartsWith(".") ? input.Substring(1) : input;
        var (matchedCommand, _) = CommandHandler.FindMatchingCommand(stripped);
        if (matchedCommand == null) return true; // not our command, let VCF handle

        // Our command — execute it and tell VCF it was handled
        try
        {
            var player = PlayerService.GetPlayerFromUser(chatCtx.Event.SenderUserEntity);
            CommandHandler.ExecuteCommand(player, input);
        }
        catch (Exception e)
        {
            Plugin.PluginLog.LogInfo($"[VcfPatch] Exception: {e}");
        }

        __result = CommandResult.Success;
        return false; // skip VCF's Handle entirely (no "command not found")
    }

    public static void Postfix(ICommandContext ctx, string input, CommandResult __result)
    {
        // After VCF handles .help, append our ETSMods command list
        bool isHelp = input.TrimStart('.').Equals("help", StringComparison.OrdinalIgnoreCase);
        if (!isHelp) return;
        if (ctx is not ChatCommandContext chatCtx) return;

        try
        {
            var player = PlayerService.GetPlayerFromUser(chatCtx.Event.SenderUserEntity);
            CommandHandler.ExecuteCommand(player, input);
        }
        catch (Exception e)
        {
            Plugin.PluginLog.LogInfo($"[VcfPatch] .help append exception: {e}");
        }
    }
}

/// <summary>
/// Forwards OnPlayerChatMessage event for mods that subscribe to it.
/// VCF handles the entity lifecycle; we just broadcast the event here.
/// </summary>
[HarmonyPatch(typeof(ChatMessageSystem), nameof(ChatMessageSystem.OnUpdate))]
public static class ChatMessageSystemPatch
{
    public static void Prefix(ChatMessageSystem __instance)
    {
        try
        {
            var entities = __instance.__query_661171423_0.ToEntityArray(Unity.Collections.Allocator.Temp);
            if (!entities.IsCreated) return;

            foreach (var entity in entities)
            {
                try
                {
                    var chatMessageEvent = __instance.EntityManager.GetComponentData<ChatMessageEvent>(entity);
                    var fromCharacter = __instance.EntityManager.GetComponentData<FromCharacter>(entity);
                    var player = PlayerService.GetPlayerFromUser(fromCharacter.User);
                    GameEvents.OnPlayerChatMessage?.Invoke(player, entity, chatMessageEvent);
                }
                catch { }
            }

            entities.Dispose();
        }
        catch { }
    }
}
