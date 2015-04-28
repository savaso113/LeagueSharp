using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;

namespace TheGaren.ComboSystem
{
    abstract class Skill
    {
        public bool ComboEnabled = true, LaneclearEnabled = true, HarassEnabled = true; // If this skill should be used in Combo, ...
        private float _castTime;
        private string _castName;
        private Spell _castSpell;
        protected const float SafeCastMaxTime =0.5f;
        protected Spell Spell { get; private set; }
        protected ComboProvider Provider { get; private set; }

        protected Skill(Spell spell)
        {
            Spell = spell;
        }

        /// <summary>
        /// The safe cast mechanism is used to set a skill "on cooldown" even though the server hasn't even sent the cooldown update.
        /// Results in less ability spam, (faster execution ?) and it's easier to debug
        /// </summary>
        protected void SafeCast(Action spellCastAction, string name = "")
        {
            if (string.IsNullOrEmpty(name))
                name = Spell.Instance.Name;
            if (HasBeenSafeCast(name))
                return;
            _castTime = Game.Time;
            _castName = name;
            _castSpell = Spell;
            spellCastAction();
        }

        /// <summary>
        /// The safe cast mechanism is used to set a skill "on cooldown" even though the server hasn't even sent the cooldown update.
        /// Results in less ability spam, (faster execution ?) and it's easier to debug
        /// </summary>
        protected void SafeCast(Spell spell, Action spellCastAction, string name = "")
        {
            if (string.IsNullOrEmpty(name))
                name = spell.Instance.Name;
            if (HasBeenSafeCast(name))
                return;
            _castTime = Game.Time;
            _castName = name;
            _castSpell = spell;
            spellCastAction();
        }

        /// <summary>
        /// Add Initialisation logic in sub class. Called by ComboProvider.SetActive(skill)
        /// </summary>
        /// <param name="combo"></param>
        public virtual void Initialize(IMainContext context, ComboProvider combo)
        {
        }

        public virtual void SetEnabled(Orbwalking.OrbwalkingMode mode, bool enabled)
        {
            switch (mode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    ComboEnabled = enabled;
                    break;
                case Orbwalking.OrbwalkingMode.LaneClear:
                    LaneclearEnabled = enabled;
                    break;
                case Orbwalking.OrbwalkingMode.Mixed:
                    HarassEnabled = enabled;
                    break;
            }
        }

        public bool GetEnabled(Orbwalking.OrbwalkingMode mode)
        {
            switch (mode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    return ComboEnabled;
                case Orbwalking.OrbwalkingMode.LaneClear:
                    return LaneclearEnabled;
                case Orbwalking.OrbwalkingMode.Mixed:
                    return HarassEnabled;
            }
            return false;
        }


        public void Update(Orbwalking.OrbwalkingMode mode, IMainContext context, ComboProvider combo)
        {
            switch (mode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    if (ComboEnabled)
                        Combo(context, combo);
                    break;
                case Orbwalking.OrbwalkingMode.LaneClear:
                    if (LaneclearEnabled)
                        LaneClear(context, combo);
                    break;
                case Orbwalking.OrbwalkingMode.Mixed:
                    if (HarassEnabled)
                        Harass(context, combo);
                    break;
            }
        }

        public abstract void Combo(IMainContext context, ComboProvider combo);
        public virtual void LaneClear(IMainContext context, ComboProvider combo) { }
        public virtual void Harass(IMainContext context, ComboProvider combo) { }


        public virtual float GetDamage(Obj_AI_Hero enemy)
        {
            return Spell.Instance.State == SpellState.Ready ? (float) ObjectManager.Player.GetSpellDamage(enemy, Spell.Slot) : 0f;
        }

        /// <summary>
        /// If this returns true an other skill of the same or lower priority can't grab control. If this skill
        /// has control but this return false, the control will be terminated.
        /// </summary>
        /// <returns></returns>
        public abstract bool NeedsControl();

        public abstract int GetPriority();

        /// <summary>
        /// Gets called if some other skill wants total control OR this skill doesn't need control even though it has it.
        /// </summary>
        /// <returns></returns>
        public virtual bool TryTerminate(IMainContext context) { return true; }

        /// <summary>
        /// If the spell seems available. Not SafeCast compatible
        /// </summary>
        /// <returns></returns>
        public virtual bool CanExecute()
        {
            return Spell.Instance.State == SpellState.Ready;
        }

        /// <summary>
        /// If the spell as been cast. SafeCast compatible
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        protected bool HasBeenSafeCast(string name)
        {
            return (_castName == name && Game.Time - _castTime < SafeCastMaxTime) || (_castSpell != null && Spell.Instance.State == SpellState.Cooldown) || (_castSpell != null && Spell.Instance.Name != _castName);
        }

        /// <summary>
        /// If the spell as been cast. SafeCast compatible
        /// </summary>
        /// <param name="name"></param>
        /// <param name="castTime"></param>
        /// <returns></returns>
        protected bool IsInSafeCast(string name, float castTime = SafeCastMaxTime)
        {
            return (!string.IsNullOrEmpty(_castName) && _castName != name) || Game.Time - _castTime < castTime;
        }
    }
}
