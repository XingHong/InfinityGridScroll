using UnityEngine;
using System.Collections;
namespace OneP.InfinityScrollView
{
	public class InfinityBaseItem : MonoBehaviour {
		private InfinityGridScrollView infinityScrollView;
		private int index = int.MinValue;
		public int Index{
			private set{ 
				index=value;
			}
			get{ 
				return index;
			}
		}
		public InfinityGridScrollView GetInfinityScrollView(){
			return infinityScrollView;
		}

		// using for setup data
		public virtual void Reload(InfinityGridScrollView infinity,int _index){
			infinityScrollView = infinity;
			Index = _index;
			//todo
		}

        public virtual void Reload(int _index)
        {
            Index = _index;
            //todo
        }

        public virtual void SelfReload(){
			if (Index != int.MinValue) {
				//todo
			}
		}
	}
}
