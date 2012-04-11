using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NServiceBus;
using NServiceBus.Faults;

namespace NServiceBus.Management.Errors.TestHarness
{
    class Bootstrapper : IWantToRunAtStartup
    {
        public IBus Bus { get; set; }
        
        public void Run()
        {
            
            Console.WriteLine("Press any key to generate a message that will result in the error queue");
            int i = 1;
            while (Console.ReadLine() != null)
            {
                Console.WriteLine("Sending a message...");
                Bus.SendLocal(new SendTestMessage() { Name = string.Format("Hello {0}", i) });
                i++;

                Console.WriteLine();
                Console.WriteLine("Press any key to generate another message that will result in the error queue");
            }
        }

        public void Stop()
        {
            
        }
    }
}
