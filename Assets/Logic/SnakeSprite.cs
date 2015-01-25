using UnityEngine;
using System.Collections.Generic;

public class SnakeSprite : MonoBehaviour {

	public GameObject prefabBody;
	public Vector2 offset;

	public BlackWhiteSnakes.ISnakeSpriteData dataSource;

	struct Body {
		public GameObject gameObject;
		public Animator animator;
	}
	static private int KEY_INTARGET_PARTLY = Animator.StringToHash ("InTargetPartly");

	private List<Body> bodyList = new List<Body> ();
	private int activeCount = 0;
	
	void Update () {
		if (this.dataSource == null) {
			return;
		}
		int length = this.dataSource.Length;
		if (length > this.bodyList.Count) {
			for (int i = 0; i < length - this.bodyList.Count; ++i) {
				var go = Object.Instantiate (this.prefabBody) as GameObject;
				var body = new Body () {
					gameObject = go,
					animator = go.GetComponent <Animator> (),
				};
				bodyList.Add (body);
			}
		}
		for (int i = this.activeCount - 1; i >= length; --i) {
			this.bodyList[i].gameObject.SetActive (false);
		}
		this.activeCount = length;
		for (int i = 0; i < length; ++i) {
			var b = this.bodyList[i];
			var go = b.gameObject;
			go.SetActive (true);
			int x, y, t;
			this.dataSource.GetBodyData (i, out x, out y, out t);
			go.transform.position = new Vector2 ((float) x, (float) y) + this.offset;
			b.animator.SetBool (KEY_INTARGET_PARTLY, t != 0);
		}
	}
}
