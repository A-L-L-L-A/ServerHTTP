using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    class Program
    {
        static void Main(string[] args)
        {
            string path;
            //L:\Web\data\sites
            Console.WriteLine("Введите к файлу сайта");
            path = Console.ReadLine();
            Console.WriteLine("Введите end для завершения\n");

            HTTPServer server = new HTTPServer(path);

            while (Console.ReadLine() != "end")
                Console.WriteLine("Введите end для завершения\n");

            server.Stop();
            
        }
    }
}
