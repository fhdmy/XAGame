using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XAGame
{
	public enum GFLevel
	{
		level0,     //g:+0attack f:+0armor
		level1,     //g:+2attack f:+2armor
		level2,     //g:+5attack f:+5armor
		level3      //g:+10attack f:+10armor
	}
	public class Technology
	{
		public Player player;

		public List<BuildingType> canAddBuildings;

		public GFLevel attackLevel;
		public GFLevel armorLevel;
		public int[] improveLevelRound;
		public int improvedAttackLevelRound;
		public int improvedArmorLevelRound;
		public int[] improveLevelGold;
		public int[] improveValue;

		public Technology()
		{
			canAddBuildings = new List<BuildingType>();
			canAddBuildings.Add(BuildingType.castle);
			canAddBuildings.Add(BuildingType.barracks);
			canAddBuildings.Add(BuildingType.farm);
			attackLevel=GFLevel.level0;
			armorLevel=GFLevel.level0;
			improveLevelRound = new int[3]{1,2,3};
			improvedAttackLevelRound=-1;
			improvedArmorLevelRound=-1;
			improveLevelGold = new int[3]{50,100,150};
			improveValue = new int[3]{2,3,5};
		}

		public void AddCanAddBuilding(BuildingType buildingType)
		{
			switch (buildingType)
			{
				case BuildingType.barracks:
					if(!canAddBuildings.Contains(BuildingType.archery))
						canAddBuildings.Add(BuildingType.archery);
					if (!canAddBuildings.Contains(BuildingType.tower))
						canAddBuildings.Add(BuildingType.tower);
					if (!canAddBuildings.Contains(BuildingType.temple))
						canAddBuildings.Add(BuildingType.temple);
					if (!canAddBuildings.Contains(BuildingType.blacksmith))
						canAddBuildings.Add(BuildingType.blacksmith);
					break;
				case BuildingType.blacksmith:
					if (!canAddBuildings.Contains(BuildingType.mageTower))
						canAddBuildings.Add(BuildingType.mageTower);
					break;
				case BuildingType.mageTower:
					if (!canAddBuildings.Contains(BuildingType.stables))
						canAddBuildings.Add(BuildingType.stables);
					break;
			}
		}

		public void UpdateAddBuilding()
		{
			List<BuildingType> temp = new List<BuildingType>();
			temp.Add(BuildingType.castle);
			temp.Add(BuildingType.barracks);
			temp.Add(BuildingType.farm);
			bool barrack = false;
			bool blacksmith = false;
			bool mageTower = false;
			foreach (Building b in player.buildings)
			{
				if (b.buildingType.Equals(BuildingType.barracks))
				{
					barrack = true;
				}
				else if (b.buildingType.Equals(BuildingType.blacksmith))
				{
					blacksmith = true;
				}
				else if (b.buildingType.Equals(BuildingType.mageTower))
				{
					mageTower = true;
				}
			}
			if (barrack)
			{
				temp.Add(BuildingType.archery);
				temp.Add(BuildingType.tower);
				temp.Add(BuildingType.temple);
				temp.Add(BuildingType.blacksmith);
				if (blacksmith)
				{
					temp.Add(BuildingType.mageTower);
					if (mageTower)
					{
						temp.Add(BuildingType.stables);
					}
				}
			}
			canAddBuildings = temp;
		}

		public int TestUpgradeGF(int index) //index=0: attack index=1: armor
		{
			//attack
			if(index==0)
			{
				if(attackLevel.Equals(GFLevel.level3))
					return -1;
				if(improvedAttackLevelRound!=-1)
					return -2;
			}
			//armor
			else
			{
				if(armorLevel.Equals(GFLevel.level3))
					return -1;
				if(improvedArmorLevelRound!=-1)
					return -2;
			}
			return 0;
		}

		public int GetImproveGold(int index) //index=0: attack index=1: armor
		{
			//attack
			if(index==0)
			{
				return improveLevelGold[(int)attackLevel];
			}
			//armor
			else
			{
				return improveLevelGold[(int)attackLevel];
			}
		}

		public void StartUpgradeGF(int index) //index=0: attack index=1: armor
		{
			//attack
			if(index==0)
			{
				improvedAttackLevelRound=0;
			}
			//armor
			else
			{
				improvedArmorLevelRound=0;
			}
		}

		public int NewRoundUpgradeGF()
		{
			if(improvedAttackLevelRound!=-1)
			{
				improvedAttackLevelRound+=1;
			}
			if(improvedArmorLevelRound!=-1)
			{
				improvedArmorLevelRound+=1;
			}
			int atl=0;
			int arl=0;
			if(improvedAttackLevelRound==improveLevelRound[(int)attackLevel])
			{
				switch(attackLevel)
				{
					case GFLevel.level0:
						attackLevel=GFLevel.level1;
						break;
					case GFLevel.level1:
						attackLevel=GFLevel.level2;
						break;
					case GFLevel.level2:
						attackLevel=GFLevel.level3;
						break;
				}
				improvedAttackLevelRound=-1;
				atl=1;
			}
			if(improvedArmorLevelRound==improveLevelRound[(int)armorLevel])
			{
				switch(armorLevel)
				{
					case GFLevel.level0:
						armorLevel=GFLevel.level1;
						break;
					case GFLevel.level1:
						armorLevel=GFLevel.level2;
						break;
					case GFLevel.level2:
						armorLevel=GFLevel.level3;
						break;
				}
				improvedArmorLevelRound=-1;
				arl=2;
			}
			return arl+atl;
		}
    }
}
