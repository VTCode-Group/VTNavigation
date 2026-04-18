using System;
using System.Collections.Generic;

namespace DataStructures.PriorityQueue
{
    public class HeapQueue<T> where T : IComparable<T>
    {

        private List<T> array;

        public HeapQueue(int initSize)
        {
            array = new List<T>(initSize);
        }

        public int Count
        {
            get
            {
                return array.Count;
            }
        }

        private void Swap(int idx1, int idx2)
        {
            T temp = array[idx1];
            array[idx1] = array[idx2];
            array[idx2] = temp;
        }

        private void UpdateHeapUp(int index)
        {
            if (0 == index)
            {
                return;
            }
            int parent = index >> 1;
            if (1 != array[parent].CompareTo(array[index]))
            {
                return;
            }
            Swap(index, parent);
            UpdateHeapUp(parent);
        }

        private void UpdateHeapDown(int index)
        {
            int leftChild = index << 1;
            if (array.Count <= leftChild)
            {
                return;
            }
            int rightChild = leftChild | 1;
            int swapIndex = index;
            if (-1 == array[leftChild].CompareTo(array[swapIndex]))
            {
                swapIndex = leftChild;
            }
            if (rightChild < array.Count && -1 == array[rightChild].CompareTo(array[swapIndex]))
            {
                swapIndex = rightChild;
            }
            if (swapIndex == index)
            {
                return;
            }
            Swap(index, swapIndex);
            UpdateHeapDown(swapIndex);
        }

        public void Enqueue(T value)
        {
            array.Add(value);
            UpdateHeapUp(array.Count - 1);
        }

        public T Dequeue()
        {
            if (0 == array.Count)
            {
                throw new InvalidOperationException();
            }
            T retValue = array[0];
            array[0] = array[array.Count - 1];
            array.RemoveAt(array.Count - 1);
            UpdateHeapDown(0);
            return retValue;
        }

        public void Clear()
        {
            array.Clear();
        }
    }
}
