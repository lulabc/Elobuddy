using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EloBuddy;
namespace LuckyAio
{
    class Loader
    {
        static void Main(string[] args)
        {
            EloBuddy.SDK.Events.Loading.OnLoadingComplete+= Loading;
        }
        static void Loading(EventArgs args)
        {
            if (Player.Instance.ChampionName == "Sivir")
            {
                Champions.Sivir.SivirLoading();
            }
            if (Player.Instance.ChampionName == "Twitch")
            {
                Champions.Twitch.TwitchLoading();
            }
            return;
        }
    }
}
