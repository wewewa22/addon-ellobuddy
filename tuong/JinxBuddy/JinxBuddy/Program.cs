using System;
using System.Collections.Generic;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Constants;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using EloBuddy.SDK.Rendering;
using SharpDX;

namespace JinxBuddy
{
    internal class Program
    {
        public static Spell.Active Q = new Spell.Active(SpellSlot.Q);

        public static Spell.Skillshot W = new Spell.Skillshot(SpellSlot.W, 1450, SkillShotType.Linear, 600, 3300, 100)
        {
            MinimumHitChance = HitChance.High, AllowedCollisionCount = 0
        };

        public static Spell.Skillshot E = new Spell.Skillshot(SpellSlot.E, 900, SkillShotType.Circular, 1200, 1750, 1);
        public static Spell.Skillshot R = new Spell.Skillshot(SpellSlot.R, 3000, SkillShotType.Linear, 600, 1700, 140);

        public static Menu Menu, ComboMenu, HarassMenu, FarmMenu, MiscMenu, DrawMenu;

        private static void Main(string[] args)
        {
            Loading.OnLoadingComplete += Loading_OnLoadingComplete;
        }

        private static void Loading_OnLoadingComplete(EventArgs args)
        {
            if (Player.Instance.Hero != Champion.Jinx) return;

            Menu = MainMenu.AddMenu("JINX", "ADDON JINX (KhoNan Demo)");
            Menu.AddGroupLabel("Jinx Buddy");
            Menu.AddLabel("made by the KhoNan VietHoa");
            Menu.AddSeparator();

            ComboMenu = Menu.AddSubMenu("Kết hợp Skill", "Combo Jinx");
            ComboMenu.AddGroupLabel("cài đặt Combo");
            ComboMenu.Add("kết hợp sử dụng Q", new CheckBox("sử dụng Q"));
            ComboMenu.Add("kết hợp sử dụng Q Splash", new CheckBox("sử dụng Q Splash"));
            ComboMenu.Add("kết hợp sử dụng W", new CheckBox("sử dụng W"));
            ComboMenu.Add("kết hợp sử dụng W E", new CheckBox("sử dụng E"));
            ComboMenu.Add("kết hợp sử dụng R", new CheckBox("sử dụng R"));

            HarassMenu = Menu.AddSubMenu("quấy rối", "quấy rối - Jinx");
            HarassMenu.AddGroupLabel("quấy rối cài đặt");
            HarassMenu.Add("sử dụng Q Quấy rối", new CheckBox("sử dụng Q"));
            HarassMenu.Add("sử dụng W Quấy rối", new CheckBox("sử dụng W"));

            FarmMenu = Menu.AddSubMenu("giết Lính", "giết Lính -Jinx");
            FarmMenu.AddGroupLabel("Farm Settings");
            FarmMenu.AddLabel("sóng rõ ràng");
            FarmMenu.Add("sử dụng Q giết Lính", new CheckBox("sử dụng Q"));
            FarmMenu.Add("Vô hiệu hóa tên lửa", new CheckBox("Vô hiệu hóa tên lửa (chỉ khi sử dụng q off)", false));
            FarmMenu.AddLabel("kết thúc");
            FarmMenu.Add("Vô hiệu hóa tên lửa", new CheckBox("Vô hiệu hóa tên lửa"));

            MiscMenu = Menu.AddSubMenu("Misc", "MiscMenuJinx");
            MiscMenu.AddGroupLabel("Misc Settings");
            MiscMenu.Add("khoảng cách gần hơn", new CheckBox("khoảng cách gần hơn E"));
            MiscMenu.Add("interruptor", new CheckBox("Interruptor E"));
            MiscMenu.Add("CCE", new CheckBox("On CC'd E"));

            DrawMenu = Menu.AddSubMenu("Cài đặt bản vẽ");
            DrawMenu.Add("Phạm vi rút", new CheckBox("Vẽ phạm vi rút"));
            DrawMenu.Add("Vẽ W", new CheckBox("Vẽ W"));
            DrawMenu.Add("Vẽ E", new CheckBox("Vẽ E"));

            Game.OnTick += Game_OnTick;
            Gapcloser.OnGapcloser += Events.Gapcloser_OnGapCloser;
            Interrupter.OnInterruptableSpell += Interrupter_OnInterruptableSpell;
            Drawing.OnDraw += Drawing_OnDraw;
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (DrawMenu["Phạm vi rút"].Cast<CheckBox>().CurrentValue)
            {
                Circle.Draw(Color.HotPink, !Events.FishBonesActive ? Events.FishBonesBonus + Events.MinigunRange() - Player.Instance.BoundingRadius/2 : Events.MinigunRange() - Player.Instance.BoundingRadius/2, Player.Instance.Position);
            }
            if (DrawMenu["Vẽ W"].Cast<CheckBox>().CurrentValue)
            {
                Circle.Draw(W.IsReady() ? Color.HotPink : Color.DarkRed, W.Range, Player.Instance.Position);
            }
            if (DrawMenu["Vẽ E"].Cast<CheckBox>().CurrentValue)
            {
                Circle.Draw(W.IsReady() ? Color.HotPink : Color.DarkRed, W.Range, Player.Instance.Position);
            }
        }

        private static void Interrupter_OnInterruptableSpell(Obj_AI_Base sender,
            Interrupter.InterruptableSpellEventArgs e)
        {
            if (MiscMenu["interruptor"].Cast<CheckBox>().CurrentValue && sender.IsEnemy &&
                e.DangerLevel == DangerLevel.High && sender.IsValidTarget(900))
            {
                E.Cast(sender);
            }
        }

        private static void Game_OnTick(EventArgs args)
        {
            Orbwalker.ForcedTarget = null;

            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo)) Combo();
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Harass)) Harass();
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear)) WaveClear();
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LastHit)) LastHit();

            if (MiscMenu["CCE"].Cast<CheckBox>().CurrentValue)
            {
                foreach (var enemy in EntityManager.Heroes.Enemies)
                {
                    if (enemy.Distance(Player.Instance) < E.Range &&
                        (enemy.HasBuffOfType(BuffType.Stun)
                         || enemy.HasBuffOfType(BuffType.Snare)
                         || enemy.HasBuffOfType(BuffType.Suppression)))
                    {
                        E.Cast(enemy);
                    }
                }
            }
        }

        public static void LastHit()
        {
            if (FarmMenu["vô hiệu hóa tên lửa"].Cast<CheckBox>().CurrentValue && Events.FishBonesActive)
            {
                Q.Cast();
            }
        }

        public static void WaveClear()
        {
            if (Orbwalker.IsAutoAttacking) return;
            if (FarmMenu["sử dụng Q giết lính"].Cast<CheckBox>().CurrentValue)
            {
                var unit =
                    EntityManager.MinionsAndMonsters.GetLaneMinions()
                        .Where(
                            a =>
                                a.IsValidTarget(Events.MinigunRange(a) + Events.FishBonesBonus) &&
                                a.Health < Player.Instance.GetAutoAttackDamage(a)*1.1)
                        .FirstOrDefault(minion => EntityManager.MinionsAndMonsters.EnemyMinions.Count(
                            a => a.Distance(minion) < 150 && a.Health < Player.Instance.GetAutoAttackDamage(a)*1.1) > 1);

                if (unit != null)
                {
                    if (!Events.FishBonesActive)
                    {
                        Q.Cast();
                    }
                    Orbwalker.ForcedTarget = unit;
                    return;
                }

                if (Events.FishBonesActive)
                {
                    Q.Cast();
                }
            }
            else if (FarmMenu["vô hiệu hóa tên lửa"].Cast<CheckBox>().CurrentValue && Events.FishBonesActive)
            {
                Q.Cast();
            }
        }

        public static void Harass()
        {
            var targetW = TargetSelector.SelectedTarget != null &&
                         TargetSelector.SelectedTarget.Distance(Player.Instance) < W.Range
                ? TargetSelector.SelectedTarget
                : TargetSelector.GetTarget(W.Range, DamageType.Physical);

            var target = TargetSelector.SelectedTarget != null &&
                         TargetSelector.SelectedTarget.Distance(Player.Instance) < (!Events.FishBonesActive ? Player.Instance.GetAutoAttackRange() + Events.FishBonesBonus : Player.Instance.GetAutoAttackRange()) + 300
                ? TargetSelector.SelectedTarget
                : TargetSelector.GetTarget((!Events.FishBonesActive ? Player.Instance.GetAutoAttackRange() + Events.FishBonesBonus : Player.Instance.GetAutoAttackRange()) + 300, DamageType.Physical);

            Orbwalker.ForcedTarget = null;

            if (Orbwalker.IsAutoAttacking) return;

            if (targetW != null)
            {
                // W out of range
                if (HarassMenu["useWHarass"].Cast<CheckBox>().CurrentValue && W.IsReady() &&
                    target.Distance(Player.Instance) > Player.Instance.GetAutoAttackRange(targetW) &&
                    targetW.IsValidTarget(W.Range))
                {
                    W.Cast(targetW);
                }
            }

            if (target != null)
            {

                if (HarassMenu["useQHarass"].Cast<CheckBox>().CurrentValue)
                {
                    // Aoe Logic
                    foreach (
                        var enemy in
                            EntityManager.Heroes.Enemies.Where(
                                a => a.IsValidTarget(Events.MinigunRange(a) + Events.FishBonesBonus))
                                .OrderBy(TargetSelector.GetPriority))
                    {
                        if (enemy.CountEnemiesInRange(150) > 1 &&
                            (enemy.NetworkId == target.NetworkId || enemy.Distance(target) < 150))
                        {
                            if (!Events.FishBonesActive)
                            {
                                Q.Cast();
                            }
                            Orbwalker.ForcedTarget = enemy;
                            return;
                        }
                    }

                    // Regular Q Logic
                    if (Events.FishBonesActive)
                    {
                        if (target.Distance(Player.Instance) <= Player.Instance.GetAutoAttackRange(target) - Events.FishBonesBonus)
                        {
                            Q.Cast();
                        }
                    }
                    else
                    {
                        if (target.Distance(Player.Instance) > Player.Instance.GetAutoAttackRange(target))
                        {
                            Q.Cast();
                        }
                    }
                }
            }
        }

        public static void Combo()
        {

            var targetW = TargetSelector.SelectedTarget != null &&
                         TargetSelector.SelectedTarget.Distance(Player.Instance) < W.Range
                ? TargetSelector.SelectedTarget
                : TargetSelector.GetTarget(W.Range, DamageType.Physical);


            var target = TargetSelector.SelectedTarget != null &&
                         TargetSelector.SelectedTarget.Distance(Player.Instance) < (!Events.FishBonesActive ? Player.Instance.GetAutoAttackRange() + Events.FishBonesBonus : Player.Instance.GetAutoAttackRange()) + 300
                ? TargetSelector.SelectedTarget
                : TargetSelector.GetTarget((!Events.FishBonesActive ? Player.Instance.GetAutoAttackRange() + Events.FishBonesBonus : Player.Instance.GetAutoAttackRange()) + 300, DamageType.Physical);
            var rtarget = TargetSelector.GetTarget(3000, DamageType.Physical);

            Orbwalker.ForcedTarget = null;

            if (Orbwalker.IsAutoAttacking) return;

            // E on immobile

            if (target != null && ComboMenu["useECombo"].Cast<CheckBox>().CurrentValue && (target.HasBuffOfType(BuffType.Snare) ||  target.HasBuffOfType(BuffType.Stun)))
            {
                E.Cast(target);
            }

            if (rtarget != null)
            {
                // W/R KS
                var wPred = W.GetPrediction(rtarget);

                if (ComboMenu["useWCombo"].Cast<CheckBox>().CurrentValue &&
                    wPred.HitChance >= HitChance.Medium && W.IsReady() && rtarget.IsValidTarget(W.Range) &&
                    Damages.WDamage(target) >= rtarget.Health)
                {
                    W.Cast(rtarget);
                }
                else if (ComboMenu["useRCombo"].Cast<CheckBox>().CurrentValue && rtarget != null &&
                         rtarget.Distance(Player.Instance) > Events.MinigunRange(target) + Events.FishBonesBonus &&
                         rtarget.IsKillableByR())
                {
                    R.Cast(rtarget);
                }
            }

            // W out of range
            if (targetW != null && ComboMenu["useWCombo"].Cast<CheckBox>().CurrentValue && W.IsReady() && targetW.Distance(Player.Instance) > Player.Instance.GetAutoAttackRange(targetW) &&
                targetW.IsValidTarget(W.Range))
            {
                W.Cast(targetW);
            }

            if (target != null && ComboMenu["useQSplash"].Cast<CheckBox>().CurrentValue)
            {
                // Aoe Logic
                foreach (var enemy in EntityManager.Heroes.Enemies.Where(
                    a => a.IsValidTarget(Events.MinigunRange(a) + Events.FishBonesBonus))
                    .OrderBy(TargetSelector.GetPriority).Where(enemy => enemy.CountEnemiesInRange(150) > 1 && (enemy.NetworkId == target.NetworkId || enemy.Distance(target) < 150)))
                {
                    if (!Events.FishBonesActive)
                    {
                        Q.Cast();
                    }
                    Orbwalker.ForcedTarget = enemy;
                    return;
                }
            }

            // Regular Q Logic
            if (target != null && ComboMenu["useQCombo"].Cast<CheckBox>().CurrentValue && Events.FishBonesActive)
            {
                if (target.Distance(Player.Instance) <= Player.Instance.GetAutoAttackRange(target) - Events.FishBonesBonus)
                {
                    Q.Cast();
                }
            }
            else if(target != null && ComboMenu["useQCombo"].Cast<CheckBox>().CurrentValue)
            {
                if (target.Distance(Player.Instance) > Player.Instance.GetAutoAttackRange(target))
                {
                    Q.Cast();
                }
            }
        }
    }
}