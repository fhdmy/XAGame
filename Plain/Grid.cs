using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XAGame
{
	public enum GridType
	{
		grass,
		plain,
		rock1,
		rock2,
		water1,
		water2,
		water3
	}

	[RequireComponent(typeof(MeshCollider))]
	public class Grid : MonoBehaviour
	{
		public List<GameObject> gridGameObjects;
		public GridType gridType;
		public Grain grain;

		//AI factors
		public int buildFactor;
		public int gridGroup;

		public void ShowAddModel(bool showAddModel)
		{
			if (showAddModel)
			{
				transform.GetChild(0).gameObject.SetActive(true);
			}
			else
			{
				transform.GetChild(0).gameObject.SetActive(false);
			}
		}
	}
}

