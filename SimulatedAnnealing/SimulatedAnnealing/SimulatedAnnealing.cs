using SimulatedAnnealing.Data;
using SimulatedAnnealing.Scheduler;
using SimulatedAnnealing.SimulatedAnnealing.Data.LazyStore;

namespace SA
{
    public class SimulatedAnnealing
    {
        private readonly JobScheduler _jobScheduler;
        private readonly ISequenceStore _sequenceStore;
        private readonly List<Requirement> _requirements;
        private readonly Random _random;

        public SimulatedAnnealing()
        {
            // 1<3<5 i 2<4
            List<Requirement> requirements = new List<Requirement>()
            {
                new Requirement(){ JobId = 3, RequiresId = 1 },
                new Requirement(){ JobId = 5, RequiresId = 3 },
                new Requirement(){ JobId = 5, RequiresId = 1 },
                new Requirement(){ JobId = 4, RequiresId = 2 },
            };

            _requirements = requirements;
            _jobScheduler = new JobScheduler(requirements);
            _sequenceStore = new LazySequenceStore(requirements);
            _random = new Random();
        }

        public void Run()
        {
            int minX = 0;
            int globalMinX = 0;

            int currentX = 1;
            int newX;

            double T = 1000;
            double alpha = 0.3;

            while (T > 0)
            {
                newX = GetNeighbour();

                if (GetValue(currentX) < GetValue(minX))
                {
                    minX = currentX;
                    if (GetValue(minX) < GetValue(globalMinX))
                    {
                        globalMinX = minX;
                    }
                }

                /*
                 * Wraz ze spadkiem temperatury będzie malała szansa na zaakceptowanie mniej optymalnego parametru x
                 */
                else if (Math.Exp((GetValue(currentX) - GetValue(newX)) / T) > RandomDouble(0, 1))
                {
                    currentX = newX;
                }

                T *= alpha;
            }

            Console.WriteLine($"MIN: {GetValue(globalMinX)}\n");
            JobScheduler schedulerDraw = new JobScheduler(_requirements, true);
            schedulerDraw.RunJobs(_sequenceStore.GetSequence(globalMinX));
        }

        private int GetNeighbour()
        {
            return _random.Next(1500);
        }

        private int GetValue(int x)
        {
            return _jobScheduler.RunJobs(_sequenceStore.GetSequence(x));
        }

        private double RandomDouble(double minValue, double maxValue)
        {
            var next = _random.NextDouble();
            return minValue + (next * (maxValue - minValue));
        }
    }
}
