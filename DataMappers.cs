 using DataPuller.Data;
using Discord;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace bsrpc
{
    internal class DataMappers
    {
        internal const int SECONDS_IN_HOUR = 60 * 60;

        internal static string FormatLine(List<string> inputList, string joinChar)
        {
            List<string> outputList = new List<string>();
            if (inputList != null)
            {
                foreach (string input in inputList)
                {
                    var replacedValues = new List<string>();
                    string formattedString = Regex.Replace(input, @"{{(.*?)}}", match =>
                    {
                        string key = match.Groups[1].Value;
                        var value = GetValueForFormat(key);
                        replacedValues.Add(value);
                        return value;
                    });

                    if (!replacedValues.Exists(value => string.IsNullOrEmpty(value)))
                    {
                        // Add to output if none of the replaced values are empty
                        outputList.Add(formattedString);
                    }
                }
            }

            return string.Join(joinChar, outputList);
        }

        internal static string GetValueForFormat(string key)
        {
            switch (key)
            {
                case "songName": return MapData.Instance.SongName;
                case "songSubName": return MapData.Instance.SongSubName;
                case "songAuthor": return MapData.Instance.SongAuthor;
                case "mapper": return MapData.Instance.Mapper;
                case "mappers": return MapData.Instance.Mappers.Count > 0 ? string.Join(", ", MapData.Instance.Mappers) : "";
                case "lighters": return MapData.Instance.Lighters.Count > 0 ? string.Join(", ", MapData.Instance.Lighters) : "";
                case "difficulty": return GetReadableDifficulty(MapData.Instance.Difficulty);
                case "customDifficulty": return MapData.Instance.CustomDifficultyLabel;
                case "stars": return MapData.Instance.RankedState.ScoresaberStars > 0 ? $"{MapData.Instance.RankedState.ScoresaberStars:N2}" : "";
                case "blstars": return MapData.Instance.RankedState.BeatleaderStars > 0 ? $"{MapData.Instance.RankedState.BeatleaderStars:N2}" : "";
                case "pp": return MapData.Instance.PP > 0 ? $"{MapData.Instance.PP:N2}" : "";
                case "playState": return GetPlayState();
                case "modifiersState": return GetModifiersState();
                case "lobbyType": return GetLobbyType();
                case "mapType": return GetMapType();
                case "score": return LiveData.Instance.Score > 0 ? $"{LiveData.Instance.Score}" : "";
                case "topScore": return MapData.Instance.PreviousRecord > 0 ? $"{MapData.Instance.PreviousRecord}" : "";
                case "combo": return LiveData.Instance.Combo > 0 ? $"{LiveData.Instance.Combo:N0}" : "";
                case "misses": return LiveData.Instance.Misses > 0 ? $"{LiveData.Instance.Misses:N0}" : "";
                case "energy": return $"{LiveData.Instance.PlayerHealth:N0}";
                case "bpm": return MapData.Instance.BPM > 0 ? $"{MapData.Instance.BPM:N0}" : "";
                case "njs": return MapData.Instance.NJS > 0 ? $"{MapData.Instance.NJS:F2}" : "";
                case "accuracy": return $"{LiveData.Instance.Accuracy:F2}";
                case "rank": return GetReadableRank(LiveData.Instance.Rank);
                case "bsr": return MapData.Instance.BSRKey;
                case "fc": return LiveData.Instance.FullCombo ? PluginConfig.Instance.FullComboValue : "";
                case "ranked": return MapData.Instance.RankedState.Ranked ? PluginConfig.Instance.RankedValue : "";
                case "qualified": return MapData.Instance.RankedState.Qualified ? PluginConfig.Instance.QualifiedValue : "";
                case "gameVersion": return Regex.Replace(MapData.GameVersion, @"_\d+$", "");
                case "pluginVersion": return Plugin.PluginVersion;
                case "remainingTime": return GetTimeLeft();
                case "elapsedTime": return FormatSeconds(LiveData.Instance.TimeElapsed);
                case "totalTime": return FormatSeconds(MapData.Instance.Duration);
                case "remainingTimePlaying": return !MapData.Instance.LevelPaused ? GetTimeLeft() : "";
                case "pausedSince": return MapData.Instance.LevelPaused ? GetPausedSince() : "";
                default: return $"{{{key}}}";
            }
        }

        internal static string FormatSeconds(int seconds) {
            var ts = TimeSpan.FromSeconds(seconds);
            if (seconds > SECONDS_IN_HOUR)
            {
                return ts.ToString(@"h\:mm\:ss");
            }
            return ts.ToString(@"m\:ss");
        }
        internal static string FormatMilliseconds(long milliseconds) {
            return FormatSeconds(Convert.ToInt32(milliseconds / 1000));
        }

        internal static string GetTimeLeft() {
            return FormatSeconds(MapData.Instance.Duration - LiveData.Instance.TimeElapsed);
        }

        internal static string GetPausedSince()
        {
            if (MapData.Instance.LevelPaused)
            {
                if (Plugin.LastPauseDateTime.Value != null)
                {
                    var secondsSincePause = Math.Max(0, DateTimeToUnixTimestampSeconds(DateTime.Now) - DateTimeToUnixTimestampSeconds(Plugin.LastPauseDateTime.Value));
                    return FormatSeconds(Convert.ToInt32(secondsSincePause));
                }
            }
            return "";
        }

        internal static bool IsMultiplayer()
        {
            return PluginConfig.Instance.MultiplayerDetection &&
                MapData.Instance.IsMultiplayer &&
                MapData.Instance.MultiplayerLobbyMaxSize > 0 &&
                MapData.Instance.MultiplayerLobbyCurrentSize > 0;
        }

        internal static string GetLobbyType()
        {
            if (IsMultiplayer())
            {
                return PluginConfig.Instance.LobbyTypeEmoji.Multiplayer;
            }
            if (MapData.Instance.PracticeMode)
            {
                return PluginConfig.Instance.LobbyTypeEmoji.Practice;
            }
            return PluginConfig.Instance.LobbyTypeEmoji.Singleplayer;
        }

        internal static long DateTimeToUnixTimestampMs(DateTime ticks)
        {
            return ((DateTimeOffset)ticks).ToUnixTimeMilliseconds();
        }
        internal static long DateTimeToUnixTimestampSeconds(DateTime ticks)
        {
            return ((DateTimeOffset)ticks).ToUnixTimeSeconds();
        }

        internal static string GetPlayState()
        {
            if (MapData.Instance.LevelPaused)
            {
                return PluginConfig.Instance.PlayStateEmoji.Paused;
            }

            if (MapData.Instance.LevelFailed)
            {
                return PluginConfig.Instance.PlayStateEmoji.Failed;
            }

            if (MapData.Instance.LevelFinished)
            {
                return PluginConfig.Instance.PlayStateEmoji.Failed;
            }

            if (LiveData.Instance.TimeElapsed > 0)
            {
                return PluginConfig.Instance.PlayStateEmoji.Playing;
            }

            return PluginConfig.Instance.PlayStateEmoji.Loading;
        }

        internal static string GetReadableRank(string originalRank)
        {
            switch (originalRank)
            {
                case "SSS":
                    // Match in-game rank shown during zen mode
                    return "E";
                default:
                    return originalRank;
            }
        }

        internal static string GetReadableDifficulty(string originalDifficulty)
        {
            switch (originalDifficulty)
            {
                case "ExpertPlus":
                    return "Expert+";
                default:
                    return originalDifficulty;
            }
        }

        internal static string GetModifiersState()
        {
            var modifiersString = new StringBuilder();
            if (LiveData.Instance.TimeElapsed > 0)
            {
                if (MapData.Instance.Modifiers.ZenMode)
                {
                    modifiersString.Append(PluginConfig.Instance.ModifierEmoji.ZenMode);
                }
                else
                {
                    // Life modifiers
                    if (MapData.Instance.Modifiers.NoFailOn0Energy || MapData.Instance.Modifiers.OneLife ||
                        MapData.Instance.Modifiers.FourLives)
                    {
                        if (MapData.Instance.Modifiers.NoFailOn0Energy)
                        {
                            modifiersString.Append(PluginConfig.Instance.ModifierEmoji.NoFail);
                        }
                        else if (MapData.Instance.Modifiers.OneLife)
                        {
                            modifiersString.Append(PluginConfig.Instance.ModifierEmoji.OneLife);
                        }
                        else if (MapData.Instance.Modifiers.FourLives)
                        {
                            modifiersString.Append(PluginConfig.Instance.ModifierEmoji.FourLives);
                        }

                        modifiersString.Append(" ");
                    }

                    // Difficulty decreasing modifiers
                    if (MapData.Instance.Modifiers.NoBombs || MapData.Instance.Modifiers.NoWalls ||
                        MapData.Instance.Modifiers.NoArrows)
                    {
                        modifiersString.Append(PluginConfig.Instance.ModifierEmoji.NoPrefix);
                        if (MapData.Instance.Modifiers.NoBombs)
                        {
                            modifiersString.Append(PluginConfig.Instance.ModifierEmoji.NoBombs);
                        }

                        if (MapData.Instance.Modifiers.NoWalls)
                        {
                            modifiersString.Append(PluginConfig.Instance.ModifierEmoji.NoWalls);
                        }

                        if (MapData.Instance.Modifiers.NoArrows)
                        {
                            modifiersString.Append(PluginConfig.Instance.ModifierEmoji.NoArrows);
                        }

                        modifiersString.Append(" ");
                    }

                    // Difficulty increasing modifiers
                    if (MapData.Instance.Modifiers.GhostNotes || MapData.Instance.Modifiers.DisappearingArrows)
                    {
                        if (MapData.Instance.Modifiers.GhostNotes)
                        {
                            modifiersString.Append(PluginConfig.Instance.ModifierEmoji.GhostNotes);
                        }
                        else if (MapData.Instance.Modifiers.DisappearingArrows)
                        {
                            modifiersString.Append(PluginConfig.Instance.ModifierEmoji.DisappearingArrows);
                        }

                        modifiersString.Append(" ");
                    }

                    if (MapData.Instance.Modifiers.SmallNotes || MapData.Instance.Modifiers.ProMode ||
                        MapData.Instance.Modifiers.StrictAngles)
                    {
                        if (MapData.Instance.Modifiers.SmallNotes)
                        {
                            modifiersString.Append(PluginConfig.Instance.ModifierEmoji.SmallNotes);
                        }

                        if (MapData.Instance.Modifiers.ProMode)
                        {
                            modifiersString.Append(PluginConfig.Instance.ModifierEmoji.ProMode);
                        }

                        if (MapData.Instance.Modifiers.StrictAngles)
                        {
                            modifiersString.Append(PluginConfig.Instance.ModifierEmoji.StrictAngles);
                        }

                        modifiersString.Append(" ");
                    }
                }

                // Speed modifiers
                if (MapData.Instance.Modifiers.SlowerSong)
                {
                    modifiersString.Append(PluginConfig.Instance.ModifierEmoji.SlowerSong);
                }
                else if (MapData.Instance.Modifiers.FasterSong)
                {
                    modifiersString.Append(PluginConfig.Instance.ModifierEmoji.FasterSong);
                }
                else if (MapData.Instance.Modifiers.SuperFastSong)
                {
                    modifiersString.Append(PluginConfig.Instance.ModifierEmoji.SuperFastSong);
                }
            }

            return modifiersString.ToString().Trim();
        }

        internal static double GetDurationScaleFactor()
        {
            if (MapData.Instance.Modifiers.SuperFastSong)
            {
                return 1.5;
            }
            if (MapData.Instance.Modifiers.FasterSong)
            {
                return 1.2;
            }
            return 1;
        }

        internal static Activity GetActivityData()
        {
            var activity = new Activity();
            activity.Assets = GetActivityAssets();
            if (MapData.Instance.InLevel)
            {
                activity.Details = FormatLine(PluginConfig.Instance.DetailsFormat, PluginConfig.Instance.DetailsSeparator);
                activity.State = FormatLine(PluginConfig.Instance.StateFormat, PluginConfig.Instance.StateSeparator);

                if (LiveData.Instance.TimeElapsed > 0 && !MapData.Instance.LevelPaused)
                {
                    var elapsedTimeOffset = GetRealtimeElapsedTime();
                    if (PluginConfig.Instance.UseEndTime)
                    {
                        var totalTimeOffset = GetRealtimeTotalTime();
                        activity.Timestamps.End = DateTimeToUnixTimestampMs(DateTime.Now.AddSeconds(totalTimeOffset - elapsedTimeOffset));
                    } else
                    {
                        activity.Timestamps.Start = DateTimeToUnixTimestampMs(DateTime.Now.AddSeconds(-elapsedTimeOffset));
                    }
                }

            }
            else
            {
                if (IsMultiplayer())
                {
                    activity.State = PluginConfig.Instance.MultiplayerLobbyValue;
                    if (PluginConfig.Instance.MultiplayerPartyInfo)
                    {
                        activity.Party.Size = new ActivityPartySize();
                        activity.Party.Size.MaxSize = MapData.Instance.MultiplayerLobbyMaxSize;
                        activity.Party.Size.CurrentSize = MapData.Instance.MultiplayerLobbyCurrentSize;

                        var joinCode = MapData.Instance.MultiplayerLobbyJoinCode;
                        var source = MapData.Instance.MultiplayerLobbySource;
                        if (!string.IsNullOrEmpty(joinCode) && !string.IsNullOrEmpty(source))
                        {
                            activity.Party.Id = joinCode;
                            var modName = MapData.Instance.MultiplayerCoreLobbyMod;
                            activity.Secrets.Join = string.IsNullOrEmpty(modName)
                                ? $"bsrpc://{source}/{joinCode}"
                                : $"bsrpc://{source}/{modName}/{joinCode}";
                        }
                    }
                }
                else
                {
                    activity.State = Harmony.MenuScreenService.GetCurrentScreenDisplay();
                }
            }

            return activity;
        }

        internal static double GetRealtimeElapsedTime()
        {
            return LiveData.Instance.TimeElapsed / GetDurationScaleFactor();
        }

        internal static double GetRealtimeTotalTime()
        {
            return MapData.Instance.Duration / GetDurationScaleFactor();
        }

        internal enum Scenes
        {
            Unknown,
            MainMenu,
            MultiplayerLobby,
            Playing,
            Paused,
        }

        internal static Scenes GetCurrentScene()
        {
            if (MapData.Instance.InLevel)
            {
                return MapData.Instance.LevelPaused ? Scenes.Paused : Scenes.Playing;

            }

            if (IsMultiplayer())
            {
                return Scenes.MultiplayerLobby;
            }

            return Scenes.MainMenu;
        }

        internal static string GetMapType()
        {
            switch (MapData.Instance.MapType)
            {
                case "Standard": return PluginConfig.Instance.MapType.Standard;
                case "OneSaber": return PluginConfig.Instance.MapType.OneSaber;
                case "NoArrows": return PluginConfig.Instance.MapType.NoArrows;
                case "360Degree": return PluginConfig.Instance.MapType.ThreeSixty;
                case "90Degree": return PluginConfig.Instance.MapType.Ninety;
                default:
                    return PluginConfig.Instance.MapType.Fallback != null ? PluginConfig.Instance.MapType.Fallback : MapData.Instance.MapType;
            }
        }

        internal static string GetSmallImage()
        {
            switch (MapData.Instance.MapType)
            {
                case "Standard": return RichPresenceAssetKeys.Standard;
                case "OneSaber": return RichPresenceAssetKeys.OneSaber;
                case "NoArrows": return RichPresenceAssetKeys.NoArrows;
                case "360Degree": return RichPresenceAssetKeys.ThreeSixty;
                case "90Degree": return RichPresenceAssetKeys.Ninety;
                default: return RichPresenceAssetKeys.BlankMapType;
            }
        }
        internal static string GetLargeImage()
        {
            if (PluginConfig.Instance.LargeImageSongCover)
            {
                var coverImageUrl = MapData.Instance.CoverImage;
                if (coverImageUrl != null && coverImageUrl.Length > 0 && coverImageUrl.StartsWith("https"))
                {
                    return MapData.Instance.CoverImage;
                }
            }

            return GetEnvironmentImage();
        }

        internal static ActivityAssets GetActivityAssets()
        {
            var assets = new ActivityAssets();
            if (MapData.Instance.InLevel)
            {
                assets.LargeImage = GetLargeImage();

                if (PluginConfig.Instance.ShowMapType)
                {
                    assets.SmallImage = GetSmallImage();
                    assets.SmallText = FormatLine(PluginConfig.Instance.SmallTextFormat, PluginConfig.Instance.SmallTextSeparator);
                }
            }
            else
            {
                if (IsMultiplayer())
                {
                    assets.LargeImage = RichPresenceAssetKeys.MultiplayerLobby;
                }
                else
                {
                    assets.LargeImage = RichPresenceAssetKeys.MainMenu;
                }
            }

            if (assets.LargeImage != null)
            {
                assets.LargeText = FormatLine(PluginConfig.Instance.LargeTextFormat, PluginConfig.Instance.LargeTextSeparator);
            }

            return assets;
        }

        internal static string GetEnvironmentImage()
        {
            switch (MapData.Instance.Environment)
            {
                case "TheWeekndEnvironment":
                    return RichPresenceAssetKeys.TheWeekend;
                case "LizzoEnvironment":
                    return RichPresenceAssetKeys.Lizzo;
                case "TheSecondEnvironment":
                    return RichPresenceAssetKeys.TheSecond;
                case "EDMEnvironment":
                    return RichPresenceAssetKeys.Edm;
                case "PyroEnvironment":
                    return RichPresenceAssetKeys.FallOutBoy;
                case "WeaveEnvironment":
                    return RichPresenceAssetKeys.Weave;
                case "GagaEnvironment":
                    return RichPresenceAssetKeys.Gaga;
                case "HalloweenEnvironment":
                    return RichPresenceAssetKeys.Spooky;
                case "BillieEnvironment":
                    return RichPresenceAssetKeys.Billie;
                case "SkrillexEnvironment":
                    return RichPresenceAssetKeys.Skrillex;
                case "InterscopeEnvironment":
                    return RichPresenceAssetKeys.Interscope;
                case "KaleidoscopeEnvironment":
                    return RichPresenceAssetKeys.Kaleidoscope;
                case "BTSEnvironment":
                    return RichPresenceAssetKeys.Bts;
                case "LinkinParkEnvironment":
                    return RichPresenceAssetKeys.LinkinPark;
                case "FitBeatEnvironment":
                    return RichPresenceAssetKeys.FitBeat;
                case "TimbalandEnvironment":
                    return RichPresenceAssetKeys.Timbaland;
                case "GreenDayEnvironment":
                    return RichPresenceAssetKeys.GreenDay;
                case "GreenDayGrenadeEnvironment":
                    return RichPresenceAssetKeys.GreenDayGrenade;
                case "RocketEnvironment":
                    return RichPresenceAssetKeys.Rocket;
                case "PanicEnvironment":
                    return RichPresenceAssetKeys.Panic;
                case "CrabRaveEnvironment":
                    return RichPresenceAssetKeys.CrabRave;
                case "MonstercatEnvironment":
                    return RichPresenceAssetKeys.Monstercat;
                case "KDAEnvironment":
                    return RichPresenceAssetKeys.Kda;
                case "DragonsEnvironment":
                    return RichPresenceAssetKeys.Dragons;
                case "BigMirrorEnvironment":
                    return RichPresenceAssetKeys.BigMirror;
                case "NiceEnvironment":
                    return RichPresenceAssetKeys.Nice;
                case "TriangleEnvironment":
                    return RichPresenceAssetKeys.Triangle;
                case "OriginsEnvironment":
                    return RichPresenceAssetKeys.Origins;
                case "LatticeEnvironment":
                    return RichPresenceAssetKeys.Lattice;
                case "DefaultEnvironment":
                default:
                    return RichPresenceAssetKeys.TheFirst;
            }
        }
    }
}
