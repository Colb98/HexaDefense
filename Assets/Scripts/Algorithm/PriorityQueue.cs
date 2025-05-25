using System;
using System.Collections.Generic;

public class PriorityQueue<TElement, TPriority> where TPriority : IComparable<TPriority>
{
    private List<(TElement Element, TPriority Priority)> heap = new();

    public int Count => heap.Count;

    public void Enqueue(TElement element, TPriority priority)
    {
        heap.Add((element, priority));
        HeapifyUp(heap.Count - 1);
    }

    public TElement Dequeue()
    {
        if (heap.Count == 0)
            throw new InvalidOperationException("Queue is empty");

        TElement result = heap[0].Element;
        heap[0] = heap[^1];
        heap.RemoveAt(heap.Count - 1);
        HeapifyDown(0);
        return result;
    }

    public TElement Peek()
    {
        if (heap.Count == 0)
            throw new InvalidOperationException("Queue is empty");
        return heap[0].Element;
    }

    private void HeapifyUp(int index)
    {
        while (index > 0)
        {
            int parent = (index - 1) / 2;
            if (heap[index].Priority.CompareTo(heap[parent].Priority) < 0)
            {
                Swap(index, parent);
                index = parent;
            }
            else break;
        }
    }

    private void HeapifyDown(int index)
    {
        int count = heap.Count;
        while (true)
        {
            int left = index * 2 + 1;
            int right = index * 2 + 2;
            int smallest = index;

            if (left < count && heap[left].Priority.CompareTo(heap[smallest].Priority) < 0)
                smallest = left;
            if (right < count && heap[right].Priority.CompareTo(heap[smallest].Priority) < 0)
                smallest = right;

            if (smallest == index) break;

            Swap(index, smallest);
            index = smallest;
        }
    }

    private void Swap(int i, int j)
    {
        (heap[i], heap[j]) = (heap[j], heap[i]);
    }

    public bool IsEmpty => heap.Count == 0;

    public bool Contains(TElement element)
    {
        foreach (var item in heap)
            if (EqualityComparer<TElement>.Default.Equals(item.Element, element))
                return true;
        return false;
    }
}
