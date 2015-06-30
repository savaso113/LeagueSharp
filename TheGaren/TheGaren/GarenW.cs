using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using TheGaren.Commons;
using TheGaren.Commons.ComboSystem;

namespace TheGaren
{
    class GarenW : Skill
    {
        public bool UseAlways;
        public float MinDamagePercent; // per sec
        public bool UseOnUltimates;
        private float _healthTime;
        private float _healthValue;
        private bool _shouldUse;

        public GarenW(Spell spell)
            : base(spell)
        {
            Spellbook.OnCastSpell += OnSpellcast;
            OnlyUpdateIfTargetValid = false;
        }

        private void OnSpellcast(Spellbook sender, SpellbookCastSpellEventArgs args)
        {
            if (sender.Owner.IsEnemy && args.Slot == SpellSlot.R && UseOnUltimates)
            {
                var halfLineLength = (args.EndPosition - args.StartPosition).Length() / 2f;
                if (ObjectManager.Player.Position.Distance(args.StartPosition) > halfLineLength && ObjectManager.Player.Position.Distance(args.EndPosition) > halfLineLength) return;
                if (UseAlways)
                    SafeCast();
                else
                    _shouldUse = true;
            }
        }

        public override void Update(Orbwalking.OrbwalkingMode mode, ComboProvider combo, Obj_AI_Hero target)
        {
            if (Game.Time - _healthTime > 1)
            {
                _healthTime = Game.Time;
                _healthValue = ObjectManager.Player.Health;
            }

            base.Update(mode, combo, target);
            if (UseAlways && ShouldUse())
                SafeCast();
            _shouldUse = false;
        }

        public override void Cast(Obj_AI_Hero target, bool force = false)
        {
            if (!UseAlways && ShouldUse())
                SafeCast();
        }

        private bool ShouldUse()
        {
            return ObjectManager.Player.GetHealthPercent(ObjectManager.Player.Health - HealthPrediction.GetHealthPrediction(ObjectManager.Player, 1)) > MinDamagePercent || ObjectManager.Player.GetHealthPercent(_healthValue - ObjectManager.Player.Health) > MinDamagePercent || _shouldUse;
        }

        public override int GetPriority()
        {
            return 3;
        }
    }
}
