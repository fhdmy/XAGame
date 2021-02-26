using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XAGame
{
	public struct GridGroup
	{
		public float battleFactor;
		public List<Grid> grids;
		public float scoutFactor;
		public int unvisitedRounds;
	}

	public class AIPlayer : Player
	{
		public List<Building> enemyBuildings;
		private List<Building> ViewableEnemyBuildings;
		private List<Building> newFindBuildings;   //用于记录新发现建筑费用的估算
		public List<Unit> enemyUnits; // private
		private List<Unit> ViewableEnemyUnits;
		private List<Unit> newFindUnits;    //用于记录新发现建筑费用的估算

		private int enemyRecource;

		private float scoutFactor;

		public List<GameObject> unitModel;
		public List<GameObject> buildingModel;

		public GameObject terrain;
		private List<Grid> grids;

		private GridGroup[] gridGroups;
		private List<Grid> grainGrids; 
		private int randomRound;

		// Start is called before the first frame update
		void Start()
		{
			timerManager = GameObject.Find("TimerManager").GetComponent<TimerManager>();

			resource = new Resource();
			technology = new Technology();
			resource.AddResource(100);
			technology.player = this;

			canAddGrids = new List<Grid>();
			
			ViewableEnemyBuildings=new List<Building>();
			newFindBuildings = new List<Building>();
			enemyUnits=new List<Unit>();
			ViewableEnemyUnits=new List<Unit>();
			newFindUnits=new List<Unit>();

			enemyRecource = 100;

			grids=new List<Grid>();
			gridGroups=new GridGroup[25];
			grainGrids=new List<Grid>();
			for(int i=0;i<gridGroups.Length;i++)
			{
				gridGroups[i].grids=new List<Grid>();
			}
			for(int i=0;i<terrain.transform.childCount;i++)
			{
				if(terrain.transform.GetChild(i).tag=="Terrain")
				{
					grids.Add(terrain.transform.GetChild(i).GetComponent<Grid>());
					gridGroups[terrain.transform.GetChild(i).GetComponent<Grid>().gridGroup].grids.Add(terrain.transform.GetChild(i).GetComponent<Grid>());
					gridGroups[terrain.transform.GetChild(i).GetComponent<Grid>().gridGroup].unvisitedRounds=0;
					if(terrain.transform.GetChild(i).GetComponent<Grid>().grain!=null)
					{
						grainGrids.Add(terrain.transform.GetChild(i).GetComponent<Grid>());
					}
				}	
			}
			randomRound=Random.Range(1,6);

			Invoke("StartDecide",1f);
		}

		public override void NewRound()
		{
			resource.AddPerRound();
			int rtn = technology.NewRoundUpgradeGF();
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
			Debug.Log("gold: "+resource.gold);

			//预估
			Estimate();
			Invoke("StartDecide",1f);
		}

		private void Estimate()
		{
			enemyRecource+=resource.perRoundGain; //每回合基础收入
			foreach(Building ebu in enemyBuildings)
			{
				if(ebu.buildingType==BuildingType.farm && ebu.buildStage.Equals(BuildStage.finish))
				{
					enemyRecource+=10;   //农场收入
				}
			}
			foreach(Building nbu in newFindBuildings)
			{
				if(nbu!=null)
				{
					enemyRecource-=nbu.cost;          //看到建筑费用
					enemyBuildings.Add(nbu);
				}	
			}
			newFindBuildings.Clear();
			foreach(Unit nu in newFindUnits)
			{
				if(nu==null)
					continue;
				enemyRecource-=nu.cost;           //看到单位费用
				foreach(Building prebu in nu.preBuildings)
				{
					bool flag = true;
					foreach(Building orbu in enemyBuildings)
					{
						if(orbu.buildingType.Equals(prebu.buildingType))
						{
							flag=false;
							break;
						}
					}
					if(flag)
					{
						enemyRecource-=prebu.cost;    //如果看到的单位的前置建筑不在敌方建筑列表内
						prebu.gameObject.transform.position=enemyBuildings[0].gameObject.transform.position;
						enemyBuildings.Add(prebu);
						Debug.Log("Add building: " + prebu);
					}
				}
				enemyUnits.Add(nu);
			}
			newFindUnits.Clear();
			Debug.Log("enemyRecource: "+enemyRecource);
		}

		public void StartDecide()
		{
			int k = CalK();
			Debug.Log("k: "+k);
			//如果余钱大于k，则认为敌方农场数+1
			if(enemyRecource>k)
			{
				Building temp =  buildingModel[1].GetComponent<Building>();
				enemyRecource-=temp.cost;
				temp.gameObject.transform.position=enemyBuildings[0].gameObject.transform.position;
				temp.buildStage=BuildStage.finish;
				enemyBuildings.Add(temp);
			}
			if(enemyRecource<0)
			{
				enemyRecource=0;
				Building tempbu =  buildingModel[1].GetComponent<Building>();
				tempbu.gameObject.transform.position=enemyBuildings[0].gameObject.transform.position;
				tempbu.buildStage=BuildStage.finish;
				enemyBuildings.Add(tempbu);
			}

			//农民数量小于px则先造农民
			int pnum=0;
			int px=timerManager.presentRound/10+2;
			foreach(Unit u in units)
			{
				if(u.unitType.Equals(UnitType.peasant))
				{
					pnum+=1;
				}
			}
			if(pnum<px && resource.gold>=unitModel[0].GetComponent<Unit>().cost)
			{
				Grid peasantGrid=null;
				Building peasantCastle=null;
				foreach(Building b in buildings)
				{
					if(b.buildingType.Equals(BuildingType.castle) && b.buildStage.Equals(BuildStage.finish))
					{
						List<Grid> castleGrids = b.GetAddUnitGrids(UnitType.peasant);
						float minDis=float.PositiveInfinity;
						foreach(Grid cg in castleGrids)
						{
							float dis = Vector3.Distance(cg.transform.position,enemyBuildings[0].gameObject.transform.position);
							if(dis<minDis)
							{
								minDis=dis;
								peasantGrid=cg;
							}
						}
						peasantCastle=b;
						break;
					}
				}
				if(peasantGrid!=null && peasantCastle!=null)
				{
					AddUnit(peasantGrid,0,peasantCastle);
				}
			}

			List<Peasant> peasants=new List<Peasant>();
			foreach(Unit u in units)
			{
				if(u.unitType.Equals(UnitType.peasant))
					peasants.Add((Peasant)u);
			}
			//尽量完成农民的任务
			foreach(Peasant pp in peasants)
			{
				if(pp.peasantTask.Count>0)
				{
					if(Vector3.Distance(pp.gameObject.transform.position,pp.peasantTask[0].targetGrid.gameObject.transform.position)<=3f)
					{
						if(resource.gold>=buildingModel[1].GetComponent<Building>().cost && pp.peasantTask[0].targetGrid.gridGameObjects.Count==0)
						{
							AddBuilding(pp.peasantTask[0].targetGrid,pp.peasantTask[0].buildingIndex);
							pp.peasantTask.Remove(pp.peasantTask[0]);
						}
					}
				}	
			}

			CalGridFactor();

			float battleFactor=CalBattleFactor(units,enemyUnits);
			float marcoBattleFactor=battleFactor*scoutFactor;
			Debug.Log("marcoBattleFactor: "+marcoBattleFactor);

			//随机回合造兵营
			if(timerManager.presentRound==randomRound)
			{
				bool flag = true;
				foreach(Building rb in buildings)
				{
					if(rb.buildingType.Equals(BuildingType.barracks))
					{
						flag=false;
						break;
					}
				}
				//如果有出兵建筑就不造了，不然造出兵建筑
				if(flag)
				{
					ChoosePeasantBuild(2,peasants);
				}
			}
			
			int random=Random.Range(0,2);
			bool randomFlag=random==0?true:false;

			//建造建筑/部队
			if(marcoBattleFactor>0.8f || randomFlag)
			{
				random=Random.Range(0,10);
				// 1/10概率先建造出兵建筑/科技建筑
				if(random==0)
				{

				}
				//不然先造兵，有钱再造建筑
				else
				{
					//从最新建造的建筑开始
					List<Grid> addedGrid=new List<Grid>();
					for(int i=buildings.Count-1;i>=0;i--)
					{
						if(buildings[i].buildStage.Equals(BuildStage.finish) && !buildings[i].buildingType.Equals(BuildingType.castle) && buildings[i].relateUnitType.Count>0)
						{
							bool loopFlag=true;
							while(loopFlag)
							{
								if(buildings[i].presentAddUnitNum >= buildings[i].addUnitNum)
									loopFlag=false;
								int r=Random.Range(0,buildings[i].relateUnitType.Count-1);
								List<Grid> addGrids = buildings[i].GetAddUnitGrids(buildings[i].relateUnitType[r]);
								float minDis=float.PositiveInfinity;
								Grid addGrid=null;
								foreach(Grid cg in addGrids)
								{
									if(addedGrid.Contains(cg))
										continue;
									float dis = Vector3.Distance(cg.transform.position,enemyBuildings[0].gameObject.transform.position);
									if(dis<minDis)
									{
										minDis=dis;
										addGrid=cg;
									}
								}
								int x=-1;
								for(int j=0;j<unitModel.Count;j++)
								{
									if(unitModel[j].GetComponent<Unit>().unitType==buildings[i].relateUnitType[r])
									{
										x=j;
										break;
									}
								}
								if(resource.gold>=unitModel[x].GetComponent<Unit>().cost && addGrid!=null)	
								{
									AddUnit(addGrid,x,buildings[i]);
									addedGrid.Add(addGrid);
								}
								else
									loopFlag=false;
							}
						}
					}
				}
			}
			//造农场
			else
			{
				//计算最近可以造农场的地方
				float minDis=float.PositiveInfinity;
				Grid targetGrid=null;
				Peasant chosenPeasant=null;
				foreach(Grid g in grainGrids)
				{
					//如果自己的建筑或单位在格子上，则不能在此处建造农场，跳过
					bool flag=false;
					foreach(GameObject obj in g.gridGameObjects)
					{
						if(obj.tag=="Unit")
						{
							if(obj.GetComponent<Unit>().player.playerType.Equals(playerType))
							{
								flag=true;
								break;
							}
						}
						else if(obj.tag=="Building")
						{
							if(obj.GetComponent<Building>().player.playerType.Equals(playerType))
							{
								flag=true;
								break;
							}
						}
					}
					if(flag)
						continue;
					foreach(Peasant p in peasants)
					{
						float dis=Vector3.Distance(g.gameObject.transform.position,p.gameObject.transform.position);
						float c=p.peasantTask.Count;
						dis*=(0.5f*c+1);
						if(dis<minDis)
						{
							minDis=dis;
							chosenPeasant=p;
							targetGrid=g;
						}
					}
				}

				PeasantTask pt=new PeasantTask();
				pt.targetGrid=targetGrid;
				pt.buildingIndex=1;
				chosenPeasant.peasantTask.Add(pt);
			}

			Invoke("DecideUnitMove",1f);
		}

		private void DecideUnitMove()
		{
			GridGroup[] temp=gridGroups;
			foreach(Unit u in units)
			{
				//对每个单位计算目的地
				GridGroup targetGroup=new GridGroup();
				float f=float.PositiveInfinity;
				int index=-1;
				for(int i=0;i<temp.Length;i++)
				{
					float battleFactor=temp[i].battleFactor;
					float scoutFactor=temp[i].scoutFactor;
					float inf=(1+scoutFactor)/battleFactor;
					if(temp[i].grids.Contains(u.grid))
					{
						if(battleFactor<1f)
							inf*=1.5f;
						else
							inf/=1.5f;
					}
					if(inf<f)
					{
						f=inf;
						targetGroup=temp[i];
						index=i;
					}
				}
				float minDis=float.PositiveInfinity;
				Grid targetGrid=null;
				foreach(Grid tgs in targetGroup.grids)
				{
					float dis=Vector3.Distance(tgs.gameObject.transform.position,enemyBuildings[0].gameObject.transform.position);
					if(dis<minDis)
					{
						minDis=dis;
						targetGrid=tgs;
					}
				}
				u.SetMoveDestination(targetGrid.gameObject.transform.position);

				//重新计算temp的相关值
				//battleFactor
				List<Unit> myUnits=new List<Unit>();
				List<Unit> enemyUnits=new List<Unit>();
				foreach(Grid gf in temp[index].grids)
				{
					foreach(GameObject obj in gf.gridGameObjects)
					{
						if(obj.tag=="Unit")
						{
							if(obj.GetComponent<Unit>().player.playerType.Equals(playerType))
								myUnits.Add(obj.GetComponent<Unit>());
							else
								enemyUnits.Add(obj.GetComponent<Unit>());
						}
					}
				}
				myUnits.Add(u);
				temp[index].battleFactor=CalBattleFactor(myUnits,enemyUnits);
			}
		}

		private void ChoosePeasantBuild(int index,List<Peasant> ps)
		{
			Peasant chosenPeasant=null;
			float minF=float.PositiveInfinity; 
			Grid targetGrid=null;
			foreach(Grid barckg in grids)
			{
				if(!(barckg.gridType==GridType.grass || barckg.gridType == GridType.plain || barckg.gridType == GridType.rock1))
					continue;
				foreach(Peasant p in ps)
				{
					float factor=(barckg.buildFactor+Vector3.Distance(p.gameObject.transform.position,barckg.gameObject.transform.position))*(0.5f*p.peasantTask.Count+1);
					if(factor<minF)
					{
						minF=factor;
						targetGrid=barckg;
						chosenPeasant=p;
					}
				}
			}

			if(chosenPeasant!=null && targetGrid!=null)
			{
				PeasantTask ptask=new PeasantTask();
				ptask.targetGrid=targetGrid;
				ptask.buildingIndex=index;
				chosenPeasant.peasantTask.Add(ptask);
			}
		}

		private void CalGridFactor()
		{
			foreach(Grid g in grids)
			{
				if(g.gridGameObjects.Count>0)
				{
					g.buildFactor=int.MaxValue;
					continue;
				}
				else
				{
					float sum=0;
					foreach(Building b in buildings)
					{
						float dis = Vector3.Distance(g.transform.position,b.gameObject.transform.position);
						if(dis<=3)
							dis*=buildings.Count;
						sum+=dis;
					}
					g.buildFactor=(int)sum;
				}
			}
			for(int i=0;i<gridGroups.Length;i++)
			{
				//battleFactor
				List<Unit> myUnits=new List<Unit>();
				List<Unit> enemyUnits=new List<Unit>();
				foreach(Grid gf in gridGroups[i].grids)
				{
					foreach(GameObject obj in gf.gridGameObjects)
					{
						if(obj.tag=="Unit")
						{
							if(obj.GetComponent<Unit>().player.playerType.Equals(playerType))
								myUnits.Add(obj.GetComponent<Unit>());
							else
								enemyUnits.Add(obj.GetComponent<Unit>());
						}
					}
				}
				gridGroups[i].battleFactor=CalBattleFactor(myUnits,enemyUnits);
				//scoutFactor
				CalGroupScoutFactor(i);
			}
		}

		private void AddUnit(Grid g,int index,Building relativeBuilding)
		{
			GameObject instance = Instantiate(unitModel[index]);
			instance.GetComponent<Unit>().player = this;
			instance.transform.parent = GameObject.Find("Units").transform;
			instance.transform.position =new Vector3(g.gameObject.transform.position.x,0.711f,g.gameObject.transform.position.z);
			instance.name = unitModel[index].name + " - " + GameObject.Find("Units").transform.childCount + "(" + playerType.ToString() + ")";

			units.Add(instance.GetComponent<Unit>());
			resource.ReduceResource(unitModel[index].GetComponent<Unit>().cost);

			//攻防
			for(int j=0;j<(int)technology.attackLevel;j++){
				instance.GetComponent<Unit>().attack+=technology.improveValue[j];
			}
			for(int k=0;k<(int)technology.armorLevel;k++){
				instance.GetComponent<Unit>().armor+=technology.improveValue[k];
			}

			//建筑的每回合产生unit自增
			if (instance != null && relativeBuilding!=null)
			{
				relativeBuilding.presentAddUnitNum += 1;
			}
		}

		private void AddBuilding(Grid g,int index)
		{
			GameObject instance = Instantiate(buildingModel[index]);
			instance.transform.parent = GameObject.Find("Buildings").transform;
			instance.transform.position = new Vector3(g.gameObject.transform.position.x,0.711f,g.gameObject.transform.position.z);
			instance.name = buildingModel[index].name + " - " + GameObject.Find("Buildings").transform.childCount + "(" + playerType.ToString() + ")";

			instance.GetComponent<Building>().player = this;
			instance.GetComponent<Building>().grid = g;
			g.gridGameObjects.Add(instance);
			instance.GetComponent<Building>().ClearGrain();
			buildings.Add(instance.GetComponent<Building>());
			resource.ReduceResource(buildingModel[index].GetComponent<Building>().cost);
		}

		private int CalK()
		{
			List<GameObject> viewableGrids = GetViewableArea();
			float minDis = float.PositiveInfinity;
			foreach(GameObject g in viewableGrids)
			{
				float dis = Vector3.Distance(g.transform.position,enemyBuildings[0].gameObject.transform.position);
				if( minDis > dis)
					minDis = dis;
			}
			float baseDis=Vector3.Distance(enemyBuildings[0].gameObject.transform.position,buildings[0].gameObject.transform.position);
			scoutFactor =1/(float)(1+System.Math.Pow(System.Math.E,-minDis/baseDis));    //视野距离敌方远近
			float income = 0;
			foreach(Building ebu in enemyBuildings)
			{
				if(ebu.buildingType==BuildingType.farm && ebu.buildStage.Equals(BuildStage.finish))
				{
					income+=10;   //农场收入
				}
			}
			Debug.Log("scoutFactor: "+scoutFactor);
			return (int)(income*10*scoutFactor+260);
		}

		private void CalGroupScoutFactor(int index)
		{
			List<GameObject> viewableGrids = GetViewableArea();
			float sum=0;
			foreach(Grid g in gridGroups[index].grids)
			{
				if(viewableGrids.Contains(g.gameObject))
					continue;
				float minDis=float.PositiveInfinity;
				foreach(GameObject vg in viewableGrids)
				{
					float dis = Vector3.Distance(vg.transform.position,g.transform.position);
					if( minDis > dis)
						minDis = dis;
				}
				sum += minDis;
			}
			float avgdis=sum/gridGroups[index].grids.Count;
			gridGroups[index].scoutFactor = avgdis /(1+0.5f*gridGroups[index].unvisitedRounds);
			if(avgdis>10f)
				gridGroups[index].unvisitedRounds+=1;
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

		public override void AddEnemyGameObject(GameObject g)
		{
			if(g.tag=="Unit")
			{
				if(!newFindUnits.Contains(g.GetComponent<Unit>()) && !enemyUnits.Contains(g.GetComponent<Unit>()) && g.GetComponent<Unit>().player.playerType!=playerType)
				{
					newFindUnits.Add(g.GetComponent<Unit>());
				}
			}
			else if(g.tag=="Building")
			{
				if(!newFindBuildings.Contains(g.GetComponent<Building>()) && !enemyBuildings.Contains(g.GetComponent<Building>()) && g.GetComponent<Building>().player.playerType!=playerType)
				{
					newFindBuildings.Add(g.GetComponent<Building>());
				}
			}
		}

		private float CalBattleFactor(List<Unit> myUnits,List<Unit> enemyUnits)
		{
			float myF=10,enemyF=10;
			foreach(Unit myu in myUnits)
			{
				myF+=myu.presentHp;
				myF+=myu.attack;
				myF+=myu.armor;
			}
			foreach(Unit enu in enemyUnits)
			{
				enemyF+=enu.presentHp;
				enemyF+=enu.attack;
				enemyF+=enu.armor;
			}
			return (float)(enemyF/myF);
		}
	}
}

