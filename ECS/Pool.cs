using System;
using System.Collections;
using System.Collections.Generic;

namespace ECS
{
    /// <summary>
    /// <see cref="IPoolable"/> is an object thats stored in an <see cref="Archetype.Pool{T}"/>.
    /// </summary>
    public interface IPoolable { }

    /// <summary>
    /// A resizable contiguous collection used in <see cref="Archetype"/>. 
    /// </summary>
    internal interface IPool
    {
        IPoolable this[int index] { get; set; }
        void Resize(int newSize);
        void Remove(int index);
    }

    /// <summary>
    /// implements <see cref="IPool"/> 
    /// </summary>
    public class Pool<T> : IPool where T : IPoolable
        {
            private T[] _array = new T[Archetype.DEFAULT_ARRAY_SIZE];
            public ref T this[int index] => ref _array[index];
            IPoolable IPool.this[int index]
            {
                get => _array[index];
                set => _array[index] = (T)value;
            }
            void IPool.Remove(int index) => _array[index] = default;
            void IPool.Resize(int newSize) => Array.Resize(ref _array, newSize);

            public override string ToString()
            {
                return $"Pool<{typeof(T).Name}>[{_array.Length}]";
            }
        }
}