using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XAGame
{
	public class Paladin : Unit
	{
		public int heal;

		protected override void Attack()
		{
			if (targetObject != null && isAttacking)
			{
				PlayerType plt = player.playerType;
				if (targetObject.tag == "Unit")
				{
					plt = targetObject.GetComponent<Unit>().player.playerType;
				}
				else if (targetObject.tag == "Building")
				{
					plt = targetObject.GetComponent<Building>().player.playerType;
				}

				//创建信息条目
				Inform i = new Inform();
				if(plt==player.playerType)
					i.battleInformationType = BattleInformationType.Heal;
				else
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
					int damage;
					if (plt == player.playerType)
						damage = targetUnit.Damage(-heal);
					else
						damage = targetUnit.Damage(attack);
					i.informs.Add(damage.ToString());
					hasAttacked = true;
				}
				else if (targetObject.tag == "Building")
				{
					Building targetBuilding = targetObject.GetComponent<Building>();
					int damage = targetBuilding.Damage(attack);
					i.informs.Add(damage.ToString());
					hasAttacked = true;
				}
				uiManager.AddBattleInformation(i);

				isAttacking = false;
				Invoke("EndAniToMove", 0.7f);
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
								if (gm.GetComponent<Unit>().player.playerType == player.playerType)
								{
									if (gm.GetComponent<Unit>().presentHp < gm.GetComponent<Unit>().maxHp)
									{
										hp = gm.GetComponent<Unit>().presentHp;
									}
								}
								else
									hp = gm.GetComponent<Unit>().presentHp;
							}
							if (gm.tag == "Building")
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
