using System.Collections.Generic;
using System.Linq;
using EloBuddy.SDK;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;

namespace EvadePlus
{
    internal class EvadeMenu
    {
        public static Menu MainMenu { get; private set; }
        public static Menu SkillshotMenu { get; private set; }
        public static Menu SpellMenu { get; private set; }
        public static Menu DrawMenu { get; private set; }
        public static Menu HotkeysMenu { get; private set; }

        public static readonly Dictionary<string, EvadeSkillshot> MenuSkillshots = new Dictionary<string, EvadeSkillshot>();

        public static void CreateMenu()
        {
            if (MainMenu != null)
            {
                return;
            }

            MainMenu = EloBuddy.SDK.Menu.MainMenu.AddMenu("Né Tuyệt chiêu :V)", "Né Tuyệt chiêu tung Skill :D By KhoNan Demo)");

            // Set up main menu
            MainMenu.AddGroupLabel("Cài đặt chung");
            MainMenu.Add("Phát hiện", new CheckBox("Kích hoạt tính năng phát hiện"));
            MainMenu.AddLabel("Ngày: cho dodging trong sương mù của chiến tranh, Off: cho hành vi của con người hơn");
            MainMenu.AddSeparator(3);

            MainMenu.Add("Phát hiện quá trình Spell", new CheckBox("Kích hoạt tính năng Phát hiện quá trình Spell"));
            MainMenu.AddLabel("phát hiện skillshot trước khi tên lửa được tạo ra, khuyến cáo: On");
            MainMenu.AddSeparator(3);

            MainMenu.Add("Hạn chế chính tả phát hiện Dải", new CheckBox("Hạn chế chính tả phát hiện Dải"));
            MainMenu.AddLabel("phát hiện chỉ có skillshots gần bạn, đề nghị: On");
            MainMenu.AddSeparator(3);

            MainMenu.Add("tính toán lại vị", new CheckBox("Cho phép tính toán lại vị trí né tránh", false));
            MainMenu.AddLabel("cho phép thay đổi trốn tránh con đường , đề nghị : Tắt");
            MainMenu.AddSeparator(3);

            MainMenu.Add("di chuyển đến vị trí ban đầu", new CheckBox("Di chuyển đến vị trí mong muốn sau khi né tránh.", false));
            MainMenu.AddLabel("di chuyển đến vị trí mong muốn của bạn sau khi trốn");
            MainMenu.AddSeparator(3);

            MainMenu.Add("Máy chủ thời gian đệm", new Slider("Máy chủ thời gian đệm", 0, 0, 200));
            MainMenu.AddLabel("thêm thời gian nó được bao gồm trong tính né tránh");
            MainMenu.AddSeparator();

            MainMenu.AddGroupLabel("Humanizer");
            MainMenu.Add("kỹ năng bắn Activation trễ", new Slider("né tránh chậm trễ", 0, 0, 400));
            MainMenu.AddSeparator(10);

            MainMenu.Add("Thêm né tránh Dải", new Slider("Thêm né tránh Dải", 0, 0, 300));
            MainMenu.Add("randomizeExtraEvadeRange", new CheckBox("Randomize Extra Evade Range", false));

            // Set up skillshot menu
            var heroes = Program.DeveloperMode ? EntityManager.Heroes.AllHeroes : EntityManager.Heroes.Enemies;
            var heroNames = heroes.Select(obj => obj.ChampionName).ToArray();
            var skillshots =
                SkillshotDatabase.Database.Where(s => heroNames.Contains(s.SpellData.ChampionName)).ToList();
            skillshots.AddRange(
                SkillshotDatabase.Database.Where(
                    s =>
                        s.SpellData.ChampionName == "Tất cả Tướng" &&
                        heroes.Any(obj => obj.Spellbook.Spells.Select(c => c.Name).Contains(s.SpellData.SpellName))));

            SkillshotMenu = MainMenu.AddSubMenu("Skillshots");
            SkillshotMenu.AddLabel(string.Format("Skillshots Loaded {0}", skillshots.Count));
            SkillshotMenu.AddSeparator();

            foreach (var c in skillshots)
            {
                var skillshotString = c.ToString().ToLower();

                if (MenuSkillshots.ContainsKey(skillshotString))
                    continue;

                MenuSkillshots.Add(skillshotString, c);

                SkillshotMenu.AddGroupLabel(c.DisplayText);
                SkillshotMenu.Add(skillshotString + "/enable", new CheckBox("Dodge"));
                SkillshotMenu.Add(skillshotString + "/draw", new CheckBox("Draw"));

                var dangerous = new CheckBox("Dangerous", c.SpellData.IsDangerous);
                dangerous.OnValueChange += delegate(ValueBase<bool> sender, ValueBase<bool>.ValueChangeArgs args)
                {
                    GetSkillshot(sender.SerializationId).SpellData.IsDangerous = args.NewValue;
                };
                SkillshotMenu.Add(skillshotString + "/dangerous", dangerous);

                var dangerValue = new Slider("Danger Value", c.SpellData.DangerValue, 1, 5);
                dangerValue.OnValueChange += delegate(ValueBase<int> sender, ValueBase<int>.ValueChangeArgs args)
                {
                    GetSkillshot(sender.SerializationId).SpellData.DangerValue = args.NewValue;
                };
                SkillshotMenu.Add(skillshotString + "/dangervalue", dangerValue);

                SkillshotMenu.AddSeparator();
            }

            // Set up spell menu
            SpellMenu = MainMenu.AddSubMenu("Evading Spells");
            SpellMenu.AddGroupLabel("Flash");
            SpellMenu.Add("flash", new Slider("Danger Value", 5, 0, 5));

            // Set up draw menu
            DrawMenu = MainMenu.AddSubMenu("Bản vẽ");
            DrawMenu.AddGroupLabel("Evade Bản vẽ");
            DrawMenu.Add("vô hiệu hóa tất cả Bản vẽ", new CheckBox("vô hiệu hóa tất cả Bản vẽ", false));
            DrawMenu.Add("drawEvadePoint", new CheckBox("vẽ điểm né tránh"));
            DrawMenu.Add("drawEvadeStatus", new CheckBox("Draw Evade Status"));
            DrawMenu.Add("drawDangerPolygon", new CheckBox("Draw Danger Polygon", false));
            DrawMenu.AddSeparator();
            DrawMenu.Add("drawPath", new CheckBox("Draw Autpathing Path"));

            // Set up controls menu
            HotkeysMenu = MainMenu.AddSubMenu("Hotkeys");
            HotkeysMenu.AddGroupLabel("Hotkeys");
            HotkeysMenu.Add("enableEvade", new KeyBind("Enable Evade", true, KeyBind.BindTypes.PressToggle, 'M'));
            HotkeysMenu.Add("dodgeOnlyDangerous", new KeyBind("Dodge Only Dangerous", false, KeyBind.BindTypes.HoldActive));
        }

        private static EvadeSkillshot GetSkillshot(string s)
        {
            return MenuSkillshots[s.ToLower().Split('/')[0]];
        }

        public static bool IsSkillshotEnabled(EvadeSkillshot skillshot)
        {
            var valueBase = SkillshotMenu[skillshot + "/enable"];
            return valueBase != null && valueBase.Cast<CheckBox>().CurrentValue;
        }

        public static bool IsSkillshotDrawingEnabled(EvadeSkillshot skillshot)
        {
            var valueBase = SkillshotMenu[skillshot + "/draw"];
            return valueBase != null && valueBase.Cast<CheckBox>().CurrentValue;
        }
    }
}
