using System;
using System.Collections;
using System.Collections.Generic;

namespace ECS
{
    /// <summary>
    /// A module of data that can attach to entities to provide functionality. 
    /// All data relating to an <see cref="Entity"/> is stored through an <see cref="IComponent"/>.
    /// </summary>
    public interface IComponent : IPoolable { }

    /// <summary>
    /// Assigns each <see cref="IComponent"/> an ID used for early binding initiation
    /// </summary>
    public static class ComponentManager
    {
        private static byte _count;
        private static Type[] _types = new Type[byte.MaxValue];
        private static IInitiator[] _initiators = new IInitiator[byte.MaxValue];
        
        private static byte RegisterID<TComponent>() where TComponent : IComponent, new()
        {
            if (_count == byte.MaxValue) throw new MaxComponentLimitExceeded();

            _types[_count] = typeof(TComponent);
            _initiators[_count] = new Initiator<TComponent>();
            return _count++;
        }
        
        public static byte ID<T>() where T : IComponent, new() => ComponentType<T>.ID;
        public static byte[] ID<T1, T2>()
            where T1 : IComponent, new()
            where T2 : IComponent, new()
        {
            return new byte[]
            {
                ComponentType<T1>.ID,
                ComponentType<T2>.ID,
            };
        }
        public static byte[] ID<T1, T2, T3>()
            where T1 : IComponent, new()
            where T2 : IComponent, new()
            where T3 : IComponent, new()
        {
            return new byte[]
            {
                ComponentType<T1>.ID,
                ComponentType<T2>.ID,
                ComponentType<T3>.ID,
            };
        }
        public static byte[] ID<T1, T2, T3, T4>()
            where T1 : IComponent, new()
            where T2 : IComponent, new()
            where T3 : IComponent, new()
            where T4 : IComponent, new()
        {
            return new byte[]
            {
                ComponentType<T1>.ID,
                ComponentType<T2>.ID,
                ComponentType<T3>.ID,
                ComponentType<T4>.ID,
            };
        }

        internal static byte Count => _count;
        internal static Type GetType(byte ID) => _types[ID];
        internal static IComponent InitComponent(byte ID) => _initiators[ID].CreateComponent();
        internal static IPool InitPool(byte ID) => _initiators[ID].CreatePool();
        
        private interface IInitiator
        {
            IComponent CreateComponent();
            IPool CreatePool();
        }
        private class Initiator<TComponent> : IInitiator where TComponent : IComponent, new()
        {
            IComponent IInitiator.CreateComponent() => new TComponent();
            IPool IInitiator.CreatePool() => new Pool<TComponent>();
        }
        private static class ComponentType<TComponent> where TComponent : IComponent, new()
        {
            public static readonly byte ID;
            static ComponentType()
            {
                ID = RegisterID<TComponent>();
            }
        }
    }

}
