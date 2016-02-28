using System.Linq;
using EloBuddy.SDK;
using Settings = KalistaByKhoNanDemo.Config.Modes.JungleClear;

namespace KalistaByKhoNanDemo.Modes
{
    public class JungleClear : ModeBase
    {
        public override bool ShouldBeExecuted()
        {
            return Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.JungleClear);
        }

        public override void Execute()
        {
            if (!Settings.UseE || !E.IsReady())
            {
                return;
            }

            // Get a jungle mob that can die with E
            if (EntityManager.MinionsAndMonsters.GetJungleMonsters(Player.ServerPosition, E.Range, false).Any(m => m.IsRendKillable()))
            {
                E.Cast();
            }
        }
    }
}
