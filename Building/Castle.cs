using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XAGame
{
	public class Castle : Building
	{
		protected override void Start()
		{
			uiManager = GameObject.Find("UIManager").GetComponent<UIManager>();
			outline = gameObject.GetComponent<Highline>();

			gameObject.GetComponent<CapsuleCollider>().radius = viewDisance;
			outline.enabled = false;
			isSelected = false;

			ChangeModelColor();
			addUnitGrids = new List<Grid>();

			presentBuildRound = 0;
		}
	}
}

