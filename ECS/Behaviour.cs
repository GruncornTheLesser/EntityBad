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
        private readonly static List<Behaviour> All;
        static Behaviour()
        {
            All = new List<Behaviour>();
        }

        private readonly Query query;
        public List<Archetype> archetypes { get; internal set; } = new List<Archetype>();

        protected Behaviour(byte[] allFilter, byte[] anyFilter, byte[] noneFilter)
        {
            this.query = new Query(allFilter, anyFilter, noneFilter);
            All.Add(this);

            foreach (Archetype A in Archetype.FindApplicable(query))
                archetypes.Add(A);
        }

        /// <summary>
        /// Searches all <see cref="Behaviour"/>s for queries that <paramref name="compSet"/> applies to.
        /// </summary>
        internal static IEnumerable<Behaviour> FindApplicable(ComponentSet compSet)
        {
            for (int i = 0; i < All.Count; i++)
                if (All[i].query.Check(compSet))
                    yield return All[i];
        }

        public override string ToString()
        {
            return $"{archetypes.Count}";
        }
    }
}
