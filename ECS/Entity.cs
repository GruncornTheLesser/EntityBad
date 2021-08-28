using System;

namespace ECS
{
    /// <summary>
    /// A container object for which <see cref="IComponent"/>s can be added and removed.
    /// <see cref="Entity"/> stores methods to access <see cref="IComponent"/>s but doesn't
    /// store the <see cref="IComponent"/> themselves
    /// </summary>
    public class Entity : IPoolable
    {
        internal Archetype archetype;   // the archetype it belongs to
        internal int poolIndex;         // the index in the archetype of itself and all its components
        
        /// <summary>
        /// initiates <see cref="Entity"/> with <paramref name="Components"/>.
        /// Use <see cref="ComponentManager.ID{T1, T2, T3, T4}"/>.
        /// </summary>
        public Entity(byte[] Components)
        {
            // if Archetype null adds to empty archetype in context
            this.archetype = Archetype.FindOrCreate(new ComponentSet(Components));
            this.archetype.InitEntity(this, out poolIndex); // initiates entity into archetype
        }
        /// <summary>
        /// initiates empty <see cref="Entity"/>.
        /// </summary>
        public Entity()
        {
            this.archetype = Archetype.Empty;
            this.archetype.InitEntity(this, out poolIndex); // initiates entity into archetype
        }
       
        /// <summary>
        /// Adds a new <typeparamref name="TComponent"/> to <see cref="Entity"/>.
        /// Calls <see cref="Archetype.MoveEntity(Entity, Archetype)"/> and sets <typeparamref name="TComponent"/>.
        /// </summary>
        public void AddComponent<TComponent>(TComponent Component = default) where TComponent : IComponent, new()
        {
            byte compID = ComponentManager.ID<TComponent>();
            archetype.MoveEntity(this, archetype.FindNext(compID));
            archetype.components[compID][poolIndex] = Component;
        }

        /// <summary>
        /// Removes <typeparamref name="TComponent"/> from <see cref="Entity"/>.
        /// Calls <see cref="Archetype.MoveEntity(Entity, Archetype)"/> and gets <typeparamref name="TComponent"/>.
        /// </summary>
        /// <returns>Removed <typeparamref name="TComponent"/></returns>
        public TComponent RemoveComponent<TComponent>() where TComponent : IComponent, new()
        {
            byte compID = ComponentManager.ID<TComponent>();
            TComponent Component = (TComponent)archetype.components[compID][poolIndex];
            archetype.MoveEntity(this, archetype.FindPrev(compID));
            return Component;
        }



        public void ApplyComponents(ComponentSet Set)
        {
            archetype.MoveEntity(this, Archetype.FindOrCreate(Set));
        }


        /// <summary>
        /// returns true if <see cref="Entity"/> contains <typeparamref name="TComponent"/>.
        /// </summary>
        /// <typeparam name="TComponent"></typeparam>
        /// <returns></returns>
        public bool HasComponent<TComponent>() where TComponent : IComponent, new()
        {
            return archetype.compSet.Contains(ComponentManager.ID<TComponent>());
        }

        /// <summary>
        /// return <typeparamref name="TComponent"/> by reference. Use in property.
        /// </summary>
        public ref TComponent GetComponent<TComponent>() where TComponent : IComponent, new()
        {
            return ref (archetype.components[ComponentManager.ID<TComponent>()] as Archetype.Pool<TComponent>)[poolIndex];
        }
    }
}