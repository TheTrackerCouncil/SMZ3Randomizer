using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
