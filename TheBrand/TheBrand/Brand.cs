using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using LeagueSharp;
using LeagueSharp.Common;
using TheBrand.ComboSystem;
using TheBrand.Commons;

namespace TheBrand
{
    class Brand : IMainContext
    {
        private ComboProvider _comboProvider;
        private Menu _mainMenu;
        private Orbwalking.Orbwalker _orbwalker;
        private MenuItem _drawQ, _drawW, _drawE, _drawR;


        public void Load(EventArgs loadargs)
        {
            try
            {
                if (ObjectManager.Player.ChampionName != "Brand")
                    return;

                var notification = new Notification("The Brand loaded", 3) { TextColor = new SharpDX.ColorBGRA(255, 0, 0, 255), BorderColor = new SharpDX.ColorBGRA(139, 100, 0, 255) };
                Notifications.AddNotification(notification);
                var y = Color.OrangeRed;
                _comboProvider = new BrandCombo(new Skill[] { new BrandQ(new Spell(SpellSlot.Q)), new BrandW(new Spell(SpellSlot.W)), new BrandE(new Spell(SpellSlot.E)), new BrandR(new Spell(SpellSlot.R)) }.ToList(), 1050);

                _mainMenu = CreateMenu("The Brand", true);
                var orbwalkerMenu = CreateMenu("Orbwalker", _mainMenu);
                var targetSelectorMenu = CreateMenu("Target Selector", _mainMenu);
                var comboMenu = CreateMenu("Combo", _mainMenu);
                var harassMenu = CreateMenu("Harass", _mainMenu);
                var laneclearMenu = CreateMenu("Laneclear", _mainMenu);
                ManaManager.Initialize(_mainMenu);
                IgniteManager.Initialize(_mainMenu);
                var miscMenu = CreateMenu("Misc", _mainMenu);
                var antiGapcloser = CreateMenu("Anti gapcloser", _mainMenu);
                var interrupter = CreateMenu("Interrupter", _mainMenu);
                var drawingMenu = CreateMenu("Drawing", _mainMenu);

                _orbwalker = new Orbwalking.Orbwalker(orbwalkerMenu);
                TargetSelector.AddToMenu(targetSelectorMenu);


                comboMenu.AddMItem("Use Q", true, (sender, args) => _comboProvider.SetEnabled<BrandQ>(Orbwalking.OrbwalkingMode.Combo, args.GetNewValue<bool>()));
                comboMenu.AddMItem("Use W", true, (sender, args) => _comboProvider.SetEnabled<BrandW>(Orbwalking.OrbwalkingMode.Combo, args.GetNewValue<bool>()));
                comboMenu.AddMItem("Use E", true, (sender, args) => _comboProvider.SetEnabled<BrandE>(Orbwalking.OrbwalkingMode.Combo, args.GetNewValue<bool>()));
                comboMenu.AddMItem("Use R", true, (sender, args) => _comboProvider.SetEnabled<BrandR>(Orbwalking.OrbwalkingMode.Combo, args.GetNewValue<bool>()));
                comboMenu.ProcStoredValueChanged<bool>();

                var rOptions = CreateMenu("Ult Options", comboMenu);
                rOptions.AddMItem("Bridge R", true, (sender, args) => _comboProvider.GetSkill<BrandR>().UseBridgeUlt = args.GetNewValue<bool>()).ProcStoredValueChanged<bool>();
                rOptions.AddMItem("");
                rOptions.AddMItem("Risky R", true, (sender, args) => _comboProvider.GetSkill<BrandR>().RiskyUlt = args.GetNewValue<bool>()).ProcStoredValueChanged<bool>();
                rOptions.AddMItem("(R bounces, no 100% success)");
                rOptions.AddMItem("");
                rOptions.AddMItem("Ult non killable", true, (sender, args) => _comboProvider.GetSkill<BrandR>().UltNonKillable = args.GetNewValue<bool>()).ProcStoredValueChanged<bool>();
                rOptions.AddMItem("when min X targets", new Slider(HeroManager.Enemies.Count - 1, 1, HeroManager.Enemies.Count), (sender, args) => _comboProvider.GetSkill<BrandR>().MinBounceTargets = args.GetNewValue<Slider>().Value).ProcStoredValueChanged<Slider>();
                rOptions.AddMItem("");
                rOptions.AddMItem("Don't R with", true, (sender, args) => _comboProvider.GetSkill<BrandR>().AntiOverkill = args.GetNewValue<bool>()).ProcStoredValueChanged<bool>();
                rOptions.AddMItem("% Health difference", new Slider(60), (sender, args) => _comboProvider.GetSkill<BrandR>().OverkillPercent = args.GetNewValue<Slider>().Value).ProcStoredValueChanged<Slider>();
                rOptions.AddMItem("Ignore when fleeing", true, (sender, args) => _comboProvider.GetSkill<BrandR>().IgnoreAntiOverkillOnFlee = args.GetNewValue<bool>()).ProcStoredValueChanged<bool>();


                laneclearMenu.AddMItem("Use W", true, (sender, args) => _comboProvider.SetEnabled<BrandW>(Orbwalking.OrbwalkingMode.LaneClear, args.GetNewValue<bool>()));
                laneclearMenu.AddMItem("Use E", false, (sender, args) => _comboProvider.SetEnabled<BrandE>(Orbwalking.OrbwalkingMode.LaneClear, args.GetNewValue<bool>()));
                laneclearMenu.ProcStoredValueChanged<bool>();
                laneclearMenu.AddMItem("Min W targets", new Slider(3, 0, 10));
                laneclearMenu.AddMItem("Enemy near", new StringList(new[] { "Harass", "Laneclear" }), (sender, args) => _comboProvider.GetSkills().ToList().ForEach(item => item.SwitchClearToHarassOnTarget = args.GetNewValue<StringList>().SelectedIndex == 0));

                harassMenu.AddMItem("Use Q", true, (sender, args) => _comboProvider.SetEnabled<BrandQ>(Orbwalking.OrbwalkingMode.Mixed, args.GetNewValue<bool>()));
                harassMenu.AddMItem("Use W", true, (sender, args) => _comboProvider.SetEnabled<BrandW>(Orbwalking.OrbwalkingMode.Mixed, args.GetNewValue<bool>()));
                harassMenu.AddMItem("Use E", true, (sender, args) => _comboProvider.SetEnabled<BrandE>(Orbwalking.OrbwalkingMode.Mixed, args.GetNewValue<bool>()));
                harassMenu.ProcStoredValueChanged<bool>();
                harassMenu.AddMItem("Hitchance", new StringList(new[] { "Low", "Medium", "High", "VeryHigh" }));

                miscMenu.AddMItem("E on fire-minion", true, (sender, args) => _comboProvider.GetSkill<BrandE>().UseMinions = args.GetNewValue<bool>());
                miscMenu.AddMItem("E farm assist", true, (sender, args) => _comboProvider.GetSkill<BrandE>().FarmAssist = args.GetNewValue<bool>());
                miscMenu.AddMItem("E Killsteal", true, (sender, args) => _comboProvider.GetSkill<BrandE>().Killsteal = args.GetNewValue<bool>());
                miscMenu.AddMItem("Force AA in combo", false);

                var gapcloserSpells = CreateMenu("Enemies");
                _comboProvider.AddGapclosersToMenu(gapcloserSpells);
                antiGapcloser.AddSubMenu(gapcloserSpells);
                antiGapcloser.AddMItem("Enabled", true);

                interrupter.AddMItem("E Usage", true);
                interrupter.AddMItem("W Usage", true);
                interrupter.AddMItem("Enabled", true);
                var spellMenu = CreateMenu("Spells");
                _comboProvider.AddInterruptablesToMenu(spellMenu);
                interrupter.AddSubMenu(spellMenu);

                drawingMenu.AddMItem("Damage indicator3", new Circle(true, Color.Yellow), (sender, args) =>
                {
                    DamageIndicator.Enabled = args.GetNewValue<Circle>().Active;
                    DamageIndicator.Fill = true;
                    DamageIndicator.FillColor = Color.FromArgb(100, args.GetNewValue<Circle>().Color);
                    DamageIndicator.Color = Color.FromArgb(200, DamageIndicator.FillColor);
                    DamageIndicator.DamageToUnit = _comboProvider.GetComboDamage;
                }).ProcStoredValueChanged<Circle>();

                drawingMenu.AddMItem("W Prediction", new Circle(false, Color.Red));

                _drawW = drawingMenu.AddMItem("W Range", new Circle(true, Color.Red));
                _drawQ = drawingMenu.AddMItem("Q Range", new Circle(false, Color.OrangeRed));
                _drawE = drawingMenu.AddMItem("E Range", new Circle(true, Color.Goldenrod));
                _drawR = drawingMenu.AddMItem("R Range", new Circle(false, Color.DarkViolet));


                _mainMenu.AddToMainMenu();
                _comboProvider.Initialize(this);


                Game.OnUpdate += Tick;
                Drawing.OnDraw += Draw;

            }
            catch (Exception ex)
            {
                Console.WriteLine("such exception: " + ex);
            }
        }

        private void Draw(EventArgs args)
        {
            if (ObjectManager.Player.IsDead) return;

            var q = _drawQ.GetValue<Circle>();
            var w = _drawW.GetValue<Circle>();
            var e = _drawE.GetValue<Circle>();
            var r = _drawR.GetValue<Circle>();

            if (q.Active)
                Render.Circle.DrawCircle(ObjectManager.Player.Position, 1050, q.Color);
            if (w.Active)
                Render.Circle.DrawCircle(ObjectManager.Player.Position, 900, w.Color);
            if (e.Active)
                Render.Circle.DrawCircle(ObjectManager.Player.Position, 650, e.Color);
            if (r.Active)
                Render.Circle.DrawCircle(ObjectManager.Player.Position, 750, r.Color);

        }



        private void Tick(EventArgs args)
        {

            var watch = Stopwatch.StartNew();
            try
            {
                _comboProvider.Update(this);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                //  throw;
            }
            watch.Stop();
        }

        private Menu CreateMenu(string name, Menu menu)
        {
            var newMenu = new Menu(name, name);
            menu.AddSubMenu(newMenu);
            return newMenu;
        }

        private Menu CreateMenu(string name, bool root = false)
        {
            return new Menu(name, name, root);
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
