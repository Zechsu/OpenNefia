﻿using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Config;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Game;
using OpenNefia.Core.ResourceManagement;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.Utility;
using OpenNefia.Analyzers;

namespace OpenNefia.Core.Audio
{
    public class LoveAudioSystem : EntitySystem, IAudioSystem
    {
        [Dependency] private readonly IResourceCache _resourceCache = default!;
        [Dependency] private readonly ICoords _coords = default!;

        private class PlayingSource
        {
            public Love.Source Source;

            public PlayingSource(Love.Source source)
            {
                Source = source;
            }
        }

        private List<PlayingSource> _playingSources = new();

        public override void Initialize()
        {
            SubscribeLocalEvent<PlayerComponent, EntityPositionChangedEvent>(OnPlayerPositionChanged, nameof(OnPlayerPositionChanged));
        }

        private Love.Source GetLoveSource(PrototypeId<SoundPrototype> prototype)
        {
            var fileData = _resourceCache.GetResource<LoveFileDataResource>(prototype.ResolvePrototype().Filepath);
            return Love.Audio.NewSource(fileData, Love.SourceType.Static);
        }

        /// <inheritdoc />
        public void Play(PrototypeId<SoundPrototype> prototype, AudioParams? audioParams = null)
        {
            if (!ConfigVars.EnableSound)
                return;

            var source = GetLoveSource(prototype);

            if (source.GetChannelCount() == 1)
            {
                source.SetRelative(true);
                source.SetAttenuationDistances(0, 0);
            }

            source.SetVolume(Math.Clamp(audioParams?.Volume ?? 1f, 0f, 1f));

            Love.Audio.Play(source);

            _playingSources.Add(new PlayingSource(source));
        }

        /// <inheritdoc />
        public void Play(PrototypeId<SoundPrototype> prototype, EntityUid entity, AudioParams? audioParams = null)
        {
            if (EntityManager.TryGetComponent<SpatialComponent>(entity, out var spatial))
                Play(prototype, spatial.MapPosition, audioParams);
            else
                Play(prototype);
        }

        /// <inheritdoc />
        public void Play(PrototypeId<SoundPrototype> prototype, MapCoordinates coordinates, AudioParams? audioParams = null)
        {
            if (coordinates.MapId != GameSession.ActiveMap?.Id)
                return;

            var screenPosition = _coords.TileToScreen(coordinates.Position);

            Play(prototype, screenPosition, audioParams);
        }

        /// <inheritdoc />
        public void Play(PrototypeId<SoundPrototype> prototype, Vector2i screenPosition, AudioParams? audioParams = null)
        {
            if (!ConfigVars.EnableSound)
                return;

            var source = GetLoveSource(prototype);

            if (source.GetChannelCount() == 1)
            {
                source.SetRelative(false);
                source.SetPosition(screenPosition.X, screenPosition.Y, 0f);
                source.SetAttenuationDistances(100, 500);
            }

            source.SetVolume(Math.Clamp(audioParams?.Volume ?? 1f, 0f, 1f));

            Love.Audio.Play(source);

            _playingSources.Add(new PlayingSource(source));
        }

        // TODO: when implementing scrolling, run this every frame to account for sub-tile positioning.
        private void OnPlayerPositionChanged(EntityUid uid, PlayerComponent player, ref EntityPositionChangedEvent evt)
        {
            SpatialComponent? spatial = null;

            if (!Resolve(uid, ref spatial))
                return;

            var listenerPos = _coords.TileToScreen(spatial.MapPosition.Position) + _coords.TileSize / 2;
            Love.Audio.SetPosition(listenerPos.X, listenerPos.Y, 0f);
        }
    }

    [EventArgsUsage(EventArgsTargets.ByRef)]
    public struct FrameUpdateEventArgs
    {
        public float Dt;
    }
}
