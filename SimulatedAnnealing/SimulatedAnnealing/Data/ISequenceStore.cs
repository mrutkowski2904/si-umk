using SimulatedAnnealing.Scheduler;

namespace SimulatedAnnealing.Data
{
    /*
     * Interfejs reprezentujący zbiór wszystkich prawidłowych sekwencji zadań
     */
    public interface ISequenceStore
    {
        /*
         * Pobranie prawidłowej sekwencji dla podanego indeksu
         */
        public JobSequence GetSequence(int index);
    }
}
