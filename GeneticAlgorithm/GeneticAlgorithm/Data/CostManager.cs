namespace GA_SI.Data
{
    static class CostManager
    {
        // koszt 0 - zadanie nie może się wykonywać na tym procesorze
        private static readonly int[] p1Costs = { 2, 5, 7, 6, 7, 0, 6 };
        private static readonly int[] p2Costs = { 5, 1, 3, 4, 8, 7, 3 };

        public static int JobCount { get { return p1Costs.Length; } }

        static CostManager()
        {
            if(p1Costs.Length != p2Costs.Length)
            {
                throw new ApplicationException("Incorret job cost arrays, sizes don't match");
            }
        }

        public static int GetCost(int jobId, int processorId)
        {
            int minJobId = 1;
            int maxJobId = p1Costs.Length;

            if (jobId < minJobId || jobId > maxJobId)
            {
                throw new ArgumentException($"Incorrect jobId: {jobId}, correct value range: min: {minJobId} | max: {maxJobId}");
            }

            if (processorId == 1)
            {
                return p1Costs[jobId - 1];
            }
            else if (processorId == 2)
            {
                return p2Costs[jobId - 1];
            }
            else
            {
                throw new ArgumentException($"Incorrect processorId: {processorId}, correct values: 1, 2");
            }
        }
    }
}
