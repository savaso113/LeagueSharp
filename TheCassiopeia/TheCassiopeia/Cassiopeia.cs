using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using TheCassiopeia.Commons;
using TheCassiopeia.Commons.ComboSystem;
using TheCassiopeia.Commons.Debug;

namespace TheCassiopeia
{
    class Cassiopeia
    {
        public void Load(EventArgs eArgs)
        {
            var mainMenu = new Menu("The Cassiopeia", "TheCassiopeia", true);
            var orbwalkerMenu = mainMenu.CreateSubmenu("Orbwalker");
            var targetselectorMenu = mainMenu.CreateSubmenu("Target selector");
            var comboMenu = mainMenu.CreateSubmenu("Combo");
            var ultMenu = mainMenu.CreateSubmenu("Ultimate Settings");
            var harassMenu = mainMenu.CreateSubmenu("Harass");
            var laneclearMenu = mainMenu.CreateSubmenu("Laneclear");
            var lasthitMenu = mainMenu.CreateSubmenu("Lasthit");
            var gapcloserMenu = mainMenu.CreateSubmenu("Gapcloser");
            var interrupterMenu = mainMenu.CreateSubmenu("Interrupter");
            var manamanagerMenu = mainMenu.CreateSubmenu("Manamanager");
            var igniteMenu = mainMenu.CreateSubmenu("Ignite");
            var drawingMenu = mainMenu.CreateSubmenu("Drawing");
            var autolevelMenu = mainMenu.CreateSubmenu("Auto level spells");

            Console.WriteLine("iksde");

            var orbwalker = new Orbwalking.Orbwalker(orbwalkerMenu);
            TargetSelector.AddToMenu(targetselectorMenu);

            var provider = new CassioCombo(1000, orbwalker, new CassQ(SpellSlot.Q), new CassW(SpellSlot.W), new CassE(SpellSlot.E), new CassR(SpellSlot.R));

            provider.CreateBasicMenu(comboMenu, harassMenu, laneclearMenu, gapcloserMenu, interrupterMenu, manamanagerMenu, igniteMenu, null, drawingMenu);
            provider.CreateAutoLevelMenu(autolevelMenu, ComboProvider.SpellOrder.RQEW, ComboProvider.SpellOrder.REQW);

            ultMenu.AddMItem("(Will ult if one condition is met)");
            ultMenu.AddMItem("Min Enemies hit", new Slider(2, 1, HeroManager.Enemies.Count), (sender, args) => provider.GetSkill<CassR>().MinTargets = args.GetNewValue<Slider>().Value);
            ultMenu.AddMItem("Ult if target killable with combo", true, (sender, args) => provider.GetSkill<CassR>().UltOnKillable = args.GetNewValue<bool>());

            comboMenu.AddMItem("Fast combo (small chance to E non-poisoned)", true, (sender, args) => provider.GetSkill<CassQ>().FastCombo = args.GetNewValue<bool>());
            comboMenu.AddMItem("Risky E (uses fast combo often, but more fails)", true, (sender, args) => provider.GetSkill<CassQ>().RiskyCombo = args.GetNewValue<bool>());
            comboMenu.AddMItem("Force AA in combo", true, (sender, args) => provider.AutoInCombo = args.GetNewValue<bool>());
            comboMenu.ProcStoredValueChanged<bool>();


            Circle q = new Circle(false, Color.GreenYellow), e = new Circle(false, Color.Red);

            drawingMenu.AddMItem("Q Range", q, (sender, args) => q = args.GetNewValue<Circle>());
            drawingMenu.AddMItem("E Range", e, (sender, args) => e = args.GetNewValue<Circle>());
            drawingMenu.ProcStoredValueChanged<Circle>();


            var pushItem = laneclearMenu.AddMItem("E only killable \nminions (laneclear)", new KeyBind(78, KeyBindType.Toggle) { Active = true }, (sender, args) => provider.GetSkill<CassE>().WaveclearPush = !args.GetNewValue<KeyBind>().Active);
            pushItem.ProcStoredValueChanged<KeyBind>();
            pushItem.Permashow();
            new MenuItem("-- Waveclear --", "").Permashow();


            lasthitMenu.AddMItem("Use E", true, (sender, args) => provider.GetSkill<CassE>().Farm = args.GetNewValue<bool>());
            lasthitMenu.AddMItem("Lasthit assist", true, (sender, args) => provider.GetSkill<CassE>().FarmAssist = args.GetNewValue<bool>());
            lasthitMenu.AddMItem("Lasthit non-poisoned if mana < ? %", new Slider(0), (sender, args) => provider.GetSkill<CassE>().FarmNonPoisonedPercent = args.GetNewValue<Slider>().Value);
            lasthitMenu.ProcStoredValueChanged<bool>();
            lasthitMenu.ProcStoredValueChanged<Slider>();

            mainMenu.AddToMainMenu();
            provider.Initialize();

            DevAssistant.Init();

            Game.OnUpdate += (args) =>
            {
                provider.Update();


            };

            Drawing.OnDraw += (args) =>
            {
                if (q.Active)
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, 850, q.Color);
                if (e.Active)
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, 700, e.Color);

            };
        }



    }
}
