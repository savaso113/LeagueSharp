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
    class GarenR : Skill
    {
        public bool Killsteal;

        public GarenR(Spell spell)
            : base(spell) { }

        public override void Cast(Obj_AI_Hero target, bool force = false)
        {
            if (Killsteal)
            {
                foreach (var enemy in HeroManager.Enemies.Where(enemy => enemy.IsValidTarget(375)))
                {
                    if (Spell.IsKillable(enemy))
                        SafeCast(enemy);
                }
            }
            else if (Spell.IsKillable(target) && HealthPrediction.GetHealthPrediction(target, 1000) > ObjectManager.Player.GetAutoAttackDamage(target) && !Provider.ShouldBeDead(target))
            {
                SafeCast(target);
            }
        }

        public override int GetPriority()
        {
            return 4;
        }
    }
}
