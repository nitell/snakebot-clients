using System;
using Cygni.Snake.Client;
using Cygni.Snake.SampleBot.Nitell;

namespace Cygni.Snake.SampleBot
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var client = SnakeClient.CreateSnakeClient(new Uri("ws://snake.cygni.se:80/training"), new GamePrinter());
            client.Start(new Glennbot(), true);

            Console.ReadLine();
        }
    }
}