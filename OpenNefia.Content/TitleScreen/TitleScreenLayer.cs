﻿using OpenNefia.Content.UI.Element;
using OpenNefia.Content.UI.Element.List;
using OpenNefia.Core;
using OpenNefia.Core.Audio;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.UI.Layer;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.UI;
using OpenNefia.Core.UI;

namespace OpenNefia.Content.TitleScreen
{
    [Localize("Elona.TitleScreen.Layer")]
    public class TitleScreenLayer : UiLayerWithResult<UINone, TitleScreenResult>, ITitleScreenLayer
    {
        public enum TitleScreenChoice
        {
            Restore,
            Generate,
            Incarnate,
            About,
            Options,
            Mods,
            Exit
        }

        public class TitleScreenCell : UiListCell<TitleScreenChoice>
        {
            private const int ITEM_HEIGHT = 35;

            [Localize("Subtext")]
            public IUiText TextSubtext;

            public override string? LocalizeKey => Enum.GetName(Data);

            public TitleScreenCell(TitleScreenChoice submenu)
                : base(submenu, new UiText(UiFonts.ListTitleScreenText))
            {
                TextSubtext = new UiText(UiFonts.ListTitleScreenSubtext);
            }

            public override void Localize(LocaleKey key)
            {
                base.Localize(key);
                SetPosition(X, Y);
            }

            public override void SetPosition(int x, int y)
            {
                base.SetPosition(x, y);
                if (TextSubtext.Text != string.Empty)
                {
                    TextSubtext.SetPosition(x + 40, y - 4);
                    UiText.SetPosition(x + 40 + XOffset + 4, y + 8);
                }
                else
                {
                    UiText.SetPosition(x + 40 + XOffset + 4, y + 1);
                }
            }

            public override void SetSize(int width, int height)
            {
                height = ITEM_HEIGHT;

                UiText.SetSize(width, height);
                TextSubtext.SetSize(width, height);
                base.SetSize(Math.Max(width, UiText.Width), height);
            }

            public override void Draw()
            {
                GraphicsEx.SetColor(Love.Color.White);
                AssetSelectKey.Draw(X, Y - 1);
                KeyNameText.Draw();
                UiText.Draw();
                if (Loc.IsFullwidth())
                {
                    TextSubtext.Draw();
                }
                UiUtils.DebugDraw(this);
            }

            public override void Update(float dt)
            {
                KeyNameText.Update(dt);
                UiText.Update(dt);
                TextSubtext.Update(dt);
            }

            public override void Dispose()
            {
                base.Dispose();
                TextSubtext.Dispose();
            }
        }

        private FontSpec FontTitleText = UiFonts.TitleScreenText;
        private IAssetInstance AssetTitle;
        private IAssetInstance AssetG4;

        private UiText[] TextInfo;

        [Localize]
        private UiWindow Window;

        [Localize]
        private UiList<TitleScreenChoice> List;

        public TitleScreenLayer()
        {
            AssetTitle = Assets.Get(AssetPrototypeOf.Title);
            AssetG4 = Assets.Get(AssetPrototypeOf.G4);

            var version = "1.22";
            TextInfo = new UiText[3];

            TextInfo[0] = new UiText(FontTitleText, $"Elona version {version}  Developed by Noa");
            if (Loc.Language == LanguagePrototypeOf.Japanese)
            {
                TextInfo[1] = new UiText(FontTitleText, "Contributor MSL / View the credits for more");
            }
            else if (Loc.Language == LanguagePrototypeOf.English)
            {
                TextInfo[1] = new UiText(FontTitleText, "Contributor f1r3fly, Sunstrike, Schmidt, Elvenspirit / View the credits for more");
            }
            TextInfo[2] = new UiText(FontTitleText, $"{Engine.NameBase} version {Engine.Version} Developed by Ruin0x11");

            Window = new UiWindow();

            var items = new TitleScreenCell[] {
                new TitleScreenCell(TitleScreenChoice.Restore),
                new TitleScreenCell(TitleScreenChoice.Generate),
                new TitleScreenCell(TitleScreenChoice.Incarnate),
                new TitleScreenCell(TitleScreenChoice.About),
                new TitleScreenCell(TitleScreenChoice.Options),
                new TitleScreenCell(TitleScreenChoice.Mods),
                new TitleScreenCell(TitleScreenChoice.Exit),
            };
            List = new UiList<TitleScreenChoice>(items);
            List.EventOnActivate += (_, evt) => RunTitleScreenAction(evt.SelectedCell.Data);

            AddChildren();
        }

        public override void OnFocused()
        {
            base.OnFocused();
            List.GrabFocus();
        }

        public void AddChildren()
        {
            foreach (var text in TextInfo)
            {
                AddChild(text);
            }
            AddChild(Window);
            AddChild(List);
        }

        private void RunTitleScreenAction(TitleScreenChoice selectedChoice)
        {
            if (selectedChoice != TitleScreenChoice.Generate)
            {
                Sounds.Play(Protos.Sound.Ok1);
            }

            switch (selectedChoice)
            {
                case TitleScreenChoice.Restore:
                    Finish(new TitleScreenResult(TitleScreenAction.StartGame));
                    break;
                case TitleScreenChoice.Exit:
                    Finish(new TitleScreenResult(TitleScreenAction.Quit));
                    break;
                default:
                    break;
            }
        }

        public override void SetSize(int width, int height)
        {
            base.SetSize(width, height);
            Window.SetSize(320, 355);
            List.SetPreferredSize();
        }

        public override void SetPosition(int x, int y)
        {
            base.SetPosition(x, y);
            TextInfo[0].SetPosition(X + 20, Y + 20);
            TextInfo[1].SetPosition(X + 20, Y + 20 + FontTitleText.LoveFont.GetHeight() + 5);
            TextInfo[2].SetPosition(X + 20, Y + 20 + (FontTitleText.LoveFont.GetHeight() + 5) * 2);
            Window.SetPosition(X + 80, (Height - 308) / 2);
            List.SetPosition(Window.X + 40, Window.Y + 48);
        }

        public override void OnQuery()
        {
            Music.Play(Protos.Music.Opening);
        }

        public override void Update(float dt)
        {
            foreach (var text in TextInfo)
                text.Update(dt);

            Window.Update(dt);
            List.Update(dt);
        }

        public override void Draw()
        {
            GraphicsEx.SetColor(Love.Color.White);
            AssetTitle.Draw(X, Y, Width, Height);

            foreach (var text in TextInfo)
                text.Draw();

            Window.Draw();
            List.Draw();

            var bgPicWidth = Window.Width / 5 * 4;
            var bgPicHeight = Window.Height - 80;
            GraphicsEx.SetColor(255, 255, 255, 50);
            AssetG4.Draw(Window.X + 160 - bgPicWidth / 2,
                              Window.Y + Window.Height / 2 - bgPicHeight / 2,
                              bgPicWidth,
                              bgPicHeight);

            UiUtils.DebugDraw(List);
        }

        public override void Dispose()
        {
            foreach (var text in TextInfo)
            {
                text.Dispose();
            }
            Window.Dispose();
            List.Dispose();
        }
    }
}
