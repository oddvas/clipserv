using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ClipservWindows
{
    public class Program
    {
        static ManualResetEvent _quitEvent = new ManualResetEvent(false);

        public static void Main(string[] args)
        {
            Console.CancelKeyPress += (sender, eArgs) => {
                _quitEvent.Set();
                eArgs.Cancel = true;
            };



            _quitEvent.WaitOne();
        }
    }
}
