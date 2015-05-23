using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using TheBrand.ComboSystem;

namespace TheBrand
{
    class BrandW : Skill
    {
        private BrandE _brandE;
        private BrandQ _brandQ;
        private MenuItem _wInterrupt, _eInterrupt, _wPrediction, _waveClearTargets, _harassHitchance;

        public BrandW(Spell spell)
            : base(spell)
        {
            spell.SetSkillshot(1.15f, 230f, int.MaxValue, false, SkillshotType.SkillshotCircle); // adjusted the range, for some reason the prediction was off, and missplaced it alot
        }

        public override void Initialize(IMainContext context, ComboProvider combo)
        {
            _brandE = combo.GetSkill<BrandE>();
            _brandQ = combo.GetSkill<BrandQ>();
            _wInterrupt = context.GetRootMenu().GetMenuItem("Interrupter.WUsage");
            _eInterrupt = context.GetRootMenu().GetMenuItem("Interrupter.EUsage");
            _wPrediction = context.GetRootMenu().GetMenuItem("Drawing.WPrediction");
            _waveClearTargets = context.GetRootMenu().GetMenuItem("Laneclear.MinWtargets");
            _harassHitchance = context.GetRootMenu().GetMenuItem("Harass.Hitchance");
            Drawing.OnDraw += Draw;
            base.Initialize(context, combo);
        }

        private void Draw(EventArgs args)
        {
            var drawingOption = _wPrediction.GetValue<Circle>();
            if (!drawingOption.Active) return;
            var target = Provider.GetTarget();
            if (Context.GetOrbwalker().ActiveMode == Orbwalking.OrbwalkingMode.Combo || target == null) return;
            var prediction = Spell.GetPrediction(target, true);
            if(prediction.CastPosition.Distance(ObjectManager.Player.Position) < 900)
            Render.Circle.DrawCircle(prediction.CastPosition, 240f, drawingOption.Color);
        }

        public override void Cast(Obj_AI_Hero target, bool force = false, HitChance minChance = HitChance.Low)
        {
            if (target != null && !HasBeenSafeCast() && target.Distance(ObjectManager.Player) < 900)
            {
                var prediction = Spell.GetPrediction(target, true);
                if (prediction.Hitchance >= minChance)
                    SafeCast(() =>
                    {
                        Spell.Cast(prediction.CastPosition);

                        //Todo: un-comment below code, error was: target was often out of range, especially on a chase. Will do faster combo though
                        //if (prediction.Hitchance >= HitChance.High && target.Position.Distance(ObjectManager.Player.Position) > 650)
                        //    _brandQ.Cast(target, true);
                    });
            }
        }

        public override void Harass(IMainContext context, ComboProvider combo, Obj_AI_Hero target)
        {

            Cast(target, false, (HitChance)Enum.Parse(typeof(HitChance), _harassHitchance.GetValue<StringList>().SelectedValue, true));
        }

        public override float GetDamage(Obj_AI_Hero enemy)
        {
            var baseDamage = base.GetDamage(enemy);
            return enemy.HasBuff("brandablaze") || _brandE.CanBeCast() && enemy.Distance(ObjectManager.Player) < 650 ? baseDamage * 1.25f : baseDamage;
        }

        public override void LaneClear(IMainContext context, ComboProvider combo, Obj_AI_Hero target)
        {
            if (HasBeenSafeCast()) return;
            var location = Spell.GetCircularFarmLocation(MinionManager.GetMinions(900 + 120));
            if (location.MinionsHit >= _waveClearTargets.GetValue<Slider>().Value)
                SafeCast(() => Spell.Cast(location.Position));

        }

        public override void Interruptable(IMainContext context, ComboProvider combo, Obj_AI_Hero sender, ComboProvider.InterruptableSpell interruptableSpell)
        {
            var distance = sender.Distance(ObjectManager.Player);
            if (distance > 900 || _brandE.Spell.Instance.State == SpellState.Ready && _eInterrupt.GetValue<bool>() && distance < 650 || sender.HasBuff("brandablaze") || _brandQ.HasBeenSafeCast() || !_wInterrupt.GetValue<bool>()) return;
            var stunprediction = _brandQ.Spell.GetPrediction(sender);
            if (stunprediction.CollisionObjects.Count > 0) return;
            Cast(sender);
            _brandQ.Spell.Cast(stunprediction.CastPosition);
        }

        public override int GetPriority()
        {
            return 2;
        }
    }
}
