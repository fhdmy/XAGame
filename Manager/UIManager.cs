using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace XAGame
{
	public enum AlertTextType
	{
		nextRoundAlert,      //点击下一回合时候，实际上已经在移动或攻击阶段
		rightMouseTimeOut,   //单位右箭时，非操作阶段
		haveNoMoneyAdd,      //没有钱造兵或造钱
		cantBuildForTech,    //缺少前置科技
		addOutOfArea,        //在范围外建造/生产
		gfHightest,          //攻防已经最高
		improvingGF,         //正在升级攻防
	}
	public enum BattleInformationType
	{
		SetUnitMove,    // time([Player]): Set [Unit] move to [Pos].
		Attack,         // time([Player]): [Unit] attack [Unit] damge([damage]).
		Heal,         // time([Player]): [Unit] heal [Unit] for([heal]).
		Die,            // time([Player]): [Unit] die.
		AddUnit,        // time([Player]): Add unit [Unit].
		AddBuilding,    // time([Player]): Add building [Building].
		NewRound,        // time([Player]): Round [round].Add resource [resource]
		ImproveGF,        // time([Player]): Start improving [attack/armor] level to [GFLevel] costs([gold]).
		ImprovedGF,        // time([Player]): [attack/armor] level upgraded to [GFLevel].
	}
	public struct Inform
	{
		public BattleInformationType battleInformationType;
		public List<string> informs;
	}

	public class UIManager : MonoBehaviour
	{
		public GameObject alertText;
		public GameObject battleInformation;
		public GameObject goldText;

		public GameObject addBuildingBtn;
		public GameObject addUnitBtn;
		public GameObject showBtn;

		public GameObject blacksmithBtn;

		public GameObject unitModeBtn;

		private List<Inform> informPool;

		public Player player;

		void Start()
		{
			informPool = new List<Inform>();
		}

		public void AlertText(AlertTextType alert)
		{
			switch (alert)
			{
				case AlertTextType.nextRoundAlert:
					if(!alertText.activeSelf)
						alertText.SetActive(true);
					if(alertText.GetComponent<Text>().text!= "当前不能进行此操作")
					{
						alertText.GetComponent<Text>().text = "当前不能进行此操作";
						alertText.GetComponent<Animator>().SetTrigger("Pop");
						Invoke("InvokeAlert", 3.5f);
					}
					break;
				case AlertTextType.rightMouseTimeOut:
					if (!alertText.activeSelf)
						alertText.SetActive(true);
					if (alertText.GetComponent<Text>().text != "当前不能进行此操作")
					{
						alertText.GetComponent<Text>().text = "当前不能进行此操作";
						alertText.GetComponent<Animator>().SetTrigger("Pop");
						Invoke("InvokeAlert", 3.5f);
					}
					break;
				case AlertTextType.haveNoMoneyAdd:
					if (!alertText.activeSelf)
						alertText.SetActive(true);
					if (alertText.GetComponent<Text>().text != "没有足够金钱")
					{
						alertText.GetComponent<Text>().text = "没有足够金钱";
						alertText.GetComponent<Animator>().SetTrigger("Pop");
						Invoke("InvokeAlert", 3.5f);
					}
					break;
				case AlertTextType.cantBuildForTech:
					if (!alertText.activeSelf)
						alertText.SetActive(true);
					if (alertText.GetComponent<Text>().text != "缺乏前置建筑")
					{
						alertText.GetComponent<Text>().text = "缺乏前置建筑";
						alertText.GetComponent<Animator>().SetTrigger("Pop");
						Invoke("InvokeAlert", 3.5f);
					}
					break;
				case AlertTextType.addOutOfArea:
					if (!alertText.activeSelf)
						alertText.SetActive(true);
					if (alertText.GetComponent<Text>().text != "超出生产/建造范围")
					{
						alertText.GetComponent<Text>().text = "超出生产/建造范围";
						alertText.GetComponent<Animator>().SetTrigger("Pop");
						Invoke("InvokeAlert", 3.5f);
					}
					break;
				case AlertTextType.gfHightest:
					if (!alertText.activeSelf)
						alertText.SetActive(true);
					if (alertText.GetComponent<Text>().text != "已达到最高科技")
					{
						alertText.GetComponent<Text>().text = "已达到最高科技";
						alertText.GetComponent<Animator>().SetTrigger("Pop");
						Invoke("InvokeAlert", 3.5f);
					}
					break;
				case AlertTextType.improvingGF:
					if (!alertText.activeSelf)
						alertText.SetActive(true);
					if (alertText.GetComponent<Text>().text != "正在升级该科技")
					{
						alertText.GetComponent<Text>().text = "正在升级该科技";
						alertText.GetComponent<Animator>().SetTrigger("Pop");
						Invoke("InvokeAlert", 3.5f);
					}
					break;
			}
		}

		private void InvokeAlert()
		{
			if (alertText.activeSelf)
			{
				alertText.GetComponent<Text>().text = "";
				alertText.SetActive(false);
			}	
		}

		public void UpdateGoldText(int gold)
		{
			goldText.GetComponent<Text>().text = gold.ToString();
		}

		public void AddBattleInformation(Inform i)
		{
			switch (i.battleInformationType)
			{
				case BattleInformationType.SetUnitMove:
					battleInformation.transform.Find("Viewport/BattleContent").GetComponent<Text>().text += i.informs[0]+"("+ i.informs[1]+"): set "+ i.informs[2] +" move to "+ i.informs[3]+".\n";
					break;
				case BattleInformationType.Attack:
					battleInformation.transform.Find("Viewport/BattleContent").GetComponent<Text>().text += i.informs[0] + "(" + i.informs[1] + "): " + i.informs[2] + " attack " + i.informs[3] + " damge("+ i.informs[4] + ").\n";
					break;
				case BattleInformationType.Heal:
					battleInformation.transform.Find("Viewport/BattleContent").GetComponent<Text>().text += i.informs[0] + "(" + i.informs[1] + "): " + i.informs[2] + " heal " + i.informs[3] + " for(" + i.informs[4] + ").\n";
					break;
				case BattleInformationType.Die:
					battleInformation.transform.Find("Viewport/BattleContent").GetComponent<Text>().text += i.informs[0] + "(" + i.informs[1] + "): " + i.informs[2] + " die.\n";
					break;
				case BattleInformationType.AddUnit:
					battleInformation.transform.Find("Viewport/BattleContent").GetComponent<Text>().text += i.informs[0] + "(" + i.informs[1] + "): Add unit " + i.informs[2] + ".\n";
					break;
				case BattleInformationType.AddBuilding:
					battleInformation.transform.Find("Viewport/BattleContent").GetComponent<Text>().text += i.informs[0] + "(" + i.informs[1] + "): Add building " + i.informs[2] + ".\n";
					break;
				case BattleInformationType.NewRound:
					battleInformation.transform.Find("Viewport/BattleContent").GetComponent<Text>().text += i.informs[0] + "(" + i.informs[1] + "): Round "+ i.informs[2] + ". Add resource "+ i.informs[3] + ".\n";
					break;
				case BattleInformationType.ImproveGF:
					battleInformation.transform.Find("Viewport/BattleContent").GetComponent<Text>().text += i.informs[0] + "(" + i.informs[1] + "): Start improving "+ i.informs[2] + " level to "+ i.informs[3]+" costs("+ i.informs[4] + ").\n";
					break;
				case BattleInformationType.ImprovedGF:
					battleInformation.transform.Find("Viewport/BattleContent").GetComponent<Text>().text += i.informs[0] + "(" + i.informs[1] + "): "+ i.informs[2] + " level upgraded to "+ i.informs[3]+".\n";
					break;
			}
			var rt = battleInformation.transform.Find("Viewport/BattleContent").GetComponent<RectTransform>();
			rt.sizeDelta = new Vector2(rt.sizeDelta.x,rt.sizeDelta.y+20);
			informPool.Add(i);
		}

		public void AddUnit(GameObject g)
		{
			player.GetAddUnit(g);
		}

		public void AddBuilding(GameObject g)
		{
			player.GetAddBuilding(g);
		}

		public void ShowUnitOrBuildingBtn()
		{
			if(addUnitBtn.activeSelf==true)
			{
				addUnitBtn.SetActive(false);
				addBuildingBtn.SetActive(true);
				showBtn.GetComponent<Text>().text="Building";
			}
			else if(addBuildingBtn.activeSelf==true)
			{
				addBuildingBtn.SetActive(false);
				addUnitBtn.SetActive(true);
				showBtn.GetComponent<Text>().text="Unit";
			}
			else
			{
				addUnitBtn.SetActive(true);
				showBtn.GetComponent<Text>().text="Unit";
			}
		}
		public void SetShowBtnNull()
		{
			addUnitBtn.SetActive(false);
			addBuildingBtn.SetActive(false);
			showBtn.GetComponent<Text>().text="NULL";
		}

		public void ShowBlacksmithBtn(bool flag)
		{
			if(flag)
			{
				blacksmithBtn.SetActive(true);
			}
			else
			{
				blacksmithBtn.SetActive(false);
			}
		}

		public void ShowUnitModeBtn(bool flag,List<GameObject> u)
		{
			if(flag)
			{
				unitModeBtn.SetActive(true);
				SetUnitModeBtnUI(u);
			}
			else
			{
				unitModeBtn.SetActive(false);
			}
		}

		protected void SetUnitModeBtnUI(List<GameObject> u)
		{
			bool flag=true;
			UnitMode um=u[0].GetComponent<Unit>().unitMode;
			foreach (GameObject uitem in u)
			{
				if(uitem.GetComponent<Unit>().unitMode != um)
				{
					flag=false;
					break;
				}
			}
			if(!flag)
			{
				unitModeBtn.transform.GetChild(0).gameObject.GetComponent<Image>().color=new Color32(53,53,53,255);
				unitModeBtn.transform.GetChild(1).gameObject.GetComponent<Image>().color=new Color32(53,53,53,255);
				unitModeBtn.transform.GetChild(2).gameObject.GetComponent<Image>().color=new Color32(53,53,53,255);
				return;
			}
			switch(um)
			{
				case UnitMode.taskFirst:
					unitModeBtn.transform.GetChild(0).gameObject.GetComponent<Image>().color=new Color32(234,17,32,255);
					unitModeBtn.transform.GetChild(1).gameObject.GetComponent<Image>().color=new Color32(53,53,53,255);
					unitModeBtn.transform.GetChild(2).gameObject.GetComponent<Image>().color=new Color32(53,53,53,255);
					break;
				case UnitMode.balanceMode:
					unitModeBtn.transform.GetChild(0).gameObject.GetComponent<Image>().color=new Color32(53,53,53,255);
					unitModeBtn.transform.GetChild(1).gameObject.GetComponent<Image>().color=new Color32(234,17,32,255);
					unitModeBtn.transform.GetChild(2).gameObject.GetComponent<Image>().color=new Color32(53,53,53,255);
					break;
				case UnitMode.killingFirst:
					unitModeBtn.transform.GetChild(0).gameObject.GetComponent<Image>().color=new Color32(53,53,53,255);
					unitModeBtn.transform.GetChild(1).gameObject.GetComponent<Image>().color=new Color32(53,53,53,255);
					unitModeBtn.transform.GetChild(2).gameObject.GetComponent<Image>().color=new Color32(234,17,32,255);
					break;
			}
		}

		public void ChangeUnitMode(int i)   //i=0：taskFirst  i=1: balanceMode  i=2: killingFirst
		{
			player.ChangeUnitMode(i);
			switch(i)
			{
				case 0:
					unitModeBtn.transform.GetChild(0).gameObject.GetComponent<Image>().color=new Color32(234,17,32,255);
					unitModeBtn.transform.GetChild(1).gameObject.GetComponent<Image>().color=new Color32(53,53,53,255);
					unitModeBtn.transform.GetChild(2).gameObject.GetComponent<Image>().color=new Color32(53,53,53,255);
					break;
				case 1:
					unitModeBtn.transform.GetChild(0).gameObject.GetComponent<Image>().color=new Color32(53,53,53,255);
					unitModeBtn.transform.GetChild(1).gameObject.GetComponent<Image>().color=new Color32(234,17,32,255);
					unitModeBtn.transform.GetChild(2).gameObject.GetComponent<Image>().color=new Color32(53,53,53,255);
					break;
				case 2:
					unitModeBtn.transform.GetChild(0).gameObject.GetComponent<Image>().color=new Color32(53,53,53,255);
					unitModeBtn.transform.GetChild(1).gameObject.GetComponent<Image>().color=new Color32(53,53,53,255);
					unitModeBtn.transform.GetChild(2).gameObject.GetComponent<Image>().color=new Color32(234,17,32,255);
					break;
			}
		}
	}
}

