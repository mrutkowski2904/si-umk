using SimulatedAnnealing.Data;
using SimulatedAnnealing.Data.LazyStore;
using SimulatedAnnealing.Scheduler;

namespace SimulatedAnnealing.SimulatedAnnealing.Data.LazyStore
{
    public class LazySequenceStore : ISequenceStore
    {
        private readonly List<GeneratedSequence> _generatedSequences = new List<GeneratedSequence>();
        private readonly List<Requirement> _requirements;

        public LazySequenceStore(List<Requirement> requirements)
        {
            _requirements = requirements;
        }

        /*
         * Funkcja generuje prawidłową sekwencję zadań podczas odczytu,
         * jeżeli wcześniej doszło do wygenerowania sekwencji dla podanego indeksu, 
         * zwraca wcześniej wygenerowaną wartość
         */
        public JobSequence GetSequence(int index)
        {
            var sequence = _generatedSequences.FirstOrDefault(s => s.X == index);
            if (sequence != null)
            {
                return sequence.Sequence;
            }
            return new JobSequence(new List<Job>(), new List<Job>());
        }
    }
}
