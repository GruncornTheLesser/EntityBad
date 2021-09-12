using System;

namespace ECS
{
    /// <summary>
    /// A container object for which <see cref="IComponent"/>s can be added and removed.
    /// <see cref="Entity"/> stores methods to access <see cref="IComponent"/>s but doesn't
    /// store the <see cref="IComponent"/> themselves
    /// </summary>
    public partial class Entity : IPoolable
    {
        internal Archetype archetype;   // the archetype it belongs to
        internal int poolIndex;         // the index in the archetype of itself and all its components
        
        /// <summary>
        /// initiates <see cref="Entity"/> with <paramref name="Components"/>.
        /// Use <see cref="ComponentManager.ComponentID{T1, T2, T3, T4}"/>.
        /// </summary>
        public Entity(Context context, params byte[] Components)
        {
            // if Archetype null adds to empty archetype in context
            this.archetype = context.FindOrCreateArchetype(new CompSet(Components));
            this.archetype.InitEntity(this, out poolIndex); // initiates entity into archetype
        }
        
        /// <summary>
        /// initiates empty <see cref="Entity"/>.
        /// </summary>
        public Entity(Context context)
        {
            this.archetype = context.EmptyArchetype;
            this.archetype.InitEntity(this, out poolIndex); // initiates entity into archetype
        }

        /// <summary>
        /// Adds a new <typeparamref name="TComponent"/> to <see cref="Entity"/>.
        /// Calls <see cref="Archetype.MoveEntity(Entity, Archetype)"/> and sets <typeparamref name="TComponent"/>.
        /// </summary>
        protected void AddComponent<TComponent>(TComponent Component = default) where TComponent : IComponent, new()
        {
            byte compID = archetype.context.ComponentID<TComponent>();
            archetype.MoveEntity(this, archetype.FindNext(compID));
            archetype.components[compID][poolIndex] = Component;
        }

        /// <summary>
        /// Removes <typeparamref name="TComponent"/> from <see cref="Entity"/>.
        /// Calls <see cref="Archetype.MoveEntity(Entity, Archetype)"/> and gets <typeparamref name="TComponent"/>.
        /// </summary>
        /// <returns>Removed <typeparamref name="TComponent"/></returns>
        protected TComponent RemoveComponent<TComponent>() where TComponent : IComponent, new()
        {
            byte compID = archetype.context.ComponentID<TComponent>();
            TComponent Component = (TComponent)archetype.components[compID][poolIndex];
            archetype.MoveEntity(this, archetype.FindPrev(compID));
            return Component;
        }

        /// <summary>
        /// sets <see cref="IComponent"/>s to new <see cref="ComponentSet"/>. <see cref="IComponent"/>s 
        /// previously assigned, if still present, are copied over.
        /// </summary>
        protected void AssignComponents(CompSet Set)
        {
            archetype.MoveEntity(this, archetype.context.FindOrCreateArchetype(Set));
        }

        /// <summary>
        /// returns true if <see cref="Entity"/> contains <typeparamref name="TComponent"/>.
        /// </summary>
        /// <typeparam name="TComponent"></typeparam>
        /// <returns></returns>
        public bool HasComponent<TComponent>() where TComponent : IComponent, new()
        {
            return archetype.compSet.Contains(archetype.context.ComponentID<TComponent>());
        }

        /// <summary>
        /// return <typeparamref name="TComponent"/> by reference. Use in property.
        /// </summary>
        public ref TComponent GetComponent<TComponent>() where TComponent : IComponent, new()
        {
            return ref (archetype.components[archetype.context.ComponentID<TComponent>()] as Pool<TComponent>)[poolIndex];
        }
    }
}