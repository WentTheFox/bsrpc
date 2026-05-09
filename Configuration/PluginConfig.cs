using IPA.Config.Stores;
using IPA.Config.Stores.Attributes;
using IPA.Config.Stores.Converters;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo(GeneratedStore.AssemblyVisibilityTarget)]

namespace bsrpc
{
    internal class PluginConfig
    {
        public event Action? OnReloaded;
        public event Action? OnChanged;

        public static PluginConfig Instance { get; set; } = null!;

        [NonNullable]
        public virtual long DiscordClientId { get; set; } = 1028340906740420711;

        [NonNullable]
        public virtual bool MultiplayerDetection { get; set; } = true;

        [NonNullable]
        public virtual bool MultiplayerPartyInfo { get; set; } = true;

        [NonNullable]
        public virtual bool MultiplayerLobbyJoining { get; set; } = true;

        [NonNullable]
        public virtual bool LargeImageSongCover { get; set; } = true;

        [NonNullable]
        public virtual bool ShowElapsedTimes { get; set; } = true;

        [NonNullable]
        public virtual bool ShowMapType { get; set; } = true;

        [NonNullable]
        public virtual bool UseEndTime { get; set; } = false;
        [NonNullable]
        public virtual bool TrackLastSceneSwitch { get; set; } = false;

        public class MapTypeValues
        {
            [NonNullable]
            public virtual string Standard { get; set; } = "Standard";
            [NonNullable]
            public virtual string OneSaber { get; set; } = "One Saber";
            [NonNullable]
            public virtual string NoArrows { get; set; } = "No Arrows";
            [NonNullable]
            public virtual string ThreeSixty { get; set; } = "360°";
            [NonNullable]
            public virtual string Ninety { get; set; } = "90°";
            public virtual string? Fallback { get; set; } = null;
        }

        [NonNullable]
        public virtual MapTypeValues MapType { get; set; } = new MapTypeValues();

        [UseConverter(typeof(ListConverter<string>))]
        public virtual List<string> DetailsFormat { get; set; } = new List<string>() { "{{songName}}", "{{songSubName}}", "by {{songAuthor}}", "[{{mapper}}]", "{{difficulty}}", "{{ranked}}", "{{qualified}}", "{{stars}}" };

        [NonNullable]
        public virtual string DetailsSeparator { get; set; } = " ";

        [UseConverter(typeof(ListConverter<string>))]
        public virtual List<string> StateFormat { get; set; } = new List<string>() { "{{playState}}", "{{remainingTimePlaying}} left", "for {{pausedSince}}", "{{modifiersState}}", "{{lobbyType}}", "{{score}}", "🔗{{combo}}", "{{fc}}", "❌{{misses}}", "{{accuracy}}%", "({{rank}})" };

        [NonNullable]
        public virtual string StateSeparator { get; set; } = " ";

        [UseConverter(typeof(ListConverter<string>))]
        public virtual List<string> LargeTextFormat { get; set; } = new List<string>() { "bsrpc v{{pluginVersion}}", "Beat Saber v{{gameVersion}}" };

        [NonNullable]
        public virtual string LargeTextSeparator { get; set; } = " / ";

        [UseConverter(typeof(ListConverter<string>))]
        public virtual List<string> SmallTextFormat { get; set; } = new List<string>() { "{{mapType}}", "!bsr {{bsr}}" };

        [NonNullable]
        public virtual string SmallTextSeparator { get; set; } = " / ";


        [NonNullable]
        public virtual string FullComboValue { get; set; } = "✅FC";
        [NonNullable]
        public virtual string RankedValue { get; set; } = "⭐";
        [NonNullable]
        public virtual string QualifiedValue { get; set; } = "✨";
        [NonNullable]
        public virtual string MainMenuValue { get; set; } = "Main Menu";
        [NonNullable]
        public virtual string MultiplayerLobbyValue { get; set; } = "Multiplayer Lobby";

        public class LobbyTypeEmojiValues
        {
            [NonNullable]
            public virtual string Singleplayer { get; set; } = "👤";
            [NonNullable]
            public virtual string Multiplayer { get; set; } = "👥";
            [NonNullable]
            public virtual string Practice { get; set; } = "🧪";
        }

        [NonNullable]
        public virtual LobbyTypeEmojiValues LobbyTypeEmoji { get; set; } = new LobbyTypeEmojiValues();

        public class PlayStateEmojiValues
        {
            [NonNullable]
            public virtual string Playing { get; set; } = "▶️";
            [NonNullable]
            public virtual string Paused { get; set; } = "⏸️";
            [NonNullable]
            public virtual string Failed { get; set; } = "🪦";
            [NonNullable]
            public virtual string Finished { get; set; } = "🎉";
            [NonNullable]
            public virtual string Loading { get; set; } = "⌛";
        }

        [NonNullable]
        public virtual PlayStateEmojiValues PlayStateEmoji { get; set; } = new PlayStateEmojiValues();
        public class ModifierEmojiValues
        {
            [NonNullable]
            public virtual string NoFail { get; internal set; } = "🦺";
            [NonNullable]
            public virtual string OneLife { get; internal set; } = "1🤍";
            [NonNullable]
            public virtual string FourLives { get; internal set; } = "4🤍";
            [NonNullable]
            public virtual string NoPrefix { get; internal set; } = "🚫";
            [NonNullable]
            public virtual string NoBombs { get; internal set; } = "💣";
            [NonNullable]
            public virtual string NoWalls { get; internal set; } = "🧱";
            [NonNullable]
            public virtual string NoArrows { get; internal set; } = "🔽";
            [NonNullable]
            public virtual string GhostNotes { get; internal set; } = "👻";
            [NonNullable]
            public virtual string DisappearingArrows { get; internal set; } = "🟦";
            [NonNullable]
            public virtual string SmallNotes { get; internal set; } = "🔹";
            [NonNullable]
            public virtual string ProMode { get; internal set; } = "⚔️";
            [NonNullable]
            public virtual string StrictAngles { get; internal set; } = "📐";
            [NonNullable]
            public virtual string ZenMode { get; internal set; } = "🧘";
            [NonNullable]
            public virtual string SlowerSong { get; internal set; } = "🐌";
            [NonNullable]
            public virtual string FasterSong { get; internal set; } = "⏩";
            [NonNullable]
            public virtual string SuperFastSong { get; internal set; } = "⏩";
        }

        [NonNullable]
        public virtual ModifierEmojiValues ModifierEmoji { get; set; } = new ModifierEmojiValues();

        public virtual void Changed()
        {
            OnChanged?.Invoke();
        }
        public virtual void OnReload()
        {
            OnReloaded?.Invoke();
        }
    }
}
