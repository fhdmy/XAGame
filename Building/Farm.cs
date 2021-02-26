using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XAGame
{
	public class Farm : Building
	{
		public override void ClearGrain()
		{
			if(grid.grain!=null)
				grid.grain.gameObject.SetActive(false);
		}

		protected override void DestroySelf()
		{
			player.buildings.Remove(gameObject.GetComponent<Building>());
			player.selectedPool.Remove(gameObject);
			grid.gridGameObjects.Remove(gameObject);
			if (grid.grain!=null)
				grid.grain.gameObject.SetActive(true);

			//创建信息条目
			Inform i = new Inform();
			i.battleInformationType = BattleInformationType.Die;
			i.informs = new List<string>();
			string presentTime = Camera.main.GetComponent<GameManager>().GetPresentTime();
			i.informs.Add(presentTime);
			i.informs.Add(player.playerType.ToString());
			i.informs.Add(gameObject.name);
			uiManager.AddBattleInformation(i);

			Destroy(gameObject);
		}
	}
}
