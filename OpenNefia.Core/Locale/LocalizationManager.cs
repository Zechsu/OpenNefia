﻿using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.UI;
using OpenNefia.Core.Utility;
using System.Reflection;
using NLua;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.ContentPack;
using OpenNefia.Core.UI.Layer;
using System.Buffers;
using OpenNefia.Core.Random;
using OpenNefia.Core.Log;
using OpenNefia.Core.Reflection;
using OpenNefia.Core.Graphics;

namespace OpenNefia.Core.Locale
{
    public interface ILocalizationFetcher
    {
        string Get(LocaleKey key, params LocaleArg[] args);
    }

    public interface ILocalizationManager : ILocalizationFetcher
    {
        PrototypeId<LanguagePrototype> Language { get; }

        void Initialize(PrototypeId<LanguagePrototype> language);

        bool IsFullwidth();
        void SwitchLanguage(PrototypeId<LanguagePrototype> language);

        void DoLocalize(object o, LocaleKey key);

        void LoadContentFile(ResourcePath luaFile);
        void LoadString(string luaScript);
        void Resync();
    }

    /// <summary>
    /// An argument for localizing text using a Lua function.
    /// </summary>
    public struct LocaleArg
    {
        /// <summary>
        /// Name of this argument in the Lua source.
        /// </summary>
        /// <remarks>
        /// NOTE: This is purely for documentation purposes.
        /// </remarks>
        public string Name { get; set; }

        /// <summary>
        /// Value of this argument to pass to a Lua function.
        /// </summary>
        public object? Value { get; set; }

        public LocaleArg(string name, object? value)
        {
            Name = name;
            Value = value;
        }

        public static implicit operator LocaleArg((string, object?) tuple)
        {
            return new(tuple.Item1, tuple.Item2);
        }
    }

    public sealed partial class LocalizationManager : ILocalizationManager
    {
        [Dependency] private readonly IUiLayerManager _uiLayers = default!;
        [Dependency] private readonly IResourceManager _resourceManager = default!;
        [Dependency] private readonly IReflectionManager _reflectionManager = default!;
        [Dependency] private readonly IGraphics _graphics = default!;
        [Dependency] private readonly IRandom _random = default!;

        private readonly ResourcePath LocalePath = new ResourcePath("/Locale");
        
        private LocalizationEnv _env = null!;

        public void Initialize(PrototypeId<LanguagePrototype> language)
        {
            _env = new LocalizationEnv(_resourceManager, _reflectionManager);

            SwitchLanguage(language);

            _graphics.OnWindowFocused += WindowFocusedChanged;

            WatchResources();
        }

        public PrototypeId<LanguagePrototype> Language { get; private set; } = LanguagePrototypeOf.English;
        
        public void SwitchLanguage(PrototypeId<LanguagePrototype> language)
        {
            Language = language;
            _env.SetLanguage(language);
            _env.LoadAll(language, LocalePath);
            _env.Resync();

            foreach (var layer in _uiLayers.ActiveLayers)
            {
                layer.Localize(layer.GetType()!.FullName!);
            }
        }

        public string Get(LocaleKey key, params LocaleArg[] args)
        {
            if (_env._StringStore.TryGetValue(key, out var str))
            {
                return str;
            }

            if (_env._FunctionStore.TryGetValue(key, out var func))
            {
                var shared = ArrayPool<object?>.Shared;
                var rented = shared.Rent(args.Length);

                for (int i = 0; i < args.Length; i++)
                    rented[i] = args[i].Value;

                string resultStr;

                try
                {
                    var result = func.Call(rented)[0];
                    resultStr = $"{result}";
                }
                catch (Exception ex)
                {
                    Logger.ErrorS("loc", ex, $"Error in locale function: {ex}");
                    resultStr = $"<exception: {ex.Message} ({key})>";
                }

                shared.Return(rented);

                return resultStr;
            }

            if (_env._ListStore.TryGetValue(key, out var list))
            {
                // This is meant to emulate the `txt` function in the HSP source.
                return _random.Pick(list);
            }

            return $"<Missing key: {key}>";
        }

        public bool IsFullwidth()
        {
            return Language == LanguagePrototypeOf.Japanese;
        }

        public void LoadContentFile(ResourcePath luaFile)
        {
            _env.LoadContentFile(luaFile);
        }

        public void LoadString(string luaScript)
        {
            _env.LoadString(luaScript);
        }

        public void Resync()
        {
            _env.Resync();
        }
    }
}
