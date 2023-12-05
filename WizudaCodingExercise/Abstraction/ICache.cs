using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WizudaCodingExercise.Abstraction
{
    public interface ILRUCache<TKey, TValue>
    {
        void AddOrUpdate(TKey key, TValue value);
        bool TryGetValue(TKey key, out TValue value);
    }
}
