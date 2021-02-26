using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XAGame
{
	public class Resource
	{
		public int gold { get; private set; }

		public int perRoundGain = 30;

		public int AddResource(int e)
		{
			gold += e;
			return gold;
		}

		public int ReduceResource(int e)
		{
			gold -= e;
			return gold;
		}

		public int AddPerRound()
		{
			gold += perRoundGain;
			return gold;
		}
	}
}

