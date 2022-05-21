using GA_SI.Data;
using GA_SI.Scheduler;
using GAF;
using GAF.Operators;

namespace GA_SI
{
    internal class Program
    {
        /*
            // Przykładowy wynik działania programu: 

            Generation: 1 | Fitness: 0,6521739130434783
            Generation: 3 | Fitness: 0,7142857142857143
            Generation: 13 | Fitness: 0,75
            Generation: 17 | Fitness: 0,7894736842105263
            Generation: 74 | Fitness: 0,8333333333333334
            Generation: 267 | Fitness: 0,9375
            Generation: 598 | Fitness: 1

            TIM  P1   P2
            1    1    2
            2    1    W
            3    4    3
            4    4    3
            5    4    3
            6    4    6
            7    4    6
            8    4    6
            9    5    6
            10   5    6
            11   5    6
            12   5    6
            13   5    7
            14   5    7
            15   5    7
         */

        readonly static int maxGenerations = 1000;

        // poprzednia zmiana - do wyświetlania postępu
        // (generacja, fitness)
        static (int, double) lastFitnessChange = (0, 0);

        // 1<3<5 i 2<4
        static readonly List<Requirement> requirements = new List<Requirement>()
        {
            new Requirement(){ JobId = 3, RequiresId = 1 },
            new Requirement(){ JobId = 5, RequiresId = 3 },
            new Requirement(){ JobId = 5, RequiresId = 1 },
            new Requirement(){ JobId = 4, RequiresId = 2 },
        };

        static readonly JobScheduler scheduler = new JobScheduler(requirements);

        static void Main(string[] args)
        {
            const double crossoverProbability = 0.85;
            const double mutationProbability = 0.08;
            const int elitismPercentage = 15;

            /*
             * Budowa chromosomu:
             * N pierwszych bitów (gdzie N to liczba zadań) - wybór procesora dla zadania n
             * Pozostałe bity - seed dla funkcji Shuffle zmieniającej kolejność zadań na procesorach
             * Rozmiar chromosomu: 32 bity
             */

            var population = new Population(10, CostManager.JobCount + (32 - CostManager.JobCount), false, false);

            var elite = new Elite(elitismPercentage);

            var crossover = new Crossover(crossoverProbability, true)
            {
                CrossoverType = CrossoverType.SinglePoint
            };

            var mutation = new BinaryMutate(mutationProbability, true);

            var ga = new GeneticAlgorithm(population, EvaluateFitness);

            ga.OnGenerationComplete += ga_OnGenerationComplete;

            ga.Operators.Add(elite);
            ga.Operators.Add(crossover);
            ga.Operators.Add(mutation);

            ga.Run(TerminateAlgorithm);
        }

        public static double EvaluateFitness(Chromosome chromosome)
        {
            double fitnessValue = 0;

            if (chromosome != null)
            {
                var result = ChromosomeToSequence(chromosome);
                if (result.Item1 == false)
                {
                    return 0;
                }

                var sequenceCost = scheduler.RunJobs(result.Item2!) + result.Item3;
                fitnessValue = 15.0 / sequenceCost;
            }
            else
            {
                throw new ArgumentNullException("chromosome", "The specified Chromosome is null.");
            }

            return fitnessValue;
        }

        static (bool, JobSequence?, int) ChromosomeToSequence(Chromosome chromosome)
        {
            UInt32 processorsBin = Convert.ToUInt32(chromosome.ToBinaryString(0, CostManager.JobCount));
            var seedBinaryString = chromosome.ToBinaryString(CostManager.JobCount, (32 - CostManager.JobCount));

            int shuffleSeed = 0;
            for (int i = 0; i < seedBinaryString.Length; i++)
            {
                shuffleSeed += (int)System.Math.Pow(2, i) + Convert.ToInt32(seedBinaryString[i]);
            }

            List<Job> p1Jobs = new List<Job>();
            List<Job> p2Jobs = new List<Job>();

            for (int i = 0; i < CostManager.JobCount; i++)
            {
                // pobranie 1 bitu na pozycji i
                var currentJobProcessor = (processorsBin >> i) & 0x00000001;

                if (currentJobProcessor == 1)
                {
                    var cost = CostManager.GetCost(i + 1, 1);
                    if (cost != 0)
                    {
                        p1Jobs.Add(new Job()
                        {
                            Id = i + 1,
                            Cost = cost
                        });
                    }
                    else
                    {
                        p2Jobs.Add(new Job()
                        {
                            Id = i + 1,
                            Cost = CostManager.GetCost(i + 1, 2)
                        });
                    }
                }
                else
                {
                    var cost = CostManager.GetCost(i + 1, 2);
                    if (cost != 0)
                    {
                        p2Jobs.Add(new Job()
                        {
                            Id = i + 1,
                            Cost = cost
                        });
                    }
                    else
                    {
                        p1Jobs.Add(new Job()
                        {
                            Id = i + 1,
                            Cost = CostManager.GetCost(i + 1, 1)
                        });
                    }
                }
            }

            Shuffle<Job>(p1Jobs, shuffleSeed);
            Shuffle<Job>(p2Jobs, shuffleSeed);

            JobSequence generatedSequence = new JobSequence(p1Jobs, p2Jobs);

            int retries = 0;
            while (true)
            {
                var requirementTest = RequirementsMet(generatedSequence);
                if (requirementTest)
                {
                    break;
                }
                retries++;

                if (retries > 10)
                {
                    return (false, null, 0);
                }

            }
            return (true, generatedSequence, retries);
        }

        public static bool TerminateAlgorithm(Population population, int currentGeneration, long currentEvaluation)
        {
            return currentGeneration == maxGenerations;
        }

        private static void ga_OnGenerationComplete(object sender, GaEventArgs e)
        {
            var chromosome = e.Population.GetTop(1)[0];
            if (lastFitnessChange.Item2 < chromosome.Fitness)
            {
                lastFitnessChange.Item1 = e.Generation;
                lastFitnessChange.Item2 = chromosome.Fitness;
                Console.WriteLine($"Generation: {e.Generation} | Fitness: {chromosome.Fitness}");
            }

            if (e.Generation == maxGenerations)
            {
                JobScheduler drawingScheduler = new JobScheduler(requirements, true);
                drawingScheduler.RunJobs(ChromosomeToSequence(chromosome).Item2!);
            }

        }

        static void Shuffle<T>(IList<T> list, int seed)
        {
            Random rng = new Random(seed);
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        static private bool RequirementsMet(JobSequence sequence)
        {
            foreach (var requirement in requirements)
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
    }
}

