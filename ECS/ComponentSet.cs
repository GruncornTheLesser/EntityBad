using System;
using System.Collections;
using System.Collections.Generic;

namespace ECS
{
    public sealed class Query
    {
        private readonly ulong[] bitsAll;
        private readonly ulong[] bitsAny;
        private readonly ulong[] bitsNon;
        private bool doAnySearch;

        public Query(byte[] all, byte[] any, byte[] none)
        {
            bitsAll = new ulong[4];
            bitsAny = new ulong[4];
            bitsNon = new ulong[4];

            foreach (byte allID in all)
                bitsAll[allID / 64] |= (1ul << (allID % 64));

            foreach (byte anyID in any)
                bitsAny[anyID / 64] |= (1ul << (anyID % 64));

            foreach (byte nonID in none)
                bitsNon[nonID / 64] |= (1ul << (nonID % 64));

            doAnySearch = any.Length > 0;
        }

        /// <summary>
        /// returns true if <paramref name="compSet"/> would pass query check.
        /// </summary>
        public bool Check(ComponentSet compSet)
        {
            bool All =  ((bitsAll[0] & compSet.bits[0]) == bitsAll[0]) && // All, contain all
                        ((bitsAll[1] & compSet.bits[1]) == bitsAll[1]) &&
                        ((bitsAll[2] & compSet.bits[2]) == bitsAll[2]) &&
                        ((bitsAll[3] & compSet.bits[3]) == bitsAll[3]);

            bool Any =  !doAnySearch || (
                        ((bitsAny[0] & compSet.bits[0]) > 0) && // Any, overlap
                        ((bitsAny[1] & compSet.bits[1]) > 0) &&
                        ((bitsAny[2] & compSet.bits[2]) > 0) &&
                        ((bitsAny[3] & compSet.bits[3]) > 0));

            bool None = ((bitsNon[0] & compSet.bits[0]) == 0) && // None, no overlap
                        ((bitsNon[1] & compSet.bits[1]) == 0) &&
                        ((bitsNon[2] & compSet.bits[2]) == 0) &&
                        ((bitsNon[3] & compSet.bits[3]) == 0);

            return All && Any && None;
        }
    }

    /// <summary>
    /// A 256 bit number that represents a unique of set of <see cref="IComponent"/>s. 
    /// </summary>
    /// <remarks>
    /// ComponentSet also stores an array of bytes which have been added to this Set.
    /// This Array is not regulated, so repeat values may exist and affect performance.
    ///<remarks/>
    public struct ComponentSet : IEnumerable<byte>
    {
        
        internal readonly ulong[] bits; // 256 bits = 32 bytes
        internal readonly byte[] compIDs; // 8 * 255 bits = 255bytes

        public int Count => compIDs.Length;

        /// <summary>
        /// constructs from byte IDs. Use <see cref="ComponentManager.ID{T1, T2, T3, T4}"/>.
        /// </summary>
        /// <param name="IDs"></param>
        internal ComponentSet(byte[] IDs)
        {
            compIDs = IDs;
            bits = new ulong[4]; // doesnt need to be sorted
            for (byte j = 0; j < IDs.Length; j++)
                bits[IDs[j] / 64] |= (1ul << (IDs[j] % 64)); // ORs each bit
        }
        // private constructor for Adding and removing
        private ComponentSet(ulong[] bits, byte[] compIDs)
        {
            this.bits = bits;
            this.compIDs = compIDs;
        }

        public int CompareTo(ComponentSet other)
        {
            // long is little endian
            if (this.bits[3] != other.bits[3]) return this.bits[3].CompareTo(other.bits[3]);
            if (this.bits[2] != other.bits[2]) return this.bits[2].CompareTo(other.bits[2]);
            if (this.bits[1] != other.bits[1]) return this.bits[1].CompareTo(other.bits[1]);
            return this.bits[0].CompareTo(other.bits[0]); // lowest 
        }
        
        /// <summary>
        /// Checks if this set contains <paramref name="Component_ID"/>.
        /// </summary>
        public bool Contains(byte CompID) 
        {
            return (bits[CompID / 64] & (1ul << (CompID % 64))) > 0;
        }
        
        /// <summary>
        /// adds a single <see cref="IComponent"/> ID and returns the new <see cref="ComponentSet"/>.
        /// </summary>
        internal ComponentSet Add(byte newComp)
        {
            ulong[] newBits = new ulong[4];
            bits.CopyTo(newBits, 0);
            (newBits[newComp / 64]) |= (1ul << (newComp % 64));

            byte[] newCompIDs = new byte[compIDs.Length + 1];
            compIDs.CopyTo(newCompIDs, 0);
            newCompIDs[compIDs.Length] = newComp;

            return new ComponentSet(newBits, newCompIDs);
        }

        /// <summary>
        /// removes a single <see cref="IComponent"/> ID and returns the new <see cref="ComponentSet"/>.
        /// </summary>
        internal ComponentSet Remove(byte oldComp)
        {
            ulong[] newBits = new ulong[4];
            bits.CopyTo(newBits, 0);
            (newBits[oldComp / 64]) &= ~(1ul << (oldComp % 64));

            int i = 0;
            byte[] newCompIDs = new byte[compIDs.Length - 1];
            foreach (byte compID in compIDs)
                if (compID != oldComp)
                    newCompIDs[i++] = compID;

            return new ComponentSet(newBits, newCompIDs);
        }       
        
        public IEnumerator<byte> GetEnumerator() => ((IEnumerable<byte>)compIDs).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => compIDs.GetEnumerator();

        public string ToHexString()
        {
            string str = bits[0].ToString().PadRight(3, ' ') + " : ";

            for (int i = 3; i >= 0; i--)
            {
                byte[] bytes = BitConverter.GetBytes(bits[i]); // little end is stored first
                for (int j = 7; j >= 0; j--)
                    str += Convert.ToString(bytes[j], 16).PadLeft(2, '0');
                

            }
            return str;
        }

        public string ToBinString()
        {
            string str = bits[0].ToString().PadRight(3, ' ') + " : ";
            str += Convert.ToString(BitConverter.GetBytes(bits[0])[0], 2).PadLeft(8, '0');

            return str;
        }
    }
}