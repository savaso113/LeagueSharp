using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using TheTwitch.Commons;
using TheTwitch.Commons.ComboSystem;

namespace TheTwitch
{
    class TwitchE : Skill
    {
        public bool Killsteal;
        public bool FarmAssist;
        public bool HarassActivateWhenLeaving;
        public int MinFarmMinions;
        public int MinFarmDamageMinions;
        public Circle DrawRange;

        private static readonly float[] BaseDamage = { 20, 35, 50, 65, 80 };
        private static readonly float[] StackDamage = { 15, 20, 25, 30, 35 };
        private static readonly float[] MaxDamage = { 110, 155, 200, 245, 290 };


        public TwitchE(Spell spell)
            : base(spell)
        {
            Orbwalking.OnNonKillableMinion += OnNotKillableMinion;
        }

        private void OnNotKillableMinion(AttackableUnit minion)
        {
            if (!FarmAssist || minion.Position.Distance(ObjectManager.Player.Position) > 1100 || !ManaManager.CanUseMana(Orbwalking.OrbwalkingMode.LastHit)) return;
            var target = (Obj_AI_Base)minion;
            if (GetActivateDamage(target, target.GetBuffCount("twitchdeadlyvenom")) > minion.Health)
            {
                SafeCast();
            }
        }

        public override void Cast(Obj_AI_Hero target, bool force = false)
        {  
            if (Killsteal)
            {
                foreach (var enemy in HeroManager.Enemies)
                {
                    if (!enemy.IsValidTarget(1100) || enemy.NetworkId == target.NetworkId) continue;
                    var targetBuffCountKs = target.GetBuffCount("twitchdeadlyvenom");
                    if (targetBuffCountKs == 0) continue;
                    if (GetPassiveAndActivateDamage(target, targetBuffCountKs) > enemy.Health)
                        SafeCast();

                }
            }

            var targetBuffCount = target.GetBuffCount("twitchdeadlyvenom");
            if (targetBuffCount == 0 || !target.IsValidTarget(1100)) return;
            if (GetPassiveAndActivateDamage(target, targetBuffCount) > target.Health)
                SafeCast();
        }

        public override void Harass(ComboProvider combo, Obj_AI_Hero target)
        {
            base.Harass(combo, target);
            if (HarassActivateWhenLeaving && target.HasBuff("twitchdeadlyvenom") && target.Distance(ObjectManager.Player) > 550 + target.BoundingRadius)
                SafeCast();
        }

        private float GetPassiveAndActivateDamage(Obj_AI_Hero target, int targetBuffCount = 0)
        {
            if (targetBuffCount == 0) return 0;
            return (float)(ObjectManager.Player.CalcDamage(target, Damage.DamageType.True, GetPoisonTickDamage()) + GetActivateDamage(target, targetBuffCount));
        }

        private float GetActivateDamage(Obj_AI_Base target, int targetBuffCount = 0)
        {
            if (targetBuffCount == 0) return 0;
            return (float)ObjectManager.Player.CalcDamage(target, Damage.DamageType.Physical, Math.Min(MaxDamage[Spell.Level - 1] + ObjectManager.Player.TotalAttackDamage * 1.5f, targetBuffCount * (StackDamage[Spell.Level - 1] + ObjectManager.Player.TotalAttackDamage * 0.15f) + BaseDamage[Spell.Level - 1]));
        }

        private static float GetRemainingPoisonDamage(Obj_AI_Base target)
        {
            var buff = target.GetBuff("twitchdeadlyvenom");
            if (buff == null) return 0f;
            return ((int)(buff.EndTime - Game.Time) + 1) * GetPoisonTickDamage();
        }

        private static int GetPoisonTickDamage()
        {
            if (ObjectManager.Player.Level > 16) return 6;
            if (ObjectManager.Player.Level > 12) return 5;
            if (ObjectManager.Player.Level > 8) return 4;
            if (ObjectManager.Player.Level > 4) return 3;
            return 2;
        }

        public override void LaneClear(ComboProvider combo, Obj_AI_Hero target)
        {
            var killable = 0;
            var poison = 0;
            var minions = MinionManager.GetMinions(1100, MinionTypes.All, MinionTeam.NotAlly);
            foreach (var minion in minions)
            {
                var buffCount = minion.GetBuffCount("twitchdeadlyvenom");
                if (buffCount == 0) continue;
                poison++;
                if (minion.Health < GetActivateDamage(minion, buffCount))
                    killable++;
            }
            if (MinFarmMinions <= killable || MinFarmDamageMinions <= poison)
            {
                SafeCast();
            }
            base.LaneClear(combo, target);
        }

        public override void Draw()
        {
            if (DrawRange.Active)
                Render.Circle.DrawCircle(ObjectManager.Player.Position, 1100, DrawRange.Color);
        }

        public override int GetPriority()
        {
            return 4;
        }
    }
}
