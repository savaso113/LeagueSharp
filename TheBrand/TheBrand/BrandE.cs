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
        private Obj_AI_Base _recentFarmTarget;
        public bool UseMinions;
        public bool FarmAssist;
        public bool Killsteal;
        public bool KillstealCombo;
        public bool InterruptE;

        public BrandE(Spell spell)
            : base(spell)
        {
        }


        public override void Initialize(ComboProvider combo)
        {
            _brandQ = combo.GetSkill<BrandQ>();
            Orbwalking.OnNonKillableMinion += OnMinionUnkillable;
            Orbwalking.BeforeAttack += Orbwalking_BeforeAttack;
            base.Initialize(combo);
        }

        void Orbwalking_BeforeAttack(Orbwalking.BeforeAttackEventArgs args)
        {
            _recentFarmTarget = args.Target.Type == GameObjectType.obj_AI_Base ? (Obj_AI_Base)args.Target : _recentFarmTarget;
        }

        public override void Update(Orbwalking.OrbwalkingMode mode, ComboProvider combo, Obj_AI_Hero target)
        {
            if (Killsteal && (mode == Orbwalking.OrbwalkingMode.Combo || !KillstealCombo))
                foreach (var enemy in HeroManager.Enemies)
                {
                    if (enemy.Distance(ObjectManager.Player) > 650 || ObjectManager.Player.GetSpellDamage(enemy, SpellSlot.E) < enemy.Health + enemy.MagicShield + enemy.AttackShield) continue;
                    Obj_AI_Hero currentEnemy = enemy;
                    SafeCast(currentEnemy);
                }
            base.Update(mode, combo, target);
        }

        public override void Cast(Obj_AI_Hero target, bool force = false)
        {
            var distance = target.Distance(ObjectManager.Player); //Todo: make him use fireminions even in range, just for showoff and potential AOE. Check if hes on fire too though
            if (distance < 950 && distance > 650 && UseMinions)
            {
                var fireMinion = MinionManager.GetMinions(650, MinionTypes.All, MinionTeam.Enemy, MinionOrderTypes.None).Where(minion => minion.HasBuff("brandablaze") && minion.Distance(target) < 300).MinOrDefault(minion => minion.Distance(target));
                if (fireMinion != null)
                {
                    Console.WriteLine("e cast 123");
                    SafeCast(fireMinion);
                    _brandQ.Cast(target, true);
                }
            }
            if (distance < 650)
            {
                Console.WriteLine("e cast 123");
                SafeCast(target);
                _brandQ.Cast(target, true);
            }
        }



        public override void LaneClear(ComboProvider combo, Obj_AI_Hero target)
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
            SafeCast(bestMinion);

            base.LaneClear(combo, target);
        }

        void OnMinionUnkillable(AttackableUnit minion)
        {
            if (!FarmAssist) return;
            if (Provider.Orbwalker.ActiveMode != Orbwalking.OrbwalkingMode.Combo && minion.Position.Distance(ObjectManager.Player.Position) < 650 && ManaManager.CanUseMana(Orbwalking.OrbwalkingMode.LastHit) && (_recentFarmTarget == null || minion.NetworkId != _recentFarmTarget.NetworkId))
            {
               
                SafeCast(minion as Obj_AI_Base);
            }
        }

        public override void Gapcloser(ComboProvider combo, ActiveGapcloser gapcloser)
        {
            if (_brandQ.HasBeenSafeCast()) return;
            Cast(gapcloser.Sender, true);
            _brandQ.Cast(gapcloser.Sender, true);
        }

        public override void Interruptable(ComboProvider combo, Obj_AI_Hero sender, ComboProvider.InterruptableSpell interruptableSpell)
        {
            if (!InterruptE || _brandQ.HasBeenSafeCast() || HasBeenSafeCast() || sender.Distance(ObjectManager.Player) > 650) return;
            Cast(sender, true);
            _brandQ.Cast(sender, true);
        }

        public override int GetPriority()
        {
            var target = Provider.GetTarget();
            return target != null ? (target.HasBuff("brandablaze") ? 0 : 4) : 0;
        }
    }
}
