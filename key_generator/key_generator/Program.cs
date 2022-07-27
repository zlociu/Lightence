using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductKey
{
    class Program
    {
        static void Main(string[] args)
        {
            bool debug = false;
            if (args.Length == 3)
            {
                if (args[2] == "--debug") debug = true;
                else
                {
                    Console.WriteLine("Wrong parameter");
                    return;
                }
            }
            else
            {
                if (args.Length != 2)
                {
                    Console.WriteLine("Usage: <numberOfKeys> <KeyMultiplier> (--debug)");
                    return;
                }
            }

            if(debug==true) Console.WriteLine("Generated {0} keys with {1} uses each. \n",args[0],args[1]);
            var key = new ProductKey();
            try
            {
                for (var i = 0; i < int.Parse(args[0]); i++)
                {
                    var result = key.generateKey(int.Parse(args[1]));
                    if (result == false) Console.WriteLine("Wrong max users number");
                    Console.WriteLine(key.ToString());
                }
                if (debug == true)
                {
                    Console.WriteLine("Verify key: " + key.verifyKey());
                    Console.WriteLine("Max users check: " + key.checkMaxUsers());
                }       
            }
            catch (Exception) { }
            
        }
    }
}
