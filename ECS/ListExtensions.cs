using System;
using System.Collections;
using System.Collections.Generic;
namespace ECS
{
    public static partial class ListExtensions
    {
        /// <summary>
        /// Binary search to find an index which equals <paramref name="value"/>.
        /// </summary>
        /// <returns>
        /// An index of the <paramref name="value"/> in the <paramref name="list"/>. 
        /// If an index is not found, returns a bitwise complement index into which the value 
        /// should be inserted.
        /// </returns>
        internal static int BinarySearch<T1, T2>(this List<T1> list, T2 value) where T1 : IComparable<T2>
        {
            int index = 0;
            int lower = 0;
            int upper = list.Count - 1;

            while (lower <= upper)
            {
                index = (upper + lower) / 2;

                int diff = list[index].CompareTo(value);
                if (diff > 0) upper = index - 1;
                else if (diff < 0) lower = index + 1;
                else return index;
            }
            if (index == upper) index += 1;
            return ~index;
        }
        
        /// <summary>
        /// Binary search to find the first index which equals <paramref name="value"/>.
        /// </summary>
        /// <returns>
        /// The first index of the <paramref name="value"/> in the <paramref name="list"/>. 
        /// If an index is not found, returns a bitwise complement index into which the value 
        /// should be inserted.
        /// </returns>
        internal static int BinaryFirst<T1, T2>(this List<T1> list, T2 value) where T1 : IComparable<T2>
        {
            int result = -1;
            int index = 0;
            int lower = 0;
            int upper = list.Count - 1;

            while (lower <= upper)
            {
                index = (upper + lower) / 2;

                int diff = list[index].CompareTo(value);
                if (diff > 0) upper = index - 1;
                else if (diff < 0) lower = index + 1;
                else
                {
                    result = index;
                    upper = index - 1;
                }
            }
            if (result == -1)
            {
                if (index == upper) index += 1;
                return ~index;

            }
            return result;
           
        }
        
        /// <summary>
        /// Binary search to find the last index which equals <paramref name="value"/>.
        /// </summary>
        /// <returns>
        /// The first index of the <paramref name="value"/> in the <paramref name="list"/>. 
        /// If an index is not found, returns a bitwise complement index into which the value 
        /// should be inserted.
        /// </returns>
        internal static int BinaryLast<T1, T2>(this List<T1> list, T2 value) where T1 : IComparable<T2>
        {
            int result = -1;
            int index = 0;
            int lower = 0;
            int upper = list.Count - 1;

            while (lower <= upper)
            {
                index = (upper + lower) / 2;

                int diff = list[index].CompareTo(value);
                if (diff > 0) upper = index - 1;
                else if (diff < 0) lower = index + 1;
                else
                {
                    result = index;
                    lower = index + 1;
                }
            }
            if (result == -1)
            {
                if (index == upper) index += 1;
                return ~index;
            }
            return result;
        }
    }
}