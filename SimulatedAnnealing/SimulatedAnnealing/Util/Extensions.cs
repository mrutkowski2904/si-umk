using SimulatedAnnealing.Scheduler;

namespace SimulatedAnnealing.Util
{
    public static class Extensions
    {
        public static List<(int, int)> CloneList(this List<(int, int)> list)
        {
            List<(int, int)> clone = new List<(int, int)>();
            foreach (var item in list)
            {
                clone.Add((item.Item1, item.Item2));
            }
            return clone;
        }

    }
}
