using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using TheKalista.Commons;
using TheKalista.Commons.ComboSystem;
using TheKalista.Commons.Debug;

namespace TheKalista
{
    class KalistaE : Skill
    {
        public bool FarmAssist;
        public int FarmAssistMana;
        public int MinLaneclear;
        public bool StealJungle;

        public KalistaE(SpellSlot slot, float range, TargetSelector.DamageType damageType)
            : base(slot, range, damageType)
        {
        }

        public KalistaE(SpellSlot slot)
            : base(slot)
        {
        }

        public override void Initialize(ComboProvider combo)
        {
            Obj_AI_Base recentTarget = null;
            var dead = false;

            Orbwalking.AfterAttack += (sender, target) =>
            {
                if (sender.IsMe)
                {
                    var targetObj = target as Obj_AI_Base;
                    if (targetObj == null) return;
                    recentTarget = targetObj;
                    dead = HealthPrediction.GetHealthPrediction(targetObj, 500) - ObjectManager.Player.GetAutoAttackDamage(targetObj) <= 0;
                }
            };

            Orbwalking.OnNonKillableMinion += minion =>
            {
                if (minion.NetworkId == recentTarget.NetworkId && dead) return;
                if (FarmAssist && IsKillable(minion as Obj_AI_Base) && (Provider.Orbwalker.ActiveMode != Orbwalking.OrbwalkingMode.Combo && Provider.Orbwalker.ActiveMode != Orbwalking.OrbwalkingMode.None))
                {
                    if (ObjectManager.Player.ManaPercent > FarmAssistMana)
                        Cast();
                    else if (MinionManager.GetMinions(Range, MinionTypes.All, MinionTeam.NotAlly, MinionOrderTypes.None).Any(min => min.HasBuff("Kalistaexpungemarker") && IsKillable(min) && min.NetworkId != minion.NetworkId))
                        Cast();
                }
            };
            base.Initialize(combo);
        }

        public override void Update(Orbwalking.OrbwalkingMode mode, ComboProvider combo, Obj_AI_Hero target)
        {
            if (mode != Orbwalking.OrbwalkingMode.None && StealJungle && MinionManager.GetMinions(Range, MinionTypes.All, MinionTeam.NotAlly, MinionOrderTypes.None).Any(min => min.HasBuff("Kalistaexpungemarker") && IsKillable(min) && !min.Name.Contains("Mini") && !min.Name.Contains("Dragon") && !min.Name.Contains("Baron")))
                Cast();

            base.Update(mode, combo, target);
        }

        public override void Execute(Obj_AI_Hero target)
        {
            var enemyWithStacks = false;
            var enemyInAutoRange = false;
            var sumDamage = 0f;
            Profiler.StartEndSection("e_collectData");
            foreach (var enemy in HeroManager.Enemies)
            {
                var valid = enemy.IsValidTarget(Range);
                if (valid)
                {
                    if (!enemyInAutoRange && Provider.Orbwalker.InAutoAttackRange(enemy))
                        enemyInAutoRange = true;

                    if (enemy.IsMelee && enemy.AttackRange + ObjectManager.Player.BoundingRadius > ObjectManager.Player.Position.Distance(enemy.Position))
                    {
                        enemyWithStacks = false;
                        SlowWithReset();
                        break;
                    }

                    if (enemy.HasBuff("Kalistaexpungemarker"))
                    {
                        enemyWithStacks = true;
                        sumDamage += GetDamage(enemy);
                    }
                }

            }

            Profiler.StartEndSection("e_useData");
            if (!enemyInAutoRange && enemyWithStacks)
                SlowWithReset();
            else if (sumDamage > KalistaWalker.GetDamageForOneAuto(target, Level))
            {
                Console.WriteLine(sumDamage + " > " + KalistaWalker.GetDamageForOneAuto(target, Level));
                SlowWithReset();
            }

            KillAnyone();
        }

        public override void LaneClear()
        {
            if (TickLimiter.Limit(100, 2))
            {
                var minions = MinionManager.GetMinions(Range, MinionTypes.All, MinionTeam.NotAlly, MinionOrderTypes.None);
                if (minions.Count(min => min.HasBuff("Kalistaexpungemarker") && IsKillable(min)) >= MinLaneclear)
                    Cast();
                else if (minions.Any(min => min.HasBuff("Kalistaexpungemarker") && IsKillable(min) && (min.SkinName.ToLower().Contains("siege") || min.SkinName.ToLower().Contains("super"))))
                    Cast();
            }
        }

        public override void Harass(Obj_AI_Hero target)
        {
            KillAnyone();
            base.Harass(target);
        }

        private void KillAnyone()
        {
            if (HeroManager.Enemies.Any(enemy => enemy.IsValidTarget(Range) && IsKillable(enemy) && !enemy.HasSpellShield() && !TargetSelector.IsInvulnerable(enemy, TargetSelector.DamageType.Physical)))
                Cast();
        }

        private void SlowWithReset()
        {
            foreach (var objectType in MinionManager.GetMinions(Range, MinionTypes.All, MinionTeam.NotAlly, MinionOrderTypes.None))
            {
                if (objectType.HasBuff("Kalistaexpungemarker") && GetDamage(objectType) > objectType.Health + 10f && HealthPrediction.GetHealthPrediction(objectType, (int)(Delay * 1000f)) > 0)
                {
                    Cast();
                    return;
                }
            }

            //if (ObjectManager.Get<Obj_AI_Base>().Any(minion => minion.IsValidTarget(Range) && minion.HasBuff("Kalistaexpungemarker") && GetDamage(minion) > minion.Health + 10f && HealthPrediction.GetHealthPrediction(minion, (int)(Delay * 1000f)) > 0))
            //{
            //    Cast();
            //}
        }

        public override int GetPriority()
        {
            return 4;
        }
    }
}
