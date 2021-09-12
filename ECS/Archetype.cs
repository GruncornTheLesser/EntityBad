using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace ECS
{
    /// <summary>
    /// A collection of <see cref="Entity"/> which all share the same types of <see cref="IComponent"/>.
    /// </summary>
    public class Archetype : IComparable<CompSet>                               // Archetype.All sorted by ComponentSet
    {
        internal readonly Context context;
        internal readonly CompSet compSet;                                      // a bit array represent which components this Archetype contains
        internal readonly byte[] compIDs;

        internal readonly IPool entities = new Pool<Entity>();                  // the storage for entities     
        internal readonly IPool[] components = new IPool[byte.MaxValue + 1];    // the storage for components

        private readonly Archetype[] _next = new Archetype[byte.MaxValue + 1];  // a graph to find the next archetype
        private readonly Archetype[] _prev = new Archetype[byte.MaxValue + 1];  // a graph to find the prev archetype

        /// <summary>
        /// The number of <see cref="Entity"/>s stored in this <see cref="Archetype"/>.
        /// </summary>
        public int Length { get; private set; }
        
        internal const int DEFAULT_ARRAY_SIZE = 128;
        private int _arraySize = DEFAULT_ARRAY_SIZE;                // the max number of entities before pools require resizing

        internal Archetype(Context Context, CompSet CompSet)
        {
            this.context = Context;
            this.compSet = CompSet;
            this.compIDs = CompSet.ExtractIDs();

            foreach (byte CompID in compIDs)
                components[CompID] = context.InitPool(CompID);      // init component pools
            entities = new Pool<Entity>();                          // init entity pool

            foreach (Behaviour B in context.FindApplicableBehaviour(compSet))
                B.archetypes.Add(this);
        }

        #region GetPools
        /// <summary>
        /// gets the component pool.
        /// </summary>
        public Pool<TComponent> GetPool<TComponent>() where TComponent : IComponent, new()
        {
            return (Pool<TComponent>)components[context.ComponentID<TComponent>()];
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
                foreach (byte compID in compIDs)
                    components[compID].Resize(_arraySize);
            }
            
            entities[Length] = entity;
            foreach (byte compID in compIDs)
                components[compID][Length] = context.InitComponent(compID); // reset all components
            poolIndex = Length++;
        }
        
        /// <summary>
        /// Removes the <see cref="Entity"/> into this Archetype
        /// </summary>
        internal void RemoveEntity(int index)
        {
            // move end of pool to overwrite layer
            if (index != --Length)
                foreach(byte compID in compIDs)
                    components[compID][index] = components[compID][Length];
            // entity has been moved so index must be updated 
            (entities[index] as Entity).poolIndex = index;


            // remove layer at end of pools
            foreach(byte compID in compIDs)
                components[compID].Remove(Length);

            // resize if neccessary
            if (DEFAULT_ARRAY_SIZE < Length && Length < _arraySize / 2)
            {
                _arraySize /= 2;
                entities.Resize(_arraySize);
                foreach (byte compID in compIDs)
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
                foreach (byte compID in newArchetype.compIDs)
                    newArchetype.components[compID].Resize(newArchetype._arraySize);
            }

             
            newArchetype.entities[newArchetype.Length] = entities[entity.poolIndex];// copy entities
            foreach (byte compID in compIDs) // copy components
                if (newArchetype.compSet.Contains(compID))
                    newArchetype.components[compID][newArchetype.Length] = components[compID][entity.poolIndex];


            // remove component from 
            // move end of pool to overwrite layer
            if (entity.poolIndex != --Length)
            {
                // entity has been moved so index must be updated 
                entities[entity.poolIndex] = entities[Length];
                (entities[entity.poolIndex] as Entity).poolIndex = entity.poolIndex;
                foreach(byte compID in compIDs)
                    components[compID][entity.poolIndex] = components[compID][Length];
            }


            // remove layer at end of pools
            entities.Remove(Length);
            foreach(byte compID in compIDs)
                components[compID].Remove(Length);

            // dcrease size of this archetype if neccessary
            if (DEFAULT_ARRAY_SIZE < Length && Length < _arraySize / 2)
            {
                _arraySize /= 2;
                entities.Resize(_arraySize);
                foreach (byte compID in compIDs)
                    components[compID].Resize(_arraySize);
            }

            entity.archetype = newArchetype;
            entity.poolIndex = newArchetype.Length;
            newArchetype.Length++;
        }
        #endregion

        #region Find Archetype
        /// <summary>
        /// Finds the <see cref="Archetype"/> for this <see cref="Archetype"/>'s <see cref="ComponentSet"/>
        /// minus the <paramref name="compID"/>.
        /// </summary>
        internal Archetype FindNext(byte compID)
        {
            CompSet newCompSet = compSet.Add(compID);
            
            // look up
            if (_next[compID] == null)
                _next[compID] = context.FindOrCreateArchetype(newCompSet);
            return _next[compID];
        }

        /// <summary>
        /// Finds the <see cref="Archetype"/> for this <see cref="Archetype"/>'s <see cref="CompSet"/>
        /// minus the <paramref name="compID"/>.
        /// </summary>
        internal Archetype FindPrev(byte compID)
        {
            CompSet newCompSet = compSet.Remove(compID);
           
            // look up
            if (_prev[compID] == null)
                _prev[compID] = context.FindOrCreateArchetype(newCompSet);
            return _prev[compID];
        }
        #endregion

        public int CompareTo(CompSet other) => compSet.CompareTo(other);
    }
}