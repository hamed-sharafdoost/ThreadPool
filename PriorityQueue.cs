using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VDap.Task6
{
    public class PriorityQueue<T,U> 
    {
        SortedList<U,T> list;
        public PriorityQueue()
        {
            list = new SortedList<U,T>();
        }
        public void Add(T item, U index)
        {
            list.Add(index, item);
        }
        public T Dequeue()
        {
            T item = list.ElementAt(list.Count - 1).Value;
            list.RemoveAt(list.Count-1);
            return item;
        }
        public T Peek()
        {
            return list.ElementAt(list.Count-1).Value;
        }
        public int Count()
        {
            return list.Count;
        }
        public void Clear()
        {
            list.Clear();
        }
    }
}
