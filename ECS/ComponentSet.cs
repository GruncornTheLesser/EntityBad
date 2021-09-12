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


    public struct CompSet
    {
        internal readonly uint[] bits;

        public CompSet(byte[] components) 
        {
            int max = 0;
            foreach (byte ID in components)
                if (ID > max) max = ID;
            bits = new uint[(max / 32) + 1];
            foreach (byte ID in components)
                bits[ID / 32] |= 1u << (ID % 32);
        }
        // private constructor directly from _bits data
        private CompSet(uint[] data)
        {
            bits = data;
        }
        
        internal CompSet Add(byte ID) 
        {
            int Length = (ID / 32) + 1 > bits.Length ? (ID / 32) + 1 : bits.Length;
            uint[] newbits = new uint[Length];

            bits.CopyTo(newbits, Length - bits.Length);
            newbits[ID / 32] |= 1u << (ID % 32);

            return new CompSet(newbits);
        }
        internal CompSet Remove(byte ID) 
        {
            if (ID / 32 == bits.Length - 1 && bits[ID / 32] == (1u << ID))
            {
                uint[] newbits = new uint[bits.Length - 1];
                bits.CopyTo(newbits, -1);
                return new CompSet(newbits);
            }
            else
            {
                uint[] newbits = new uint[bits.Length];
                newbits[ID / 32] &= ~(1u << (ID % 32));
                return new CompSet(newbits);
            }

        }
        internal byte[] ExtractIDs()
        {
            List<byte> IDs = new List<byte>();
            for (int i = 0; i < bits.Length; i++)
            {
                if (bits[i] == 0) continue;
                for (byte id = 0; id < 32; id++)
                    if ((bits[i] & (1u << id)) > 0)
                        IDs.Add((byte)(id + (32 * i)));
            }
            return IDs.ToArray();
        }
        internal int CompareTo(CompSet other) 
        {   
            // word concatenated comparison
            if (this.bits.Length > other.bits.Length) return 1;
            if (this.bits.Length < other.bits.Length) return -1;

            for (int i = 0; i < bits.Length; i++)
            {
                int comparison = this.bits[i].CompareTo(other.bits[i]);
                if (comparison != 0) return comparison;
            }
            return 0;
        }
        
        public bool Contains(byte ID)
        {
            if (ID / 32 > bits.Length) return false;
            else return (bits[ID / 32] & (1 << (ID % 32))) > 0;
        }

        
    }
    public sealed class Query
    {
        private readonly ulong[] bitsAll; // must have every component in all
        private readonly ulong[] bitsNon; // must have none of the components in none
        private bool doAnySearch;

        public Query(byte[] all, byte[] any, byte[] none)
        {
            bitsAll = new ulong[4];
            bitsNon = new ulong[4];

            foreach (byte allID in all)
                bitsAll[allID / 64] |= (1ul << (allID % 64));

            foreach (byte nonID in none)
                bitsNon[nonID / 64] |= (1ul << (nonID % 64));

            foreach (byte anyID in any) // any encode with both all and none
            {
                bitsAll[anyID / 64] |= (1ul << (anyID % 64));
                bitsNon[anyID / 64] |= (1ul << (anyID % 64));
            }

            doAnySearch = any.Length > 0;
        }

        /// <summary>
        /// returns true if <paramref name="compSet"/> would pass query check.
        /// </summary>
        public bool Check(CompSet compSet)
        {
            bool All = ((bitsAll[0] & compSet.bits[0]) == bitsAll[0]) && // All, contain all
                        ((bitsAll[1] & compSet.bits[1]) == bitsAll[1]) &&
                        ((bitsAll[2] & compSet.bits[2]) == bitsAll[2]) &&
                        ((bitsAll[3] & compSet.bits[3]) == bitsAll[3]);

            bool Any = !doAnySearch ||
                        ((bitsAll[0] & bitsNon[0] & compSet.bits[0]) > 0) || // Any, overlap
                        ((bitsAll[1] & bitsNon[1] & compSet.bits[1]) > 0) ||
                        ((bitsAll[2] & bitsNon[2] & compSet.bits[2]) > 0) ||
                        ((bitsAll[3] & bitsNon[3] & compSet.bits[3]) > 0);

            bool None = ((bitsNon[0] & compSet.bits[0]) == 0) && // None, no overlap
                        ((bitsNon[1] & compSet.bits[1]) == 0) &&
                        ((bitsNon[2] & compSet.bits[2]) == 0) &&
                        ((bitsNon[3] & compSet.bits[3]) == 0);

            return All && Any && None;
        }

    }


}