using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PriorityQueue<P, T>
{
    public List<KeyValuePair<P, T>> elements;
    public Comparer<P> comparer;

    public PriorityQueue(Comparer<P> comparer)
    {
        elements = new();
        this.comparer = comparer;
    }

    public void Enqueue(P priority, T item)
    {
        elements.Add(new KeyValuePair<P, T>(priority, item));
        int i = elements.Count - 1;
        while (i > 0 && comparer.Compare(elements[i].Key, elements[(i - 1) / 2].Key) < 0)
        {
            Swap(i, (i - 1) / 2);
            i = (i - 1) / 2;
        }
    }

    public KeyValuePair<P, T> Dequeue()
    {
        KeyValuePair<P, T> result = elements[0];
        int size = elements.Count - 1;
        elements[0] = elements[size];
        elements.RemoveAt(size);
        HeapifyDown(0);
        return result;
    }

    public void HeapifyDown(int i)
    {
        int size = elements.Count;
        while (2 * i + 1 < size)
        {
            int child = 2 * i + 1;
            if (child + 1 < size && comparer.Compare(elements[child + 1].Key, elements[child].Key) < 0)
                child++;
            if (comparer.Compare(elements[i].Key, elements[child].Key) < 0)
                break;
            Swap(i, child);
            i = child;
        }
    }

    public int GetCount()
    {
        return elements.Count;
    }

    private void Swap(int i, int j)
    {
        KeyValuePair<P, T> temp = elements[i];
        elements[i] = elements[j];
        elements[j] = temp;
    }

}
