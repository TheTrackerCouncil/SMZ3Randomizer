using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace Randomizer.SMZ3.Tracking.Vocabulary
{
    public class PhraseSet : Collection<Phrase>
    {
        public PhraseSet()
        {
        }

        public PhraseSet(params Phrase[] phrases)
        {
            foreach (var item in phrases)
                Add(item);
        }

        /// <summary>
        /// Picks a phrase at random, taking into account any weights associated
        /// with the phrases.
        /// </summary>
        /// <param name="random">The random number generator to use.</param>
        /// <returns>
        /// A random phrase from the phrase set, or <c>null</c> if the phrase
        /// set contains no items.
        /// </returns>
        public Phrase? Random(Random random)
        {
            if (Count == 0)
                return null;

            var target = random.NextDouble() * GetTotalWeight();
            foreach (var item in this)
            {
                if (target < item.Weight)
                    return item;

                target -= item.Weight;
            }

            throw new Exception("This code should not be reachable.");
        }

        private double GetTotalWeight() => this.Sum(x => x.Weight);
    }
}
