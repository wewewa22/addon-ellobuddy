using System;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;

namespace BaseUltPlus
{
    public class Program
    {
        public static Menu BaseUltMenu { get; set; }

        public static void Main(string[] args)
        {
            // Wait till the name has fully loaded
            Loading.OnLoadingComplete += LoadingOnOnLoadingComplete;
        }

        private static void LoadingOnOnLoadingComplete(EventArgs args)
        {
            //Menu
            BaseUltMenu = MainMenu.AddMenu("cơ sở tiện ích+", "cơ sở tiện ích Menu");
            BaseUltMenu.AddGroupLabel("cơ sở tiện ích+ General");
            BaseUltMenu.AddSeparator();
            BaseUltMenu.Add("cơ sở tiện ích", new CheckBox("cơ sở tiện ích"));
            BaseUltMenu.Add("chương trình thu hồi", new CheckBox("chương trình thu hồi"));
            BaseUltMenu.Add("Hiện Đồng Minh", new CheckBox("Hiện Đồng Minh"));
            BaseUltMenu.Add("Hiện kẻ thù", new CheckBox("Hiện kẻ thù"));
            BaseUltMenu.Add("kiểm tra va chạm", new CheckBox("Kiểm tra va chạm"));
            BaseUltMenu.AddSeparator();
            BaseUltMenu.Add("Thời gian giới hạn", new Slider("Thời gian giới hạn (SEC)", 20, 0, 120));
            BaseUltMenu.AddSeparator();
            BaseUltMenu.Add("Không cơ sở tiện ích", new KeyBind("Không cơ sở tiện ích trong khi", false, KeyBind.BindTypes.HoldActive, 32));
            BaseUltMenu.AddSeparator();
            BaseUltMenu.Add("x", new Slider("Bù lại X", 0, -500, 500));
            BaseUltMenu.Add("y", new Slider("Bù lại Y", 0, -500, 500));
            BaseUltMenu.AddGroupLabel("cơ sở tiện ích+ Mục tiêu");
            foreach (var unit in HeroManager.Enemies)
            {
                BaseUltMenu.Add("target" + unit.ChampionName, new CheckBox(string.Format("{0} ({1})", unit.ChampionName, unit.Name)));
            }
            BaseUltMenu.AddGroupLabel("BaseUlt+ Credits");
            BaseUltMenu.AddLabel("By: KhoNan");
            BaseUltMenu.AddLabel("Testing: KhoNan");

            // Initialize the Addon
            OfficialAddon.Initialize();

            // Listen to the two main events for the Addon
            Game.OnUpdate += args1 => OfficialAddon.OnUpdate();
            Drawing.OnEndScene += args1 => OfficialAddon.OnEndScene();
            Teleport.OnTeleport += OfficialAddon.OnTeleport;
        }
    }
}
