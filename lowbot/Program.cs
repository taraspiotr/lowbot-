using System;
using System.Linq;

namespace lowbot
{
    class Program
    {
        static void Main(string[] args)
        {
            string Deck = Draw.GenerateDeck(5);
            Console.WriteLine(Deck);

            string Hand = Deck.Substring(0, 5);
            Console.WriteLine(Hand);
            Console.WriteLine(Draw.SortHand(Hand));

            int[] numbers = new int[12] { 5, 5, 5, 7, 7, 7, 9, 7, 9, 9, 9, 1 };

            var count = numbers
                .GroupBy(e => e)
                .OrderByDescending(e => e.Count())
                .ThenByDescending(e => e.Key).ToList();
            foreach (var el in count)
                Console.WriteLine("{0} {1}", el.Key, el.Count());

            Console.WriteLine(Draw.CompareHands("AKQ32", "T9854"));

            string History = "rcf";
            Console.WriteLine(Draw.GetCurrentPlayer(History));
            Console.WriteLine(Draw.GetPotContribution(History, 1));

        Console.ReadKey();
        }
    }
}
