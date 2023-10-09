using System.Collections.Generic;

namespace Events.EventsManager
{
    public class EventsManager : IEventsManager
    {
        private readonly List<IEventsUser> _interfaceObjects = new();
        
        public void RegisterObject(IEventsUser obj)
        {
            _interfaceObjects.Add(obj);
            obj.Subscribe();
        }

        public void UnregisterAll()
        {
            foreach (var obj in _interfaceObjects)
            {
                obj.Unsubscribe();
            } 
        }
    }
}