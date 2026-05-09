using DataPuller.Data;
using HarmonyLib;
using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace bsrpc.Harmony
{
    internal static class BspJoinService
    {
        private const string AssemblyName = "BeatSaberPlus_Multiplayer";
        private const string ViewTypeName = "BeatSaberPlus_Multiplayer.UI.MultiplayerPMainView";

        internal static void TryPatch(global::HarmonyLib.Harmony harmony)
        {
            try
            {
                var asm = AppDomain.CurrentDomain.GetAssemblies()
                    .FirstOrDefault(a => a.GetName().Name == AssemblyName);
                if (asm == null) return;

                var viewType = asm.GetType(ViewTypeName);
                if (viewType == null) return;

                var method = viewType.GetMethod("OnViewActivation",
                    BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
                if (method == null) return;

                harmony.Patch(method, postfix: new HarmonyMethod(
                    typeof(BspJoinService), nameof(OnViewActivation_Postfix)));

                Plugin.Log.Info("BSP join: patched MultiplayerPMainView.OnViewActivation");
            }
            catch (Exception ex)
            {
                Plugin.Log.Warn($"BSP join patch failed: {ex.Message}");
            }
        }

        private static void OnViewActivation_Postfix(object __instance)
        {
            var code = MultiplayerJoinService._pendingCode;
            var source = MultiplayerJoinService._pendingSource;
            if (string.IsNullOrEmpty(code) || source != MultiplayerLobbySourceType.BeatSaberPlus_Multiplayer)
                return;

            MultiplayerJoinService.Clear();

            try
            {
                var viewType = __instance.GetType();
                var flags = BindingFlags.NonPublic | BindingFlags.Instance;
                var joinMethod = viewType.GetMethod("Coroutine_JoinRoom", flags);
                var connectMethod = viewType.GetMethod("Coroutine_Connect", flags);

                if (joinMethod == null || connectMethod == null)
                {
                    Plugin.Log.Warn("BSP join: couldn't find coroutine methods on MultiplayerPMainView");
                    return;
                }

                var capturedCode = code;
                Func<IEnumerator> continuation = () => ((IEnumerator)joinMethod.Invoke(__instance, new object[] { capturedCode! }))!;
                var enumerator = ((IEnumerator)connectMethod.Invoke(__instance, new object[] { capturedCode!, false, continuation }))!;
                ((MonoBehaviour)__instance).StartCoroutine(enumerator);

                Plugin.Log.Info($"BSP join: started connection coroutine for code {capturedCode}");
            }
            catch (Exception ex)
            {
                Plugin.Log.Warn($"BSP join failed: {ex.Message}");
            }
        }
    }
}
