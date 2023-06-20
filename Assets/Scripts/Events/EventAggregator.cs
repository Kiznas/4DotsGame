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
    public class CellAdded { public CellInstance.Cell CellInstance; };
    public class AddToNearbyCells { public CellInstance.Cell CellInstance; }
    public class CheckNearbyCells { public CellInstance.Cell CellInstance; }
}