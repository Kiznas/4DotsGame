using Services;

namespace Events.EventsManager
{
    public interface IEventsManager : IService 
    {
    void RegisterObject(IEventsUser obj);
    void UnregisterAll();
    }
}