using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
namespace LuckyAio.Champions
{
    static class Twitch
    {
        static Spell.Active Q;
        static Spell.Skillshot W;
        static Spell.Active E;
        static Menu Menu;
        static Menu ComboMenu, MixedMenu, LaneClearMenu, Misc;
        public static void TwitchLoading()
        {
            SetSpells();
            SetMenu();
            Game.OnUpdate += Game_OnUpdate;
            Spellbook.OnCastSpell += Spellbook_OnCastSpell;
        }
        static void SetSpells()
        {
            W = new Spell.Skillshot(SpellSlot.W, 950, SkillShotType.Circular, 250, 1400, 120);
            E = new Spell.Active(SpellSlot.E);
        }
        static void SetMenu()
        {
            Menu = MainMenu.AddMenu("Mysterious Twitch", "Mysterious Twitch");
            ComboMenu = Menu.AddSubMenu("Combo Settings", "Combo Settings");
            ComboMenu.Add("useW", new CheckBox("Use W"));
            ComboMenu.Add("useE", new CheckBox("Use E Killable"));
            MixedMenu = Menu.AddSubMenu("Mixed Settings", "Mixed Settings");
            MixedMenu.Add("useW", new CheckBox("Use W"));
            MixedMenu.Add("useE", new CheckBox("Use E"));
            Misc = Menu.AddSubMenu("Misc Settings", "Misc Settings");
            Misc.Add("Youmuus", new CheckBox("Youmuus with R"));
            Misc.Add("Wtower", new CheckBox("Don't W Under Tower"));
            Misc.Add("noW", new Slider("No W if x aa can kill", 2, 0, 10));
            Misc.Add("EMana", new CheckBox("Save Mana for E"));
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
        static void Game_OnUpdate(EventArgs args)
        {
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
            {
                Combo();
            }
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Harass) || Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear))
            {
                Mixed();
            }
            if (getCheckBoxItem(ComboMenu, "useE") && E.IsReady())
            {
                if (EntityManager.Heroes.Enemies.Any(x => x.IsPoisonKillable() && x.IsValidTarget(E.Range)))
                {
                    E.Cast();   
                }
            }
        }
        static void Spellbook_OnCastSpell(Spellbook sender, SpellbookCastSpellEventArgs args)
        {
            if (args.Slot == SpellSlot.R && sender.Owner.IsMe && getCheckBoxItem(Misc, "Youmuus"))
            {
                if (!EntityManager.Heroes.Enemies.Any(x => ObjectManager.Player.Distance(x) <= 900)) { return; }
                if (Item.HasItem(ItemId.Youmuus_Ghostblade))
                {
                    Item.UseItem(ItemId.Youmuus_Ghostblade);
                }
            }
        }
        static void Combo()
        {
            if (getCheckBoxItem(ComboMenu, "useW") && W.IsReady())
            {
                if (getCheckBoxItem(Misc, "EMana") && ObjectManager.Player.Mana <= ObjectManager.Player.Spellbook.GetSpell(SpellSlot.E).SData.Mana + ObjectManager.Player.Spellbook.GetSpell(SpellSlot.W).SData.Mana)
                {
                    return;
                }
                if (getCheckBoxItem(Misc, "Wtower") && ObjectManager.Player.IsUnderEnemyturret())
                {
                    return;
                }
                var wTarget = TargetSelector.GetTarget(W.Range, DamageType.Physical);
                if (wTarget != null && wTarget.Health < ObjectManager.Player.GetAutoAttackDamage(wTarget, true)* getSliderItem(Misc, "noW"))
                {
                    return;
                }
                if (wTarget.IsValidTarget(W.Range) && !ObjectManager.Player.HasBuff("TwitchHideInShadows"))
                {
                    var prediction = W.GetPrediction(wTarget);
                    if (prediction.HitChance >= HitChance.High)
                    {
                        W.Cast(prediction.CastPosition);
                    }
                }
            }

        }
        static void Mixed()
        {
            if (getCheckBoxItem(MixedMenu, "useW") && W.IsReady())
            {
                var wTarget = TargetSelector.GetTarget(W.Range, DamageType.Physical);
                if (wTarget.IsValidTarget(W.Range))
                {
                    var prediction = W.GetPrediction(wTarget);
                    if (prediction.HitChance >= HitChance.High)
                    {
                        W.Cast(prediction.CastPosition);
                    }
                }
            }
        }
        static bool HasUndyingBuff(this Obj_AI_Base target1)
        {
            var target = target1 as AIHeroClient;
            if (target == null) return false;
            if (target.ChampionName == "Tryndamere"
                && target.Buffs.Any(
                    b => b.Caster.NetworkId == target.NetworkId && b.IsValid && b.DisplayName == "Undying Rage"))
            {
                return true;
            }

            if (target.Buffs.Any(b => b.IsValid && b.DisplayName == "Chrono Shift"))
            {
                return true;
            }
            if (target.Buffs.Any(b => b.IsValid && b.DisplayName == "JudicatorIntervention"))
            {
                return true;
            }

            if (target.Buffs.Any(b => b.IsValid && b.Name == "kindredrnodeathbuff"))
            {
                return true;
            }
            return false;
        }
        static float GetPoisonDamage(this Obj_AI_Base target)
        {
            if (target == null || !target.HasBuff("twitchdeadlyvenom") || target.IsInvulnerable
                || target.HasUndyingBuff() || target.HasBuff("KindredRNoDeathBuff")
                || target.HasBuffOfType(BuffType.SpellShield))
            {
                return 0;
            }

            double baseDamage = EDamage(target);
            if (ObjectManager.Player.HasBuff("SummonerExhaust"))
            {
                baseDamage *= 0.6;
            }
            if (ObjectManager.Player.HasBuff("urgotentropypassive"))
            {
                baseDamage *= 0.85;
            }

            if (target.HasBuff("MasteryWardenOfTheDawn"))
            {
                var bondofstoneBuffCount = target.GetBuffCount("MasteryWardenOfTheDawn");
                if (bondofstoneBuffCount > 0)
                {
                    baseDamage *= 1 - (0.06 * bondofstoneBuffCount);
                }
            }
            var phantomdancerBuff = ObjectManager.Player.GetBuff("itemphantomdancerdebuff");
            if (phantomdancerBuff != null && phantomdancerBuff.Caster == target)
            {
                baseDamage *= 0.88;
            }
            if (target.HasBuff("FerociousHowl"))
            {
                baseDamage *= 0.6 - new[] { 0.1, 0.2, 0.3 }[target.Spellbook.GetSpell(SpellSlot.R).Level - 1];
            }
            if (target.HasBuff("Tantrum"))
            {
                baseDamage -= new[] { 2, 4, 6, 8, 10 }[target.Spellbook.GetSpell(SpellSlot.E).Level - 1];
            }
            if (target.HasBuff("BraumShieldRaise"))
            {
                baseDamage *= 1
                              - new[] { 0.3, 0.325, 0.35, 0.375, 0.4 }[target.Spellbook.GetSpell(SpellSlot.E).Level - 1];
            }
            if (target.HasBuff("GalioIdolOfDurand"))
            {
                baseDamage *= 0.5;
            }
            if (target.HasBuff("GarenW"))
            {
                baseDamage *= 0.7;
            }
            if (target.HasBuff("GragasWSelf"))
            {
                baseDamage *= 1
                              - new[] { 0.1, 0.12, 0.14, 0.16, 0.18 }[target.Spellbook.GetSpell(SpellSlot.W).Level - 1];
            }
            if (target.HasBuff("KatarinaEReduction"))
            {
                baseDamage *= 0.85;
            }
            if (target.HasBuff("MaokaiDrainDefense"))
            {
                baseDamage *= 0.8;
            }
            if (target.HasBuff("Meditate"))
            {
                baseDamage *= 1 - new[] { 0.5, 0.55, 0.6, 0.65, 0.7 }[target.Spellbook.GetSpell(SpellSlot.W).Level - 1];
            }
            if (target.HasBuff("urgotswapdef"))
            {
                baseDamage *= 1 - new[] { 0.3, 0.4, 0.5 }[target.Spellbook.GetSpell(SpellSlot.R).Level - 1];
            }
            if (target.HasBuff("YorickUnholySymbiosis"))
            {
                baseDamage *= 1
                              - (ObjectManager.Get<Obj_AI_Minion>()
                                     .Count(
                                         g =>
                                         g.Team == target.Team
                                         && (g.Name.Equals("Clyde") || g.Name.Equals("Inky") || g.Name.Equals("Blinky")
                                             || (g.HasBuff("yorickunholysymbiosis")
                                                 && g.GetBuff("yorickunholysymbiosis").Caster == target))) * 0.05);
            }
            if (target is Obj_AI_Minion)
            {
                if (target.Name.Contains("Baron"))
                {
                    baseDamage *= 0.5f;
                }
            }

            return (float)baseDamage;
        }
        static float GetRealHealth(this Obj_AI_Base target)
        {
            return target.Health + (target.AttackShield > 0 ? target.AttackShield : 0) + target.HPRegenRate;
        }
        static float GetRemainingBuffTime(this Obj_AI_Base target, string buffName)
        {
            return
                target.Buffs.OrderByDescending(buff => buff.EndTime - Game.Time)
                    .Where(buff => string.Equals(buff.Name, buffName, StringComparison.CurrentCultureIgnoreCase))
                    .Select(buff => buff.EndTime)
                    .FirstOrDefault() - Game.Time;
        }
        static bool IsPoisonKillable(this Obj_AI_Base target)
        {
            return GetPoisonDamage(target) >= GetRealHealth(target);
        }
        static float EDamage(Obj_AI_Base target)
        {
            if (target is AIHeroClient)
            {
                if (Stack(target) == 0) return 0;

                float stacksChamps = Stack(target);

                float EDamageChamp = new[] { 20, 35, 50, 65, 80 }[ObjectManager.Player.Spellbook.GetSpell(SpellSlot.E).Level - 1] ;

                if (stacksChamps > 1)
                {
                    EDamageChamp += (new[] { 15, 20, 25, 30, 35 }[ObjectManager.Player.Spellbook.GetSpell(SpellSlot.E).Level - 1] + (ObjectManager.Player.Spellbook.GetSpell(SpellSlot.E).SData.PhysicalDamageRatio * ObjectManager.Player.FlatPhysicalDamageMod) + (ObjectManager.Player.Spellbook.GetSpell(SpellSlot.E).SData.SpellDamageRatio * ObjectManager.Player.FlatMagicDamageMod)) * (stacksChamps - 1);
                }

                return ObjectManager.Player.CalculateDamageOnUnit(target, DamageType.Physical,EDamageChamp);
            }
            return 0;
        }

        static int Stack(Obj_AI_Base obj)
        {
            var Ec = 0;
            for (var t = 1; t < 7; t++)
            {
                if (ObjectManager.Get<Obj_GeneralParticleEmitter>().Any(s => s.Position.Distance(obj.ServerPosition) <= 175 && s.Name == "twitch_poison_counter_0" + t + ".troy"))
                {
                    Ec = t;
                }
            }
            return Ec;
        }
    }
}
