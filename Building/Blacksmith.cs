using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XAGame
{
	public class Blacksmith : Building
	{
		public override void IsSelected()
		{
			if (!isSelected)
			{
				outline.enabled = true;
				isSelected = true;
				if(buildStage.Equals(BuildStage.finish))
					uiManager.ShowBlacksmithBtn(true);
			}
		}

		public override void ExitSelected()
		{
			if (isSelected)
			{
				outline.enabled = false;
				isSelected = false;
				uiManager.ShowBlacksmithBtn(false);
			}
		}
	}
}
