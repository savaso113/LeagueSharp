using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using TheGaren.ComboSystem;

namespace TheGaren
{
    class GarenQ : Skill
    {
        private readonly Obj_AI_Hero _player;
        private bool _isDoingWindingUpProcess;
        private IMainContext _mainContext;

        public GarenQ(Spell spell)
            : base(spell)
        {
            _player = ObjectManager.Player;
            Interrupter2.OnInterruptableTarget += OnPossibleInterrupt;
        }

        public override void Initialize(IMainContext context, ComboProvider combo)
        {
            _mainContext = context;
            base.Initialize(context, combo);
        }

        private void OnPossibleInterrupt(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (args.EndTime > 0.5f && _mainContext.GetRootMenu().SubMenu("Misc").Item("Misc.QInterrupt").GetValue<bool>() && sender.Distance(ObjectManager.Player.Position) < ObjectManager.Player.AttackRange + _player.BoundingRadius + sender.BoundingRadius + 50)
            {
                if (Spell.Instance.State == SpellState.Ready)
                    SafeCast(() => Spell.Cast());

                ObjectManager.Player.IssueOrder(GameObjectOrder.AttackUnit, sender);
            }
        }



        public override void Combo(IMainContext context, ComboProvider combo)
        {

            var target = TargetSelector.GetTarget(400, TargetSelector.DamageType.Physical);

            if ((!HasBeenSafeCast("GarenQ")) && ObjectManager.Player.Buffs.All(buff => buff.Name != "GarenQ") &&  combo.GrabControl(this))
            {
                //attkrange bounding + 65
                if (target == null || target.Distance(_player) > _player.AttackRange + _player.BoundingRadius + target.BoundingRadius + 50f || (!context.GetRootMenu().SubMenu("Misc").Item("Misc.Qafterattack").GetValue<bool>()))
                {
                    SafeCast(() => Spell.Cast());
                    _isDoingWindingUpProcess = false;
                }
                else
                {
                    _isDoingWindingUpProcess = true;
                    if (AAHelper.JustFinishedAutoattack)
                    {
                        SafeCast(() => Spell.Cast());
                        _isDoingWindingUpProcess = false;
                    }
                }
            }
        }

        public override void Harass(IMainContext context, ComboProvider combo)
        {
            Combo(context, combo);
            base.Harass(context, combo);
        }

        public override void LaneClear(IMainContext context, ComboProvider combo)
        {
            var minions = MinionManager.GetMinions(400);
            if (minions.Count > 0 && (!HasBeenSafeCast("GarenQ")) && ObjectManager.Player.Buffs.All(buff => buff.Name != "GarenQ") && AAHelper.JustFinishedAutoattack && combo.GrabControl(this))
                SafeCast(() => Spell.Cast());

            base.LaneClear(context, combo);
        }

        public override bool NeedsControl()
        {
            return Spell.Instance.State == SpellState.Ready || IsInSafeCast("GarenQ") || ObjectManager.Player.Buffs.Any(buff => buff.Name == "GarenQ") || _isDoingWindingUpProcess;
        }

        public override int GetPriority()
        {
            return 2;
        }
    }
}
