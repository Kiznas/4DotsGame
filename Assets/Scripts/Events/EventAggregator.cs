using System.Collections.Generic;
using UnityEngine;

namespace EventHandler
{
    public static class EventAggregator
    {
        public static void Subscribe<T>(System.Action<object, T> eventCallback)
        {
            EventHelper<T>.Event += eventCallback;
        }

        public static void Unsubscribe<T>(System.Action<object, T> eventCallback)
        {
            EventHelper<T>.Event -= eventCallback;
        }

        public static void Post<T>(object sender, T eventData)
        {
            EventHelper<T>.Post(sender, eventData);
        }

        private static class EventHelper<T>
        {
            public static event System.Action<object, T> Event;

            public static void Post(object sender, T eventData)
            {
                Event?.Invoke(sender, eventData);
            }
        }
    }

    //EVENTS//
    public class UpdateImages { };
    public class CellAdded { public Cell Cell; }
    public class NextTurn { public Team cellTeam; }
    public class AddBots { public List<Team> teams; }
    public class AddToNearbyCells { public Cell cell; }
    public class PlayerLost { public string PlayerName; }
    public class GetTurn { public GameStates gameState; }
    public class PrepareForNextTurn { public Team cellTeam; }
    public class Initialization { public Color[] teamsColorList; }
}