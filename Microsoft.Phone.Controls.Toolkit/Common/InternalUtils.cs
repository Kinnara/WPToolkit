namespace Microsoft.Phone.Controls
{
    internal static class InternalUtils
    {
        internal static bool AreValuesEqual(object o1, object o2)
        {
            if (o1 == o2)
            {
                return true;
            }

            if (o1 == null || o2 == null)
            {
                return false;
            }

            if (o1.GetType().IsValueType || o1.GetType() == typeof(string))
            {
                return object.Equals(o1, o2);
            }

            return object.ReferenceEquals(o1, o2);
        }
    }
}
