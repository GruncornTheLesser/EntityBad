using System;
using System.Collections;
using System.Collections.Generic;
namespace ECS
{
    /// <summary>
    /// A group of <see cref="Archetype"/>s containing a filtered selection of components.
    /// Used to perform logic over a filtered selection of <see cref="Entity"/>.
    /// </summary>
    public abstract class Behaviour
    {
        internal readonly Query query;
        public List<Archetype> archetypes { get; internal set; } = new List<Archetype>();

        protected Behaviour(Context context, byte[] allFilter, byte[] anyFilter, byte[] noneFilter)
        {
            this.query = new Query(allFilter, anyFilter, noneFilter);

            context.AddBehaviour(this);
            foreach (Archetype A in context.FindApplicableArchetypes(query))
                archetypes.Add(A);
        }

        public override string ToString()
        {
            return $"{archetypes.Count}";
        }
    }
}
