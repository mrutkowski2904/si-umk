using SimulatedAnnealing.Model;

namespace SimulatedAnnealing
{
    public class Scheduler
    {
        private readonly int _maxCost = 2000;
        private readonly IList<Requirement> _requirements;

        public Scheduler(IList<Requirement> requirements)
        {
            _requirements = requirements;
        }

        public int RunJobs(JobSequence jobSequence)
        {
            // Kopia zadań do wykonania, żeby nie modyfikować oryginalnych danych
            JobSequence sequence = jobSequence.Clone();
            List<Job> finishedJobs = new List<Job>();

            int sequenceCost = 0;

            while (sequence.P1Jobs.Count > 0 || sequence.P2Jobs.Count > 0)
            {
                DoUnitOfWork(ref sequence, ref finishedJobs);
                sequenceCost++;

                // Wykrywanie zakleszczeń
                if (sequenceCost > _maxCost)
                {
                    HandleDeadlock();
                }
            }


            return sequenceCost;
        }

        /*
         * Funkcja "przydzielająca" jednostkę czasu procesora zadaniom,
         * jeżeli nie są spełnione wymagania, zadanie aktywnie oczekuje.
         */
        private void DoUnitOfWork(ref JobSequence pendingSequence, ref List<Job> finishedJobs)
        {
            Job? p1Job = null;
            Job? p2Job = null;

            // Procesor 1
            if (pendingSequence.P1Jobs.Count > 0)
            {
                p1Job = pendingSequence.P1Jobs[0];
                if (AreRequirementsMet(p1Job, finishedJobs))
                {
                    p1Job.Cost--;
                }
            }

            // Procesor 2
            if (pendingSequence.P2Jobs.Count > 0)
            {
                p2Job = pendingSequence.P2Jobs[0];
                if (AreRequirementsMet(p2Job, finishedJobs))
                {
                    p2Job.Cost--;
                }
            }

            // Zadanie ukończone -> przeniesienie do ukończonych
            if (p1Job != null && p1Job.Cost == 0)
            {
                pendingSequence.P1Jobs.Remove(p1Job);
                finishedJobs.Add(p1Job);
            }
            if (p2Job != null && p2Job.Cost == 0)
            {
                pendingSequence.P2Jobs.Remove(p2Job);
                finishedJobs.Add(p2Job);
            }
        }

        private bool AreRequirementsMet(Job pendingJob, List<Job> finishedJobs)
        {
            int[] requiredIds = _requirements.Where(r => r.JobId == pendingJob.Id).Select(r => r.RequiresId).ToArray();

            for (int i = 0; i < requiredIds.Length; i++)
            {
                // Jeżeli lista zadań ukończonych, nie zawiera zadania o wymaganym id, wymagania nie są spełnione
                if (!finishedJobs.Any(j => j.Id == requiredIds[i]))
                {
                    return false;
                }
            }

            return true;
        }

        private void HandleDeadlock()
        {
            Console.WriteLine("TODO: Zakleszczenie");
            while (true)
            {
            }
        }
    }
}
