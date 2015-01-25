using UnityEngine;
using System.Collections.Generic;

public class ItemSprites : MonoBehaviour {

	public Sprite[] itemSpriteSheet;
	public BlackWhiteSnakes.IItemMapData itemMapData;
	public Vector2 offset;

	private List<SpriteRenderer> rendererList;
	
	void Start () {
		this.rendererList = new List<SpriteRenderer>();
	}

	void Update () {
		if (this.itemMapData == null)
			return;
		if (!this.itemMapData.Dirty)
			return;
		this.itemMapData.Dirty = false;
		var count = this.itemMapData.Count;
		for (int i = this.rendererList.Count; i < count; ++i) {
			var go = new GameObject ();
			var renderer = go.AddComponent<SpriteRenderer> ();
			this.rendererList.Add (renderer);
		}
		for (int i = 0; i < count; ++i) {
			var renderer = this.rendererList[i];
			int x, y, t;
			this.itemMapData.GetItemAtIndex (i, out x, out y, out t);
			renderer.transform.position = new Vector2 (x, y) + offset;
			renderer.sprite = this.itemSpriteSheet[t];
			renderer.sortingLayerName = "Item";
			renderer.gameObject.SetActive (true);
			renderer.gameObject.name = "Item " + t;
		}
		for (int i = count; i < this.rendererList.Count; ++i) {
			var renderer = this.rendererList[i];
			renderer.gameObject.SetActive (false);
		}
	}
}