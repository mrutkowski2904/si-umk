namespace GA_SI.Scheduler
{
    public class JobSequence
    {
        public List<Job> P1Jobs { get; set; }
        public List<Job> P2Jobs { get; set; }

        public JobSequence(List<Job> p1, List<Job> p2)
        {
            P1Jobs = p1;
            P2Jobs = p2;
        }

        public JobSequence(JobSequence sequence)
        {
            P1Jobs = sequence.P1Jobs;
            P2Jobs = sequence.P2Jobs;
        }

        public JobSequence Clone()
        {
            List<Job> p1Copy = new List<Job>();
            List<Job> p2Copy = new List<Job>();

            foreach (var job in P1Jobs)
            {
                p1Copy.Add(new Job()
                {
                    Id = job.Id,
                    //ProcessorId = job.ProcessorId,
                    Cost = job.Cost
                });
            }

            foreach (var job in P2Jobs)
            {
                p2Copy.Add(new Job()
                {
                    Id = job.Id,
                    // ProcessorId = job.ProcessorId,
                    Cost = job.Cost
                });
            }

            JobSequence clone = new JobSequence(p1Copy, p2Copy);
            return clone;
        }

        public int GetJobIndex(int jobId)
        {
            var p1Index = P1Jobs.FindIndex(j => j.Id == jobId);
            if (p1Index == -1)
                return P2Jobs.FindIndex(j => j.Id == jobId);

            return p1Index;
        }
    }
}
