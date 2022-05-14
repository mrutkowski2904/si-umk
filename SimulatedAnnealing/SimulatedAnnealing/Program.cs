using SimulatedAnnealing.Model;

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
                //new Requirement(){ JobId = 5, RequiresId = 3 },
                //new Requirement(){ JobId = 5, RequiresId = 1 },
                //new Requirement(){ JobId = 4, RequiresId = 2 },
            };

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

            Scheduler scheduler = new Scheduler(requirements);
            Console.WriteLine($"Cost: {scheduler.RunJobs(sequence)}");
        }
    }
}


