using System;
using System.Collections.Generic;
using System.IO;
using System.Timers;
using System.Collections.Concurrent;

namespace lowbot
{
    internal class DrawTrainer
    {
        private readonly int iterations;
        private ConcurrentDictionary<String, Node> NodeMap = new ConcurrentDictionary<string, Node>();


        public DrawTrainer(int iter, int id)
        {
            iterations = iter;
        }

        public DrawTrainer(int iter, int id, ConcurrentDictionary<String, Node> Nodes)
        {
            iterations = iter;
            NodeMap = Nodes;
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

        private void Train(int iter)
        {
            double Util = 0.0;

            for (int i = 1; i <= iter; i++)
            {
                Iteration(i, iter);
            }

            Console.WriteLine("Average game value: {0}", Util / iter);
            SaveToFile("strategy.csv");
        }

        private void Iteration(int i, int iter)
        {
            string Deck = Draw.GenerateDeck();
            string Hand1 = Draw.SortHand(Deck.Substring(0, Draw.HAND_CARDS));
            string Hand2 = Draw.SortHand(Deck.Substring(Draw.HAND_CARDS, Draw.HAND_CARDS));

            CFR(Deck, "r", Hand1, Hand2, 1, 1);

            Console.WriteLine("Iteration {0} / {1}", i, iter);
            if (i % 100000 == 0)
                SaveToFile("strategy_" + Convert.ToString(i / 100000) + ".csv");
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
            if (NodeMap.ContainsKey(InfoSet))
            {
                Node = NodeMap[InfoSet];
                NumActions = Node.NumActions;
                Node.Count++;
            }
            else
            {
                if (Actions == Draw.DRAW || Actions == Draw.LAST_DRAW)
                    NumActions = (int)Math.Pow(2, Draw.HAND_CARDS);
                else
                    NumActions = Actions.Length;
                Node = new Node(NumActions, Actions, InfoSet);

                NodeMap.Add(InfoSet, Node);
            }

            double[] Strategy = Node.GetStrategy((Player == 0) ? p0 : p1);

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

        public void main()
        {
            Train(iterations);
        }
    }
}