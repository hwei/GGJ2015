using UnityEngine;
using System.Collections;

namespace BlackWhiteSnakes {
	
	public class Game {
		public delegate void OnGameEventDelegate ();
		public delegate void OnGameLevelDelegate (int n);
		public event OnGameEventDelegate OnGameOver;
		public event OnGameEventDelegate OnWin;
		public event OnGameLevelDelegate OnLevelBegin;

		Map map;
		Snake blackSnake;
		Snake whiteSnake;
		ItemDropper itemDropper;

		public int dropItemInterval = 5;
		private int dropItemCountDown = 0;
		
		public ISnakeSpriteData BlackSnakeSpriteData { get { return this.blackSnake; } }
		public ISnakeSpriteData WhiteSnakeSpriteData { get { return this.whiteSnake; } }
		public IItemMapData ItemMapData { get { return this.map.ItemMapData; } }
		public ITargetMapData BlackTargetMapData { get { return this.map.BlackTargetMapData; } }
		public ITargetMapData WhiteTargetMapData { get { return this.map.WhiteTargetMapData; } }

		enum Status {
			Title,
			Play,
			Win,
			Death,
		}
		private Status status;
		private string[] levels;
		private int nextLevel = 0;
		
		public Game (string[] levels) {
			this.levels = levels;
			this.map = new Map (30, 30);
			this.blackSnake = new Snake (
				0, this.map.BlackSnakeMap, this.map.ItemMap, this.map.CollisionMap, this.map.BlackTargetMap);
			this.blackSnake.Reset (new Point2 (4, 4));
			this.blackSnake.OnDeath += this.onDeath;
			this.whiteSnake = new Snake (
				1, this.map.WhiteSnakeMap, this.map.ItemMap, this.map.CollisionMap, this.map.WhiteTargetMap);
			this.whiteSnake.Reset (new Point2 (25, 4));
			this.whiteSnake.OnDeath += this.onDeath;

			this.itemDropper = new ItemDropper (this.map.ItemMap);
			this.status = Status.Play;
		}
		
		public void logicUpdate (DirectionEnum input0, DirectionEnum input1) {
			switch (this.status) {
			case Status.Play:
				this.updatePlay (input0, input1);
				break;
			case Status.Death:
				this.updateDeath ();
				break;
			case Status.Win:
				this.updateWin ();
				break;
			}
		}

		public void NextLevel () {
			this.map.Load (levels[this.nextLevel]);
			if (this.OnLevelBegin != null) {
				this.OnLevelBegin (this.nextLevel + 1);
			}
			this.nextLevel = (this.nextLevel + 1) % this.levels.Length;
			this.blackSnake.Reset (new Point2 (4, 4));
			this.whiteSnake.Reset (new Point2 (25, 4));
			this.dropItemCountDown = 0;
			this.status = Status.Play;
		}

		private void updatePlay (DirectionEnum input0, DirectionEnum input1) {
			bool complete0 = this.blackSnake.UpdatePos (input0);
			bool complete1 = this.whiteSnake.UpdatePos (input1);
			if (complete0 && complete1) {
				Debug.Log ("Complete");
				this.status = Status.Win;
				if (this.OnWin != null) {
					this.OnWin ();
				}
			}
			if (this.dropItemCountDown == 0) {
				this.itemDropper.Update ();
				this.dropItemCountDown = this.dropItemInterval - 1;
			} else {
				--this.dropItemCountDown;
			}
		}

		private void updateWin () {
		}

		private void updateDeath () {
		}

		private void onDeath () {
			if (this.OnGameOver != null) {
				this.OnGameOver ();
			}
			this.status = Status.Death;
		}
	}
	
	public struct Point2 {
		public int x;
		public int y;
		public Point2 (int x, int y) {
			this.x = x;
			this.y = y;
		}
		public static Point2 operator + (Point2 a, Point2 b) {
			return new Point2 (a.x + b.x, a.y + b.y);
		}
		public override bool Equals (object obj) 
		{
			return obj is Point2 && this == (Point2)obj;
		}
		public override int GetHashCode() 
		{
			return this.x.GetHashCode() ^ this.y.GetHashCode();
		}
		public static bool operator == (Point2 a, Point2 b) 
		{
			return a.x == b.x && a.y == b.y;
		}
		public static bool operator != (Point2 a, Point2 b) 
		{
			return !(a == b);
		}
		public static Point2 Clamp (Point2 pos, int cx, int cy) {
			var x = pos.x % cx;
			var y = pos.y % cy;
			if (x < 0) x += cx;
			if (y < 0) y += cy;
			return new Point2 (x, y);
		}
	}
	
	public interface ISnakeSpriteData {
		int Length { get; }
		void GetBodyData (int index, out int x, out int y, out int t);
	}

	public interface IItemMapData {
		int Count { get; }
		void GetItemAtIndex (int index, out int x, out int y, out int t);
		bool Dirty { get; set; }
	}
	
	interface ISnakeMap {
		void AddCollision (int x, int y, int n);
		int GetCollision (int x, int y);
		int Width { get; }
		int Height { get; }
	}
	
	interface IItemMap {
		void SetItem (int x, int y, int v);
		int GetItem (int x, int y);
		int Width { get; }
		int Height { get; }
	}

	interface ICollisionMap {
		bool IsCollision (int x, int y);
		int Width { get; }
		int Height { get; }
	}

	interface ITargetMap {
		bool IsTarget (int x, int y);
		int TargetCount { get; }
		int Width { get; }
		int Height { get; }
	}

	public interface ITargetMapData {
		int Count { get; }
		void GetTargetAtIndex (int index, out int x, out int y);
		bool Dirty { get; set; }
	}
	
	public enum DirectionEnum {
		None,
		Left,
		Up,
		Right,
		Down,
		EnumCount,
	}
	
	class Directions {
		public static Point2[] DirVectors = new Point2[] {
			new Point2 (0, 0),
			new Point2 (-1, 0),
			new Point2 (0, 1),
			new Point2 (1, 0),
			new Point2 (0, -1),
		};
	}
}
