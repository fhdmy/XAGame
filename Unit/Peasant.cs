using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace XAGame
{	
	public struct PeasantTask
	{
		public Grid targetGrid;
		public int buildingIndex;
	}

	public class Peasant : Unit
	{
		public List<Grid> addBuildingGrids;

		public List<PeasantTask> peasantTask=new List<PeasantTask>();

		protected override void Start()
		{
			outline = gameObject.GetComponent<Highline>();
			ani = gameObject.GetComponent<Animator>();
			navAgent = gameObject.GetComponent<NavMeshAgent>();
			NavObstacle = gameObject.GetComponent<NavMeshObstacle>();
			timerManager = GameObject.Find("TimerManager").GetComponent<TimerManager>();
			uiManager = GameObject.Find("UIManager").GetComponent<UIManager>();

			navAgent.speed = speed;
			gameObject.GetComponents<CapsuleCollider>()[1].radius = viewDisance;
			outline.enabled = false;
			viewableObjects = new List<GameObject>();
			aroundGrids = new List<Grid>();

			isSelected = false;
			targetPos = transform.position;
			isAttacking = false;
			hasAttacked = false;

			ChangeModelColor();
			addBuildingGrids = new List<Grid>();
			// peasantTask=new List<PeasantTask>();
		}

		protected override void OnTriggerStay(Collider other)
		{
			int minHp = int.MaxValue;
			foreach(GameObject g in hiddenObjects)
			{
				if(g!=null)
				{
					if(g.GetComponent<Assassin>().isHidden && g.GetComponent<Assassin>().player.playerType != player.playerType)
					{
						viewableObjects.Remove(g);
					}
					else
						if(!viewableObjects.Contains(g))
							viewableObjects.Add(g);
				}
			}

			if(timerManager)
			{
				if(timerManager.presentStage==GameStage.moveStage || timerManager.presentStage==GameStage.moveOrAttackStage)
				{
					//ai
					if(peasantTask.Count>0)
					{
						if(Vector3.Distance(transform.position,peasantTask[0].targetGrid.gameObject.transform.position)<=3f)
						{
							StopMove();
						}
					}
					foreach (GameObject gm in viewableObjects)
					{
						if (gm != null)
						{
							int hp= int.MaxValue;
							if (gm.tag == "Unit")
							{
								if (gm.GetComponent<Unit>().player.playerType == player.playerType)
									continue;
								else
									hp = gm.GetComponent<Unit>().presentHp;
							}
							else if (gm.tag == "Building")
							{
								if (gm.GetComponent<Building>().player.playerType == player.playerType)
									continue;
								else
									hp = gm.GetComponent<Building>().presentHp;
							}
							if (hp < minHp && Vector3.Distance(transform.position, gm.transform.position) <= viewDisance)
							{
								minHp = hp;
								targetObject = gm;
							}
						}
					}

					//解决两个单位目的地相同碰撞
					if(navAgent)
					{
						if (navAgent.enabled)
						{
							if(navAgent.hasPath && other.tag == "Unit")
							{
								if(Vector3.Distance(targetPos, other.gameObject.GetComponent<Unit>().targetPos) < 1.6f && navAgent.remainingDistance < 0.8f)
								{
									StopMove();
									other.gameObject.GetComponent<Unit>().StopMove();
								}
							}
						}
					}
				}
			}
		}

		protected override void OnTriggerEnter(Collider other)
		{
			if (other.gameObject.tag == "Unit" || other.gameObject.tag == "Building")
			{
				viewableObjects.Add(other.gameObject);
				if(other.gameObject.tag == "Unit")
				{
					if(other.gameObject.GetComponent<Unit>().unitType==UnitType.assassin)
					{
						if(other.gameObject.GetComponent<Assassin>().isHidden)
						{
							viewableObjects.Remove(other.gameObject);
							if(!hiddenObjects.Contains(other.gameObject))
								hiddenObjects.Add(other.gameObject);
						}
					}
				}
			}
			else if (other.gameObject.tag == "Terrain")
			{
				aroundGrids.Add(other.gameObject.GetComponent<Grid>());
				if (Vector3.Distance(transform.position, other.gameObject.transform.position) <= 3f
					&& (other.gameObject.GetComponent<Grid>().gridType == GridType.grass
					|| other.gameObject.GetComponent<Grid>().gridType == GridType.plain
					|| other.gameObject.GetComponent<Grid>().gridType == GridType.rock1))
				{
					addBuildingGrids.Add(other.gameObject.GetComponent<Grid>());
				}
			}
		}

		public override void FlashState()
		{
			StopMove();
			targetPos = transform.position;
			hasAttacked = false;
			isAttacking = false;
			ChangeNavMode(true);

			//get add-building-grids
			for (int i = addBuildingGrids.Count - 1; i >= 0; i--)
			{
				if (Vector3.Distance(transform.position, addBuildingGrids[i].gameObject.transform.position) > 3f)
				{
					addBuildingGrids.Remove(addBuildingGrids[i]);
				}
			}
			foreach (Grid g in aroundGrids)
			{
				if (!addBuildingGrids.Contains(g))
				{
					if (Vector3.Distance(transform.position, g.gameObject.transform.position) <= 3f
						&& (g.gridType == GridType.grass
						|| g.gridType == GridType.plain
						|| g.gridType == GridType.rock1))
					{
						addBuildingGrids.Add(g);
					}
				}
			}
		}

		public override List<Grid> GetAddBuildingGrids(BuildingType buildingType)
		{
			List<Grid> temp = new List<Grid>();
			if (buildingType == BuildingType.farm)
			{
				foreach (Grid g in addBuildingGrids)
				{
					if (g.grain!=null && g.gridGameObjects.Count==0)
					{
						temp.Add(g);
					}
				}
				return temp;
			}
			else
			{
				if (!player.technology.canAddBuildings.Contains(buildingType))
				{
					uiManager.AlertText(AlertTextType.cantBuildForTech);
					return new List<Grid>();
				}
				else
				{
					foreach (Grid g in addBuildingGrids)
					{
						if (g.grain == null && g.gridGameObjects.Count == 0)
						{
							temp.Add(g);
						}
					}
					return temp;
				}
			}
		}

		public override void SetMoveToTarget()
		{
			//ai
			if(peasantTask.Count>0)
			{
				SetMoveDestination(peasantTask[0].targetGrid.GetComponent<Grid>().gameObject.transform.position);
				ChangeNavMode(false);
				Invoke("SetMove",0.5f);
			}
			//human
			else
			{
				ChangeNavMode(false);
				Invoke("SetMove",0.5f);
			}
		}

	}
}

