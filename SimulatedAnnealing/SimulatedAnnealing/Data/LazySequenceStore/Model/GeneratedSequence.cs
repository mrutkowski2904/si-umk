using SimulatedAnnealing.Scheduler;

namespace SimulatedAnnealing.Data.LazyStore
{
    internal class GeneratedSequence
    {
        public int X { get; private set; }
        public JobSequence Sequence { get; private set; }

        public GeneratedSequence(int x, JobSequence sequence)
        {
            X = x;
            Sequence = sequence;
        }
    }
}
