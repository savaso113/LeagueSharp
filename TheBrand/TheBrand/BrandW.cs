using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using LeagueSharp;
using LeagueSharp.Common;
using System.Drawing;
using TheBrand.ComboSystem;

namespace TheBrand
{
    class BrandW : Skill
    {
        private BrandE _brandE;
        private BrandQ _brandQ;
        public bool DrawPredictedW, InterruptE, InterruptW;
        public int WaveclearTargets;
        public Color PredictedWColor;

        public BrandW(Spell spell)
            : base(spell)
        {
            spell.SetSkillshot(1.15f, 230f, int.MaxValue, false, SkillshotType.SkillshotCircle); // adjusted the range, for some reason the prediction was off, and missplaced it alot
            IsAreaOfEffect = true;
        }

        public override void Initialize(ComboProvider combo)
        {
            _brandE = combo.GetSkill<BrandE>();
            _brandQ = combo.GetSkill<BrandQ>();
            Drawing.OnDraw += Draw;
            base.Initialize(combo);
        }

        private void Draw(EventArgs args)
        {
            if (!DrawPredictedW) return;
            try
            {
                var target = Provider.GetTarget();
                if (Provider.Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo || target == null) return;
                var prediction = Spell.GetPrediction(target, true);
                if (prediction.CastPosition.Distance(ObjectManager.Player.Position) < 900)
                    Render.Circle.DrawCircle(prediction.CastPosition, 240f, PredictedWColor);
            }
            catch { }
        }

        public override void Cast(Obj_AI_Hero target, bool force = false)
        {
            if (target.Distance(ObjectManager.Player) < 900 && !Provider.ShouldBeDead(target))
            {
                Console.WriteLine("w cast 123");
                SafeCast(target);

                //Todo: un-comment below code, error was: target was often out of range, especially on a chase. Will do faster combo though
                //if (prediction.Hitchance >= HitChance.High && target.Position.Distance(ObjectManager.Player.Position) > 650)
                //    _brandQ.Cast(target, true);

            }
        }

        public override float GetDamage(Obj_AI_Hero enemy)
        {
            var baseDamage = base.GetDamage(enemy);
            return enemy.HasBuff("brandablaze") || _brandE.CanBeCast() && enemy.Distance(ObjectManager.Player) < 650 ? baseDamage * 1.25f : baseDamage;
        }

        public override void LaneClear(ComboProvider combo, Obj_AI_Hero target)
        {
            if (HasBeenSafeCast()) return;
            var locationM = Spell.GetCircularFarmLocation(MinionManager.GetMinions(900 + 120));
            if (locationM.MinionsHit >= WaveclearTargets)
                SafeCast(() =>
                {
                    var location = Spell.GetCircularFarmLocation(MinionManager.GetMinions(900 + 120));
                    if (location.MinionsHit >= WaveclearTargets)
                        Spell.Cast(location.Position);
                });

        }

        public override void Interruptable(ComboProvider combo, Obj_AI_Hero sender, ComboProvider.InterruptableSpell interruptableSpell)
        {
            var distance = sender.Distance(ObjectManager.Player);
            if (distance > 900 || _brandE.Spell.Instance.State == SpellState.Ready && InterruptE && distance < 650 || sender.HasBuff("brandablaze") || !_brandQ.CanBeCast() || !InterruptW) return;
           
            var stunprediction = _brandQ.Spell.GetPrediction(sender);
            if (stunprediction.CollisionObjects.Count > 0) return;

            Cast(sender, true);
            _brandQ.Cast(sender, true);
        }

        public override int GetPriority()
        {
            return 2;
        }
    }
}
