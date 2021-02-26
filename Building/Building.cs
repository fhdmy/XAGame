using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XAGame
{
	public enum BuildingType
	{
		archery,
		barracks,
		castle,
		farm,
		mageTower,
		stables,
		tower,
		temple,
		blacksmith
	}

	public enum BuildStage
	{
		stage1,
		stage2,
		finish
	}

	public class Building : MonoBehaviour
	{
		protected Highline outline;

		public Player player;

		public BuildingType buildingType;

		public int maxHp;
		public int presentHp;
		public int armor;
		public float viewDisance;
		public int cost;
		public Grid grid;

		public List<Grid> viewableGrids;
		public List<Grid> aroundGrids;

		public bool isSelected;

		public Sprite avatar;

		public List<Material> mats;

		protected UIManager uiManager;

		public List<UnitType> relateUnitType;
		public List<Grid> addUnitGrids;
		public int addUnitNum;
		public int presentAddUnitNum;

		public BuildStage buildStage;
		public int presentBuildRound;
		public int[] buildLastTime;

		// Start is called before the first frame update
		protected virtual void Start()
		{
			uiManager = GameObject.Find("UIManager").GetComponent<UIManager>();
			outline = gameObject.GetComponent<Highline>();

			gameObject.GetComponent<CapsuleCollider>().radius = viewDisance;
			outline.enabled = false;
			isSelected = false;

			ChangeModelColor();
			addUnitGrids = new List<Grid>();

			presentBuildRound = 0;
			presentAddUnitNum = 0;
		}

		public virtual void IsSelected()
		{
			if (!isSelected)
			{
				outline.enabled = true;
				isSelected = true;
			}

		}

		public virtual void ExitSelected()
		{
			if (isSelected)
			{
				outline.enabled = false;
				isSelected = false;
			}
		}

		protected void OnTriggerEnter(Collider other)
		{
			if (other.gameObject.tag == "Terrain")
			{
				//viewablegrids
				aroundGrids.Add(other.gameObject.GetComponent<Grid>());
				//addgrids
				if (!other.gameObject.Equals(grid) && Vector3.Distance(transform.position, other.gameObject.transform.position) <= 3
					&& (other.gameObject.GetComponent<Grid>().gridType == GridType.grass
					|| other.gameObject.GetComponent<Grid>().gridType == GridType.plain
					|| other.gameObject.GetComponent<Grid>().gridType == GridType.rock1))
				{
					addUnitGrids.Add(other.gameObject.GetComponent<Grid>());
				}
			}
			else if(other.gameObject.tag == "Unit" || other.gameObject.tag == "Building")
			{
				player.AddEnemyGameObject(other.gameObject);
			}
		}

		protected void OnTriggerExit(Collider other)
		{
			aroundGrids.Remove(other.gameObject.GetComponent<Grid>());
		}

		protected void ChangeModelColor()
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
			MeshRenderer[] mrr = gameObject.GetComponentsInChildren<MeshRenderer>(true);
			foreach (MeshRenderer m in mrr)
			{
				m.material = s;
			}
		}

		public int Damage(int a)
		{
			//防止死前受伤
			if (presentHp <= 0)
				return 0;
			presentHp = presentHp - (a - armor);
			FloatingText.instance.InitializeScriptableText(0, transform.position, "-" + (a - armor).ToString());
			if (presentHp <= 0)
			{
				Dead();
			}
			return (a - armor);
		}

		public void Dead()
		{
			Invoke("DestroySelf", 1f);
		}

		protected virtual void DestroySelf()
		{
			player.buildings.Remove(gameObject.GetComponent<Building>());
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

		public List<Grid> GetAddUnitGrids(UnitType unitType)
		{
			if(presentAddUnitNum==addUnitNum)
				return new List<Grid>();
			if (relateUnitType.Contains(unitType))
			{
				List<Grid> temp = new List<Grid>();
				foreach (Grid g in addUnitGrids)
				{
					if (g.gridGameObjects.Count == 0 && g.grain==null)
						temp.Add(g);
				}
				return temp;
			}
			return new List<Grid>();
		}

		public void FlashState()
		{
			switch (buildStage)
			{
				case BuildStage.stage1:
					presentBuildRound += 1;
					if (presentBuildRound == buildLastTime[0])
					{
						buildStage = BuildStage.stage2;
						UpdateBuildUI();
						presentBuildRound = 0;
					}
					break;
				case BuildStage.stage2:
					presentBuildRound += 1;
					if (presentBuildRound == buildLastTime[1])
					{
						buildStage = BuildStage.finish;
						UpdateBuildUI();
						presentBuildRound = 0;
						player.technology.AddCanAddBuilding(buildingType);
					}
					break;
				case BuildStage.finish:
					break;
			}
			presentAddUnitNum = 0;
		}

		protected void UpdateBuildUI()
		{
			switch (buildStage)
			{
				case BuildStage.stage1:
					transform.GetChild(1).gameObject.SetActive(false);
					transform.GetChild(2).gameObject.SetActive(false);
					transform.GetChild(0).gameObject.SetActive(true);
					break;
				case BuildStage.stage2:
					transform.GetChild(0).gameObject.SetActive(false);
					transform.GetChild(2).gameObject.SetActive(false);
					transform.GetChild(1).gameObject.SetActive(true);
					break;
				case BuildStage.finish:
					transform.GetChild(0).gameObject.SetActive(false);
					transform.GetChild(1).gameObject.SetActive(false);
					transform.GetChild(2).gameObject.SetActive(true);
					break;
			}
		}

		public virtual void ClearGrain()
		{

		}
	}
}

