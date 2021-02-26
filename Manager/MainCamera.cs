using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace XAGame
{
	public class MainCamera : MonoBehaviour
	{

		private float timer;
		private Vector3 originMousePos;//移动地图用
		private bool moveCamera;

		//框选判断
		private bool drawRectangle;
		//框选的开始鼠标位置
		private Vector2 start;
		//框选颜色
		public Color rectColor;
		//画线材质
		public Material rectMat;

		private bool isOnGUI;

		public int moveSpeed;
		public int rotateSpeed;
		public Vector2Int rotateBorder; //x为最小值 y为最大值

		public Player player;


		void Start()
		{
			drawRectangle = false;
			moveCamera = false;
			rectColor = Color.blue;
			timer = 0;
			isOnGUI = false; 
		}

		void Update()
		{
			if (Input.GetMouseButtonDown(0))
			{
				if (EventSystem.current.IsPointerOverGameObject())
				{
					isOnGUI = true;
					return;
				}
				timer = 0;
				originMousePos = Input.mousePosition;
				moveCamera = false;
				drawRectangle = false;
			}
			else if (Input.GetMouseButton(0) && !isOnGUI)
			{
				timer += Time.deltaTime;
				if (timer > 0.1f)
				{
					if (timer < 0.2f && !moveCamera)
					{
						//Debug.Log(Vector3.Distance(Input.mousePosition, originMousePos));
						if (Vector3.Distance(Input.mousePosition, originMousePos) > 1f)
						{
							moveCamera = true;
						}
					}
					else if (timer >= 0.2f && !moveCamera && !drawRectangle)
					{
						drawRectangle = true;
						start = Input.mousePosition;
					}
					else if (moveCamera)
					{
						Move();
					}
				}

				else
				{
					Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);  //摄像机需要设置MainCamera的Tag这里才能找到
					RaycastHit hitInfo;
					if (Physics.Raycast(ray, out hitInfo, 200))
					{
						GameObject g = hitInfo.collider.gameObject;
						if (g.tag == "Terrain")
						{
							player.NewSelected(null);
						}
						else if(g.tag == "Unit" || g.tag == "Building")
						{
							List<GameObject> newg = new List<GameObject>();
							newg.Add(g);
							player.NewSelected(newg);
						}
					}
				}
			}
			else if (Input.GetMouseButtonUp(0))
			{
				isOnGUI = false;
				if (drawRectangle)
				{
					NewRectSelected(start, Input.mousePosition);
					drawRectangle = false;
				}
			}
			//单位移动或添加单位
			else if (Input.GetMouseButtonDown(1))
			{
				if (EventSystem.current.IsPointerOverGameObject()) return;
				player.GetMouseRight(Input.mousePosition);
			}
			else if (Input.GetAxis("Mouse ScrollWheel") != 0)
			{
				if (EventSystem.current.IsPointerOverGameObject()) return;
				Rotate(Input.GetAxis("Mouse ScrollWheel"));
			}

			//视野
			//player.UpdateViewableGrids();
		}

		void OnGUI()
		{
			//绘制方框
			if (drawRectangle)
			{
				Vector3 end = Input.mousePosition;//鼠标当前位置
				GL.PushMatrix();//保存摄像机变换矩阵
				if (!rectMat)
					return;
				rectMat.SetPass(0);
				GL.LoadPixelMatrix();//设置用屏幕坐标绘图
				GL.Begin(GL.QUADS);
				GL.Color(new Color(rectColor.r, rectColor.g, rectColor.b, 0.05f));//设置颜色和透明度，方框内部透明
				GL.Vertex3(start.x, start.y, 0);
				GL.Vertex3(end.x, start.y, 0);
				GL.Vertex3(end.x, end.y, 0);
				GL.Vertex3(start.x, end.y, 0);
				GL.End();
				GL.Begin(GL.LINES);
				GL.Color(rectColor);//设置方框的边框颜色 边框不透明
				GL.Vertex3(start.x, start.y, 0);
				GL.Vertex3(end.x, start.y, 0);
				GL.Vertex3(end.x, start.y, 0);
				GL.Vertex3(end.x, end.y, 0);
				GL.Vertex3(end.x, end.y, 0);
				GL.Vertex3(start.x, end.y, 0);
				GL.Vertex3(start.x, end.y, 0);
				GL.Vertex3(start.x, start.y, 0);
				GL.End();
				GL.PopMatrix();//恢复摄像机投影矩阵

			}
		}

		public void Move()
		{
			Camera.main.transform.position += Vector3.right * Time.deltaTime * Input.GetAxis("Mouse X") * moveSpeed;
			Camera.main.transform.position += Vector3.forward * Time.deltaTime * Input.GetAxis("Mouse Y") * moveSpeed;
		}

		public void Rotate(float deltaDis)
		{
			Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));  //摄像机需要设置MainCamera的Tag这里才能找到
			RaycastHit hitInfo;
			Vector3 centerPos=Vector3.zero;
			if (Physics.Raycast(ray, out hitInfo, 200))
			{
				centerPos=hitInfo.collider.transform.position;
			}
			if (centerPos!= Vector3.zero)
			{
				//滚轮向下
				if (deltaDis > 0)
				{
					if (transform.localEulerAngles.x < rotateBorder.y)
						transform.RotateAround(centerPos, Vector3.left, rotateSpeed * Time.deltaTime);
				}
				//滚轮向上
				else
				{
					if (transform.localEulerAngles.x > rotateBorder.x)
						transform.RotateAround(centerPos, Vector3.right, rotateSpeed * Time.deltaTime);
				}
			}
		}

		public void NewRectSelected(Vector2 startPos,Vector2 endPos)
		{
			List<GameObject> newg = new List<GameObject>();
			foreach (Unit u in player.units)
			{
				Vector3 screenPos = Camera.main.WorldToScreenPoint(u.gameObject.transform.position);
				//如果在方框中
				if (screenPos.x<Mathf.Max(startPos.x,endPos.x) && screenPos.x > Mathf.Min(startPos.x, endPos.x)
					&& screenPos.y < Mathf.Max(startPos.y, endPos.y) && screenPos.y > Mathf.Min(startPos.y, endPos.y))
				{
					newg.Add(u.gameObject);
				}
			}
			player.NewSelected(newg);
		}
	}
}

