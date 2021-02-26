using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;

namespace XAGame
{
	public enum UnitType
	{
		peasant, //农民
		warrior, //战士
		halberdier, //长矛兵
		archer, //弓箭手
		crossbowman, //弩手
		assassin, //刺客
		priest, //牧师
		mage, // 法师
		paladin, //圣骑士
		cavalry //重骑兵
	}

	public enum UnitMode
	{
		taskFirst,       //任何情况只向目的地前进，不攻击
		balanceMode,     //单位处于攻击范围才攻击（默认模式）
		killingFirst     //地方单位出现在视野范围内，会主动达到攻击范围并攻击目标
	}

	public class Unit : MonoBehaviour
	{
		public UnitType unitType;

		public Player player;

		protected Highline outline;
		protected Animator ani;
		protected NavMeshAgent navAgent;
		protected NavMeshObstacle NavObstacle;
		protected TimerManager timerManager;
		protected UIManager uiManager;

		public Grid grid;

		public GameObject targetPosModel;

		public int maxHp;
		public int presentHp;
		public int attack;
		public int armor;
		public float attackDisance;
		public float viewDisance;
		public float speed;
		public int cost;

		protected bool hasAttacked;
		public bool isAttacking;
		protected GameObject targetObject;
		public Vector3 targetPos;

		public List<GameObject> viewableObjects;
		public List<GameObject> hiddenObjects;
		public List<Grid> viewableGrids;
		public List<Grid> aroundGrids;

		public UnitMode unitMode;

		protected bool isSelected;

		public Sprite avatar;

		public List<Material> mats;
		public List<Material> hasDecideMoveMats;

		public List<Building> preBuildings;

		// Start is called before the first frame update
		protected virtual void Start()
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
			hiddenObjects = new List<GameObject>();
			aroundGrids = new List<Grid>();

			isSelected = false;
			targetPos = transform.position;
			isAttacking = false;
			hasAttacked = false;

			ChangeModelColor();
		}

		// Update is called once per frame
		void FixedUpdate()
		{
			//detect which grid the unit is on
			float minGridDis = float.PositiveInfinity;
			Grid minGrid = null;
			foreach (Grid ag in aroundGrids)
			{
				if (Vector3.Distance(transform.position, ag.transform.position) < minGridDis)
				{
					minGridDis = Vector3.Distance(transform.position, ag.transform.position);
					minGrid = ag;
				}
			}
			ExchangeGrid(minGrid);

			//choose attack target
			if (targetObject != null && !hasAttacked && !isAttacking && timerManager.presentStage == GameStage.moveOrAttackStage
			&& (unitMode!=UnitMode.taskFirst || Vector2.Distance(new Vector2(transform.position.x, transform.position.z), new Vector2(targetPos.x, targetPos.z)) < 0.1f))
			{
				AnimatorStateInfo aniInfo = ani.GetCurrentAnimatorStateInfo(0);
				if (!aniInfo.IsName("Attack") && !aniInfo.IsName("Damage"))
				{
					//attack
					if (Vector3.Distance(transform.position, targetObject.transform.position) < attackDisance)
					{
						isAttacking = true;
						if (navAgent.enabled)
							navAgent.isStopped = true;
						ani.SetBool("Walk", false);
						transform.LookAt(targetObject.transform.position);
						ani.SetTrigger("Attack");
						Invoke("Attack",0.45f);
					}
					//move
					else
					{
						if (navAgent.enabled && unitMode==UnitMode.killingFirst)
						{
							navAgent.SetDestination(targetObject.transform.position);
						}
					}
				}
				
			}

			//attack
			if (timerManager.presentStage == GameStage.moveStage ||
				timerManager.presentStage == GameStage.moveOrAttackStage)
			{
				//stop move ani
				if (navAgent.enabled)
				{
					if (isAttacking)
					{
						ani.SetBool("Walk", false);
					}
					else if (navAgent.hasPath && !isAttacking && !navAgent.isStopped)
					{
						ani.SetBool("Walk", true);
					}
					if (!navAgent.hasPath)
					{
						ani.SetBool("Walk", false);
						if (Vector2.Distance(new Vector2(transform.position.x, transform.position.z), new Vector2(targetPos.x, targetPos.z)) < 0.1f)
						{
							ChangeNavMode(true);
						}
					}
				}
			}
		}

		protected virtual void OnTriggerEnter(Collider other)
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
				player.AddEnemyGameObject(other.gameObject);
			}
			else if (other.gameObject.tag == "Terrain")
			{
				aroundGrids.Add(other.gameObject.GetComponent<Grid>());
			}
		}

		protected virtual void OnTriggerExit(Collider other)
		{
			if (other.gameObject.tag == "Unit" || other.gameObject.tag == "Building")
			{
				viewableObjects.Remove(other.gameObject);
				if(other.gameObject.tag == "Unit")
				{
					if(other.gameObject.GetComponent<Unit>().unitType==UnitType.assassin)
					{
						hiddenObjects.Remove(other.gameObject);
					}
				}
			}
			else if (other.gameObject.tag == "Terrain")
			{
				aroundGrids.Remove(other.gameObject.GetComponent<Grid>());
			}
		}

		protected virtual void OnTriggerStay(Collider other)
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

		public void StopMove()
        {
            if (navAgent.enabled)
            {
				navAgent.SetDestination(transform.position);
			}
            ani.SetBool("Walk", false);
        }

		public void IsSelected()
		{
			if (!isSelected)
			{
				outline.enabled = true;
				isSelected = true;
				//如果选取了目标位置
				if (Vector3.Distance(targetPos, transform.position) > 0.5f && targetPosModel)
				{
					GameObject Instance = Instantiate(targetPosModel);
					Instance.transform.position = targetPos;
					Destroy(Instance, 1.5f);
				}
			}	
		}

		public void ExitSelected()
		{
			if (isSelected)
			{
				outline.enabled = false;
				isSelected = false;
			}
		}

		public void SetMoveDestination(Vector3 pos)
		{
			targetPos = pos;
			ChangeModelHasDecideMove(true);
		}

		public virtual void SetMoveToTarget()
		{
			// if (navAgent.enabled)
			// {
			// 	ChangeModelHasDecideMove(false);
			// 	transform.LookAt(new Vector3(targetPos.x, transform.position.y, targetPos.z));
			// 	navAgent.isStopped = false;
			// 	navAgent.SetDestination(targetPos);
			// }
			ChangeNavMode(false);
			Invoke("SetMove",0.5f);
		}

		protected void SetMove()
		{
			if (navAgent.enabled)
			{
				ChangeModelHasDecideMove(false);
				transform.LookAt(new Vector3(targetPos.x, transform.position.y, targetPos.z));
				navAgent.isStopped = false;
				navAgent.SetDestination(targetPos);
			}
		}

		protected void ExchangeGrid(Grid minGrid)
		{
			if (minGrid == null)
				return;
			if (grid!=null)
			{
				if (!grid.Equals(minGrid))
				{
					grid.gridGameObjects.Remove(gameObject);
					grid = minGrid;
					grid.gridGameObjects.Add(gameObject);
				}
			}
			else
			{
				grid = minGrid;
				grid.gridGameObjects.Add(gameObject);
			}
		}

		protected void ChangeNavMode(bool toObstacle)
		{
			if (navAgent.enabled && toObstacle)
			{
				navAgent.enabled = false;
				Invoke("ObstacleEnable", 0.1f);
			}
			else if (!navAgent.enabled && !toObstacle)
			{
				NavObstacle.enabled = false;
				Invoke("NavEnable", 0.3f);
			}
		}

		protected void NavEnable()
		{
			if (!NavObstacle.enabled)
				navAgent.enabled = true;
		}

		protected void ObstacleEnable()
		{
			if (!navAgent.enabled)
				NavObstacle.enabled = true;
		}

		public int Damage(int a)
		{
			//攻击
			if (a > 0)
			{
				//防止死前受伤
				if (presentHp <= 0)
					return 0;
				presentHp = presentHp - (a - armor);
				FloatingText.instance.InitializeScriptableText(0, transform.position, "-" + (a - armor).ToString());
				//interrupt
				AnimatorStateInfo aniInfo = ani.GetCurrentAnimatorStateInfo(0);
				isAttacking = false;
				if (!aniInfo.IsName("Damage"))
					ani.SetTrigger("Damage");
				if (presentHp <= 0)
				{
					Dead();
				}
				else
					Invoke("EndAniToMove",1f);
				return (a - armor);
			}
			//治疗
			else
			{
				if (maxHp < presentHp - a)
				{
					a = maxHp - presentHp;
				}
				else
					a = -a;
				presentHp = presentHp + a;
				FloatingText.instance.InitializeScriptableText(1, transform.position, "+" + a.ToString());
				return -a;
			}
		}

		protected void EndAniToMove()
		{
			AnimatorStateInfo aniInfo = ani.GetCurrentAnimatorStateInfo(0);
			if (navAgent.enabled && !aniInfo.IsName("Damage") && !aniInfo.IsName("Attack"))
			{
				navAgent.isStopped = false;
				navAgent.SetDestination(targetPos);
			}
		}

		public void Dead()
		{
			int t = Random.Range(1, 2);
			ani.SetTrigger("Die" + t.ToString());
			Invoke("DestroySelf", 3f);
		}

		protected void DestroySelf()
		{
			player.units.Remove(gameObject.GetComponent<Unit>());
			player.selectedPool.Remove(gameObject);
			grid.gridGameObjects.Remove(gameObject);

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

		protected virtual void Attack()
		{
			if (targetObject != null && isAttacking)
			{
				//创建信息条目
				Inform i = new Inform();
				i.battleInformationType = BattleInformationType.Attack;
				i.informs = new List<string>();
				string presentTime = Camera.main.GetComponent<GameManager>().GetPresentTime();
				i.informs.Add(presentTime);
				i.informs.Add(player.playerType.ToString());
				i.informs.Add(gameObject.name);
				i.informs.Add(targetObject.name);

				if (targetObject.tag == "Unit")
				{
					Unit targetUnit = targetObject.GetComponent<Unit>();
					int damage = targetUnit.Damage(attack);
					i.informs.Add(damage.ToString());
					hasAttacked = true;
				}
				else if (targetObject.tag == "Building")
				{
					Building targetBuilding = targetObject.GetComponent<Building>();
					int damage=targetBuilding.Damage(attack);
					i.informs.Add(damage.ToString());
					hasAttacked = true;
				}
				uiManager.AddBattleInformation(i);

				isAttacking = false;
				Invoke("EndAniToMove",0.7f);
			}
		}

		public virtual void GetAttacked()
		{

		}

		public bool HasPath()
		{
			if (!navAgent.enabled)
				return false;
			return navAgent.hasPath;
		}

		public virtual void FlashState()
		{
			StopMove();
			targetPos = transform.position;
			hasAttacked = false;
			isAttacking = false;
			ChangeNavMode(true);
		}

		public void GetMoveTargetPos()
		{
			if (Vector3.Distance(targetPos, transform.position) < 0.5f)
				return;
			//创建信息条目
			Inform i = new Inform();
			i.battleInformationType = BattleInformationType.SetUnitMove;
			i.informs = new List<string>();
			string presentTime = Camera.main.GetComponent<GameManager>().GetPresentTime();
			i.informs.Add(presentTime);
			i.informs.Add(player.playerType.ToString());
			i.informs.Add(gameObject.name);
			i.informs.Add(new Vector2(targetPos.x,targetPos.z).ToString());
			uiManager.AddBattleInformation(i);
		}

		protected virtual void ChangeModelColor()
		{
			Material s = null;
			switch (player.playerType)
			{
				case PlayerType.playerBlue:
					s = mats[0];
					break;
				case PlayerType.playerRed:
					s = mats[1];
					break;
			}
			SkinnedMeshRenderer[] marr = gameObject.GetComponentsInChildren<SkinnedMeshRenderer>(true);
			foreach (SkinnedMeshRenderer m in marr)
			{
				m.material = s;
			}
			MeshRenderer[] mrr = gameObject.GetComponentsInChildren<MeshRenderer>(true);
			foreach (MeshRenderer m in mrr)
			{
				m.material = s;
			}
		}

		public virtual void ChangeModelHasDecideMove(bool toDecideMove)
		{
			Material s = null;
			switch (player.playerType)
			{
				case PlayerType.playerBlue:
					if(toDecideMove)
						s = hasDecideMoveMats[0];
					else
						s= mats[0];
					break;
				case PlayerType.playerRed:
					if (toDecideMove)
						s = hasDecideMoveMats[1];
					else
						s = mats[1];
					break;
			}
			SkinnedMeshRenderer[] marr = gameObject.GetComponentsInChildren<SkinnedMeshRenderer>(true);
			foreach (SkinnedMeshRenderer m in marr)
			{
				m.material = s;
			}
			MeshRenderer[] mrr = gameObject.GetComponentsInChildren<MeshRenderer>(true);
			foreach (MeshRenderer m in mrr)
			{
				m.material = s;
			}
		}

		public virtual List<Grid> GetAddBuildingGrids(BuildingType buildingType)
		{
			return null;
		}
	}
}

