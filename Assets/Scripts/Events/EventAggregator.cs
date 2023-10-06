using System.Collections.Generic;
using ConstantValues;

namespace Events
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
    
    public class CellAdded { public CellLogic.Cell Cell; }
    public class NextTurn { public Enums.Team CellTeam { get; set; } }
    public class AddBots { public List<Enums.Team> Teams; }
    public class AddToNearbyCells { public CellLogic.Cell Cell; }
    public class PlayerLost { public string PlayerName; }
    public class GetTurn { public Enums.GameStates GameState; }
    public class PrepareForNextTurn { public Enums.Team CellTeam; }
}