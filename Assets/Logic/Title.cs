using UnityEngine;
using System.Collections;

public class Title : MonoBehaviour {
	
	void Start () {
		this.Invoke ("Enter", 2);
	}
	
	void Enter () {
		Application.LoadLevel ("Main");
	}
}
