using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

namespace OneP.InfinityScrollView
{
    public enum InfinityType
    {
        Vertical,
        Horizontal
    }

    public enum VerticalType
    {
        TopToBottom,
        BottomToTop
    }

    public enum HorizontalType
    {
        LeftToRight,
        RigthToLeft
    }

    public class InfinityGridScrollView : MonoBehaviour {
		[Header("Setting Reference object")]
		public GameObject prefab; // link object item
		public ScrollRect scrollRect;// link to UGUI scrollRect
		public RectTransform content;// link to content that contain all item in scrollrect

		[Header("Setting For Custom Scroll View")]
		public InfinityType type=InfinityType.Vertical;// type scrollview
		public VerticalType verticalType=VerticalType.TopToBottom;
		public HorizontalType horizontalType=HorizontalType.LeftToRight;
		public float overrideX=0;
		public float overrideY=0;
		public float extraContentLength=0;

        [Header("Setting For Custom Data")]
        public Vector2 showCellNumber = new Vector2(3,4);
        public Vector2 cellSize = new Vector2(100, 100);
		public int itemGenerate=10; // number item generate, note: only need create +2 more item appear, if max item appear in screen is 5 =>itemGenerate =7 is enough
		public int totalNumberItem=100;// total item of scrollview

		[Header("flat check auto setup references")]
		public bool isOverrideSettingScrollbar=true;
		public bool setupOnAwake=true;

		private List<GameObject> listItem = new List<GameObject> ();
		private GameObject[] arrayCurrent = null;
		private int cacheOld= -1;
		private bool isInit=false;


		public void Setup(int numberItem){
			totalNumberItem = numberItem;
			if (totalNumberItem<0) {
				totalNumberItem = 0;
			}
			Setup ();
		}

		public void Setup(){
			if (prefab == null) {
				Debug.LogWarning("No prefab/Gameobject Item linking");
				return;		
			}
			if (type == InfinityType.Vertical) {
                int num = Mathf.CeilToInt(totalNumberItem / showCellNumber.x);
				int totalHeight = (int)(num * cellSize.y);
				content.SetHeight (totalHeight);
			} else {
                int num = Mathf.CeilToInt(totalNumberItem / showCellNumber.y);
                int totalWidth = (int)(num * cellSize.x);
				content.SetWidth (totalWidth);
			}
			//reset Array
			arrayCurrent = new GameObject[totalNumberItem];

            int itemX = horizontalType == HorizontalType.LeftToRight ? 0 : 1;
            int itemY = verticalType == VerticalType.TopToBottom ? 1 : 0;
            content.anchorMin = new Vector2(itemX, itemY);
            content.anchorMax = new Vector2(itemX, itemY);
            content.pivot = new Vector2(itemX, itemY);

            for (int i = 0; i < itemGenerate; i++) {
				GameObject obj = null;
				if (!isInit) {
					if (i < totalNumberItem) {
						obj = GameObject.Instantiate (prefab) as GameObject;
						obj.name = "item_" + (i);
						obj.transform.SetParent (content.transform, false);
						obj.transform.localScale = Vector3.one;
						listItem.Add (obj);
						RectTransform rect=obj.GetComponent<RectTransform>();
						if(rect!=null){
                            rect.anchorMin = new Vector2(itemX, itemY);
                            rect.anchorMax = new Vector2(itemX, itemY);
                            rect.pivot = new Vector2(itemX, itemY);
						}
						Reload (obj, i);
						arrayCurrent [i] = obj;
					}
				}
				else
				{
					if (i < totalNumberItem) {
						obj = listItem [i];
						obj.SetActive (true);
						Reload (obj, i);
						arrayCurrent [i] = obj;
					} else {
						obj = listItem [i];
						obj.SetActive (false);
					}
				}
			}
			isInit = true;
		}
		private float GetContentSize(){
			return content.GetHeight ();
		}

		void Awake(){
			scrollRect = GetComponent<ScrollRect> ();
			scrollRect.onValueChanged.AddListener (OnScrollChange);
			if (setupOnAwake) {
				Setup ();
			}
		}

		private int GetCurrentIndex(){
			int index = -1;
			if (type == InfinityType.Vertical) {
				if(verticalType==VerticalType.TopToBottom)
				{
					index = (int)(content.anchoredPosition.y / cellSize.y);
				}
				else
				{
					index = (int)(-content.anchoredPosition.y / cellSize.y);
				}
			} else {
				if(horizontalType==HorizontalType.LeftToRight)
				{
					index = (int)(-content.anchoredPosition.x / cellSize.x);

				}
				else
				{
					index = (int)(content.anchoredPosition.x / cellSize.x);
				}
			}
			if (index < 0)
				index = 0;
            int maxNum = type == InfinityType.Vertical ? (int)showCellNumber.x : (int)showCellNumber.y;
            int ceil = Mathf.CeilToInt(totalNumberItem / maxNum);
			if (index > ceil - 1) {
				index = ceil - 1;
			}
            //Debug.LogError("test:" + index);
			return index;
		}
		public void InternalReload(){
			
			int index = GetCurrentIndex ();
			FixFastReload (index);
		}
		public void OnScrollChange(Vector2 vec){
			if (arrayCurrent==null||arrayCurrent.Length < 1) {
				return;
			}

			int index = GetCurrentIndex ();
			if (cacheOld != index) {
				cacheOld = index;
			} else {
				return;
			}
            
            if (!FixFastReload (index)) {
                int num = type == InfinityType.Vertical ? (int)showCellNumber.x : (int)showCellNumber.y;
                int max = num + index * num;
                for (int i = index* num; i < max; i++)
                {
                    ChangeReload(i);
                }
			}
		}

        private void ChangeReload(int index)
        {
            GameObject objIndex = arrayCurrent[index];
            if (objIndex == null)
            {// truot len
                int next = index + itemGenerate;
                if (next > totalNumberItem - 1)
                {
                    return;
                }
                else
                {
                    GameObject objNow = arrayCurrent[next];
                    if (objNow != null)
                    {//swap
                        arrayCurrent[next] = objIndex;
                        arrayCurrent[index] = objNow;
                        Reload(arrayCurrent[index], index);
                    }
                }
            }
            else
            {// truot xuong
                int num = type == InfinityType.Vertical ? (int)showCellNumber.x : (int)showCellNumber.y;
                if (index > num - 1)
                {
                    GameObject obj = arrayCurrent[index - num];
                    if (obj == null)
                    {
                        return;
                    }
                    int next = index - num + itemGenerate;
                    if (next > totalNumberItem - 1)
                    {
                        return;
                    }
                    else
                    {
                        GameObject objNow = arrayCurrent[next];
                        if (objNow == null)
                        {//swap
                            arrayCurrent[next] = obj;
                            arrayCurrent[index - num] = objNow;
                            Reload(arrayCurrent[next], next);
                        }
                    }
                }
            }
        }
		
		public bool FixFastReload(int index){
			bool isNeedFix = false;

            int deletNum = type == InfinityType.Vertical ? (int)showCellNumber.x : (int)showCellNumber.y;
			int add = (index + 1) * deletNum;

            for (int i = add; i < add + itemGenerate - deletNum * 2; i++) {
				if (i < totalNumberItem) {
					GameObject obj = arrayCurrent [i];
					if (obj == null) {
						isNeedFix = true;
						break;
					} else if (!obj.name.Equals ("item_" + i)) {
						isNeedFix = true;
						break;
					}
				}
			}

			if (isNeedFix) {
				for (int i = 0; i < totalNumberItem; i++) {
					arrayCurrent [i] = null;
				}

                int maxNum = type == InfinityType.Vertical ? (int)showCellNumber.x : (int)showCellNumber.y;
				int start = index * maxNum;
				if (start + itemGenerate > totalNumberItem ) {
                    int ceil = Mathf.CeilToInt((totalNumberItem - itemGenerate) / maxNum);
                    start = ceil - 1;
				}
				//Debug.LogError ("Fix Fast reload:"+start+","+index);
				for (int i = 0; i <  itemGenerate; i++) {
					arrayCurrent [start + i] = listItem [i];
					Reload (arrayCurrent [start + i], start + i);
				}
				return true;
			}
			return false;
		}

		protected virtual void Reload(GameObject obj,int indexReload){
			obj.transform.name = "item_" + indexReload;
			Vector3 vec=Vector3.zero;
			vec=new Vector2(overrideX,overrideY);
			vec = GetLocationAppear (vec, indexReload);
            obj.transform.localPosition=vec;
			InfinityBaseItem baseItem = obj.GetComponent<InfinityBaseItem> ();
			if (baseItem != null) {
				baseItem.Reload (indexReload);
			}
		}

		private Vector3 GetLocationAppear(Vector2 initVec,int location){
			Vector3 vec=initVec;
			if (type == InfinityType.Vertical) {
				if (verticalType == VerticalType.TopToBottom) {
					vec = new Vector3 (vec.x + cellSize.x * (location % showCellNumber.x), -cellSize.y * (location / (int)showCellNumber.x), 0);
				} else {
					vec = new Vector3 (vec.x + cellSize.x * (location % showCellNumber.x), cellSize.y * (location / (int)showCellNumber.x), 0);
				}
			} else {
                float x;
                float y;
				if(horizontalType==HorizontalType.LeftToRight){
                    x = cellSize.x * (location / (int)showCellNumber.y);
                }
				else
				{
                    x = -cellSize.x * (location / (int)showCellNumber.y);
				}

                if (verticalType == VerticalType.TopToBottom)
                {
                    y = -(vec.y + cellSize.y * (location % showCellNumber.y));
                }
                else
                {
                    y = vec.y + cellSize.y * (location % showCellNumber.y);
                }
                vec = new Vector3(x, y, 0);
			}
			return vec;
		}
	}
}
