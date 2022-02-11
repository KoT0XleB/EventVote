using Qurre;
using Qurre.API;
using Qurre.API.Events;
using MEC;
using System.Collections.Generic;
using System.Linq;

using Player = Qurre.API.Player;
using Version = System.Version;

namespace EventVote
{
    public class EventVote : Plugin
    {
        public override string Developer => "KoT0XleB#4663";
        public override string Name => "EventVote";
        public override Version Version => new Version(2, 0, 0);
        public override void Enable() => RegisterEvents();
        public override void Disable() => UnregisterEvents();
        public Config CustomConfig { get; set; }
        private EventHandler handler;
        public void RegisterEvents()
        {
            CustomConfig = new Config();
            CustomConfigs.Add(CustomConfig);
            if (!CustomConfig.IsEnable) return;

            handler = new EventHandler(this);

            Qurre.Events.Round.Start += handler.OnRoundStarted;
            Qurre.Events.Round.End += handler.OnRoundEnded;

            Qurre.Events.Voice.PressPrimaryChat += handler.PressedQ;
            Qurre.Events.Voice.PressAltChat += handler.PressedV;
            Qurre.Events.Player.Leave += handler.PlayerLeave;

        }
        public void UnregisterEvents()
        {
            CustomConfigs.Remove(CustomConfig);
            if (!CustomConfig.IsEnable) return;

            Qurre.Events.Round.Start -= handler.OnRoundStarted;
            Qurre.Events.Round.End -= handler.OnRoundEnded;

            Qurre.Events.Voice.PressPrimaryChat -= handler.PressedQ;
            Qurre.Events.Voice.PressAltChat -= handler.PressedV;
            Qurre.Events.Player.Leave -= handler.PlayerLeave;

            handler = null;
        }
    }
}