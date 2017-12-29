using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace lowbot
{
    internal class DrawTrainer
    {
        private readonly int iterations;
        private readonly int num_threads;
        private static ConcurrentDictionary<String, Node> NodeMap = new ConcurrentDictionary<string, Node>();

        public DrawTrainer(int iter, int nt)
        {
            iterations = iter;
            num_threads = nt;
        }

        public void SaveToFile(string FileName)
        {
            string Path = @"E:\Lowbot\";
            using (StreamWriter File = new StreamWriter(Path + FileName, true))
            {
                foreach (Node n in NodeMap.Values)
                {
                    string Line = n.ToString();
                    //if (n.Realization < 1e-5)
                    //    Line += ",INFREQUENT";
                    File.WriteLine(Line);
                }
            }
        }

        private double Train(int iter, int ID)
        {
            double Util = 0.0;

            for (int i = 1; i <= iter; i++)
            {
                Util += Iteration(i, iter);
            }

            return Util;
            //Console.WriteLine("Average game value: {0}", Util / iter);
            //if (ID == 2) 
            //    SaveToFile("strategy.csv");
        }

        private double Iteration(int i, int iter)
        {
            string Deck = Draw.GenerateDeck();
            string Hand1 = Draw.SortHand(Deck.Substring(0, Draw.HAND_CARDS));
            string Hand2 = Draw.SortHand(Deck.Substring(Draw.HAND_CARDS, Draw.HAND_CARDS));

            double Util = CFR(Deck, "r", Hand1, Hand2, 1, 1);

            Console.WriteLine("Iteration {0} / {1}", i, iter);

            return Util;
            //if (i % 100000 == 0)
            //    SaveToFile("strategy_" + Convert.ToString(i / 100000) + ".csv");
        }

        private double CFR(string Deck, string History, string Hand1, string Hand2, double p0, double p1)
        {
            //if (p1 + p0 < 1e-5)
            //{
            //    return 0;
            //}

            if (p0 < 1e-6 && p1 < 1e-6)
                return 0;

            int Player = Draw.GetCurrentPlayer(History);
            int Opponent = 1 - Player;
            string PlayerHand = (Player == 0) ? Hand1 : Hand2;
            string OpponentHand = (Opponent == 0) ? Hand1 : Hand2;
            string Actions = Draw.GetLegalActions(History);

            if (Actions == Draw.TERMINAL_FOLD)
                return Draw.GetPotContribution(History, Opponent);

            if (Actions == Draw.TERMINAL_CALL)
                return Draw.GetPotContribution(History, Opponent) * Draw.CompareHands(PlayerHand, OpponentHand);

            string InfoSet = Draw.CreateInfoSet(History, PlayerHand);

            Node Node = null;
            int NumActions;

            if (Actions == Draw.DRAW || Actions == Draw.LAST_DRAW)
                NumActions = (int)Math.Pow(2, Draw.HAND_CARDS);
            else
                NumActions = Actions.Length;
            Node = new Node(NumActions, Actions, InfoSet);

            if (!NodeMap.TryAdd(InfoSet, Node))
            {
                Node = NodeMap[InfoSet];
                Node.Count++;
            }

            double[] Strategy = GetStrategy(Node, (Player == 0) ? p0 : p1);

            double[] Util = new double[NumActions];
            double NodeUtil = 0.0;

            for (int i = 0; i < NumActions; i++)
            {
                string NextHistory;
                string NewHand = String.Copy(PlayerHand);
                if (Actions == Draw.DRAW || Actions == Draw.LAST_DRAW)
                {
                    int NumDraw = Draw.DrawCards(History, Deck, PlayerHand, ref NewHand, i);
                    if (Actions == Draw.DRAW)
                        NextHistory = History + "(" + Convert.ToString(NumDraw);
                    else
                        NextHistory = History + Convert.ToString(NumDraw) + ")";
                }
                else
                    NextHistory = History + Actions[i];

                Util[i] = (Player == 0) ? -CFR(Deck, NextHistory, NewHand, OpponentHand, p0 * Strategy[i], p1) : -CFR(Deck, NextHistory, OpponentHand, NewHand, p0, p1 * Strategy[i]);
                if (Draw.GetCurrentPlayer(History) == Draw.GetCurrentPlayer(NextHistory))
                    Util[i] = -Util[i];

                NodeUtil += Strategy[i] * Util[i];
            }

            for (int i = 0; i < NumActions; i++)
            {
                double Regret = Util[i] - NodeUtil;
                Node.RegretSum[i] += Regret * ((Player == 0) ? p1 : p0);
            }

            return NodeUtil;
        }

        public double[] GetStrategy(Node Node, double RealizationWeight)
        {
            double NormalizingSum = 0.0;
            double[] Strategy = new double[Node.NumActions];

            for (int i = 0; i < Node.NumActions; i++)
            {
                Strategy[i] = (Node.RegretSum[i] > 0) ? Node.RegretSum[i] : 0;
                NormalizingSum += Strategy[i];
            }

            for (int i = 0; i < Node.NumActions; i++)
            {
                if (NormalizingSum > 0)
                    Strategy[i] /= NormalizingSum;
                else
                    Strategy[i] = 1.0 / Node.NumActions;

                Node.StrategySum[i] += RealizationWeight * Strategy[i];
            }

            return Strategy;
        }

        public void main()
        {
            //Task T1 = Task.Factory.StartNew(() => Train(iterations / 2, 1));
            //Task T2 = Task.Factory.StartNew(() => Train(iterations / 2, 2));
            //Task T3 = Task.Factory.StartNew(() => Train(iterations / 4, 3));
            //Task T4 = Task.Factory.StartNew(() => Train(iterations / 4, 4));
            //T1.Wait();
            //T2.Wait();
            //T3.Wait();
            //T4.Wait();
            //Train(iterations, 1);

            List<Task<double>> Tasks = new List<Task<double>>();
            double Util = 0.0;
            for (int i = 0; i < num_threads; i++)
                Tasks.Add(Task.Factory.StartNew<double>(() => Train(iterations / num_threads, i)));
            foreach (Task<double> T in Tasks)
                Util += T.Result;

            Console.WriteLine("Average game value: {0}", Util / iterations);
            SaveToFile("strategy.csv");
        }
    }
}