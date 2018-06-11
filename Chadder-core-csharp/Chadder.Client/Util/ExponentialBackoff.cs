using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Chadder.Client.Util
{
    public class ExponentialBackoff
    {
        public int NextDelay { get; set; }
        private int _maxDelay = 0;
        public int FirstDelay { get; set; }

        public ExponentialBackoff(int firstDelay, int max)
        {
            FirstDelay = firstDelay;
            NextDelay = firstDelay;
            _maxDelay = max;
        }
        public void Reset()
        {
            NextDelay = FirstDelay;
        }

        public Task Failed()
        {
            var temp = NextDelay;
            NextDelay += NextDelay;
            if (NextDelay > _maxDelay)
                NextDelay = _maxDelay;
            return Task.Delay(temp);
        }
    }
}
