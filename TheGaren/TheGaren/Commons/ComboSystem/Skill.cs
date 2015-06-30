using System;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace TheGaren.Commons.ComboSystem
{
    public abstract class Skill : IComparable<Skill>
    {
        public HitChance MinComboHitchance = HitChance.Low, MinHarassHitchance = HitChance.VeryHigh;
        public bool ComboEnabled = true, LaneclearEnabled = true, HarassEnabled = true; // If this skill should be used in Combo, ...
        private string _castName;
        public Spell Spell { get; private set; }
        protected ComboProvider Provider { get; private set; }
        protected bool UseManaManager;
        public bool SwitchClearToHarassOnTarget = true;
        protected bool OnlyUpdateIfTargetValid = true, OnlyUpdateIfCastable = true;
        public bool IsAreaOfEffect;

        protected Skill(Spell spell)
        {
            Spell = spell;
            if (spell.Slot == SpellSlot.R) HarassEnabled = false;
            UseManaManager = spell.Instance.SData.ManaCostArray.MaxOrDefault((value) => value) > 0;
        }

        #region SafeCast Overloads
        public bool SafeCast(Action action)
        {
            if (!CanBeCast()) return false;
            _castName = Spell.Instance.Name;
            Provider.AddCastAction(this, action);
            return true;
        }

        public bool SafeCast()
        {
            if (!CanBeCast()) return false;
            _castName = Spell.Instance.Name;
            Provider.AddCastAction(this, () => Spell.Cast());
            return true;
        }

        public bool SafeCast(Vector2 target)
        {
            if (!CanBeCast()) return false;
            _castName = Spell.Instance.Name;
            Provider.AddCastAction(this, () => Spell.Cast(target));
            return true;
        }

        public bool SafeCast(Vector2 from, Vector2 to)
        {
            if (!CanBeCast()) return false;
            _castName = Spell.Instance.Name;
            Provider.AddCastAction(this, () => Spell.Cast(from, to));
            return true;
        }

        public bool SafeCast(Vector3 target)
        {
            if (!CanBeCast()) return false;
            _castName = Spell.Instance.Name;
            Provider.AddCastAction(this, () => Spell.Cast(target));
            return true;
        }

        public bool SafeCast(Vector3 from, Vector3 to)
        {
            if (!CanBeCast()) return false;
            _castName = Spell.Instance.Name;
            Provider.AddCastAction(this, () => Spell.Cast(from, to));
            return true;
        }


        public bool SafeCast(Obj_AI_Base target)
        {
            if (!CanBeCast()) return false;
            _castName = Spell.Instance.Name;
            Provider.AddCastAction(this, () => Spell.Cast(target, false, IsAreaOfEffect));
            return true;
        }
        #endregion

        /// <summary>
        /// Add Initialisation logic in sub class. Called by ComboProvider.SetActive(skill)
        /// </summary>
        /// <param name="combo"></param>
        public virtual void Initialize(ComboProvider combo)
        {
            Provider = combo;
        }

        public virtual void SetEnabled(Orbwalking.OrbwalkingMode mode, bool enabled)
        {
            //Console.WriteLine(GetType().Name+": "+mode+" enabled: "+enabled);
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

        /// <summary>
        /// Note: if you already got the hitchance enum, just set the MinComboHitchance field!
        /// </summary>
        /// <param name="hitchance"></param>
        public void SetMinComboHitchance(string hitchance)
        {
            MinComboHitchance = hitchance.ToEnum<HitChance>();
        }

        /// <summary>
        /// Note: if you already got the hitchance enum, just set the MinHarassHitchance field!
        /// </summary>
        /// <param name="hitchance"></param>
        public void SetMinHarassHitchance(string hitchance)
        {
            MinHarassHitchance = hitchance.ToEnum<HitChance>();
        }

        public virtual void Update(Orbwalking.OrbwalkingMode mode, ComboProvider combo, Obj_AI_Hero target)
        {
            if (OnlyUpdateIfTargetValid && !target.IsValidTarget() && (mode == Orbwalking.OrbwalkingMode.Combo || mode == Orbwalking.OrbwalkingMode.Mixed)) return;
            if (OnlyUpdateIfCastable && !CanBeCast()) return;

            if (mode == Orbwalking.OrbwalkingMode.None) return;
            if (mode == Orbwalking.OrbwalkingMode.LaneClear && SwitchClearToHarassOnTarget && target != null && HarassEnabled)
                mode = Orbwalking.OrbwalkingMode.Mixed;
            if (UseManaManager && !ManaManager.CanUseMana(mode)) return;

            Spell.MinHitChance = mode == Orbwalking.OrbwalkingMode.Combo ? MinComboHitchance : MinHarassHitchance;

            switch (mode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    if (ComboEnabled)
                        Combo(combo, target);
                    break;
                case Orbwalking.OrbwalkingMode.LaneClear:
                    if (LaneclearEnabled)
                        LaneClear(combo, target);
                    break;
                case Orbwalking.OrbwalkingMode.Mixed:
                    if (HarassEnabled)
                        Harass(combo, target);
                    break;
            }
        }

        public abstract void Cast(Obj_AI_Hero target, bool force = false);
        public virtual void Combo(ComboProvider combo, Obj_AI_Hero target)
        {
            Cast(target);
        }
        public virtual void LaneClear(ComboProvider combo, Obj_AI_Hero target) { }
        public virtual void Harass(ComboProvider combo, Obj_AI_Hero target)
        {
            Cast(target);
        }
        public virtual void Gapcloser(ComboProvider combo, ActiveGapcloser gapcloser) { }
        public virtual void Interruptable(ComboProvider combo, Obj_AI_Hero sender, ComboProvider.InterruptableSpell interruptableSpell) { }
        public virtual void Draw() { }

        public virtual float GetDamage(Obj_AI_Hero enemy)
        {
            return Spell.GetState() == SpellState.Ready ? (float)ObjectManager.Player.GetSpellDamage(enemy, Spell.Slot) : 0f;
        }

        /// <summary>
        /// If this returns true an other skill of the same or lower priority can't grab control. If this skill
        /// has control but this return false, the control will be terminated.
        /// </summary>
        /// <returns></returns>
        public virtual bool NeedsControl() { return false; }

        public abstract int GetPriority();

        /// <summary>
        /// Gets called if some other skill wants total control OR this skill doesn't need control even though it has it.
        /// </summary>
        /// <returns></returns>
        public virtual bool TryTerminate() { return true; }

        /// <summary>
        /// If the spell seems available.
        /// </summary>
        /// <returns></returns>
        public virtual bool CanBeCast()
        {
            return Spell.GetState() == SpellState.Ready;
        }

        /// <summary>
        /// If the spell seems available. 
        /// </summary>
        /// <returns></returns>
        public virtual bool CanBeCast(string name)
        {
            return Spell.GetState() == SpellState.Ready && Spell.Instance.Name == name;
        }

        /// <summary>
        /// If the spell has been cast. Will NOT check if still in safecast time.
        /// </summary>
        /// <returns></returns>
        public bool HasBeenCast()
        {
            return (Spell.GetState() == SpellState.Cooldown) || (_castName != Spell.Instance.Name);
        }

        public int CompareTo(Skill obj)
        {
            return obj.GetPriority() - GetPriority();
        }
    }
}
