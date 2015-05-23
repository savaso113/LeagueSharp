using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LeagueSharp;
using LeagueSharp.Common;
using TheBrand.ComboSystem;

namespace TheBrand
{
    class BrandR : Skill // 550 bounce range
    {
        private Skill[] _otherSkills;
        private BrandE _brandE;
        private BrandQ _brandQ;
        public bool UseBridgeUlt;
        public bool RiskyUlt;
        public bool UltNonKillable;
        public bool AntiOverkill;
        public bool IgnoreAntiOverkillOnFlee;
        public float OverkillPercent;
        public int MinBounceTargets;
        private readonly float[] _maxDamage = { 450, 750, 1050 };
        public float MaxDamage { get { return Spell.Level > 0 ? _maxDamage[Spell.Level - 1] + ObjectManager.Player.BaseAbilityDamage * 1.5f : 0; } } //TODO: check if BaseAbilityDamage is really AP

        public BrandR(Spell spell)
            : base(spell)
        {
        }

        public override void Initialize(IMainContext context, ComboProvider combo)
        {
            var skills = combo.GetSkills().ToList();
            skills.Remove(this);
            _otherSkills = skills.ToArray();
            _brandE = combo.GetSkill<BrandE>();
            _brandQ = combo.GetSkill<BrandQ>();
            base.Initialize(context, combo);
        }


        public override void Cast(Obj_AI_Hero target, bool force = false, HitChance minChance = HitChance.Low)
        {
            if (target == null)
                target = TargetSelector.GetTarget(1200, TargetSelector.DamageType.Magical);
            if (HasBeenSafeCast() || target == null) return;

            var dmgPerBounce = ObjectManager.Player.GetSpellDamage(target, Spell.Slot);
            if (dmgPerBounce > target.Health && ObjectManager.Player.GetAutoAttackDamage(target, true) < target.Health && ((_otherSkills.All(skill => skill.Spell.Instance.State != SpellState.Ready && skill.Spell.Instance.State != SpellState.Surpressed && !skill.IsInSafeCast(1)) /*|| target.Distance(ObjectManager.Player) > 650*/) || ObjectManager.Player.GetSpellDamage(target, SpellSlot.Q) + dmgPerBounce + ObjectManager.Player.GetAutoAttackDamage(target) > target.Health && !target.HasBuff("brandablaze") && target.Distance(ObjectManager.Player) < 750))
            {
                if (target.Distance(ObjectManager.Player) > 750)
                    TryBridgeUlt(target);
                else if (ObjectManager.Player.HealthPercent - target.HealthPercent < OverkillPercent || !AntiOverkill || IgnoreAntiOverkillOnFlee && target.Distance(ObjectManager.Player) > ObjectManager.Player.AttackRange)
                {
                    if (HealthPrediction.GetHealthPrediction(target, 1) > 0)
                        SafeCast(() => Spell.Cast(target));
                }
            }


            // if (target.Distance(ObjectManager.Player) > 750) return;
            var inBounce = new bool[HeroManager.Enemies.Count];
            var canBounce = BounceCheck(target, inBounce);
            if (canBounce)
            {
                var inBounceEnemies = HeroManager.Enemies.Where(enemy => inBounce[HeroManager.Enemies.IndexOf(enemy)]).ToArray();
                var distance = target.Distance(ObjectManager.Player);

                var bounceCount = inBounce.Count(item => item);
                if (bounceCount <= 1) return;
                //Console.WriteLine("bounce r " + bounceCount);

                if ((inBounceEnemies.Any(enemy => (dmgPerBounce > enemy.Health && MaxDamage > enemy.Health)) && (bounceCount == 2 || RiskyUlt)))
                {
                    TryUlt(target, inBounceEnemies, distance);
                }
                else if (bounceCount == 2 && dmgPerBounce * 3 > target.Health && MaxDamage > target.Health && distance < 750 && RiskyUlt)
                {
                    SafeCast(() => Spell.Cast(target));
                }
                else if (dmgPerBounce * 2 > target.Health && MaxDamage > target.Health)
                {
                    TryUlt(target, inBounceEnemies, distance);
                }
                else if (UltNonKillable && MinBounceTargets <= bounceCount)
                {
                    TryUlt(target, inBounceEnemies, distance, false);
                }
            }
        }

        private void TryUlt(Obj_AI_Hero target, Obj_AI_Hero[] alternate, float distance, bool bridgeUlt = true)
        {
            if (distance > 750)
            {
                var alternateTarget = alternate.FirstOrDefault(enemy => enemy.Distance(ObjectManager.Player) < 750);
                if (alternateTarget == null && bridgeUlt)
                {
                    TryBridgeUlt(target);
                }
                else
                {
                    SafeCast(() => Spell.Cast(alternateTarget));
                }
            }
            else
                SafeCast(() => Spell.Cast(target));
        }

        private void TryBridgeUlt(Obj_AI_Hero target)
        {
            if (!UseBridgeUlt) return;
            //Console.WriteLine("BRIDGE ULT INIT");
            #region bridge ult
            if (target.Distance(ObjectManager.Player) > 750 && (_brandE.Spell.Instance.State == SpellState.Ready || _brandQ.Spell.Instance.State == SpellState.Ready))
            {
                var bridgeSpellSlot = _brandE.Spell.Instance.State == SpellState.Ready ? _brandE.Spell.Slot : _brandQ.Spell.Slot;
                var bridgeSpellRange = bridgeSpellSlot == SpellSlot.E ? 650 : 1000;
                Obj_AI_Base bridgeUnit = null;
                float bridgeUnitDistance = 0f;
                var minions = MinionManager.GetMinions(650);
                if (minions != null && minions.Count > 0)
                {
                    //Console.WriteLine("BRIDGE CHECK MINIONS");
                    var unit = GetMinimumDistanceUnit(target, minions, bridgeSpellSlot);
                    if (unit != null && unit.Distance(target) < 500)
                    {
                        bridgeUnit = unit;
                        bridgeUnitDistance = unit.Distance(ObjectManager.Player);
                    }
                }
                if (bridgeUnit == null)
                {
                    //Console.WriteLine("BRIDGE CHECK HEROS");
                    var unit = GetMinimumDistanceUnit(target, HeroManager.Enemies.Where(enemy => enemy.Distance(ObjectManager.Player) < bridgeSpellRange), bridgeSpellSlot);
                    if (unit != null && unit.Distance(target) < 500)
                    {
                        bridgeUnit = unit;
                        bridgeUnitDistance = unit.Distance(ObjectManager.Player);
                    }
                }


                if (bridgeUnit != null)
                {
                    //Console.WriteLine("BRIDGE HAS BRIDGE");
                    if (bridgeSpellSlot == SpellSlot.E && bridgeUnitDistance < 650)
                    {
                        _brandE.Spell.Cast(bridgeUnit);
                        SafeCast(() => Spell.Cast(bridgeUnit));
                    }
                    else
                    {
                        var prediction = _brandQ.Spell.GetPrediction(bridgeUnit);
                        if (prediction.CollisionObjects.Count == 0)
                        {
                            _brandQ.Spell.Cast(prediction.CastPosition);
                            SafeCast(() => Spell.Cast(bridgeUnit));
                            //Console.WriteLine("BRIDGE FIRE");
                        }
                        else
                        {
                            var collidingObj = prediction.CollisionObjects.First();
                            if (collidingObj.Distance(target) < 500)
                            {
                                //Console.WriteLine("BRIDGE FIRE");
                                _brandQ.Spell.Cast(prediction.CastPosition);
                                SafeCast(() => Spell.Cast(bridgeUnit));
                            }
                        }
                    }
                }
            }
            #endregion

        }


        private bool BounceCheck(Obj_AI_Hero target, bool[] inBounce)
        {
            for (int i = 0; i < HeroManager.Enemies.Count; i++)
            {
                if (!inBounce[i] && HeroManager.Enemies[i].Distance(target) < 500)
                {
                    var minions = MinionManager.GetMinions(target.Position, 500);
                    if (minions.Any(minion => !minion.HasBuff("brandablaze")))
                        return false;
                    inBounce[i] = true;
                    var ret = BounceCheck(HeroManager.Enemies[i], inBounce);
                    if (!ret)
                        return false;
                }
            }
            return true;
        }

        private Obj_AI_Base GetMinimumDistanceUnit(Obj_AI_Hero target, IEnumerable<Obj_AI_Base> units, SpellSlot bridge)
        {
            float minDistance = float.MaxValue;
            Obj_AI_Base minUnit = null;
            foreach (var objAiBase in units)
            {
                var distance = objAiBase.Distance(target);
                if (!(distance < minDistance && ObjectManager.Player.GetSpellDamage(objAiBase, bridge) < objAiBase.Health)) continue;
                minDistance = distance;
                minUnit = objAiBase;
            }
            return minUnit;
        }


        public override int GetPriority()
        {
            return 5;
        }
    }
}
