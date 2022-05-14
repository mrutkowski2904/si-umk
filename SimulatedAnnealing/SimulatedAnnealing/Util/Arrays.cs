namespace SimulatedAnnealing.Util
{
    public static class Arrays
    {
        public static bool Identical(int[] a, int[] b)
        {
            if(a.Length != b.Length)
            {
                return false;
            }

            bool[] diffMap = new bool[a.Length];

            for(int i = 0; i < a.Length; i++)
            {
                diffMap[i] = (a[i] == b[i]);
            }
            
            /*
             * Jeżeli istnieje chociaż jedna różnica,
             * tablice nie są identyczne
             */
            return !diffMap.Any(e => e == false);
        }
    }
}
