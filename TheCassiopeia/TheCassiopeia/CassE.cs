using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using TheCassiopeia.Commons;
using TheCassiopeia.Commons.ComboSystem;

namespace TheCassiopeia
{
    class CassE : Skill
    {
        public bool WaveclearPush;
        public bool Farm;
        public bool FarmAssist;
        public int FarmNonPoisonedPercent;


        public CassE(SpellSlot slot)
            : base(slot)
        {
            SetTargetted(0.2f, float.MaxValue);
            UseManaManager = false;
        }

        public override void Initialize(ComboProvider combo)
        {
            Orbwalking.OnNonKillableMinion += OnMinionUnkillable;
            base.Initialize(combo);
        }

        private void OnMinionUnkillable(AttackableUnit minion)
        {
            if (!FarmAssist) return;
            if (((Obj_AI_Base)minion).IsPoisoned() || ObjectManager.Player.ManaPercent < FarmNonPoisonedPercent)
                Cast((Obj_AI_Base)minion);
        }

        public override void Update(Orbwalking.OrbwalkingMode mode, ComboProvider combo, Obj_AI_Hero target)
        {
            if ((mode == Orbwalking.OrbwalkingMode.LastHit || mode == Orbwalking.OrbwalkingMode.Mixed || (mode == Orbwalking.OrbwalkingMode.LaneClear && !ManaManager.CanUseMana(Orbwalking.OrbwalkingMode.LaneClear))) && Farm)
            {
                var killableMinion = MinionManager.GetMinions(700, MinionTypes.All, MinionTeam.NotAlly).Where(minion => minion.IsPoisoned()).FirstOrDefault(minion => IsKillable(minion));
                if (killableMinion == null && FarmNonPoisonedPercent > ObjectManager.Player.ManaPercent)
                    killableMinion = MinionManager.GetMinions(700, MinionTypes.All, MinionTeam.NotAlly).FirstOrDefault(minion => IsKillable(minion));

                if (killableMinion != null)
                    Cast(killableMinion);
            }

            base.Update(mode, combo, target);
        }

        public override void Execute(Obj_AI_Hero target)
        {
            if (Provider.IsMarked(target) || target.IsPoisoned() || IsKillable(target))
            {
                Cast(target);
            }
        }

        public override void LaneClear(ComboProvider combo, Obj_AI_Hero target)
        {
            var killableMinion = WaveclearPush && ManaManager.CanUseMana(Orbwalking.OrbwalkingMode.LaneClear) ? MinionManager.GetMinions(700, MinionTypes.All, MinionTeam.NotAlly).FirstOrDefault(minion => minion.IsPoisoned()) : MinionManager.GetMinions(700, MinionTypes.All, MinionTeam.NotAlly).Where(minion => minion.IsPoisoned()).FirstOrDefault(minion => IsKillable(minion));

            if (killableMinion != null)
                Cast(killableMinion);
        }

        public override float GetDamage(Obj_AI_Hero enemy)
        {
            return base.GetDamage(enemy) * 4;
        }

        public override int GetPriority()
        {
            return 3;
        }
    }
}
