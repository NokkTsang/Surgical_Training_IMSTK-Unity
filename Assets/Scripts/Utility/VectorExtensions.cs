namespace Imstk
{
    public static class VectorExtensions
    {
        public static double SqrNorm(this Vec3d v)
        {
            return v[0] * v[0] + v[1] * v[1] + v[2] * v[2];
        }
    }
}