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

        public Context()
        {
            _archetypes = new List<Archetype>();
            _behaviours = new List<Behaviour>();
            EmptyArchetype = new Archetype(this, new ComponentSet(new byte[] { }));
        }

        /// <summary>
        /// Searches All <see cref="Archetype"/>s for matching <paramref name="compSet"/>.
        /// if none are found, creates a new <see cref="Archetype"/> matching the <paramref name="compSet"/>.
        /// </summary>
        internal Archetype FindOrCreateArchetype(ComponentSet compSet)
        {
            int index = _archetypes.BinarySearch(compSet);
            if (index >= 0) return _archetypes[index];

            Archetype newArchetype = new Archetype(this, compSet);
            _archetypes.Insert(~index, newArchetype);

            return newArchetype;
        }

        /// <summary>
        /// Finds the Archetypes that fulfill the <paramref name="query"/>.
        /// </summary>
        internal IEnumerable<Archetype> FindApplicableArchetypes(Query query)
        {
            for (int i = 0; i < _archetypes.Count; i++)
                if (query.Check(_archetypes[i].compSet))
                    yield return _archetypes[i];
        }

        /// <summary>
        /// adds a behaviour to this context.
        /// </summary>
        internal void AddBehaviour(Behaviour behaviour)
        {
            _behaviours.Add(behaviour);
        }

        /// <summary>
        /// Searches all <see cref="Behaviour"/>s for queries that <paramref name="compSet"/> applies to.
        /// </summary>
        internal IEnumerable<Behaviour> FindApplicableBehaviour(ComponentSet compSet)
        {
            for (int i = 0; i < _behaviours.Count; i++)
                if (_behaviours[i].query.Check(compSet))
                    yield return _behaviours[i];
        }
    }
}
