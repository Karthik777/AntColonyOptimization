using System;

namespace ACO
{
	class AntColonyProgram
	{
		static Random random = new Random(0);
		static int alpha = 1;
		static int beta = 1;
		static double rho = 0.01;
		static double Q = 2.0;

		static void Main(string[] args)
		{
			try
			{
				Console.WriteLine("\nBegin Ant Colony Optimization demo\n");
				int numCities = 60;
				int numAnts = 4;
				int maxTime = 1000;

				int[][] dists = MakeGraphDistances(numCities);
				int[][] ants = InitAnts(numAnts, numCities); 

				int[] bestTrail = BestTrail(ants, dists);
				double bestLength = Length(bestTrail, dists);

				double[][] pheromones = InitPheromones(numCities);

				int time = 0;
				while (time < maxTime)
				{
					UpdateAnts(ants, pheromones, dists);
					UpdatePheromones(pheromones, ants, dists);

					int[] currBestTrail = BestTrail(ants, dists);
					double currBestLength = Length(currBestTrail, dists);
					if (currBestLength < bestLength)
					{
						bestLength = currBestLength;
						bestTrail = currBestTrail;
					}
					++time;
				}

				Console.WriteLine("\nBest trail found:");
				display(bestTrail);
				Console.WriteLine("\nLength of best trail found: " +
					bestLength.ToString("F1"));

				Console.WriteLine("\nEnd Ant Colony Optimization demo\n");
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
			}
		} // Main

		static int[][] MakeGraphDistances(int numCities)
		{
			int[][] dists = new int[numCities][];
			for (int i = 0; i < dists.Length; ++i)
				dists[i] = new int[numCities];
			for (int i = 0; i < numCities; ++i)
				for (int j = i + 1; j < numCities; ++j) {
					int d = random.Next(1, 9); // [1,8]
					dists[i][j] = d; dists[j][i] = d;
				}
			return dists;
		}

		static double Distance(int cityX, int cityY, int[][] dists)
		{
			return dists[cityX][cityY];
		}

		static int[][] InitAnts(int numAnts, int numCities)
		{
			int[][] ants = new int[numAnts][];
			for (int k = 0; k < numAnts; ++k) {
				int start = random.Next(0, numCities);
				ants[k] = RandomTrail(start, numCities);
			}
			return ants;
		}

		static int[] RandomTrail(int start, int numCities)
		{
			int[] trail = new int[numCities];
			for (int i = 0; i < numCities; ++i) { trail[i] = i; }

			for (int i = 0; i < numCities; ++i) {
				int r = random.Next(i, numCities);
				int tmp = trail[r]; trail[r] = trail[i]; trail[i] = tmp;
			}

			int idx = IndexOfTarget(trail, start);
			int temp = trail[0]; trail[0] = trail[idx]; trail[idx] = temp;

			return trail;
		}

		static double[][] InitPheromones(int numCities)
		{
			double[][] pheromones = new double[numCities][];
			for (int i = 0; i < numCities; ++i)
				pheromones[i] = new double[numCities];
			for (int i = 0; i < pheromones.Length; ++i)
				for (int j = 0; j < pheromones[i].Length; ++j)
					pheromones[i][j] = 0.01;
			return pheromones;
		}

		static int IndexOfTarget(int[] trail, int start)
		{
			for (int i = 0; i < trail.Length - 1; i++) {
				if (trail [i] == start)
					return i;
				}
			return 0;
		}

		static void UpdateAnts(int[][] ants, double[][] pheromones, int[][] dists)
		{
			int numCities = pheromones.Length; 
			for (int k = 0; k < ants.Length; ++k) {
				int start = random.Next(0, numCities);
				int[] newTrail = BuildTrail(k, start, pheromones, dists);
				ants[k] = newTrail;
			}
		}

		static int[] BuildTrail(int k, int start, double[][] pheromones,
			int[][] dists)
		{
			int numCities = pheromones.Length;
			int[] trail = new int[numCities];
			bool[] visited = new bool[numCities];
			trail[0] = start;
			visited[start] = true;
			for (int i = 0; i < numCities - 1; ++i) {
				int cityX = trail[i];
				int next = NextCity(k, cityX, visited, pheromones, dists);
				trail[i + 1] = next;
				visited[next] = true;
			}
			return trail;
		}

		static int NextCity(int k, int cityX, bool[] visited,
			double[][] pheromones, int[][] dists)
		{
			double[] probs = MoveProbs(k, cityX, visited, pheromones, dists);

			double[] cumul = new double[probs.Length + 1];
			for (int i = 0; i < probs.Length; ++i)
				cumul[i + 1] = cumul[i] + probs[i];

			double p = random.NextDouble();

			for (int i = 0; i < cumul.Length - 1; ++i)
				if (p >= cumul[i] && p < cumul[i + 1])
					return i;
			throw new Exception("Failure to return valid city in NextCity");
		}

		static double[] MoveProbs(int k, int cityX, bool[] visited,
			double[][] pheromones, int[][] dists)
		{
			int numCities = pheromones.Length;
			double[] taueta = new double[numCities];
			double sum = 0.0;
			for (int i = 0; i < taueta.Length; ++i) {
				if (i == cityX)
					taueta[i] = 0.0; // Prob of moving to self is zero
				else if (visited[i] == true)
					taueta[i] = 0.0; // Prob of moving to a visited node is zero
				else {
					taueta[i] = Math.Pow(pheromones[cityX][i], alpha) *
						Math.Pow((1.0 / Distance(cityX, i, dists)), beta);
					if (taueta[i] < 0.0001)
						taueta[i] = 0.0001;
					else if (taueta[i] > (double.MaxValue / (numCities * 100)))
						taueta[i] = double.MaxValue / (numCities * 100);
				}
				sum += taueta[i];
			}

			double[] probs = new double[numCities];
			for (int i = 0; i < probs.Length; ++i)
				probs[i] = taueta[i] / sum;
			return probs;
		}
		public static double Length(int[] trail, int[][] dist)
		{
			double result = 0.0;
			for(int i=0; i< trail.Length-2;i++)
			{result += Distance(trail[i], trail[i+1], dist);
			}
			return result;
		}
		public static bool EdgeInTrail(int cityx, int cityY, int[] trail)
		{
			int lastIndex = trail.Length - 1;
			int idx = IndexOfTarget (trail, cityx);
			if (idx == 0 && trail [1] == cityY)
				return true;
			else if (idx == 0 && trail [lastIndex] == cityY)
				return true;
			else if (idx == 0)
				return false;
			else if (idx == lastIndex && trail [lastIndex - 1] == cityY)
				return true;
			else if (idx == lastIndex && trail [0] == cityY)
				return true;
			else if (idx == lastIndex)
				return false;
			else if (trail [idx - 1] == cityY)
				return true;
			else if (trail [idx + 1] == cityY)
				return true;
			else
				return true;
		}




		public static void UpdatePheromones(double [] [] pheromones,int [] [] ants, int [] [] dist ){
			for (int i = 0; i < pheromones.Length - 1; i++)
				for (int j = i + 1; j < pheromones [i].Length - 1; j++)
					for (int k = 0; k < ants.Length - 1; k++) {
						double length = Length (ants [k], dist);
						double decrease = (1.0 - rho) * (pheromones [i] [j]);
						double increase = 0.0;
						if(EdgeInTrail(i,j,ants[k]) == true)
							increase = (Q / length);
						pheromones [i] [j] = decrease + increase;
						if (pheromones [i] [j] < 0.0001) 
							pheromones [i] [j] = 0.0001;
						else if (pheromones [i][j] > 100000.0)
							pheromones [i][j] = 100000.0;
						pheromones [j] [i] = pheromones [j] [i];
					}
		}

		public static int[] BestTrail(int[][] ants, int[][] dist)
		{
			double bestLength = Length (ants [0], dist);
			int idxBestLength = 0;
			for (int k = 1; k < ants.Length - 1; k++) {
				double len = Length (ants [k], dist);
				if (len < bestLength) {
				}
				bestLength = len;
				idxBestLength = k;
			}
			int numCities = ants [0].Length;
			int[] tempbesttrail = new int[numCities - 1];
			tempbesttrail = ants[idxBestLength];
			return tempbesttrail;
		}

		public static void display(int[] trail)
		{
			for (int i = 0; i <= trail.Length - 1; i++) {
				Console.WriteLine (trail [i] + " ");

			}
			Console.WriteLine ("");

		}
	}
}
