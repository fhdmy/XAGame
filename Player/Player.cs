using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XAGame
{
	public enum PlayerType
	{
		playerBlue,
		playerRed
	}

	public class Player : MonoBehaviour
	{
		public PlayerType playerType;

		public List<Unit> units;

		public List<Building> buildings;

		public GameObject targetPosModel;

		public TimerManager timerManager;

		public List<GameObject> selectedPool;

		public Resource resource;
		public Technology technology;

		protected List<Grid> canAddGrids;

		public virtual void DecideMove(Vector3 mousePosition)
		{

		}

		public virtual void NewSelected(List<GameObject> g)
		{

		}

		public virtual void NewRound()
		{
			
		}

		public void Execute()
		{
			foreach (Unit u in units)
			{
				//u.SetMovedUI(false);
				u.SetMoveToTarget();
			}
		}

		public void GetDecisionInform()
		{
			foreach (Unit u in units)
			{
				u.GetMoveTargetPos();
			}
		}

		public virtual void GetMouseRight(Vector3 mousePosition)
		{

		}

		public virtual void GetAddUnit(GameObject g)
		{

		}

		public virtual void GetAddBuilding(GameObject g)
		{

		}

		public virtual void UpdateViewableGrids()
		{

		}

		public virtual void ClearAddGrids()
		{

		}

		public virtual void UpgradeGF(int index) //index=0: attack index=1: armor
		{
			int rtn = technology.TestUpgradeGF(index);
			if(rtn==0)
			{
				int gold=technology.GetImproveGold(index);
				if(gold <= resource.gold)
				{
					technology.StartUpgradeGF(index);
					resource.ReduceResource(gold);
				}
			}
		}

		public virtual void ChangeUnitMode(int i)  //i=0：taskFirst  i=1: balanceMode  i=2: killingFirst
		{
			foreach(GameObject g in selectedPool)
			{
				switch(i)
				{
					case 0:
						g.GetComponent<Unit>().unitMode=UnitMode.taskFirst;
						break;
					case 1:
						g.GetComponent<Unit>().unitMode=UnitMode.balanceMode;
						break;
					case 2:
						g.GetComponent<Unit>().unitMode=UnitMode.killingFirst;
						break;
				}
			}
		}

		public virtual void AddEnemyGameObject(GameObject g)
		{

		}
	}
}

