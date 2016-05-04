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
        public bool Farm;
        //private int _recentAttacked;

        public CassE(SpellSlot slot)
            : base(slot)
        {
            Range = Instance.SData.CastRange + 50;
            SetTargetted(0.2f, float.MaxValue);
            //Orbwalking.AfterAttack += AfterAutoAttack;
            UseManaManager = false;
        }

        //private void AfterAutoAttack(AttackableUnit unit, AttackableUnit target)
        //{
        //    if (unit.IsMe)
        //        _recentAttacked = target.NetworkId;
        //}

        public override void Lasthit()
        {
            var killableMinion = MinionManager.GetMinions(Range, MinionTypes.All, MinionTeam.NotAlly).FirstOrDefault(minion =>
            {
                var hpred = HealthPrediction.GetHealthPrediction(minion, 300);
                if (hpred <= 0) return false;
                return IsKillable(minion) || minion.Team == GameObjectTeam.Neutral;
            });

            if (killableMinion == null) return;

            Cast(killableMinion);
        }

        public override void Execute(Obj_AI_Hero target)
        {
            Cast(target);
        }

        public override void Harass(Obj_AI_Hero target)
        {
            if (ManaManager.CanUseMana(Orbwalking.OrbwalkingMode.Mixed))
                base.Harass(target);
        }

        public override void LaneClear()
        {
            if (!ManaManager.CanUseMana(Orbwalking.OrbwalkingMode.LaneClear)) return;

            var clearMinion = MinionManager.GetMinions(Range, MinionTypes.All, MinionTeam.NotAlly).FirstOrDefault();

            if (clearMinion != null)
                Cast(clearMinion);
        }

        public override float GetDamage(Obj_AI_Hero enemy)
        {
            return base.GetDamage(enemy) * 2;
        }

        public override int GetPriority()
        {
            return 1;
        }
    }
}
