using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECS
{
    public class Context
    {
        internal Archetype EmptyArchetype;
        private List<Archetype> _archetypes;
        private List<Behaviour> _behaviours;

        private Dictionary<Type, byte> _compDictionary = new Dictionary<Type, byte>();
        private IInitiator[] _compInitiators = new IInitiator[byte.MaxValue];
        private byte _compCount;

        public Context()
        {
            _archetypes = new List<Archetype>();
            _behaviours = new List<Behaviour>();
            EmptyArchetype = new Archetype(this, new CompSet(new byte[] { }));
        }

        #region ComponentManagerment
        /// <summary>
        /// registers the component <typeparamref name="T"/> allowing it to be used in this <see cref="Context"/>.
        /// All components must be registered before being used. 
        /// </summary>
        public byte RegisterComponent<T>() where T : IComponent, new()
        {
            if (_compCount == byte.MaxValue) throw new MaxComponentLimitExceeded();

            _compDictionary[typeof(T)] = _compCount;
            _compInitiators[_compCount] = new Initiator<T>();
            return _compCount++;
        }
        /// <summary>
        /// gets the <see cref="byte">Component ID</see> of the component <typeparamref name="T"/>.
        /// </summary>
        public byte ComponentID<T>()
        {
            return _compDictionary.TryGetValue(typeof(T), out byte ID) ? ID : throw new ComponentUnregistered();
        }
        /// <summary>
        /// gets multiple <see cref="byte">Component IDs</see> of the components <typeparamref name="T1"/>, <typeparamref name="T2"/>.
        /// </summary> 
        public byte[] ComponentID<T1, T2>()
        {
            byte ID;
            return new byte[]
            {
                _compDictionary.TryGetValue(typeof(T1), out ID) ? ID : throw new ComponentUnregistered(),
                _compDictionary.TryGetValue(typeof(T2), out ID) ? ID : throw new ComponentUnregistered(),
            };
        }
        /// <summary>
        /// gets multiple <see cref="byte">Component IDs</see> of the components 
        /// <typeparamref name="T1"/>, <typeparamref name="T2"/>, <typeparamref name="T3"/>.
        /// </summary>
        public byte[] ComponentID<T1, T2, T3>()
        {
            byte ID;
            return new byte[]
            {
                _compDictionary.TryGetValue(typeof(T1), out ID) ? ID : throw new ComponentUnregistered(),
                _compDictionary.TryGetValue(typeof(T2), out ID) ? ID : throw new ComponentUnregistered(),
                _compDictionary.TryGetValue(typeof(T3), out ID) ? ID : throw new ComponentUnregistered(),
            };
        }
        /// <summary>
        /// gets multiple <see cref="byte">Component IDs</see> of the components 
        /// <typeparamref name="T1"/>, <typeparamref name="T2"/>, <typeparamref name="T3"/>, <typeparamref name="T4"/>.
        /// </summary>
        public byte[] ComponentID<T1, T2, T3, T4>()
        {
            byte ID;
            return new byte[]
            {
                _compDictionary.TryGetValue(typeof(T1), out ID) ? ID : throw new ComponentUnregistered(),
                _compDictionary.TryGetValue(typeof(T2), out ID) ? ID : throw new ComponentUnregistered(),
                _compDictionary.TryGetValue(typeof(T3), out ID) ? ID : throw new ComponentUnregistered(),
                _compDictionary.TryGetValue(typeof(T4), out ID) ? ID : throw new ComponentUnregistered(),
            };
        }
        /// <summary>
        /// gets multiple <see cref="byte">Component IDs</see> of the components 
        /// <typeparamref name="T1"/>, <typeparamref name="T2"/>, <typeparamref name="T3"/>, <typeparamref name="T4"/>, 
        /// <typeparamref name="T5"/>
        /// </summary>
        public byte[] ComponentID<T1, T2, T3, T4, T5>()
        {
            byte ID;
            return new byte[]
            {
                _compDictionary.TryGetValue(typeof(T1), out ID) ? ID : throw new ComponentUnregistered(),
                _compDictionary.TryGetValue(typeof(T2), out ID) ? ID : throw new ComponentUnregistered(),
                _compDictionary.TryGetValue(typeof(T3), out ID) ? ID : throw new ComponentUnregistered(),
                _compDictionary.TryGetValue(typeof(T4), out ID) ? ID : throw new ComponentUnregistered(),
                _compDictionary.TryGetValue(typeof(T5), out ID) ? ID : throw new ComponentUnregistered(),
            };
        }
        /// <summary>
        /// gets multiple <see cref="byte">Component IDs</see> of the components 
        /// <typeparamref name="T1"/>, <typeparamref name="T2"/>, <typeparamref name="T3"/>, <typeparamref name="T4"/>, 
        /// <typeparamref name="T5"/>, <typeparamref name="T6"/>.
        /// </summary>
        public byte[] ComponentID<T1, T2, T3, T4, T5, T6>()
        {
            byte ID;
            return new byte[]
            {
                _compDictionary.TryGetValue(typeof(T1), out ID) ? ID : throw new ComponentUnregistered(),
                _compDictionary.TryGetValue(typeof(T2), out ID) ? ID : throw new ComponentUnregistered(),
                _compDictionary.TryGetValue(typeof(T3), out ID) ? ID : throw new ComponentUnregistered(),
                _compDictionary.TryGetValue(typeof(T4), out ID) ? ID : throw new ComponentUnregistered(),
                _compDictionary.TryGetValue(typeof(T5), out ID) ? ID : throw new ComponentUnregistered(),
                _compDictionary.TryGetValue(typeof(T6), out ID) ? ID : throw new ComponentUnregistered(),
            };
        }
        /// <summary>
        /// gets multiple <see cref="byte">Component IDs</see> of the components 
        /// <typeparamref name="T1"/>, <typeparamref name="T2"/>, <typeparamref name="T3"/>, <typeparamref name="T4"/>, 
        /// <typeparamref name="T5"/>, <typeparamref name="T6"/>, <typeparamref name="T7"/>.
        /// </summary>
        public byte[] ComponentID<T1, T2, T3, T4, T5, T6, T7>()
        {
            byte ID;
            return new byte[]
            {
                _compDictionary.TryGetValue(typeof(T1), out ID) ? ID : throw new ComponentUnregistered(),
                _compDictionary.TryGetValue(typeof(T2), out ID) ? ID : throw new ComponentUnregistered(),
                _compDictionary.TryGetValue(typeof(T3), out ID) ? ID : throw new ComponentUnregistered(),
                _compDictionary.TryGetValue(typeof(T4), out ID) ? ID : throw new ComponentUnregistered(),
                _compDictionary.TryGetValue(typeof(T5), out ID) ? ID : throw new ComponentUnregistered(),
                _compDictionary.TryGetValue(typeof(T6), out ID) ? ID : throw new ComponentUnregistered(),
                _compDictionary.TryGetValue(typeof(T7), out ID) ? ID : throw new ComponentUnregistered(),
            };
        }
        /// <summary>
        /// gets multiple <see cref="byte">Component IDs</see> of the components 
        /// <typeparamref name="T1"/>, <typeparamref name="T2"/>, <typeparamref name="T3"/>, <typeparamref name="T4"/>, 
        /// <typeparamref name="T5"/>, <typeparamref name="T6"/>, <typeparamref name="T7"/>, <typeparamref name="T8"/>.
        /// </summary>
        public byte[] ComponentID<T1, T2, T3, T4, T5, T6, T7, T8>()
        {
            byte ID;
            return new byte[]
            {
                _compDictionary.TryGetValue(typeof(T1), out ID) ? ID : throw new ComponentUnregistered(),
                _compDictionary.TryGetValue(typeof(T2), out ID) ? ID : throw new ComponentUnregistered(),
                _compDictionary.TryGetValue(typeof(T3), out ID) ? ID : throw new ComponentUnregistered(),
                _compDictionary.TryGetValue(typeof(T4), out ID) ? ID : throw new ComponentUnregistered(),
                _compDictionary.TryGetValue(typeof(T5), out ID) ? ID : throw new ComponentUnregistered(),
                _compDictionary.TryGetValue(typeof(T6), out ID) ? ID : throw new ComponentUnregistered(),
                _compDictionary.TryGetValue(typeof(T7), out ID) ? ID : throw new ComponentUnregistered(),
                _compDictionary.TryGetValue(typeof(T8), out ID) ? ID : throw new ComponentUnregistered(),
            };
        }
        #endregion

        #region Archetype & Behavioural Management
        internal Archetype FindOrCreateArchetype(CompSet compSet)
        {
            // Searches All Archetypes for matching compSet.
            // if none are found, creates a new Archetypes from the compSet.
            int index = _archetypes.BinarySearch(compSet);
            if (index >= 0) return _archetypes[index];

            Archetype newArchetype = new Archetype(this, compSet);
            _archetypes.Insert(~index, newArchetype);

            return newArchetype;
        }
        internal IEnumerable<Archetype> FindApplicableArchetypes(Query query) 
        {
            // Finds the Archetypes that fulfill the query
            for (int i = 0; i < _archetypes.Count; i++)
                if (query.Check(_archetypes[i].compSet))
                    yield return _archetypes[i];
        }
        internal void AddBehaviour(Behaviour behaviour)
        {
            // adds a behaviour to this context.
            _behaviours.Add(behaviour);
        }
        internal IEnumerable<Behaviour> FindApplicableBehaviour(CompSet compSet) 
        {
            // Searches all Behaviours for queries that compSet applies to.
            for (int i = 0; i < _behaviours.Count; i++)
                if (_behaviours[i].query.Check(compSet))
                    yield return _behaviours[i];
        }
        #endregion

        #region Internal Component Management
        internal IComponent InitComponent(byte ID) => _compInitiators[ID].CreateComponent();
        internal IPool InitPool(byte ID) => _compInitiators[ID].CreatePool();

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
        #endregion
    }
}
