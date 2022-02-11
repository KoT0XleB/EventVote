using System;
using CommandSystem;
using MEC;
using Qurre.API;

namespace EventVote
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    [CommandHandler(typeof(ClientCommandHandler))]
    public class Hack : ICommand
    {
        public string Command => "event";
        public string[] Aliases => new string[] { };
        public string Description => $"Начать голосование за ивент: event (Название) или (start) или (drop)";
        public string EventName { get; set; } = string.Empty;
        public bool EventStarted { get; set; } = false;

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            string eventName = string.Empty;
            Player admin = Player.Get((CommandSender)sender);

            if (!Round.Waiting)
            {
                response = $"{admin.Nickname} Голосование проводится в ожидании игроков!";
                return false;
            }

            if (arguments.Count < 1)
            {
                response = $"{admin.Nickname} введите название ивента: event Название";
                return false;
            }

            if (arguments.Count == 1)
            {
                if (arguments.At(0) == "start")
                {
                    if (EventName == string.Empty)
                    {
                        response = $"Сначала проведите голосование за ивент: event (Название)";
                        return false;
                    }
                    if (EventStarted)
                    {
                        response = $"Ивент {eventName} уже начался!";
                        return false;
                    }
                    EventHandler.EventStart(EventName, admin);
                    response = $"Начался ивент {eventName}";
                    return true;
                }
                if (arguments.At(0) == "drop")
                {
                    if (eventName == string.Empty)
                    {
                        response = $"Сначала проведите голосование за ивент: event (Название)";
                        return false;
                    }

                    EventHandler.EventDrop(EventName, admin);

                    response = $"Ивент {eventName} был дропнут!";
                    eventName = string.Empty;
                    return true;
                }
                eventName = arguments.At(0);
            }

            if (arguments.Count > 1)
            {
                foreach (string Arguments in arguments)
                {
                    eventName += Arguments + " ";
                }
                eventName = eventName.Remove(eventName.Length - 1);
            }

            EventName = eventName;
            if (!EventHandler.Coroutine.IsRunning)
                EventHandler.Coroutine = Timing.RunCoroutine(EventHandler.EventVoting(EventName, admin), "startvote");

            response = $"<color=green>Голосование на ивент <color=red>{EventName}</color> запущено!\n" +
                    $"{admin.Nickname} люди замьючены!\n" +
                    $"{admin.Nickname} подскажите игрокам, что необходимо нажимать на <color=red>[V]</color> или <color=red>[Q]</color></color>";
            return true;
        }
    }
}
