using System;
using System.Collections.Generic;
using System.Linq;

class PriorityQueue<TP, TV>
{
    private readonly SortedDictionary<TP, Queue<TV>> m_list = new SortedDictionary<TP, Queue<TV>>();

    public void Enqueue(TP _Priority, TV _Value)
    {
        Queue<TV> q;
        if (!m_list.TryGetValue(_Priority, out q))
        {
            q = new Queue<TV>();
            m_list.Add(_Priority, q);
        }
        q.Enqueue(_Value);
    }

    public TV Dequeue()
    {
        // will throw if there isn’t any first element!
        var pair = m_list.First();
        var v = pair.Value.Dequeue();
        if (pair.Value.Count == 0) // nothing left of the top priority.
            m_list.Remove(pair.Key);
        return v;
    }

    public bool IsEmpty
    {
        get { return !m_list.Any(); }
    }
}