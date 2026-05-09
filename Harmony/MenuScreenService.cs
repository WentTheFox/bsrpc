using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using HarmonyLib;

namespace bsrpc.Harmony
{
    internal static class MenuScreenService
    {
        private static readonly Dictionary<string, string?> KnownScreens = new Dictionary<string, string?>
        {
            { "MainFlowCoordinator", null },
            { "SoloFreePlayFlowCoordinator", "Solo" },
            { "CampaignFlowCoordinator", "Campaign" },
            { "PartyFreePlayFlowCoordinator", "Party" },
            { "MultiplayerModeSelectionFlowCoordinator", "Online" },
            { "SettingsFlowCoordinator", "Settings" },
            { "BeatAvatarEditorFlowCoordinator", "Avatar" },
            { "BSSFlowCoordinator", "Better Song Search" },
            { "Coordinator", "Camera2" },
            { "XSettingsFlowCoordinator", "Enhancements" }
        };

        internal static Action? OnScreenChanged;

        internal static string? CurrentScreen { get; private set; }

        internal static void Patch(HarmonyLib.Harmony harmony)
        {
            try
            {
                Type? flowCoordinatorType = null;
                foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
                {
                    foreach (var t in asm.GetTypes())
                        if (t.Name == "FlowCoordinator" && t.IsClass)
                        {
                            flowCoordinatorType = t;
                            break;
                        }

                    if (flowCoordinatorType != null) break;
                }

                if (flowCoordinatorType == null)
                {
                    Plugin.Log.Warn("MenuScreen: FlowCoordinator type not found");
                    return;
                }

                var presentPostfix = new HarmonyMethod(typeof(MenuScreenService), nameof(PresentFlowCoordinator_Postfix));
                var dismissPostfix = new HarmonyMethod(typeof(MenuScreenService), nameof(DismissFlowCoordinator_Postfix));
                var patched = 0;

                foreach (var method in flowCoordinatorType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                {
                    var parameters = method.GetParameters();
                    if (parameters.Length == 0 || !flowCoordinatorType.IsAssignableFrom(parameters[0].ParameterType)) continue;

                    if (method.Name == "PresentFlowCoordinator")
                        try
                        {
                            harmony.Patch(method, postfix: presentPostfix);
                            patched++;
                        }
                        catch (Exception ex)
                        {
                            Plugin.Log.Debug($"MenuScreen: skip PresentFlowCoordinator: {ex.Message}");
                        }
                    else if (method.Name == "DismissFlowCoordinator")
                        try
                        {
                            harmony.Patch(method, postfix: dismissPostfix);
                            patched++;
                        }
                        catch (Exception ex)
                        {
                            Plugin.Log.Debug($"MenuScreen: skip DismissFlowCoordinator: {ex.Message}");
                        }
                }

                Plugin.Log.Info($"MenuScreen: patched {patched} methods");
            }
            catch (Exception ex)
            {
                Plugin.Log.Warn($"MenuScreen patch failed: {ex.Message}");
            }
        }

        private static void UpdateScreen(string typeName)
        {
            string? newScreen;
            if (KnownScreens.TryGetValue(typeName, out var screenName))
            {
                newScreen = screenName;
            }
            else
            {
                var stripped = typeName.Replace("FlowCoordinator", string.Empty);
                if (stripped.EndsWith("Settings") && stripped.Length > "Settings".Length)
                    stripped = stripped.Substring(0, stripped.Length - "Settings".Length);
                newScreen = Regex.Replace(stripped, @"(?<=[a-z])(?=[A-Z])|(?<=[A-Z])(?=[A-Z][a-z])", " ").Trim();
            }

            if (newScreen != null && newScreen.Length < 2) newScreen = typeName;
            if (newScreen == CurrentScreen) return;
            CurrentScreen = newScreen;
            OnScreenChanged?.Invoke();
        }

        // Called when MainFlowCoordinator presents a child — update to that screen.
        private static void PresentFlowCoordinator_Postfix(object __instance, object __0)
        {
            if (__instance?.GetType().Name != "MainFlowCoordinator") return;
            var typeName = __0?.GetType().Name;
            if (typeName == null) return;
            UpdateScreen(typeName);
        }

        // Called when MainFlowCoordinator dismisses a child — we've returned to the main menu.
        private static void DismissFlowCoordinator_Postfix(object __instance)
        {
            if (__instance?.GetType().Name != "MainFlowCoordinator") return;
            UpdateScreen("MainFlowCoordinator");
        }

        internal static string GetCurrentScreenDisplay()
        {
            return CurrentScreen ?? PluginConfig.Instance.MainMenuValue;
        }
    }
}