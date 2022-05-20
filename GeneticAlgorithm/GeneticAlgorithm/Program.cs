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
            2    1    7
            3    4    7
            4    4    7
            5    4    3
            6    4    3
            7    4    3
            8    4    6
            9    5    6
            10   5    6
            11   5    6
            12   5    6
            13   5    6
            14   5    6
            15   5
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
            //Console.WriteLine("Hello world");

            // Bity 0-liczbaZad - który procesor
            // DalszeBity - seed dla shuffle list (+1 do momentu aż dobra kombinacja)

            // sposób inny - spróbować fitness 0 dla tych co sa nielegalne

            // zamiast seed - każde zadanie ma 3-4 bity w chromosomie które opisują pozycje na której się znajduje


            const double crossoverProbability = 0.85;
            const double mutationProbability = 0.08;
            const int elitismPercentage = 15;

            #region STARE
            /*
             * Budowa chromosomu:
             * N pierwszych bitów (gdzie N to liczba zadań) - wybór procesora dla zadania n
             * Pozostałe bity - seed dla funkcji Shuffle zmieniającej kolejność zadań na procesorach
             * Rozmiar chromosomu: 32 bity
             */

            var population = new Population(10, CostManager.JobCount + (32 - CostManager.JobCount), false, false);
            #endregion

            //var population = new Population(10, CostManager.JobCount * (1 + 4), false, false);

            //create the genetic operators 
            var elite = new Elite(elitismPercentage);

            var crossover = new Crossover(crossoverProbability, true)
            {
                CrossoverType = CrossoverType.SinglePoint
            };

            var mutation = new BinaryMutate(mutationProbability, true);

            //create the GA itself 
            var ga = new GeneticAlgorithm(population, EvaluateFitness);

            //subscribe to the GAs Generation Complete event 
            ga.OnGenerationComplete += ga_OnGenerationComplete;

            //add the operators to the ga process pipeline 
            ga.Operators.Add(elite);
            ga.Operators.Add(crossover);
            ga.Operators.Add(mutation);

            //run the GA 
            ga.Run(TerminateAlgorithm);
        }

        public static double EvaluateFitness(Chromosome chromosome)
        {
            double fitnessValue = 0;

            if (chromosome != null)
            {
                //UInt32 binChromosome = Convert.ToUInt32(chromosome.ToBinaryString());

                #region nowe
                /*
                Stack<char> binChars = new Stack<char>(chromosome.ToBinaryString());

                List<Job> p1Jobs = new List<Job>();
                List<Job> p2Jobs = new List<Job>();

                List<(int, int)> jobIdPos = new List<(int, int)>();

                int i = 0;
                while (binChars.Count > 0)
                {
                    int proc = binChars.Pop() == '1' ? 1 : 0;
                    int pos;

                    string positionString = "";

                    for (int j = 0; j < 4; j++)
                    {
                        positionString += binChars.Pop();
                    }
                    pos = Convert.ToInt32(positionString);

                    jobIdPos.Add((i + 1, pos));

                    if (proc == 1)
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
                    i++;
                }
                */
                #endregion


                #region STARE

                /*
                UInt32 processorsBin = Convert.ToUInt32(chromosome.ToBinaryString(0, CostManager.JobCount));
                var seedBinaryString = chromosome.ToBinaryString(CostManager.JobCount, (32 - CostManager.JobCount));

                int shuffleSeed = 0;
                for (int i = 0; i < seedBinaryString.Length; i++)
                {
                    shuffleSeed += (int)System.Math.Pow(2, i) + Convert.ToInt32(seedBinaryString[i]);
                }

                List<Job> p1Jobs = new List<Job>();
                List<Job> p2Jobs = new List<Job>();

                int sequenceCost = 0;

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

                //bool sequenceValid = false;

                int retries = 0;
                */
                /*
                while (true)
                {
                    var requirementTest = RequirementsMet(generatedSequence);
                    if (requirementTest.Item1)
                    {
                        //sequenceValid = true;
                        break;
                    }
                */
                /*
                if(requirementTest.Item1 == false)
                {
                    return 0.0;
                }
                */

                //Requirement unfulfiledRequirement = (Requirement)requirementTest.Item2!;

                // adjust job with requirement, move job higher (in order to pass required job)
                /*
                if (generatedSequence.P1Jobs.Any(j => j.Id == unfulfiledRequirement.JobId))
                {
                    // Job on P1
                    // 

                    sequenceCost += 5; // sequence needs adjusting, higher cost
                }
                else
                {
                    // Job on P2


                    sequenceCost += 5;
                }
                */
                /*
                retries++;

                if (retries > 1000)
                {
                    return 0;
                }

            }

            sequenceCost += scheduler.RunJobs(generatedSequence);
            fitnessValue = 15.0 / sequenceCost;
                */
                #endregion


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
                if (requirementTest.Item1)
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


            //get the best solution 
            var chromosome = e.Population.GetTop(1)[0];
            if (lastFitnessChange.Item2 < chromosome.Fitness)
            {
                lastFitnessChange.Item1 = e.Generation;
                lastFitnessChange.Item2 = chromosome.Fitness;
                Console.WriteLine($"Generation: {e.Generation} | Fitness: {chromosome.Fitness}");
            }

            //Console.WriteLine($"Generation: {e.Generation} | Fitness: {e.Population.MaximumFitness}");
            if (e.Generation == maxGenerations)
            {
                JobScheduler drawingScheduler = new JobScheduler(requirements, true);
                drawingScheduler.RunJobs(ChromosomeToSequence(chromosome).Item2!);
                //Console.WriteLine($"Najlepsze rozwiązanie: {ChromosomeToSequence(chromosome)}");
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

        public static void Move<T>(List<T> list, T item, int newIndex)
        {
            if (item != null)
            {
                var oldIndex = list.IndexOf(item);
                if (oldIndex > -1)
                {
                    list.RemoveAt(oldIndex);

                    if (newIndex > oldIndex) newIndex--;
                    // the actual index could have shifted due to the removal

                    list.Insert(newIndex, item);
                }
            }

        }

        static private (bool, Requirement?) RequirementsMet(JobSequence sequence)
        {
            foreach (var requirement in requirements)
            {
                int jobIndex = sequence.GetJobIndex(requirement.JobId);
                int requirementIndex = sequence.GetJobIndex(requirement.RequiresId);
                if (jobIndex < requirementIndex)
                {
                    return (false, requirement);
                }
            }

            return (true, null);
        }

        static private bool AllTasksDone(JobSequence sequence)
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

