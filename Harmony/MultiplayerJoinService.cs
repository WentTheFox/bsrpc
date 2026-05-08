using DataPuller.Data;
using HarmonyLib;
using HMUI;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace bsrpc.Harmony
{
    internal static class MultiplayerJoinService
    {
        internal static string _pendingCode;
        internal static string _pendingSource;
        internal static string _pendingMod;

        internal static void RequestJoin(string source, string code, string modName = null)
        {
            _pendingSource = source;
            _pendingCode = code;
            _pendingMod = modName;
            var modInfo = modName != null ? $", mod={modName}" : "";
            var isBsp = source == MultiplayerLobbySourceType.BeatSaberPlus_Multiplayer;

            if (!MapData.Instance.InLevel && !isBsp)
            {
                Plugin.Log.Info($"Join queued ({source}/{code}{modInfo}) — auto-navigating to Online screen");
                BsrpcController.Instance?.StartCoroutine(NavigateToOnlineCoroutine());
            }
            else
            {
                var hint = isBsp ? "navigate to Multiplayer+ to auto-join" : "navigate to Multiplayer to auto-join";
                Plugin.Log.Info($"Join queued ({source}/{code}{modInfo}) — {hint}");
            }
        }

        private static IEnumerator NavigateToOnlineCoroutine()
        {
            var mainFlow = Resources.FindObjectsOfTypeAll<MainFlowCoordinator>().FirstOrDefault();
            if (mainFlow == null)
            {
                Plugin.Log.Warn("Auto-navigate: MainFlowCoordinator not found");
                yield break;
            }

            var multiplayerFlow = AccessTools.Field(typeof(MainFlowCoordinator), "_multiplayerModeSelectionFlowCoordinator")
                ?.GetValue(mainFlow) as MultiplayerModeSelectionFlowCoordinator;
            if (multiplayerFlow == null)
            {
                Plugin.Log.Warn("Auto-navigate: multiplayer flow coordinator field not found");
                yield break;
            }

            var childField = AccessTools.Field(typeof(FlowCoordinator), "_childFlowCoordinator");
            var currentChild = childField?.GetValue(mainFlow) as FlowCoordinator;

            if (currentChild == (FlowCoordinator)multiplayerFlow)
            {
                Plugin.Log.Debug("Auto-navigate: already on Online screen");
                yield break;
            }

            if (currentChild != null)
            {
                mainFlow.DismissFlowCoordinator(currentChild, immediately: true);
                yield return null;
            }

            mainFlow.PresentFlowCoordinator(multiplayerFlow);
            Plugin.Log.Info("Auto-navigate: presented Online screen");
        }

        internal static void Clear()
        {
            _pendingSource = null;
            _pendingCode = null;
            _pendingMod = null;
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
