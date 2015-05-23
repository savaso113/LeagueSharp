using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LeagueSharp;
using LeagueSharp.Common;
using TheBrand.ComboSystem;

namespace TheBrand
{
    class BrandE : Skill
    {
        private BrandQ _brandQ;
        private MenuItem _eInterrupt;
        private Obj_AI_Base _recentFarmTarget;
        public bool UseMinions;
        public bool FarmAssist;
        public bool Killsteal;

        public BrandE(Spell spell)
            : base(spell)
        {
        }

        public override void Initialize(IMainContext context, ComboProvider combo)
        {
            _brandQ = combo.GetSkill<BrandQ>();
            _eInterrupt = context.GetRootMenu().GetMenuItem("Interrupter.EUsage");
            Orbwalking.OnNonKillableMinion += OnMinionUnkillable;
            Orbwalking.BeforeAttack += Orbwalking_BeforeAttack;
            base.Initialize(context, combo);
        }

        void Orbwalking_BeforeAttack(Orbwalking.BeforeAttackEventArgs args)
        {
            _recentFarmTarget = args.Target.Type == GameObjectType.obj_AI_Base ? (Obj_AI_Base)args.Target : _recentFarmTarget;
        }

        public override void Update(Orbwalking.OrbwalkingMode mode, IMainContext context, ComboProvider combo, Obj_AI_Hero target)
        {
            if (Killsteal)
                foreach (var enemy in HeroManager.Enemies)
                {
                    if (enemy.Distance(ObjectManager.Player) > 650 || ObjectManager.Player.GetSpellDamage(enemy, SpellSlot.E) < enemy.Health + enemy.MagicShield) continue;
                    Obj_AI_Hero currentEnemy = enemy;
                    SafeCast(() => Spell.Cast(currentEnemy));
                }
            base.Update(mode, context, combo, target);
        }

        public override void Cast(Obj_AI_Hero target, bool force = false, HitChance minChance = HitChance.Low)
        {
            if (HasBeenSafeCast()) return;
            if (target == null) return;
            var distance = target.Distance(ObjectManager.Player);
            if (distance < 950 && distance > 650 && UseMinions)
            {
                var fireMinion = MinionManager.GetMinions(650, MinionTypes.All, MinionTeam.Enemy, MinionOrderTypes.None).Where(minion => minion.HasBuff("brandablaze") && minion.Distance(target) < 300).MinOrDefault(minion => minion.Distance(target));
                if (fireMinion != null)
                    SafeCast(() =>
                    {
                        Spell.Cast(fireMinion);
                        _brandQ.Cast(target);
                    });
            }
            if (distance < 650)
                SafeCast(() =>
                {
                    Spell.Cast(target);
                    //_brandQ.Cast(target);
                });
        }



        public override void LaneClear(IMainContext context, ComboProvider combo, Obj_AI_Hero target)
        {
            if (HasBeenSafeCast()) return;
            var minions = MinionManager.GetMinions(650);
            if (!minions.Any(minion => minion.HasBuff("brandablaze"))) return;
            Obj_AI_Base bestMinion = minions.FirstOrDefault();
            var neighbours = 0;
            foreach (var minion in minions)
            {
                var currentNeighbours = minions.Count(neighbour => neighbour.Distance(minion) < 300);
                if (currentNeighbours <= neighbours) continue;
                bestMinion = minion;
                neighbours = currentNeighbours;
            }
            SafeCast(() => Spell.Cast(bestMinion));

            base.LaneClear(context, combo, target);
        }

        void OnMinionUnkillable(AttackableUnit minion)
        {
            if (!FarmAssist) return;
            if (Context.GetOrbwalker().ActiveMode != Orbwalking.OrbwalkingMode.Combo && minion.Position.Distance(ObjectManager.Player.Position) < 650 && ManaManager.CanUseMana(Orbwalking.OrbwalkingMode.LastHit) && (_recentFarmTarget == null || minion.NetworkId != _recentFarmTarget.NetworkId))
            {
                SafeCast(() => Spell.Cast(minion as Obj_AI_Base));
            }
        }

        public override void Gapcloser(IMainContext context, ComboProvider combo, ActiveGapcloser gapcloser)
        {
            if (_brandQ.HasBeenSafeCast()) return;
            Cast(gapcloser.Sender);
            _brandQ.Cast(gapcloser.Sender);

        }

        public override void Interruptable(IMainContext context, ComboProvider combo, Obj_AI_Hero sender, ComboProvider.InterruptableSpell interruptableSpell)
        {
            if (!_eInterrupt.GetValue<bool>() || _brandQ.HasBeenSafeCast() || HasBeenSafeCast() || sender.Distance(ObjectManager.Player) > 650) return;
            Cast(sender);
            _brandQ.Cast(sender, true);
        }

        public override int GetPriority()
        {
            var target = Provider.GetTarget();
            return target != null ? (target.HasBuff("brandablaze") ? 0 : 4) : 0;
        }
    }
}
