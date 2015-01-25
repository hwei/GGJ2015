using UnityEngine;
using System.Collections.Generic;

namespace BlackWhiteSnakes {

	class ItemDropper {

		private IItemMap itemMap;

		public ItemDropper (IItemMap itemMap) {
			this.itemMap = itemMap;
		}

		public void Update () {
			int x = Random.Range (0, this.itemMap.Width);
			int y = Random.Range (0, this.itemMap.Height);
			var type = itemMap.GetItem (x, y);
			if (type != 0) {
				Debug.Log ("Skip drop");
				return;
			}
			itemMap.SetItem (x, y, Random.Range (2, 4));
		}
	}
}
