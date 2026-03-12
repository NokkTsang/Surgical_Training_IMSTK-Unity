namespace Imstk
{
    /// <summary>
    /// Extensions to ImstkClasses
    /// </summary>
    public static class UtilityExtensions
    {
        /// <summary>
        /// Should mirror the dynamic_pointer_cast and while the test works
        /// this might not be a good idea, use with caution. 
        /// </summary>
        /// This tries to solve the issue that our custom cast `Utils.CastTo<>()`
        /// ALWAYS returns an object, either the cast succeeds or if it fails
        /// a default constructred object is returned. But there might be instances
        /// where this also doesn't work correctly
        public static T SafeCastTo<T>(object from) where T : class
        {
            if (from != null && typeof(T).IsAssignableFrom(from.GetType()))
            {
                return Utils.CastTo<T>(from);
            }
            else
            {
                return null;
            }
        }
    }
}