
namespace Protfi.Common.Reflection
{
    public static class CollectionExtensions
    {
        public static bool ContainsAll<T>(this IEnumerable<T> source, params T[] values)
        {
            return source.ContainsAll((IEnumerable<T>) values);
        }
        
        private static bool ContainsAll<T>(this IEnumerable<T> source, IEnumerable<T> values)
        {
            return !values.Except(source).Any();
        }
    }
}