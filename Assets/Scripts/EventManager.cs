
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace ImstkUnity
{

    public class ImstkEvent : UnityEvent<GameObject>
    {
    }

    public class EventManager : Singleton<EventManager>
    {
        private Dictionary<string, ImstkEvent> _callbacks = new Dictionary<string, ImstkEvent>();

        public void Register(string eventName, UnityAction<GameObject> call )
        {
            GetOrCreate(eventName).AddListener(call);
        }

        public void Unregister(string eventName, UnityAction<GameObject> call)
        {
            GetOrCreate(eventName).RemoveListener(call);
        }

        public void Emit(string eventName, GameObject sender)
        {
            GetOrCreate(eventName).Invoke(sender);
        }

        private ImstkEvent GetOrCreate(string eventName )
        {
            if ( !_callbacks.ContainsKey( eventName ) )
            {
                _callbacks.Add(eventName, new ImstkEvent());
            }
            return _callbacks[eventName];
        }
    }
}
