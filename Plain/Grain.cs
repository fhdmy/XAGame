using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XAGame
{
	public class Grain : MonoBehaviour
	{
		public int resource;

		public int GainPerRound()
		{
			int e = 10;
			int rtn = ReduceResource(e);
			if (rtn > 0)
				return e;
			else
				return -rtn;
		}

		public int AddResource(int e)
		{
			resource += e;
			return resource;
		}

		public int ReduceResource(int e)
		{
			if (resource <= e)
			{
				Destroy(gameObject,0.5f);
				return -resource;
			}
			else
			{
				resource -= e;
				return resource;
			}
		}
	}
}

