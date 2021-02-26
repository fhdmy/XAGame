using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XAGame
{
	public class Priest : Unit
	{
		public int heal;
		public GameObject bullet;

		protected override void Attack()
		{
			if (targetObject != null && isAttacking)
			{
				GameObject Instance = Instantiate(bullet);
				Instance.transform.position = new Vector3(transform.position.x + transform.forward.x * 0.7f, 1.2f, transform.position.z + transform.forward.z * 0.7f);
				Instance.transform.LookAt(new Vector3(targetObject.transform.position.x, 1.2f, targetObject.transform.position.z));
				Instance.GetComponent<Bullet>().unit = this;
				Instance.GetComponent<Bullet>().target = targetObject;

				Invoke("EndAniToMove", 0.7f);
			}
		}

		public override void GetAttacked()
		{
			//创建信息条目
			Inform i = new Inform();
			i.battleInformationType = BattleInformationType.Heal;
			i.informs = new List<string>();
			string presentTime = Camera.main.GetComponent<GameManager>().GetPresentTime();
			i.informs.Add(presentTime);
			i.informs.Add(player.playerType.ToString());
			i.informs.Add(gameObject.name);
			i.informs.Add(targetObject.name);

			if (targetObject.tag == "Unit")
			{
				Unit targetUnit = targetObject.GetComponent<Unit>();
				int damage = targetUnit.Damage(-heal);
				i.informs.Add(damage.ToString());
				hasAttacked = true;
			}
			isAttacking = false;
			uiManager.AddBattleInformation(i);
		}

		protected override void OnTriggerEnter(Collider other)
		{
			if (other.gameObject.tag == "Unit" || other.gameObject.tag == "Building")
			{
				viewableObjects.Add(other.gameObject);
			}
			else if (other.gameObject.tag == "Terrain")
			{
				aroundGrids.Add(other.gameObject.GetComponent<Grid>());
			}
		}

		protected override void OnTriggerExit(Collider other)
		{
			if (other.gameObject.tag == "Unit" || other.gameObject.tag == "Building")
			{
				viewableObjects.Remove(other.gameObject);
			}
			else if (other.gameObject.tag == "Terrain")
			{
				aroundGrids.Remove(other.gameObject.GetComponent<Grid>());
			}
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
					foreach (GameObject gm in viewableObjects)
					{
						if (gm != null)
						{
							int hp = int.MaxValue;
							if (gm.tag == "Unit")
							{
								if (gm.GetComponent<Unit>().player.playerType != player.playerType)
									continue;
								else
								{
									if (gm.GetComponent<Unit>().presentHp< gm.GetComponent<Unit>().maxHp)
									{
										hp = gm.GetComponent<Unit>().presentHp;
									}
								}	
							}
							else if (gm.tag == "Building")
							{
									continue;
							}
							if (hp < minHp && Vector3.Distance(transform.position, gm.transform.position) <= viewDisance)
							{
								minHp = hp;
								targetObject = gm;
							}
						}
					}

					//解决两个单位目的地相同碰撞
					if (navAgent.enabled)
					{
						if (navAgent.hasPath && other.tag == "Unit")
						{
							if (Vector3.Distance(targetPos, other.gameObject.GetComponent<Unit>().targetPos) < 1.6f && navAgent.remainingDistance < 0.8f)
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
}
