using UnityEngine;
using System.Collections.Generic;

public class TargetSprites : MonoBehaviour {
	
	public Sprite targetSprite;
	public BlackWhiteSnakes.ITargetMapData targetMapData;
	public Vector2 offset;
	
	private List<SpriteRenderer> rendererList;
	
	void Start () {
		this.rendererList = new List<SpriteRenderer>();
	}
	
	void Update () {
		if (this.targetMapData == null)
			return;
		if (!this.targetMapData.Dirty)
			return;
		this.targetMapData.Dirty = false;
		var count = this.targetMapData.Count;
		for (int i = this.rendererList.Count; i < count; ++i) {
			var go = new GameObject ();
			var renderer = go.AddComponent<SpriteRenderer> ();
			renderer.sprite = this.targetSprite;
			renderer.sortingLayerName = "Ground";
			renderer.gameObject.name = "Target";
			this.rendererList.Add (renderer);
		}
		for (int i = 0; i < count; ++i) {
			var renderer = this.rendererList[i];
			int x, y;
			this.targetMapData.GetTargetAtIndex (i, out x, out y);
			renderer.transform.position = new Vector2 (x, y) + offset;
			renderer.gameObject.SetActive (true);
		}
		for (int i = count; i < this.rendererList.Count; ++i) {
			var renderer = this.rendererList[i];
			renderer.gameObject.SetActive (false);
		}
	}
}
