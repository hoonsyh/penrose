using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventManager
{
    public delegate void EventReceived(string eventId, Object target, object param);

    static private EventManager sInstance = null;
    static public EventManager Instance
    {
        get
        {
            if (sInstance == null)
            {
                sInstance = new EventManager();
            }
            return sInstance;
        }
        private set { }
    }

    private Dictionary<string, Dictionary<Object, EventReceived>> m_EventMap
        = new Dictionary<string, Dictionary<Object, EventReceived>>();
    private Dictionary<Object, List<string>> m_TargetMap
        = new Dictionary<Object, List<string>>();

    public void Listen(string eventId, Object target, EventReceived callback)
    {
        //m_EventMap에 eventId에 해당하는 key가 없으면 key와 value 추가
        if (!m_EventMap.ContainsKey(eventId))
        {
            m_EventMap.Add(eventId, new Dictionary<Object, EventReceived>());
        }
        //map은 m_EventMap의 eventId에 해당하는 Dictionary
        var map = m_EventMap[eventId];

        //map에 target에 해당하는 키가 없을 경우 target을 key로 callback 추가
        if (!map.ContainsKey(target))
        {
            map.Add(target, callback);

            //m_TargetMap에 target(key)이 없을 경우 target을 key로 String List 추가
            if (!m_TargetMap.ContainsKey(target))
            {
                m_TargetMap.Add(target, new List<string>());
            }

            //m_TargetMap의 target에 해당하는 value로 eventId 추가
            m_TargetMap[target].Add(eventId);
        }
    }

    public void Broadcast(string eventId, object param)
    {
        //eventId 키가 포함되어 있으면 map = m_EventMap
        if (m_EventMap.ContainsKey(eventId))
        {
            Dictionary<Object, EventReceived> map = m_EventMap[eventId];
            //map에 있는 element의 value를 구한다.
            foreach (KeyValuePair<Object, EventReceived> p in map)
            {
                p.Value(eventId, p.Key, param);
            }
        }
    }

    public void Unlisten(string eventId, Object target)
    {
        if (m_EventMap.ContainsKey(eventId))
        {
            m_EventMap[eventId].Remove(target);
            if (!m_TargetMap.ContainsKey(target))
            {
                m_TargetMap[target].Remove(eventId);
            }
        }
    }

    public void UnlistenAll(Object target)
    {
        foreach (KeyValuePair<Object, List<string>> p in m_TargetMap)
        {
            foreach (string eventId in p.Value)
            {
                Unlisten(eventId, p.Key);
            }
        }
    }
}
