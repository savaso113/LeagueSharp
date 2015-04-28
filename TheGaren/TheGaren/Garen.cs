using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using TheGaren.ComboSystem;
using Color = System.Drawing.Color;

namespace TheGaren
{
    class Garen : IMainContext
    {
        private Menu _mainMenu;
        private Orbwalking.Orbwalker _orbwalker;
        private ComboProvider _comboProvider;
        private bool _updateSettings;

        public void Load()
        {
            if (ObjectManager.Player.ChampionName != "Garen")
                return;
            Game.PrintChat("<font color = \"#FF2222\">The Garen</font> loaded!");

            _comboProvider = new ComboProvider(new Skill[] { new GarenQ(new Spell(SpellSlot.Q)), new GarenW(new Spell(SpellSlot.W)), new GarenE(new Spell(SpellSlot.E)), new GarenR(new Spell(SpellSlot.R)) }.ToList());

            _mainMenu = CreateMenu("The Garen", true);
            var orbwalkerMenu = CreateMenu("Orbwalker");
            var targetSelectorMenu = CreateMenu("Target Selector");
            var comboMenu = CreateMenu("Combo");
            var laneClearMenu = CreateMenu("Laneclear");
            var miscMenu = CreateMenu("Misc");
            var drawingMenu = CreateMenu("Drawing");


            _orbwalker = new Orbwalking.Orbwalker(orbwalkerMenu);
            TargetSelector.AddToMenu(targetSelectorMenu);

            comboMenu.AddMItem("Use Q", true, (sender, args) => _comboProvider.SetEnabled<GarenQ>(Orbwalking.OrbwalkingMode.Combo, args.GetNewValue<bool>()));
            comboMenu.AddMItem("Use W", true, (sender, args) => _comboProvider.SetEnabled<GarenW>(Orbwalking.OrbwalkingMode.Combo, args.GetNewValue<bool>()));
            comboMenu.AddMItem("Use E", true, (sender, args) => _comboProvider.SetEnabled<GarenE>(Orbwalking.OrbwalkingMode.Combo, args.GetNewValue<bool>()));
            comboMenu.AddMItem("Use R", true, (sender, args) => _comboProvider.SetEnabled<GarenR>(Orbwalking.OrbwalkingMode.Combo, args.GetNewValue<bool>()));
            comboMenu.AddMItem("Only Ult target", true);

            laneClearMenu.AddMItem("Use Q", true, (sender, args) => _comboProvider.SetEnabled<GarenQ>(Orbwalking.OrbwalkingMode.LaneClear, args.GetNewValue<bool>()));
            laneClearMenu.AddMItem("Use E", true, (sender, args) => _comboProvider.SetEnabled<GarenE>(Orbwalking.OrbwalkingMode.LaneClear, args.GetNewValue<bool>()));

            drawingMenu.AddMItem("Damage Drawing", new StringList(new[] { "All", "Only R", "None" }), ApplyDamageDrawOptions);
            drawingMenu.AddMItem("Damage Color", Color.FromArgb(100, Color.Goldenrod), ApplyDamageDrawOptions);
            drawingMenu.AddMItem("R Range", new Circle(true, Color.Goldenrod));
            drawingMenu.AddItem(new MenuItem("credits", "Damage drawing by xSalice!"));

            miscMenu.AddMItem("Q after attack", true);
            miscMenu.AddMItem("E after attack", true);
            miscMenu.AddMItem("W Mode", new StringList(new[] { "Always", "On damage" }));
            miscMenu.AddMItem("Q Interrupt", true);


            comboMenu.ProcStoredValueChanged<bool>();
            laneClearMenu.ProcStoredValueChanged<bool>();
            drawingMenu.ProcStoredValueChanged<object>();

            _mainMenu.AddSubMenu(orbwalkerMenu);
            _mainMenu.AddSubMenu(targetSelectorMenu);
            _mainMenu.AddSubMenu(comboMenu);
            _mainMenu.AddSubMenu(laneClearMenu);
            _mainMenu.AddSubMenu(drawingMenu);
            _mainMenu.AddSubMenu(miscMenu);
            _mainMenu.AddToMainMenu();

            _comboProvider.Initialize(this);

            Game.OnUpdate += Tick;
            Drawing.OnDraw += Draw;
        }

        private void ApplyDamageDrawOptions(object sender, OnValueChangeEventArgs args)
        {
            _updateSettings = true;
        }

        private void Draw(EventArgs args)
        {
            if (_mainMenu.SubMenu("Drawing").Item("Drawing.RRange").GetValue<Circle>().Active)
                Render.Circle.DrawCircle(ObjectManager.Player.Position, 375, _mainMenu.SubMenu("Drawing").Item("Drawing.RRange").GetValue<Circle>().Color);


        }

        private Menu CreateMenu(string name, bool root = false)
        {
            return new Menu(name, name, root);
        }

        private void Tick(EventArgs args)
        {
            _comboProvider.Update(this);

            if (!_updateSettings) return;
            _updateSettings = false;

            DamageIndicator.FillColor = _mainMenu.SubMenu("Drawing").Item("Drawing.DamageColor").GetValue<Color>();
            var color = Color.FromArgb((int)(DamageIndicator.FillColor.R * 1.5f), (int)(DamageIndicator.FillColor.G * 1.5f), (int)(DamageIndicator.FillColor.B * 1.5f));

            DamageIndicator.Color = color;
            var mode = _mainMenu.SubMenu("Drawing").Item("Drawing.DamageDrawing").GetValue<StringList>().SelectedValue;
            if (mode == "All")
                DamageIndicator.DamageToUnit = _comboProvider.GetComboDamage;
            else if (mode == "Only R")
                DamageIndicator.DamageToUnit = _comboProvider.GetSkill<GarenR>().GetDamage;
            DamageIndicator.Fill = true;
            DamageIndicator.Enabled = mode != "None";
        }

        public Menu GetRootMenu()
        {
            return _mainMenu;
        }

        public Orbwalking.Orbwalker GetOrbwalker()
        {
            return _orbwalker;
        }
    }
}
