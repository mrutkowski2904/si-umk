namespace SimulatedAnnealing.Model
{
    public struct Job
    {
        public int Id { get; set; }
        // 1 or 2
        public int ProcessorId { get; set; }

        public int Cost { get; set; }
    }
}
