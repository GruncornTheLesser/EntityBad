using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace ECS
{
    /// <summary>
    /// A collection of <see cref="Entity"/> which all share the same types of <see cref="IComponent"/>.
    /// </summary>
    public partial class Archetype : IComparable<ComponentSet>                  // Archetype.All sorted by ComponentSet
    {
        public readonly static Archetype Empty;                                 // the archetype for an entity with no components
        private readonly static List<Archetype> All;                             // a list of all archetypes that have been created so far, sorted by "compSet"

        internal readonly ComponentSet compSet;                                 // a bit array represent which components this Archetype contains
        internal readonly IPool entities = new Pool<Entity>();                  // the storage for entities     
        internal readonly IPool[] components = new IPool[byte.MaxValue + 1];    // the storage for components

        private readonly Archetype[] _next = new Archetype[byte.MaxValue + 1];  // a graph to find the next archetype
        private readonly Archetype[] _prev = new Archetype[byte.MaxValue + 1];  // a graph to find the prev archetype

        /// <summary>
        /// The number of <see cref="Entity"/>s stored in this <see cref="Archetype"/>.
        /// </summary>
        public int Length { get; private set; }
        
        internal const int DEFAULT_ARRAY_SIZE = 128;
        private int _arraySize = DEFAULT_ARRAY_SIZE;                            // the max number of entities before pools require resizing

        static Archetype()
        {
            All = new List<Archetype>();
            Empty = new Archetype(new ComponentSet(new byte[0]));
            All.Add(Empty);
        }

        // constructor can be private because archetype its only called from Archetype.FindOrCreate() and static constructor
        private Archetype(ComponentSet CompSet)
        {
            this.compSet = CompSet;

            foreach (byte CompID in compSet)
                components[CompID] = ComponentManager.InitPool(CompID); // init component pools
            entities = new Pool<Entity>();                              // init entity pool

            foreach (Behaviour B in Behaviour.FindApplicable(compSet))
                B.archetypes.Add(this);
        }

        #region GetPools
        /// <summary>
        /// gets the component pool.
        /// </summary>
        public Pool<TComponent> GetPool<TComponent>() where TComponent : IComponent, new()
        {
            return (Pool<TComponent>)components[ComponentManager.ID<TComponent>()];
        }
        /// <summary>
        /// gets the entity pool.
        /// </summary>
        /// <returns></returns>
        public Pool<Entity> GetEntityPool()
        {
            return (Pool<Entity>)entities;
        }


        #endregion

        #region Moving Entity
        /// <summary>
        /// Initiates <see cref="Entity"/> into this Archetype
        /// </summary>
        internal void InitEntity(Entity entity, out int poolIndex)
        {
            if (Length == _arraySize)
            {
                _arraySize *= 2;
                entities.Resize(_arraySize);
                foreach (byte compID in compSet)
                    components[compID].Resize(_arraySize);
            }
            
            entities[Length] = entity;
            foreach (byte compID in compSet)
                components[compID][Length] = ComponentManager.InitComponent(compID); // reset all components
            poolIndex = Length++;
        }
        
        /// <summary>
        /// Removes the <see cref="Entity"/> into this Archetype
        /// </summary>
        internal void DestroyEntity(int index)
        {
            // move end of pool to overwrite layer
            if (index != --Length)
                foreach(byte compID in compSet)
                    components[compID][index] = components[compID][Length];
            // entity has been moved so index must be updated 
            (entities[index] as Entity).poolIndex = index;


            // remove layer at end of pools
            foreach(byte compID in compSet)
                components[compID].Remove(Length);

            // resize if neccessary
            if (DEFAULT_ARRAY_SIZE < Length && Length < _arraySize / 2)
            {
                _arraySize /= 2;
                entities.Resize(_arraySize);
                foreach (byte compID in compSet)
                    components[compID].Resize(_arraySize);
            }
        }

        /// <summary>
        /// moves the <see cref="Entity"/> in this <see cref="Archetype"/> to the given <paramref name="newArchetype"/>. 
        /// Copies <see cref="IComponent"/>s over. 
        /// Does not initialize any missing <see cref="IComponent"/>s. 
        /// </summary>
        public void MoveEntity(Entity entity, Archetype newArchetype)
        {
            if (entities[entity.poolIndex] != entity) throw new EntityNotFound();
            if (this == newArchetype) return; // cant move entity to archetype it already belongs to

            // increase size of new archetype if necessary
            if (newArchetype.Length == newArchetype._arraySize)
            {
                newArchetype._arraySize *= 2;
                newArchetype.entities.Resize(newArchetype._arraySize);
                foreach (byte compID in newArchetype.compSet)
                    newArchetype.components[compID].Resize(newArchetype._arraySize);
            }

             
            newArchetype.entities[newArchetype.Length] = entities[entity.poolIndex];// copy entities
            foreach (byte compID in compSet) // copy components
                if (newArchetype.compSet.Contains(compID))
                    newArchetype.components[compID][newArchetype.Length] = components[compID][entity.poolIndex];


            // remove component from 
            // move end of pool to overwrite layer
            if (entity.poolIndex != --Length)
            {
                // entity has been moved so index must be updated 
                entities[entity.poolIndex] = entities[Length];
                (entities[entity.poolIndex] as Entity).poolIndex = entity.poolIndex;
                foreach(byte compID in compSet)
                    components[compID][entity.poolIndex] = components[compID][Length];
            }


            // remove layer at end of pools
            entities.Remove(Length);
            foreach(byte compID in compSet)
                components[compID].Remove(Length);

            // dcrease size of this archetype if neccessary
            if (DEFAULT_ARRAY_SIZE < Length && Length < _arraySize / 2)
            {
                _arraySize /= 2;
                entities.Resize(_arraySize);
                foreach (byte compID in compSet)
                    components[compID].Resize(_arraySize);
            }

            entity.archetype = newArchetype;
            entity.poolIndex = newArchetype.Length;
            newArchetype.Length++;
        }
        #endregion

        #region Find Archetype
        /// <summary>
        /// Searches All <see cref="Archetype"/>s for matching <paramref name="ComponentIDs"/>.
        /// if none are found, creates a new <see cref="Archetype"/> matching the <paramref name="ComponentIDs"/>.
        /// </summary>
        public static Archetype FindOrCreate(ComponentSet compSet)
        {
            int index = All.BinarySearch(compSet);
            if (index >= 0) return All[index];

            Archetype newArchetype = new Archetype(compSet);
            All.Insert(~index, newArchetype);

            return newArchetype;
        }

        /// <summary>
        /// Finds the Archetypes that fulfill the <paramref name="query"/>.
        /// </summary>
        public static IEnumerable<Archetype> FindApplicable(Query query)
        {
            for (int i = 0; i < All.Count; i++)
                if (query.Check(All[i].compSet))
                    yield return All[i];
        }
        
        /// <summary>
        /// Finds the <see cref="Archetype"/> for this <see cref="Archetype"/>'s <see cref="ComponentSet"/>
        /// minus the <paramref name="compID"/>.
        /// </summary>
        internal Archetype FindNext(byte compID)
        {
            ComponentSet newCompSet = compSet.Add(compID);
            
            // look up
            if (_next[compID] == null)
                _next[compID] = FindOrCreate(newCompSet);
            return _next[compID];
        }

        /// <summary>
        /// Finds the <see cref="Archetype"/> for this <see cref="Archetype"/>'s <see cref="ComponentSet"/>
        /// minus the <paramref name="compID"/>.
        /// </summary>
        internal Archetype FindPrev(byte compID)
        {
            ComponentSet newCompSet = compSet.Remove(compID);
           
            // look up
            if (_prev[compID] == null)
                _prev[compID] = FindOrCreate(newCompSet);
            return _prev[compID];
        }
        #endregion

        public int CompareTo(ComponentSet other) => compSet.CompareTo(other);
        public override string ToString() => compSet.ToBinString();
    }
}