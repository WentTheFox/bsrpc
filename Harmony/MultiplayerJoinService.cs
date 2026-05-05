using DataPuller.Data;
using HarmonyLib;

namespace bsrpc.Harmony
{
    internal static class MultiplayerJoinService
    {
        internal static string _pendingCode;
        internal static string _pendingSource;

        internal static void RequestJoin(string source, string code)
        {
            _pendingSource = source;
            _pendingCode = code;
            var hint = source == MultiplayerLobbySourceType.BeatSaberPlus_Multiplayer
                ? "navigate to Multiplayer+ to auto-join"
                : "navigate to Multiplayer to auto-join";
            Plugin.Log.Info($"Join queued ({source}/{code}) — {hint}");
        }

        internal static void Clear()
        {
            _pendingSource = null;
            _pendingCode = null;
        }
    }

    [HarmonyPatch(typeof(MultiplayerModeSelectionFlowCoordinator), "ProcessDeeplinkingToLobby")]
    internal class ProcessDeeplinkingToLobby_Patch
    {
        static void Prefix(MultiplayerModeSelectionFlowCoordinator __instance)
        {
            if (string.IsNullOrEmpty(MultiplayerJoinService._pendingCode)) return;
            if (MultiplayerJoinService._pendingSource == MultiplayerLobbySourceType.BeatSaberPlus_Multiplayer) return;

            var destination = new SelectMultiplayerLobbyDestination(MultiplayerJoinService._pendingCode);
            AccessTools.Field(typeof(MultiplayerModeSelectionFlowCoordinator), "_lobbyDestination")
                .SetValue(__instance, destination);
            MultiplayerJoinService.Clear();
        }
    }
}
