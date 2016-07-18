using System;
using System.Linq;
using System.Collections.Generic;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
namespace LuckyAio.Champions
{
    class Sivir
    {
        static Menu Menu;
        static Menu ComboMenu, MixedMenu, LaneClearMenu, Emenu;
        static Spell.Skillshot Q;
        static Spell.Active W;
        static Spell.Active E;
        static Spell.Active R;
        static float QMANA = 0, WMANA = 0, EMANA = 0, RMANA = 0;
        static bool isClear = false;
        public static void SivirLoading()
        {
            SetSpells();
            SetMana();
            SetMenu();
            Game.OnUpdate += Game_OnUpdate;
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
            Orbwalker.OnPostAttack += Orbwalker_OnPostAttack;
        }

        static void Orbwalker_OnPostAttack(AttackableUnit target, EventArgs args)
        {
            if (W.IsReady() && UseW && target != null)
            {
                if ((Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo) && !Orbwalker.GotAutoAttackReset && target.Distance(ObjectManager.Player.ServerPosition) <= ObjectManager.Player.AttackRange && (target is AIHeroClient)) || (isClear == true && getSliderItem(LaneClearMenu, "mana") <= Player.Instance.ManaPercent) && (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear) || Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.JungleClear)))
                {
                    W.Cast();
                }
            }
        }

        static void SetSpells()
        {
            Q = new Spell.Skillshot(SpellSlot.Q, 1200, SkillShotType.Linear, 250, 1350, 90);
            W = new Spell.Active(SpellSlot.W, (uint)ObjectManager.Player.AttackRange);
            E = new Spell.Active(SpellSlot.E);
            R = new Spell.Active(SpellSlot.R, 1000);
        }
        static void SetMenu()
        {
            Menu = MainMenu.AddMenu("Mysterious Sivir", "Mysterious Sivir");
            ComboMenu = Menu.AddSubMenu("Combo Settings", "Combo Settings");
            ComboMenu.Add("useQ", new CheckBox("Use Q"));
            ComboMenu.Add("useW", new CheckBox("Use W"));
            ComboMenu.Add("useR", new CheckBox("Use R"));
            ComboMenu.Add("manaManager", new CheckBox("Mana Manager"));
            MixedMenu = Menu.AddSubMenu("Mixed Settings", "Mixed Settings");
            MixedMenu.Add("useQ", new CheckBox("Use Q"));
            MixedMenu.Add("useW", new CheckBox("Use W"));
            MixedMenu.Add("mana", new Slider("Min Mana %", 50, 0, 100));
            Emenu = Menu.AddSubMenu("Shield Settings", "Shield Settings");
            Emenu.Add("autoE", new CheckBox("Auto E Targeted Spells"));
            LaneClearMenu = Menu.AddSubMenu("Clear Settings", "Clear Settings");
            LaneClearMenu.Add("useQ", new CheckBox("Use Q"));
            LaneClearMenu.Add("useW", new CheckBox("Use W"));
            LaneClearMenu.Add("mana", new Slider("Min Mana %", 30, 0, 100));
        }
        static bool getCheckBoxItem(Menu m, string item)
        {
            return m[item].Cast<CheckBox>().CurrentValue;
        }

        static int getSliderItem(Menu m, string item)
        {
            return m[item].Cast<Slider>().CurrentValue;
        }

        static bool getKeyBindItem(Menu m, string item)
        {
            return m[item].Cast<KeyBind>().CurrentValue;
        }
        static bool UseQ { get { return getCheckBoxItem(ComboMenu, "useQ"); } }
        static bool UseW { get { return getCheckBoxItem(ComboMenu, "useW"); } }
        static bool UseE { get { return getCheckBoxItem(Emenu, "autoE"); } }
        static bool UseR { get { return getCheckBoxItem(ComboMenu, "useR"); } }
        static bool ManaManager { get { return getCheckBoxItem(ComboMenu, "manaManager"); } }
        static int getBoxItem(Menu m, string item)
        {
            return m[item].Cast<ComboBox>().CurrentValue;
        }
        static void Game_OnUpdate(EventArgs args)
        {
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
            {
                Combo();
            }
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Harass))
            {
                Mixed();
            }
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear) || Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.JungleClear))
            {
                Clear();
            }
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear))
            {
                Mixed();
            }
        }
        static void Combo()
        {
            var targetQ = TargetSelector.GetTarget(Q.Range, DamageType.Physical);
            var targetR = TargetSelector.GetTarget(800, DamageType.Physical);
            if (targetQ == null || !targetQ.IsValidTarget(Q.Range))
            {
                return;
            }
            var Qpos = Q.GetPrediction(targetQ);
            if (Q.IsReady() && UseQ && !ObjectManager.Player.Spellbook.IsAutoAttacking && HitChance.High <= Qpos.HitChance && ObjectManager.Player.Mana > RMANA + QMANA && targetQ.CanMove)
            {
                Q.Cast(targetQ);
            }
            if (Q.IsReady() && ObjectManager.Player.Mana > RMANA + WMANA)
            {
                foreach (var enemy in EntityManager.Heroes.Enemies.Where(enemy => enemy.IsValidTarget(Q.Range) && UseQ && !enemy.CanMove))
                    Q.Cast(enemy);
            }
            if (R.IsReady() && UseR && (ObjectManager.Player.CountEnemiesInRange(800f) > 2 || (ObjectManager.Player.HealthPercent <= 20 && ObjectManager.Player.CountEnemiesInRange(800f) >= 1)))
            {
                R.Cast();
            }
        }
        static void Mixed()
        {
            var targetQ = TargetSelector.GetTarget(Q.Range, DamageType.Physical);
            if (targetQ == null || !targetQ.IsValidTarget(Q.Range))
            {
                return;
            }
            var Qpos = Q.GetPrediction(targetQ);
            if (Q.IsReady() && getSliderItem(MixedMenu, "mana") < ObjectManager.Player.ManaPercent && getCheckBoxItem(MixedMenu, "useQ") && HitChance.High <= Qpos.HitChance &&  !ObjectManager.Player.Spellbook.IsAutoAttacking && targetQ.CanMove)
            {
                Q.Cast(targetQ);
            }
            if (Q.IsReady() && ObjectManager.Player.Mana > RMANA + WMANA)
            {
                foreach (var enemy in EntityManager.Heroes.Enemies.Where(enemy => enemy.IsValidTarget(Q.Range) && UseQ && !enemy.CanMove))
                    Q.Cast(enemy);
            }
        }
        static List<Obj_AI_Minion> Minions
        {
            get
            {
                return EntityManager.MinionsAndMonsters.EnemyMinions.Where(m => Extensions.IsMinion(m) && Extensions.IsValidTarget(m, Q.Range)).ToList();
            }
        }
        static List<Obj_AI_Minion> JungleMinions
        {
            get
            {
                return EntityManager.MinionsAndMonsters.Monsters.Where(m => Extensions.IsValidTarget(m, ObjectManager.Player.AttackRange)).ToList();
            }
        }
        static List<Obj_AI_Minion> Minions2
        {
            get
            {
                return EntityManager.MinionsAndMonsters.EnemyMinions.Where(m => Extensions.IsMinion(m) && Extensions.IsValidTarget(m, Q.Range)).ToList();
            }
        }
        static void Clear()
        {
            if (W.IsReady() && getCheckBoxItem(LaneClearMenu, "useW") && !Q.IsReady())
            {
                if (Minions.Any())
                {
                    if (Minions.Count() >= 3 && getSliderItem(LaneClearMenu, "mana") <= Player.Instance.ManaPercent)
                    {
                        isClear = true;
                    }
                    else
                        isClear = false;
                }
                else if (JungleMinions.Any())
                {
                    isClear = true;
                }
                else
                    isClear = false;
            }
            var targetQ = TargetSelector.GetTarget(800, DamageType.Physical);
            if (Q.IsReady() && getSliderItem(LaneClearMenu, "mana") <= Player.Instance.ManaPercent && getCheckBoxItem(LaneClearMenu, "useQ"))
            {
                if (Minions.Any())
                {
                    if (Minions.Count() >= 5 && (targetQ == null || !targetQ.IsValidTarget(800)))
                    {
                        Q.Cast(Minions[0].ServerPosition);
                    }
                }
                else if (JungleMinions.Any())
                {
                    Q.Cast(JungleMinions[0].ServerPosition);
                }
            }
        }
        static void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender != null && args.Target != null && sender.Type == GameObjectType.AIHeroClient && args.Target.IsMe && sender.IsEnemy && UseE && E.IsReady())
            {
                if (!args.SData.ConsideredAsAutoAttack)
                {
                    if (!args.SData.Name.Contains("summoner") && !args.SData.Name.Contains("TormentedSoil"))
                    {
                        E.Cast();
                    }
                }
                else if (args.SData.Name == "BlueCardAttack" || args.SData.Name == "RedCardAttack" || args.SData.Name == "GoldCardAttack")
                {
                    E.Cast();
                }
            }
        }
        static void SetMana()
        {
            if ((Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo) && !ManaManager) || ObjectManager.Player.HealthPercent < 20)
            {
                QMANA = 0;
                WMANA = 0;
                EMANA = 0;
                RMANA = 0;
                return;
            }

            QMANA = ObjectManager.Player.Spellbook.GetSpell(SpellSlot.Q).SData.Mana;
            WMANA = ObjectManager.Player.Spellbook.GetSpell(SpellSlot.W).SData.Mana;
            EMANA = ObjectManager.Player.Spellbook.GetSpell(SpellSlot.E).SData.Mana;

            if (!R.IsReady())
                RMANA = QMANA - ObjectManager.Player.PARRegenRate * ObjectManager.Player.Spellbook.GetSpell(SpellSlot.Q).Cooldown ;
            else
                RMANA = ObjectManager.Player.Spellbook.GetSpell(SpellSlot.R).SData.Mana;
        }
    }
}
