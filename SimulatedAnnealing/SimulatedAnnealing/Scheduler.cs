using SimulatedAnnealing.Model;

namespace SimulatedAnnealing
{
    public class Scheduler
    {
        private readonly IList<Requirement> _requirements;

        public Scheduler(IList<Requirement> requirements)
        {
            _requirements = requirements;
        }

        public int RunJobs(JobSequence jobSequence)
        {
            // Kopia zadań do wykonania, żeby nie modyfikować oryginalnych danych
            JobSequence sequence = jobSequence.Clone();


            return -1;
        }

        private void DoUnitOfWork(JobSequence sequence)
        {
            // Sprawdzenie czy spełnione są wymagania dla tego zadania

        }
    }
}
