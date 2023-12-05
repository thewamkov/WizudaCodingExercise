using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WizudaCodingExercise.Models
{
    public class EvictionEventArgs<TKey, TValue> : EventArgs
    {
        public TKey Key { get; }
        public TValue Value { get; }

        public EvictionEventArgs(TKey key, TValue value)
        {
            Key = key;
            Value = value;
        }
    }
}
