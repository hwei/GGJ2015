using UnityEngine;
using System.Collections.Generic;

public class Main : MonoBehaviour {

	public Vector2 spritOffset;
	public SnakeSprite blackSnakeSprite;
	public SnakeSprite whiteSnakeSprite;
	public ItemSprites itemSprites;
	public TargetSprites blackTargetSprites;
	public TargetSprites whiteTargetSprites;
	public GameObject gameOverUI;
	public GameObject gameWinUI;
	public UnityEngine.UI.Text levelText;
	public TextAsset[] levels;
	private BlackWhiteSnakes.Game game;
	private Input2Direction input2Direction0;
	private Input2Direction input2Direction1;
	private BlackWhiteSnakes.DirectionEnum input0;
	private BlackWhiteSnakes.DirectionEnum input1;

	void Start () {
		this.gameOverUI.SetActive (false);
		this.gameWinUI.SetActive (false);
		this.levelText.gameObject.SetActive (false);
		this.input2Direction0 = new Input2Direction ();
		this.input2Direction1 = new Input2Direction ();
		var levelData = new string[this.levels.Length];
		for (int i = 0; i < this.levels.Length; ++i) {
			levelData[i] = this.levels[i].text;
		}
		this.game = new BlackWhiteSnakes.Game (levelData);
		this.game.OnGameOver += this.showGameOver;
		this.game.OnWin += this.showWin;
		this.game.OnLevelBegin += this.showLevelText;
		this.game.NextLevel ();
		this.blackSnakeSprite.dataSource = this.game.BlackSnakeSpriteData;
		this.whiteSnakeSprite.dataSource = this.game.WhiteSnakeSpriteData;
		this.itemSprites.itemMapData = this.game.ItemMapData;
		this.blackTargetSprites.targetMapData = this.game.BlackTargetMapData;
		this.whiteTargetSprites.targetMapData = this.game.WhiteTargetMapData;

		this.blackSnakeSprite.offset = this.spritOffset;
		this.whiteSnakeSprite.offset = this.spritOffset;
		this.itemSprites.offset = this.spritOffset;
		this.blackTargetSprites.offset = this.spritOffset;
		this.whiteTargetSprites.offset = this.spritOffset;
	}

	void Update () {
		this.input0 = this.input2Direction0.Convert (
			Input.GetAxisRaw ("Horizontal"),
			Input.GetAxisRaw ("Vertical"));
		this.input1 = this.input2Direction1.Convert (
			Input.GetAxisRaw ("Horizontal1"),
			Input.GetAxisRaw ("Vertical1"));
	}

	void FixedUpdate () {
		this.game.logicUpdate (this.input0, this.input1);
	}

	void showGameOver () {
		this.gameOverUI.SetActive (true);
	}

	void showWin () {
		this.gameWinUI.SetActive (true);
	}

	public void Restart () {
		Application.LoadLevel ("Title");
	}

	public void NextLevel () {
		this.game.NextLevel ();
		this.gameWinUI.SetActive (false);
	}

	void showLevelText (int n) {
		this.levelText.gameObject.SetActive (true);
		this.levelText.text = string.Format ("Level {0}", n);
		this.Invoke ("hideLevelText", 1);
	}

	void hideLevelText () {
		this.levelText.gameObject.SetActive (false);
	}
}

class Input2Direction {
	BlackWhiteSnakes.DirectionEnum lastDirection = BlackWhiteSnakes.DirectionEnum.None;
	int lastX = 0;
	int lastY = 0;

	private int float2int (float v) {
		if (v > 0)
			return 1;
		else if (v < 0)
			return -1;
		else
			return 0;
	}

	public void Reset () {
		this.lastDirection = BlackWhiteSnakes.DirectionEnum.None;
	}

	public BlackWhiteSnakes.DirectionEnum Convert (float x, float y) {
		if (x == 0 && y == 0) {
			this.lastX = 0;
			this.lastY = 0;
			return this.lastDirection;
		}
		int ix = float2int (x);
		int iy = float2int (y);
		if (ix == this.lastX && iy == this.lastY) {
			return this.lastDirection;
		}
		bool horizontal;
		if (ix != this.lastX) {
			horizontal = ix != 0;
		} else {
			horizontal = iy == 0;
		}
		this.lastX = ix;
		this.lastY = iy;
		if (horizontal) {
			this.lastDirection = ix > 0 ?
				BlackWhiteSnakes.DirectionEnum.Right :
					BlackWhiteSnakes.DirectionEnum.Left;
		} else {
			this.lastDirection = iy > 0 ?
				BlackWhiteSnakes.DirectionEnum.Up :
					BlackWhiteSnakes.DirectionEnum.Down;
		}
		return this.lastDirection;
	}
}




