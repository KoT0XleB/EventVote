using MEC;
using Newtonsoft.Json;
using Qurre;
using Qurre.API;
using Qurre.API.Events;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EventVote
{
    public class EventHandler
    {
        public EventHandler(EventVote plugin) => Plugin = plugin;
        public static EventVote Plugin { get; private set; }
        public static CoroutineHandle Coroutine { get; set; }
        public static Audio audio { get; set; }
        public static bool EventStarted { get; set; } = false;

        public static void EventStart(string eventName, Player admin)
        {
            foreach (Player player in Player.List)
            {
                player.ClearBroadcasts();
                player.Broadcast($"<color=yellow>Начинается ивент <color=red>{eventName}</color> админа <color=red>{admin.Nickname}</color></color>", 15);
            }
            new Thread(() => { WebhookMessage($"Ивент {eventName}", $"Админ {admin.Nickname} начал проводить ивент {eventName}"); }).Start();
            Timing.KillCoroutines("startvote");
        }
        public static void EventDrop(string eventName, Player admin)
        {
            if (audio.Microphone.IsRecording) audio.Microphone.StopCapture();
            foreach (Player player in Player.List)
            {
                player.ClearBroadcasts();
                player.Broadcast($"<color=yellow>Ивент <color=red>{eventName}</color> был отменён админом <color=red>{admin.Nickname}</color></color>", 10);
            }
            new Thread(() => { WebhookMessage($"Ивент {eventName}", $"Админ {admin.Nickname} досрочно прекратил ивент {eventName}"); }).Start();
            EventStarted = false;
            Timing.KillCoroutines("startvote");
        }
        public void OnRoundStarted()
        {
            EventStarted = true;
            if (audio.Microphone.IsRecording) audio.Microphone.StopCapture();
            players.Clear();
        }
        public void OnRoundEnded(RoundEndEvent ev)
        {
            if (EventStarted)
            {
                foreach (Player player in Player.List)
                {
                    player.ClearBroadcasts();
                    player.Broadcast($"Ивент был закончен", 10);
                }
                new Thread(() => { WebhookMessage($"Ивент закончился", string.Empty); }).Start();
            }
            EventStarted = false;
        }
        public static IEnumerator<float> EventVoting(string eventName, Player admin)
        {
            new Thread(() => { WebhookMessage($"Ивент {eventName}", $"Голосование за выбор ивента {eventName} админом {admin.Nickname} началось!"); }).Start();
            Round.LobbyLock = true;
            AudioPlay();

            var time = Plugin.CustomConfig.Time;
            while (time > 0)
            {
                foreach (Player player in Player.List)
                {
                    if (player != admin) player.MuteInRound(true);
                    player.ClearBroadcasts();
                    player.Broadcast($"Опрос: <color=red>{players.Count} из {Player.List.Count()}</color> <color=yellow>игроков за ивент <color=red>{eventName}</color>!</color>\n" +
                    $"<color=yellow>Нажмите <color=red>[V]</color> <color=cyan>За</color> и <color=red>[Q]</color> <color=green>Против</color></color>\n" +
                    $"<color=yellow>Осталось <color=red>{(int)time}</color> секунд!</color>", 1);
                }
                time -= 0.5f;
                yield return Timing.WaitForSeconds(0.5f);
            }
            Round.LobbyLock = false;

            new Thread(() => { WebhookMessage($"Ивент {eventName}", $"Голосование закончилось: {players.Count} из {Player.List.Count()} за ивент {eventName} админа {admin.Nickname}"); }).Start();
            foreach (Player player in Player.List)
            {
                player.MuteInRound(false);
                player.ClearBroadcasts();
                player.Broadcast($"Опрос: <color=yellow><color=red>{players.Count} из {Player.List.Count()}</color> игроков за ивент <color=red>{eventName}</color>!\n" +
                    $"Ивент начнется по усмотрению Ивент-Мастера!</color>", 10);
            }
            yield break;
        }
        public static void AudioPlay()
        {
            try
            {
                string AudioPath = Path.Combine(PluginManager.ConfigsDirectory, "Audio");
                if (!Directory.Exists(AudioPath))
                {
                    Directory.CreateDirectory(AudioPath);
                    Log.Info("The Audio folder does not exist, so it was created in the Qurre configs");
                }
                if (AudioPath.Contains(Plugin.CustomConfig.ListMusic.RandomItem()))
                {
                    AudioPath = Path.Combine(AudioPath, Plugin.CustomConfig.ListMusic.RandomItem());

                    audio = new Audio(new FileStream(AudioPath, FileMode.Open), 20);
                }
                else Log.Info("No sound was found.");
            }
            catch { }
        }
        public static void WebhookMessage(string title, string text)
        {
            var config = Plugin.CustomConfig;
            WebRequest web = WebRequest.CreateHttp(config.Token);
            web.ContentType = "application/json";
            web.Method = "POST";
            using (var sw = new StreamWriter(web.GetRequestStream()))
            {
                string json = JsonConvert.SerializeObject(new
                {
                    embeds = new[]
                    {
                        new
                        {
                            title = title,
                            description = text,
                            color = config.Color,
                            image = new
                            {
                                url = config.Image
                            }
                        }
                    }
                });
                sw.Write(json);
            }
            var response = web.GetResponse();
        }
        public void PressedQ(PressPrimaryChatEvent ev)
        {
            if (ev.Value && Timing.IsRunning(Coroutine))
            {
                if (players.Contains(ev.Player)) players.Remove(ev.Player);
            }
        }
        public void PressedV(PressAltChatEvent ev)
        {
            if (ev.Value && Timing.IsRunning(Coroutine))
            {
                if (!players.Contains(ev.Player)) players.Add(ev.Player);
            }
        }
        public void PlayerLeave(LeaveEvent ev)
        {
            if (Timing.IsRunning(Coroutine))
            {
                if (players.Contains(ev.Player)) players.Remove(ev.Player);
            }
        }
        public static List<Player> players { get; set; } = new List<Player>();
    }
}
