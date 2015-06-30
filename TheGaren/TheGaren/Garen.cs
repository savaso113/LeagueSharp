using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using TheGaren.Commons;
using TheGaren.Commons.ComboSystem;
using Color = System.Drawing.Color;

namespace TheGaren
{
    class Garen
    {
        private ComboProvider _comboProvider;
        private Circle _drawR, _drawFlashUlt;
        private GarenR _r;
        private SpellDataInst _flash;

        public void Load()
        {
            if (ObjectManager.Player.ChampionName != "Garen")
                return;
            Notifications.AddNotification("The Garen v2 loaded!", 3);

            var mainMenu = new Menu("The Garen", "The Garen", true);
            var orbwalkerMenu = mainMenu.CreateSubmenu("Orbwalker");
            var targetSelectorMenu = mainMenu.CreateSubmenu("Target Selector");
            var comboMenu = mainMenu.CreateSubmenu("Combo");
            var laneClearMenu = mainMenu.CreateSubmenu("Laneclear");
            var miscMenu = mainMenu.CreateSubmenu("Misc");
            var items = mainMenu.CreateSubmenu("Items");
            var gapcloserMenu = mainMenu.CreateSubmenu("Gapcloser");
            var interrupterMenu = mainMenu.CreateSubmenu("Interrupter");
            var drawingMenu = mainMenu.CreateSubmenu("Drawing");


            var orbwalker = new Orbwalking.Orbwalker(orbwalkerMenu);
            TargetSelector.AddToMenu(targetSelectorMenu);

            _comboProvider = new ComboProvider(500, new Skill[] { new GarenQ(new Spell(SpellSlot.Q)), new GarenW(new Spell(SpellSlot.W)), new GarenE(new Spell(SpellSlot.E)), new GarenR(new Spell(SpellSlot.R)) }.ToList(), orbwalker);
            _r = _comboProvider.GetSkill<GarenR>();
            _flash = ObjectManager.Player.Spellbook.Spells.FirstOrDefault(spell => spell.Name == "summonerflash");
            _comboProvider.CreateBasicMenu(comboMenu, null, null, gapcloserMenu, interrupterMenu, null, mainMenu.CreateSubmenu("Ignite"), items, false);
            _comboProvider.CreateLaneclearMenu(laneClearMenu, false, SpellSlot.W);

            comboMenu.AddMItem("Q After Auto Attack", true, (sender, args) => _comboProvider.GetSkill<GarenQ>().OnlyAfterAuto = args.GetNewValue<bool>());
            comboMenu.AddMItem("E After Auto Attack", true, (sender, args) => _comboProvider.GetSkill<GarenE>().OnlyAfterAuto = args.GetNewValue<bool>());
            comboMenu.AddMItem("R Killsteal", false, (sender, args) => _comboProvider.GetSkill<GarenR>().Killsteal = args.GetNewValue<bool>());
            comboMenu.AddMItem("Q if not in range", true, (sender, args) => _comboProvider.GetSkill<GarenQ>().UseWhenOutOfRange = args.GetNewValue<bool>());
            comboMenu.ProcStoredValueChanged<bool>();

            miscMenu.AddMItem("Also W out of combo", true, (sender, args) => _comboProvider.GetSkill<GarenW>().UseAlways = args.GetNewValue<bool>());
            miscMenu.AddMItem("Min incomming DPS for W in health %", new Slider(2, 1, 15), (sender, args) => _comboProvider.GetSkill<GarenW>().MinDamagePercent = args.GetNewValue<Slider>().Value).ProcStoredValueChanged<Slider>();
            miscMenu.AddMItem("Always W enemy ults", true, (sender, args) => _comboProvider.GetSkill<GarenW>().UseOnUltimates = args.GetNewValue<bool>());
            miscMenu.ProcStoredValueChanged<bool>();


            laneClearMenu.AddMItem("E min. minions", new Slider(1, 1, 8), (sender, args) => _comboProvider.GetSkill<GarenE>().MinFarmMinions = args.GetNewValue<Slider>().Value).ProcStoredValueChanged<Slider>();
            laneClearMenu.AddMItem("Use Hydra", true, (sender, args) => _comboProvider.GetSkill<GarenE>().UseHydra = args.GetNewValue<bool>()).ProcStoredValueChanged<bool>();

            drawingMenu.AddMItem("Damage Indicator", new Circle(true, Color.FromArgb(100, Color.Goldenrod)), (sender, args) =>
            {
                DamageIndicator.DamageToUnit = _comboProvider.GetComboDamage;
                DamageIndicator.Enabled = args.GetNewValue<Circle>().Active;
                DamageIndicator.FillColor = args.GetNewValue<Circle>().Color;
                DamageIndicator.Fill = true;
                DamageIndicator.Color = Color.FromArgb(255, DamageIndicator.FillColor);
            });
            drawingMenu.AddMItem("R Range", new Circle(true, Color.Goldenrod), (sender, args) => _drawR = args.GetNewValue<Circle>());
            drawingMenu.AddMItem("Draw possible flash-ult", new Circle(true, Color.Red), (sender, args) => _drawFlashUlt = args.GetNewValue<Circle>());
            drawingMenu.AddMItem("Damage Indicator by xSalice!");
            drawingMenu.ProcStoredValueChanged<Circle>();

            mainMenu.AddMItem("Max order: R > E > Q > W! Have fun!");
            mainMenu.AddToMainMenu();
            _comboProvider.Initialize();

            Game.OnUpdate += Tick;
            Drawing.OnDraw += Draw;
        }

        private void Draw(EventArgs args)
        {
            if (_drawR.Active)
                Render.Circle.DrawCircle(ObjectManager.Player.Position, 375, _drawR.Color);
            if (_drawFlashUlt.Active && _r.Spell.IsReady())
            {
                foreach (var enemy in HeroManager.Enemies.Where(enemy => enemy.IsValidTarget(_flash != null && _flash.IsReady() ? 375 + 425 : 375) && _r.Spell.IsKillable(enemy)))
                {
                    var screenPos = Drawing.WorldToScreen(enemy.Position);
                    Drawing.DrawText(screenPos.X - 50, screenPos.Y - 50, _drawFlashUlt.Color, "Possible (Flash) ult!");
                }
            }

            //foreach (var enemy in HeroManager.Enemies.Where(enemy => enemy.IsValidTarget()))
            //{
            //    var screenPos = Drawing.WorldToScreen(enemy.Position);
            //    Drawing.DrawText(screenPos.X - 50, screenPos.Y - 50, _drawFlashUlt.Color, (enemy.Health - HealthPrediction.GetHealthPrediction(enemy,1)).ToString());
            //}

            //Drawing.DrawText(200, 100, Color.Red, ObjectManager.Player.Spellbook.GetSpell(SpellSlot.Q).GetState().ToString() + " " + (int)ObjectManager.Player.Spellbook.GetSpell(SpellSlot.Q).GetState());

        }

        private void Tick(EventArgs args)
        {
            _comboProvider.Update();
            IgniteManager.Update(_comboProvider);
        }
    }
}
