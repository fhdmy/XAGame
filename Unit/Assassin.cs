using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XAGame
{
	public class Assassin : Unit
	{
		public List<Material> hideMats;

		public bool isHidden;

		private int presentHideRound;

		protected override void Start()
		{
			outline = gameObject.GetComponent<Highline>();
			ani = gameObject.GetComponent<Animator>();
			navAgent = gameObject.GetComponent<UnityEngine.AI.NavMeshAgent>();
			NavObstacle = gameObject.GetComponent<UnityEngine.AI.NavMeshObstacle>();
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

			isHidden=true;
			presentHideRound=-1;

			ChangeModelColor();
		}

		protected override void ChangeModelColor()
		{
			Material s = null;
			switch (player.playerType)
			{
				case PlayerType.playerBlue:
					if(isHidden)
						s = hideMats[0];
					else
						s = mats[0];
					break;
				case PlayerType.playerRed:
					if(isHidden)
						s = hideMats[1];
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

		public override void ChangeModelHasDecideMove(bool toDecideMove)
		{
			Material s = null;
			switch (player.playerType)
			{
				case PlayerType.playerBlue:
					if(toDecideMove)
						s = hasDecideMoveMats[0];
					else
					{
						if(isHidden)
							s = hideMats[0];
						else
							s= mats[0];
					}
					break;
				case PlayerType.playerRed:
					if (toDecideMove)
						s = hasDecideMoveMats[1];
					else
					{
						if(isHidden)
							s = hideMats[1];
						else
							s = mats[1];
					}
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

		protected override void Attack()
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

				isHidden=false;
				ChangeModelColor();
				presentHideRound=0;
				isAttacking = false;
				Invoke("EndAniToMove",0.7f);
			}
		}

		public override void FlashState()
		{
			StopMove();
			if(presentHideRound!=-1)
				presentHideRound+=1;
			if(presentHideRound==2)
			{
				isHidden=true;
				presentHideRound=-1;
				ChangeModelColor();
			}
			targetPos = transform.position;
			hasAttacked = false;
			isAttacking = false;
			ChangeNavMode(true);
		}
	}
}

