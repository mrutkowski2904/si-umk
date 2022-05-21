namespace SimulatedAnnealing.Scheduler
{
    public class JobScheduler
    {
        private readonly int _maxCost = 2000;
        private readonly IList<Requirement> _requirements;
        private readonly bool _draw;

        public JobScheduler(IList<Requirement> requirements, bool draw = false)
        {
            _requirements = requirements;
            _draw = draw;
        }

        public int RunJobs(JobSequence jobSequence)
        {
            // Kopia zadań do wykonania, żeby nie modyfikować oryginalnych danych
            JobSequence sequence = jobSequence.Clone();
            List<Job> finishedJobs = new List<Job>();

            int sequenceCost = 0;

            if (_draw)
            {
                Console.WriteLine("\nTIM  P1   P2");
            }

            while (sequence.P1Jobs.Count > 0 || sequence.P2Jobs.Count > 0)
            {
                sequenceCost++;
                if (_draw)
                {
                    Console.Write("{0,-5}", sequenceCost);
                }
                DoUnitOfWork(ref sequence, ref finishedJobs);


                // Wykrywanie zakleszczeń
                if (sequenceCost > _maxCost)
                {
                    HandleDeadlock();
                }
            }
            if (_draw)
            {
                Console.WriteLine();
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
                    if (_draw)
                    {
                        Console.Write("{0,-5}", p1Job.Id);
                    }
                }
                else
                {
                    if (_draw)
                    {
                        Console.Write("{0,-5}", "W");
                    }
                }
            }

            // Procesor 2
            if (pendingSequence.P2Jobs.Count > 0)
            {
                p2Job = pendingSequence.P2Jobs[0];
                if (AreRequirementsMet(p2Job, finishedJobs))
                {
                    p2Job.Cost--;
                    if (_draw)
                    {
                        Console.Write("{0,-5}", p2Job.Id);
                    }
                }
                else
                {
                    if (_draw)
                    {
                        Console.Write("{0,-5}", "W");
                    }
                }
            }

            if (_draw)
            {
                Console.WriteLine();
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
            // W celach testowych - funkcja nigdy się nie wykona podczas normalnego działania programu,
            // do procesora zawsze przekazywana jest "prawidłowa" sekwencja, w której nie wystąpi zakleszczenie
            Console.WriteLine("DEBUG: Zakleszczenie");
            while (true)
            {
            }
        }
    }
}
