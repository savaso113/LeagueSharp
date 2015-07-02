using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using TheGaren.Commons.ComboSystem;

namespace TheGaren
{
    class GarenQ : Skill
    {
        public bool OnlyAfterAuto;
        private bool _recentAutoattack;
        public bool UseWhenOutOfRange;

        public GarenQ(Spell spell)
            : base(spell)
        {
            Orbwalking.AfterAttack += OnAfterAttack;
            HarassEnabled = false;
            OnlyUpdateIfTargetValid = false;
        }

        private void OnAfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            _recentAutoattack = true;
        }

        public override void Update(Orbwalking.OrbwalkingMode mode, ComboProvider combo, Obj_AI_Hero target)
        {
            base.Update(mode, combo, target);
            _recentAutoattack = false;
        }


        public override void Cast(Obj_AI_Hero target, bool force = false)
        {
            if (!force)
            {
                var buff = ObjectManager.Player.GetBuff("GarenE");
                if (buff != null && buff.EndTime - Game.Time > 0.75f * (Spell.Level + 1) + 0.5f) return;
            }
            var nearEnemyCount = ObjectManager.Player.CountEnemiesInRange(ObjectManager.Player.AttackRange * 2);
            if (nearEnemyCount > 0 && (!OnlyAfterAuto || _recentAutoattack) || nearEnemyCount == 0 && UseWhenOutOfRange)
            {
                SafeCast();
                Orbwalking.ResetAutoAttackTimer();
            }
        }

        public override void LaneClear(ComboProvider combo, Obj_AI_Hero target)
        {
            if (_recentAutoattack)
            {
                SafeCast();
                Orbwalking.ResetAutoAttackTimer();
            }
        }

        public override void Interruptable(ComboProvider combo, Obj_AI_Hero sender, ComboProvider.InterruptableSpell interruptableSpell, float endTime)
        {
            if (endTime - Game.Time > Math.Max(sender.Distance(ObjectManager.Player) - Orbwalking.GetRealAutoAttackRange(sender), 0)/ObjectManager.Player.MoveSpeed + 0.5f)
            {
                SafeCast();
                Orbwalking.Orbwalk(sender, sender.Position);
            }
        }

        public override int GetPriority()
        {
            return 2;
        }
    }
}
