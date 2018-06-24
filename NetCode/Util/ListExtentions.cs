using System;
using System.Collections.Generic;
using System.Linq;

namespace NetCode.Util
{
    public static class ListExtentions
    {
        /// <summary>
        /// Breaks the list down into smaller arrays
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list">The list to be segmented</param>
        /// <param name="segmentSize">The maximum number of items in each segment</param>
        /// <returns></returns>
        public static List<T[]> Segment<T>(this List<T> list, int segmentSize)
        {
            List<T[]> segments = new List<T[]>();
            int index = 0;

            int fullSegments = list.Count / segmentSize;
            for (int i = 0; i < fullSegments; i++)
            {
                T[] segment = new T[segmentSize];
                list.CopyTo(index, segment, 0, segmentSize);
                segments.Add(segment);
                index += segmentSize;
            }

            int remaining = list.Count - index;
            if (remaining > 0)
            {
                T[] segment = new T[remaining];
                list.CopyTo(index, segment, 0, remaining);
                segments.Add(segment);
            }

            return segments;
        }
    }
}
