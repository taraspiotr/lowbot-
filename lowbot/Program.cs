using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace lowbot
{
    class Program
    {
        static void Main(string[] args)
        {
            Stopwatch StopWatch = new Stopwatch();
            StopWatch.Start();
            
            DrawTrainer T = new DrawTrainer(2000000, 4);
            T.main();

            StopWatch.Stop();
            Console.WriteLine("Runtime = {0}s", StopWatch.ElapsedMilliseconds / 1000);
            Console.ReadKey();
        }
        /**
        private static void Run()
        {
            const int NUM_THREADS = 4;
            const int NUM_ITER = 800000;
            const int ITER_PER_THREAD = 10000;

            int RemainIter = NUM_ITER;
            Dictionary<String, Node> NodeMap = null;

            while (RemainIter - NUM_THREADS * ITER_PER_THREAD > 0)
            {
                NodeMap = RunParallel(ITER_PER_THREAD, NUM_THREADS, NodeMap);
                RemainIter -= NUM_THREADS * ITER_PER_THREAD;
            }
            NodeMap = RunParallel(ITER_PER_THREAD, RemainIter / ITER_PER_THREAD, NodeMap);
            RemainIter -= (RemainIter / ITER_PER_THREAD) * ITER_PER_THREAD;

            DrawTrainer FinalTrainer = new DrawTrainer(RemainIter, 9, NodeMap);
            FinalTrainer.main();
            FinalTrainer.SaveToFile("strategy.csv");
        }

        private static Dictionary<String, Node> RunParallel(int IterPerThread, int NumThreads, Dictionary<String, Node> NodeMap = null)
        {
            if (NumThreads == 0)
                return NodeMap;

            List<Task<Dictionary<String, Node>>> Tasks = new List<Task<Dictionary<String, Node>>>();
            for (int i = 0; i < NumThreads; i++)
            {
                int temp = i;
                DrawTrainer Trainer;
                if (NodeMap == null)
                    Trainer = new DrawTrainer(IterPerThread, temp);
                else
                    Trainer = new DrawTrainer(IterPerThread, temp, NodeMap);
                Tasks.Add(Task.Factory.StartNew<Dictionary<String, Node>>(Trainer.main));
            }

            List<Dictionary<String, Node>> NodeMaps = new List<Dictionary<string, Node>>();

            foreach (Task<Dictionary<String, Node>> T in Tasks)
                NodeMaps.Add(T.Result);

            return CombineNodeMaps(NodeMaps);
        }

        private static Dictionary<String, Node> CombineNodeMaps(List<Dictionary<String, Node>> NodeMaps)
        {
            Dictionary<String, Node> NodeMap = new Dictionary<string, Node>();

            foreach (Dictionary<String, Node> NM in NodeMaps)
            {
                foreach (Node N in NM.Values)
                {
                    if (NodeMap.ContainsKey(N.InfoSet))
                    {
                        Node TempNode = NodeMap[N.InfoSet];
                        for (int i = 0; i < TempNode.NumActions; i++)
                        {
                            TempNode.RegretSum[i] += N.RegretSum[i];
                            TempNode.StrategySum[i] += N.StrategySum[i];
                            TempNode.Count += N.Count;
                            TempNode.Realization += N.Realization;
                        }
                    }
                    else
                    {
                        NodeMap.Add(N.InfoSet, N);
                    }
                }
            }

            return NodeMap;
        }
    **/
    }
}
