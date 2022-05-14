using SimulatedAnnealing.Data;
using SimulatedAnnealing.Scheduler;
using SimulatedAnnealing.SimulatedAnnealing.Data.LazyStore;

namespace SimulatedAnnealing
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // 1<3<5 i 2<4
            List<Requirement> requirements = new List<Requirement>()
            {
                new Requirement(){ JobId = 3, RequiresId = 1 },
                new Requirement(){ JobId = 5, RequiresId = 3 },
                new Requirement(){ JobId = 5, RequiresId = 1 },
                new Requirement(){ JobId = 4, RequiresId = 2 },
            };

            ISequenceStore sequenceStore = new LazySequenceStore(requirements);
            List<JobSequence> jobs = new List<JobSequence>();
            for (int i = 0; i < 1300; i++)
            {
                jobs.Add(sequenceStore.GetSequence(i));
            }
            JobScheduler scheduler = new(requirements);
            int min = 100;
            JobSequence best= null;
            foreach (JobSequence job in jobs)
            {
                /*
                Console.Write("\nP1: ");
                foreach(Job j in job.P1Jobs)
                {
                    Console.Write($"{j.Id} ");
                }

                Console.Write("\nP2: ");
                foreach (Job j in job.P2Jobs)
                {
                    Console.Write($"{j.Id} ");
                }
                */
                var cost = scheduler.RunJobs(job);
                if(cost < min)
                {
                    min = cost;
                    best = job;
                }
                
                Console.WriteLine($"\nCost: {cost}");
            }

            Console.WriteLine($"Min: {min}");
            JobScheduler schedulerDraw = new(requirements, true);
            schedulerDraw.RunJobs(best);

            /*
            List<Job> p1Jobs = new List<Job>()
            {
                new Job(){ Id = 1, Cost = 1},
                new Job(){ Id = 3, Cost = 2},
                new Job(){ Id = 2, Cost = 3},
            };
            List<Job> p2Jobs = new List<Job>()
            {
                //new Job(){ Id = 3, Cost = 2}
                new Job(){ Id = 4, Cost = 8},
            };
            JobSequence sequence = new JobSequence(p1Jobs, p2Jobs);

            JobScheduler scheduler = new(requirements);
            Console.WriteLine($"Cost: {scheduler.RunJobs(sequence)}");
            */
        }
    }
}


