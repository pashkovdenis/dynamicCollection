using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DymanicContainer
{
    /// <summary>
    /// You should allow very large objects in order to use this collection.
    ///   <runtime>
    ///    <gcAllowVeryLargeObjects enabled = "true" />
    ///    </ runtime >
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class DynamicCollection<T> : ICollection<T>
    { 
        private readonly Stack<HashSet<T>> _containers;
        private readonly int _maxCapacity;  
        private const int _thressHold = 173741824; 
        private HashSet<T> _current;
        public int Count => Containers().Sum(c => c.Count);
        public bool IsReadOnly => false; 
        public DynamicCollection( )
        { 
            var typeSize = Marshal.SizeOf<T>(); 
            _maxCapacity = (_thressHold / typeSize) / 2;
            _containers = new Stack<HashSet<T>>();
            _containers.Push(new HashSet<T>(_maxCapacity  )); 
        } 
        private HashSet<T> GetContainer(bool add = false)
        {
            var synced = false;
            var spLock = new SpinLock(true);
            try
            {
                spLock.Enter(ref synced);
                if (_current == null)
                {
                    _current = _containers.OrderBy(x => x.Count).FirstOrDefault();
                }
                if (add && _current.Count + 1 > _maxCapacity)
                {
                    _current = new HashSet<T>(_maxCapacity);
                    _containers.Push(_current);
                }
                return _current;
            }
            finally
            {
                if (synced)
                {
                    spLock.Exit(); 
                }
            }
        }  
        private IEnumerable<HashSet<T>> Containers()
        {   
            foreach (var c in  _containers)
            {
                yield return c;
            }
        } 
        public void Add(T item)
        {
            GetContainer(true).Add(item);
        }   
        public void Clear()
        {
            var synced = false;
            var spLock = new SpinLock(true);
            try
            {
                foreach (var c in Containers())
                    {
                        c.Clear(); 
                    }
                    _containers.Clear();
                    _containers.Push(new HashSet<T>(_maxCapacity));
            }
            finally
            {
                if (synced)
                {
                    spLock.Exit();
                }
            }
        }  

        public bool Contains(T item)
        {
            var c = false;
            Parallel.ForEach(Containers(), container =>
              {
                  if (container.Contains(item) && !c)
                  {
                      c = true; 
                  } 

              });  
            return c;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            throw new NotImplementedException(); 
        } 
  
        public bool Remove(T item)
        {
            var synced = false;
            var spLock = new SpinLock(true);
            try
            {
                var removed = false;
                Parallel.ForEach(Containers(), container =>
                {
                    if (container.Contains(item) )
                    {
                        removed=container.Remove(item); 
                    } 
                });
                return removed;
            }
            finally
            {
                if (synced)
                {
                    spLock.Exit();
                }
            }
        } 
        public IEnumerator<T> GetEnumerator()
        {
            foreach (var container in Containers().Reverse())
            {
                foreach (var item in container)
                {
                    yield return item;
                }
            }
        }  
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    } 



}
