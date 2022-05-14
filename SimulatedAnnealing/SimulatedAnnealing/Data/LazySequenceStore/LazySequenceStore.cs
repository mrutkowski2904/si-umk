using SimulatedAnnealing.Data;
using SimulatedAnnealing.Data.LazyStore;
using SimulatedAnnealing.Scheduler;
using SimulatedAnnealing.Util;

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
            var savedSequence = _generatedSequences.FirstOrDefault(s => s.X == index);
            if (savedSequence != null)
            {
                return savedSequence.Sequence;
            }

            var generatedSequence = GenerateValidSequence();
            _generatedSequences.Add(new GeneratedSequence(index, generatedSequence));
            return generatedSequence;
        }

        private JobSequence GenerateValidSequence()
        {
            // (jobId, cost)
            List<(int, int)> p1Candidates = new List<(int, int)>();
            List<(int, int)> p2Candidates = new List<(int, int)>();

            for (int i = 1; i <= CostManager.JobCount; i++)
            {
                var cost1 = CostManager.GetCost(i, 1);
                if (cost1 != 0)
                {
                    p1Candidates.Add((i, cost1));
                }

                var cost2 = CostManager.GetCost(i, 2);
                if (cost2 != 0)
                {
                    p2Candidates.Add((i, cost2));
                }
            }
            Random rng = new Random();

            while (true)
            {
                var p1CandCopy = p1Candidates.CloneList();
                var p2CandCopy = p2Candidates.CloneList();
                List<int> assignedJobIds = new List<int>();

                JobSequence sequence = new JobSequence(new List<Job>(), new List<Job>());

                while (p1CandCopy.Count > 0 && p2CandCopy.Count > 0)
                {
                    #region GENERATION_LOGIC
                    int processor = rng.Next(2) + 1;
                    int jobId = rng.Next(CostManager.JobCount) + 1;

                    if (assignedJobIds.Contains(jobId))
                    {
                        continue;
                    }
                    else
                    {
                        assignedJobIds.Add(jobId);
                    }

                    // --------- Procesor 1 ---------
                    var p1ContainsJob = p1CandCopy.Any(c => c.Item1 == jobId);
                    if (processor == 1 && p1ContainsJob)
                    {
                        var jobData = p1CandCopy.FirstOrDefault(c => c.Item1 == jobId);
                        sequence.P1Jobs.Add(new Job()
                        {
                            Id = jobData.Item1,
                            Cost = jobData.Item2
                        });

                        // usunięcie z list kandydatów
                        p1CandCopy.Remove(jobData);
                        p2CandCopy.Remove(p2CandCopy.FirstOrDefault(c => c.Item1 == jobData.Item1));
                    }
                    // nie istnieje taka praca na p1, trzeba użyć p2
                    else if (processor == 1 && !p1ContainsJob)
                    {
                        var jobData = p2CandCopy.FirstOrDefault(c => c.Item1 == jobId);
                        sequence.P2Jobs.Add(new Job()
                        {
                            Id = jobData.Item1,
                            Cost = jobData.Item2
                        });

                        // usunięcie z list kandydatów
                        p2CandCopy.Remove(jobData);
                        p1CandCopy.Remove(p1CandCopy.FirstOrDefault(c => c.Item1 == jobData.Item1));
                    }



                    // --------- Procesor 2 ---------
                    var p2ContainsJob = p2CandCopy.Any(c => c.Item1 == jobId);
                    if (processor == 2 && p2ContainsJob)
                    {
                        var jobData = p2CandCopy.FirstOrDefault(c => c.Item1 == jobId);
                        sequence.P2Jobs.Add(new Job()
                        {
                            Id = jobData.Item1,
                            Cost = jobData.Item2
                        });

                        // usunięcie z list kandydatów
                        p2CandCopy.Remove(jobData);
                        p1CandCopy.Remove(p1CandCopy.FirstOrDefault(c => c.Item1 == jobData.Item1));
                    }
                    // nie istnieje taka praca na p2, trzeba użyć p1
                    else if (processor == 2 && !p2ContainsJob)
                    {
                        var jobData = p1CandCopy.FirstOrDefault(c => c.Item1 == jobId);
                        sequence.P1Jobs.Add(new Job()
                        {
                            Id = jobData.Item1,
                            Cost = jobData.Item2
                        });

                        // usunięcie z list kandydatów
                        p1CandCopy.Remove(jobData);
                        p2CandCopy.Remove(p2CandCopy.FirstOrDefault(c => c.Item1 == jobData.Item1));
                    }

                    #endregion
                }

                //if (AllTasksDone(sequence) && RequirementsMet && sequence is new(doesnt exist) )
                if (AllTasksDone(sequence) && RequirementsMet(sequence) && IsUnique(sequence))
                {
                    return sequence;
                }
            }

        }

        private bool RequirementsMet(JobSequence sequence)
        {
            foreach (var requirement in _requirements)
            {
                int jobIndex = sequence.GetJobIndex(requirement.JobId);
                int requirementIndex = sequence.GetJobIndex(requirement.RequiresId);
                if (jobIndex < requirementIndex)
                {
                    return false;
                }
            }

            return true;
        }

        private bool IsUnique(JobSequence sequence)
        {
            // Ręczna zamiana na tablice z id - .Select z Linq nie gwarantuje zachowania kolejności
            int[] p1JobIds = new int[sequence.P1Jobs.Count];
            int[] p2JobIds = new int[sequence.P2Jobs.Count];

            for (int i = 0; i < sequence.P1Jobs.Count; i++)
                p1JobIds[i] = sequence.P1Jobs[i].Id;

            for (int i = 0; i < sequence.P2Jobs.Count; i++)
                p2JobIds[i] = sequence.P2Jobs[i].Id;

            foreach (var sequenceEntry in _generatedSequences)
            {
                var genSequence = sequenceEntry.Sequence;

                if ((genSequence.P1Jobs.Count != p1JobIds.Length) && (genSequence.P2Jobs.Count != p2JobIds.Length))
                {
                    continue;
                }

                int[] p1GenJobIds = new int[genSequence.P1Jobs.Count];
                int[] p2GenJobIds = new int[genSequence.P2Jobs.Count];

                for (int i = 0; i < genSequence.P1Jobs.Count; i++)
                    p1GenJobIds[i] = genSequence.P1Jobs[i].Id;

                for (int i = 0; i < genSequence.P2Jobs.Count; i++)
                    p2GenJobIds[i] = genSequence.P2Jobs[i].Id;

                if (Arrays.Identical(p1JobIds, p1GenJobIds) && Arrays.Identical(p2JobIds, p2GenJobIds))
                {
                    return false;
                }
            }

            return true;
        }

        private bool AllTasksDone(JobSequence sequence)
        {
            for (int i = 1; i <= CostManager.JobCount; i++)
            {
                bool doneOnP1 = sequence.P1Jobs.Any(j => j.Id == i);
                bool doneOnP2 = sequence.P2Jobs.Any(j => j.Id == i);

                if (!doneOnP1 && !doneOnP2)
                {
                    return false;
                }

                if (doneOnP1 && doneOnP2)
                {
                    throw new ApplicationException("Error, generated invalid sequence");
                }
            }

            return true;
        }


    }
}
