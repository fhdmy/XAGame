using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XAGame
{
	public class HumPlayer : Player
	{
		private UIManager uiManager;

		private GameObject prepareToAddUnit;
		private GameObject prepareToAddBuilding;

		void Start()
		{
			selectedPool = new List<GameObject>();
			timerManager = GameObject.Find("TimerManager").GetComponent<TimerManager>();
			uiManager = GameObject.Find("UIManager").GetComponent<UIManager>();

			resource = new Resource();
			technology = new Technology();
			resource.AddResource(100);
			uiManager.UpdateGoldText(resource.gold);
			technology.player = this;

			canAddGrids = new List<Grid>();
		}

		public override void DecideMove(Vector3 mousePosition)
		{
			//移动
			//如果不是单位则不需要移动
			if (selectedPool.Count == 1 && selectedPool[0].tag != "Unit")
				return;
			Ray ray = Camera.main.ScreenPointToRay(mousePosition);  //摄像机需要设置MainCamera的Tag这里才能找到
			RaycastHit hitInfo;
			if (Physics.Raycast(ray, out hitInfo, Mathf.Infinity, LayerMask.GetMask("Terrain")))
			{
				foreach (GameObject g in selectedPool)
				{
					if(g!=null)
						g.GetComponent<Unit>().SetMoveDestination(hitInfo.point);
				}
				GameObject Instance = Instantiate(targetPosModel);
				Instance.transform.position = hitInfo.point + hitInfo.normal * 0.01f;
				Destroy(Instance, 1.5f);
			}
		}

		public override void NewSelected(List<GameObject> g)
		{
			//退出选中
			foreach (GameObject oldg in selectedPool)
			{
				switch (oldg.tag)
				{
					case "Unit":
						oldg.GetComponent<Unit>().ExitSelected();
						uiManager.ShowUnitModeBtn(false,g);
						break;
					case "Building":
						oldg.GetComponent<Building>().ExitSelected();
						break;
				}
			}
			selectedPool.Clear();
			ClearAdd();
			ClearAddGrids();
			if (g == null)
			{
				uiManager.SetShowBtnNull();
				return;
			}
	
			foreach(GameObject newg in g)
			{
				//新增选中
				selectedPool.Add(newg);
				switch (newg.tag)
				{
					case "Unit":
						newg.GetComponent<Unit>().IsSelected();
						uiManager.ShowUnitModeBtn(true,g);
						break;
					case "Building":
						newg.GetComponent<Building>().IsSelected();
						break;
				}
			}
		}

		public override void GetMouseRight(Vector3 mousePosition)
		{
			if (timerManager.presentStage == GameStage.decisionStage)
			{
				if (selectedPool.Count > 0 && prepareToAddUnit == null && prepareToAddBuilding == null)
				{
					DecideMove(mousePosition);
				}
				if (prepareToAddUnit != null)
				{
					AddUnit(mousePosition);
				}
				else if(prepareToAddBuilding != null)
				{
					AddBuilding(mousePosition);
				}
			}
			else
			{
				uiManager.AlertText(AlertTextType.rightMouseTimeOut);
			}
		}

		private void ClearAdd()
		{
			prepareToAddUnit = null;
			prepareToAddBuilding = null;
		}

		public override void GetAddUnit(GameObject g)
		{
			if(timerManager.presentStage!= GameStage.decisionStage)
			{
				uiManager.AlertText(AlertTextType.rightMouseTimeOut);
				return;
			}
			ClearAdd();
			ClearAddGrids();
			prepareToAddUnit = g;

			//add unit grids
			List<Grid> addGrids = new List<Grid>();
			foreach (Building b in buildings)
			{
				if (b.buildStage.Equals(BuildStage.finish))
				{
					List<Grid> temp = b.GetAddUnitGrids(g.GetComponent<Unit>().unitType);
					foreach (Grid gs in temp)
					{
						addGrids.Add(gs);
					}
				}	
			}
			//show add unit ui
			foreach (Grid ggs in addGrids)
			{
				canAddGrids.Add(ggs);
				ggs.ShowAddModel(true);
			}
		}

		public override void GetAddBuilding(GameObject g)
		{
			if (timerManager.presentStage != GameStage.decisionStage)
			{
				uiManager.AlertText(AlertTextType.rightMouseTimeOut);
				return;
			}
			ClearAdd();
			ClearAddGrids();
			prepareToAddBuilding = g;

			//add building grids
			List<Grid> addGrids = new List<Grid>();
			foreach (Unit u in units)
			{
				if (u.unitType == UnitType.peasant)
				{
					List<Grid> temp = u.GetAddBuildingGrids(g.GetComponent<Building>().buildingType);
					foreach(Grid gs in temp)
					{
						addGrids.Add(gs);
					}
				}
			}
			//show add building ui
			foreach(Grid ggs in addGrids)
			{
				canAddGrids.Add(ggs);
				ggs.ShowAddModel(true);
			}
		}

		private void AddUnit(Vector3 mousePosition)
		{
			Ray ray = Camera.main.ScreenPointToRay(mousePosition);  //摄像机需要设置MainCamera的Tag这里才能找到
			RaycastHit hitInfo;
			if (Physics.Raycast(ray, out hitInfo, 200))
			{
				if (hitInfo.collider.tag == "Terrain" && GetViewableArea().Contains(hitInfo.collider.gameObject))
				{
					//没钱造
					if (prepareToAddUnit.GetComponent<Unit>().cost>resource.gold)
					{
						//提示信息
						uiManager.AlertText(AlertTextType.haveNoMoneyAdd);
						return;
					}

					//需要在生产/建造范围内
					if (!canAddGrids.Contains(hitInfo.collider.gameObject.GetComponent<Grid>()))
					{
						//提示信息
						uiManager.AlertText(AlertTextType.addOutOfArea);
						return;
					}

					Building tempB=null;
					//建筑的每回合产生unit自增
					foreach (Building bu in buildings)
					{
						if (bu.GetAddUnitGrids(prepareToAddUnit.GetComponent<Unit>().unitType).Contains(hitInfo.collider.gameObject.GetComponent<Grid>()))
						{
							tempB = bu;
							break;
						}
					}

					GameObject instance = Instantiate(prepareToAddUnit);
					instance.transform.parent = GameObject.Find("Units").transform;
					instance.transform.position = new Vector3(hitInfo.collider.transform.position.x, hitInfo.point.y, hitInfo.collider.transform.position.z);
					instance.name = prepareToAddUnit.name + " - " + GameObject.Find("Units").transform.childCount + "(" + playerType.ToString() + ")";

					instance.GetComponent<Unit>().player = this;
					instance.GetComponent<Unit>().targetPosModel = targetPosModel;
					units.Add(instance.GetComponent<Unit>());
					resource.ReduceResource(prepareToAddUnit.GetComponent<Unit>().cost);
					uiManager.UpdateGoldText(resource.gold);

					hitInfo.collider.gameObject.GetComponent<Grid>().ShowAddModel(false);
					canAddGrids.Remove(hitInfo.collider.gameObject.GetComponent<Grid>());

					//攻防
					for(int j=0;j<(int)technology.attackLevel;j++){
						instance.GetComponent<Unit>().attack+=technology.improveValue[j];
					}
					for(int k=0;k<(int)technology.armorLevel;k++){
						instance.GetComponent<Unit>().armor+=technology.improveValue[k];
					}

					//建筑的每回合产生unit自增
					if (instance != null && tempB!=null)
					{
						tempB.presentAddUnitNum += 1;
						if (tempB.presentAddUnitNum == tempB.addUnitNum)
						{
							foreach (Grid gtempb in tempB.addUnitGrids)
							{
								gtempb.ShowAddModel(false);
								canAddGrids.Remove(gtempb);
							}
						}
					}

					//创建信息条目
					Inform i = new Inform();
					i.battleInformationType = BattleInformationType.AddUnit;
					i.informs = new List<string>();
					string presentTime = Camera.main.GetComponent<GameManager>().GetPresentTime();
					i.informs.Add(presentTime);
					i.informs.Add(playerType.ToString());
					i.informs.Add(instance.name);
					uiManager.AddBattleInformation(i);
				}
				
			}
		}

		private void AddBuilding(Vector3 mousePosition)
		{
			Ray ray = Camera.main.ScreenPointToRay(mousePosition);  //摄像机需要设置MainCamera的Tag这里才能找到
			RaycastHit hitInfo;
			if (Physics.Raycast(ray, out hitInfo, 200))
			{
				if (hitInfo.collider.tag == "Terrain" && GetViewableArea().Contains(hitInfo.collider.gameObject))
				{
					//没钱造
					if (prepareToAddBuilding.GetComponent<Building>().cost > resource.gold)
					{
						//提示信息
						uiManager.AlertText(AlertTextType.haveNoMoneyAdd);
						return;
					}

					//需要在生产/建造范围内
					if (!canAddGrids.Contains(hitInfo.collider.gameObject.GetComponent<Grid>()))
					{
						//提示信息
						uiManager.AlertText(AlertTextType.addOutOfArea);
						return;
					}

					GameObject instance = Instantiate(prepareToAddBuilding);
					instance.transform.parent = GameObject.Find("Buildings").transform;
					instance.transform.position = new Vector3(hitInfo.collider.transform.position.x,hitInfo.point.y, hitInfo.collider.transform.position.z);
					instance.name = prepareToAddBuilding.name + " - " + GameObject.Find("Buildings").transform.childCount + "(" + playerType.ToString() + ")";

					instance.GetComponent<Building>().player = this;
					instance.GetComponent<Building>().grid = hitInfo.collider.gameObject.GetComponent<Grid>();
					hitInfo.collider.gameObject.GetComponent<Grid>().gridGameObjects.Add(instance);
					instance.GetComponent<Building>().ClearGrain();
					hitInfo.collider.gameObject.GetComponent<Grid>().ShowAddModel(false);
					canAddGrids.Remove(hitInfo.collider.gameObject.GetComponent<Grid>());
					buildings.Add(instance.GetComponent<Building>());
					resource.ReduceResource(prepareToAddBuilding.GetComponent<Building>().cost);
					uiManager.UpdateGoldText(resource.gold);

					//创建信息条目
					Inform i = new Inform();
					i.battleInformationType = BattleInformationType.AddBuilding;
					i.informs = new List<string>();
					string presentTime = Camera.main.GetComponent<GameManager>().GetPresentTime();
					i.informs.Add(presentTime);
					i.informs.Add(playerType.ToString());
					i.informs.Add(instance.name);
					uiManager.AddBattleInformation(i);
				}
			}
		}

		public override void UpdateViewableGrids()
		{
			GameObject[] allGrids = GameObject.FindGameObjectsWithTag("Terrain");
			List<GameObject> ag = GetViewableArea();
			for (int i = 0; i < allGrids.Length; i++)
			{
				if (!ag.Contains(allGrids[i]))
				{
					allGrids[i].transform.GetChild(1).gameObject.SetActive(true);
					foreach (GameObject obj in allGrids[i].GetComponent<Grid>().gridGameObjects)
					{
						if (obj.tag == "Unit")
						{
							if (!units.Contains(obj.GetComponent<Unit>()))
							{
								SkinnedMeshRenderer[] marr = obj.GetComponentsInChildren<SkinnedMeshRenderer>(true);
								foreach (SkinnedMeshRenderer m in marr)
								{
									m.enabled = false;
								}
								MeshRenderer[] mrr = obj.GetComponentsInChildren<MeshRenderer>(true);
								foreach (MeshRenderer m in mrr)
								{
									m.enabled = false;
								}
							}
						}
						else if (obj.tag == "Building")
						{
							if (!buildings.Contains(obj.GetComponent<Building>()))
							{
								SkinnedMeshRenderer[] marr = obj.GetComponentsInChildren<SkinnedMeshRenderer>(true);
								foreach (SkinnedMeshRenderer m in marr)
								{
									m.enabled = false;
								}
								MeshRenderer[] mrr = obj.GetComponentsInChildren<MeshRenderer>(true);
								foreach (MeshRenderer m in mrr)
								{
									m.enabled = false;
								}
							}
						}
					}
					//grain
					if (allGrids[i].GetComponent<Grid>().grain != null)
					{
						GameObject grainObj = allGrids[i].GetComponent<Grid>().grain.gameObject;
						MeshRenderer[] mrs = grainObj.GetComponentsInChildren<MeshRenderer>(true);
						foreach (MeshRenderer m in mrs)
						{
							m.enabled = false;
						}
					}
				}
				else
				{
					allGrids[i].transform.GetChild(1).gameObject.SetActive(false);
					foreach (GameObject obj in allGrids[i].GetComponent<Grid>().gridGameObjects)
					{
						if(obj.tag=="Unit")
						{
							if(!units.Contains(obj.GetComponent<Unit>()) && obj.GetComponent<Unit>().unitType==UnitType.assassin)
							{
								if(obj.GetComponent<Assassin>().isHidden)
									continue;
							}
						}
						SkinnedMeshRenderer[] marr = obj.GetComponentsInChildren<SkinnedMeshRenderer>();
						foreach (SkinnedMeshRenderer m in marr)
						{
							m.enabled = true;
						}
						MeshRenderer[] mrr = obj.GetComponentsInChildren<MeshRenderer>();
						foreach (MeshRenderer m in mrr)
						{
							m.enabled = true;
						}
					}
					//grain
					if (allGrids[i].GetComponent<Grid>().grain != null)
					{
						GameObject grainObj = allGrids[i].GetComponent<Grid>().grain.gameObject;
						MeshRenderer[] mrs = grainObj.GetComponentsInChildren<MeshRenderer>(false);
						foreach (MeshRenderer m in mrs)
						{
							m.enabled = true;
						}
					}
				}
			}
		}

		private List<GameObject> GetViewableArea()
		{
			List<GameObject> temp = new List<GameObject>();
			foreach (Unit u in units)
			{
				foreach (Grid g in u.aroundGrids)
				{
					if (!temp.Contains(g.gameObject))
						temp.Add(g.gameObject);
				}
			}
			foreach (Building b in buildings)
			{
				if (b.buildStage.Equals(BuildStage.finish))
				{
					foreach (Grid bag in b.aroundGrids)
					{
						if (!temp.Contains(bag.gameObject))
							temp.Add(bag.gameObject);
					}
				}
				else
					temp.Add(b.gameObject);
			}
			return temp;
		}

		public override void NewRound()
		{
			int oldGold = resource.gold;
			resource.AddPerRound();
			int rtn=technology.NewRoundUpgradeGF();
			switch(rtn)
			{
				case 0:
					break;
				//attack
				case 1:
					//创建信息条目
					Inform i1 = new Inform();
					i1.battleInformationType = BattleInformationType.ImprovedGF;
					i1.informs = new List<string>();
					i1.informs.Add(Camera.main.GetComponent<GameManager>().GetPresentTime());
					i1.informs.Add(playerType.ToString());
					i1.informs.Add("attack");
					i1.informs.Add(System.Enum.GetName(typeof(GFLevel), (int)technology.attackLevel).ToString());
					uiManager.AddBattleInformation(i1);
					break;
				//armor
				case 2:
					//创建信息条目
					Inform i2 = new Inform();
					i2.battleInformationType = BattleInformationType.ImprovedGF;
					i2.informs = new List<string>();
					i2.informs.Add(Camera.main.GetComponent<GameManager>().GetPresentTime());
					i2.informs.Add(playerType.ToString());
					i2.informs.Add("armor");
					i2.informs.Add(System.Enum.GetName(typeof(GFLevel), (int)technology.armorLevel).ToString());
					uiManager.AddBattleInformation(i2);
					break;
				//attack and armor
				case 3:
					//创建信息条目
					Inform i3 = new Inform();
					i3.battleInformationType = BattleInformationType.ImprovedGF;
					i3.informs = new List<string>();
					i3.informs.Add(Camera.main.GetComponent<GameManager>().GetPresentTime());
					i3.informs.Add(playerType.ToString());
					i3.informs.Add("attack");
					i3.informs.Add(System.Enum.GetName(typeof(GFLevel), (int)technology.attackLevel).ToString());
					uiManager.AddBattleInformation(i3);
					Inform i4 = new Inform();
					i4.battleInformationType = BattleInformationType.ImprovedGF;
					i4.informs = new List<string>();
					i4.informs.Add(Camera.main.GetComponent<GameManager>().GetPresentTime());
					i4.informs.Add(playerType.ToString());
					i4.informs.Add("armor");
					i4.informs.Add(System.Enum.GetName(typeof(GFLevel), (int)technology.armorLevel).ToString());
					uiManager.AddBattleInformation(i4);
					break;
			}

			foreach (Unit u in units)
			{
				u.FlashState();
				switch(rtn)
				{
					case 0:
						break;
					//attack
					case 1:
						u.attack+=technology.improveValue[((int)technology.attackLevel)-1];
						break;
					//armor
					case 2:
						u.armor+=technology.improveValue[((int)technology.armorLevel)-1];
						break;
					//attack and armor
					case 3:
						u.attack+=technology.improveValue[((int)technology.attackLevel)-1];
						u.armor+=technology.improveValue[((int)technology.armorLevel)-1];
						break;
				}
			}
			foreach (Building b in buildings)
			{
				b.FlashState();
				if (b.buildingType==BuildingType.farm && b.buildStage.Equals(BuildStage.finish))
				{
					if (b.grid.grain != null)
						resource.AddResource(b.grid.grain.GainPerRound());
				}
			}
			//创建信息条目
			Inform i = new Inform();
			i.battleInformationType = BattleInformationType.NewRound;
			i.informs = new List<string>();
			string presentTime = Camera.main.GetComponent<GameManager>().GetPresentTime();
			i.informs.Add(presentTime);
			i.informs.Add(playerType.ToString());
			i.informs.Add(timerManager.presentRound.ToString());
			i.informs.Add((resource.gold-oldGold).ToString());
			uiManager.AddBattleInformation(i);

			uiManager.UpdateGoldText(resource.gold);
		}

		public override void ClearAddGrids()
		{
			foreach(Grid g in canAddGrids)
			{
				g.ShowAddModel(false);
			}
			canAddGrids.Clear();
		}

		public override void UpgradeGF(int index) //index=0: attack index=1: armor
		{
			int rtn = technology.TestUpgradeGF(index);
			//提示信息：攻防已经最高
			if(rtn==-1)
			{
				uiManager.AlertText(AlertTextType.gfHightest);
				return;
			}
			else if(rtn==-2)
			{
				uiManager.AlertText(AlertTextType.improvingGF);
				return;
			}
			else if(rtn==0)
			{
				int gold=technology.GetImproveGold(index);
				if(gold <= resource.gold)
				{
					technology.StartUpgradeGF(index);
					resource.ReduceResource(gold);
					uiManager.UpdateGoldText(resource.gold);

					//创建信息条目
					Inform i = new Inform();
					i.battleInformationType = BattleInformationType.ImproveGF;
					i.informs = new List<string>();
					string presentTime = Camera.main.GetComponent<GameManager>().GetPresentTime();
					i.informs.Add(presentTime);
					i.informs.Add(playerType.ToString());
					string st=index==0?"attack":"armor";
					i.informs.Add(st);
					if(index==0)
						i.informs.Add(System.Enum.GetName(typeof(GFLevel), (int)technology.attackLevel+1).ToString());
					else
						i.informs.Add(System.Enum.GetName(typeof(GFLevel), (int)technology.armorLevel+1).ToString());
					i.informs.Add(gold.ToString());
					uiManager.AddBattleInformation(i);
				}
				else
					uiManager.AlertText(AlertTextType.haveNoMoneyAdd);
			}
		}
	}
}

