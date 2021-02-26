using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XAGame
{
	public class Mage : Unit
	{
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
				int damage = targetBuilding.Damage(attack);
				i.informs.Add(damage.ToString());
				hasAttacked = true;
			}
			isAttacking = false;
			uiManager.AddBattleInformation(i);
		}
	}
}
