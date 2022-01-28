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
        public static bool Enabled { get; internal set; }
        public override string Developer => "KoT0XleB#4663";
        public override string Name => "EventVote";
        public override Version Version => new Version(1, 0, 0);
        public override void Enable() => RegisterEvents();
        public override void Disable() => UnregisterEvents();
        public bool EventOn { get; set; } = false;
        public string EventName { get; set; } = string.Empty;
        public string AdminName { get; set; } = string.Empty;
        public Config CustomConfig { get; set; }
        public void RegisterEvents()
        {
            CustomConfig = new Config();
            CustomConfigs.Add(CustomConfig);
            if (!CustomConfig.IsEnable) return;

            Qurre.Events.Round.Start += OnRoundStarted;
            Qurre.Events.Round.End += OnRoundEnded;

            Qurre.Events.Voice.PressPrimaryChat += PressedQ;
            Qurre.Events.Voice.PressAltChat += PressedV;
            Qurre.Events.Server.SendingRA += SendRA;
            Qurre.Events.Player.Leave += PlayerLeave;

        }
        public void UnregisterEvents()
        {
            CustomConfigs.Remove(CustomConfig);
            if (!CustomConfig.IsEnable) return;

            Qurre.Events.Round.Start -= OnRoundStarted;
            Qurre.Events.Round.End -= OnRoundEnded;

            Qurre.Events.Voice.PressPrimaryChat -= PressedQ;
            Qurre.Events.Voice.PressAltChat -= PressedV;
            Qurre.Events.Server.SendingRA -= SendRA;
            Qurre.Events.Player.Leave -= PlayerLeave;
        }
        public void SendRA(SendingRAEvent ev)
        {
            if (ev.Name == "event" && Round.Waiting)
            {
                EventName = string.Empty;
                AdminName = ev.Player.Nickname;

                if (ev.Args.Count() == 0)
                {
                    ev.Allowed = false;
                    ev.Success = false;
                    ev.ReplyMessage = $"<color=red>{AdminName} введите название ивента: event Название</color>";
                    return;
                }

                if (ev.Args.Count() > 0)
                {
                    if (ev.Args[0] == "drop")
                    {
                        /*
                        if (EventName == string.Empty)
                        {
                            ev.Allowed = false;
                            ev.Success = false;
                            ev.ReplyMessage = $"<color=red>{AdminName} вы не запустили голосование, поэтому его нельзя дропнуть!</color>";
                            return;
                        }
                        if (EventName != string.Empty)
                        {
                            ev.Allowed = true;
                            ev.Success = true;

                            BanLogger.BanLogger.WebhookMessage($"Голосование: {AdminName} остановил проведение голосования/ивента {EventName}", $"{EventName}", "65280", $"{CustomConfig.Token}", "");
                            ev.ReplyMessage = $"<color=green>{AdminName} ваше голосование/ивент {EventName} был дропнут!\nОн не будет отображаться в логах!</color>";

                            Timing.KillCoroutines("event");
                            EventName = string.Empty;
                            return;
                        }
                        */
                    }

                    foreach (string Arguments in ev.Args)
                    {
                        EventName += Arguments + " ";
                    }
                }
                ev.ReplyMessage = $"<color=green>Голосование на ивент <color=red>{EventName}</color>запущено!\n" +
                    $"{AdminName} замутьте людей, чтобы они вам не мешали!\n" +
                    $"{AdminName} подскажите игрокам, что необходимо нажимать на <color=red>[V]</color> или <color=red>[Q]</color></color>";

                EventOn = true;
                Round.LobbyLock = true;
                BanLogger.BanLogger.WebhookMessage($"Голосование: {AdminName} начал выбор ивента {EventName}", $"{EventName}", "65280", $"{CustomConfig.Token}", "");
                Timing.KillCoroutines("event");
                Timing.RunCoroutine(EventVoting(), "event");
            }
            else if (ev.Name == "event")
            {
                ev.Allowed = false;
                ev.Success = false;
                ev.ReplyMessage = $"<color=red>{AdminName} Голосование проводится в ожидании игроков!</color>";
                return;
            }
        }
        public IEnumerator<float> EventVoting()
        {
            var time = CustomConfig.Time;
            while (time > 0)
            {
                foreach (Player player in Player.List)
                {
                    player.ClearBroadcasts();
                    player.Broadcast($"Опрос: <color=red>{players.Count} из {Player.List.Count()}</color> <color=yellow>игроков за ивент <color=red>{EventName}</color>!</color>\n" +
                    $"<color=yellow>Нажмите <color=red>[V]</color> <color=cyan>За</color> и <color=red>[Q]</color> <color=green>Против</color></color>\n" +
                    $"<color=yellow>Осталось <color=red>{(int)time}</color> секунд!</color>", 1);
                }
                time -= 0.5f;
                yield return Timing.WaitForSeconds(0.5f);
            }

            EventOn = false;
            Round.LobbyLock = false;

            foreach (Player player in Player.List)
            {
                player.ClearBroadcasts();
                player.Broadcast($"Опрос: <color=yellow><color=red>{players.Count} из {Player.List.Count()}</color> игроков за ивент <color=red>{EventName}</color>! \n Ивент начнется по усмотрению Ивент-Мастера!</color>", 10);
            }
            yield break;
        }
        public void OnRoundStarted()
        {
            Timing.KillCoroutines("event");
            if (EventName != string.Empty) BanLogger.BanLogger.WebhookMessage($"Ивент: {AdminName} начал проводить ивент {EventName}", $"{EventName}", "65280", $"{CustomConfig.Token}", "");
            players.Clear();
        }
        public void OnRoundEnded(RoundEndEvent ev)
        {
            if (EventName != string.Empty) BanLogger.BanLogger.WebhookMessage($"Ивент: {AdminName} закончил проводить ивент {EventName}", $"{EventName}", "65280", $"{CustomConfig.Token}", "");
        }
        public void PressedQ(PressPrimaryChatEvent ev)
        {
            if (ev.Value && EventOn)
            {
                if (players.Contains(ev.Player)) players.Remove(ev.Player);
            }
        }
        public void PressedV(PressAltChatEvent ev)
        {
            if (ev.Value && EventOn)
            {
                if (!players.Contains(ev.Player)) players.Add(ev.Player);
            }
        }
        public void PlayerLeave(LeaveEvent ev)
        {
            if (EventOn)
            {
                if (players.Contains(ev.Player)) players.Remove(ev.Player);
            }
        }
        public List<Player> players { get; set; } = new List<Player>();
    }
}