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
            if (!OnlyAfterAuto || _recentAutoattack || ObjectManager.Player.CountEnemiesInRange(ObjectManager.Player.AttackRange * 2) == 0 && UseWhenOutOfRange)
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

        public override int GetPriority()
        {
            return 2;
        }
    }
}
