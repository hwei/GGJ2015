using UnityEngine;
using System.Collections.Generic;
using System;

namespace BlackWhiteSnakes {

	class Snake : ISnakeSpriteData {
		enum Status {
			Begin,
			Walk,
			Dead,
		}

		public delegate void OnDeathDelegate ();
		public event OnDeathDelegate OnDeath;
		
		public List <Point2> PosList { get; private set; }
		public List <bool> inTargetList;
		public int Length { get { return this.PosList.Count; } }
		int type;
		ISnakeMap snakeMap;
		IItemMap itemMap;
		ICollisionMap collisionMap;
		ITargetMap targetMap;
		DirectionEnum headDirection;
		Status status;
		public Snake (
			int type, ISnakeMap snakeMap, IItemMap itemMap,
			ICollisionMap collisionMap, ITargetMap targetMap) {
			this.type = type;
			this.PosList = new List<Point2> ();
			this.inTargetList = new List<bool> ();
			this.snakeMap = snakeMap;
			this.itemMap = itemMap;
			this.collisionMap = collisionMap;
			this.targetMap = targetMap;
			this.headDirection = DirectionEnum.None;
		}
		
		public void GetBodyData (int index, out int x, out int y, out int t) {
			var pos = this.PosList[index];
			x = pos.x;
			y = pos.y;
			t = this.inTargetList[index] ? 1 : 0;
		}
		
		public void Reset (Point2 pos, int length = 1) {
			this.PosList.Clear ();
			this.inTargetList.Clear ();
			for (int i = 0; i < length; ++i) {
				this.PosList.Add (pos);
				this.inTargetList.Add (false);
				this.snakeMap.AddCollision (pos.x, pos.y, 1);
			}
			this.headDirection = DirectionEnum.None;
			this.status = Status.Begin;
		}
		
		private bool updatePosBegin (DirectionEnum input) {
			if (input != DirectionEnum.None) {
				this.status = Status.Walk;
				return updatePosWalk (input);
			}
			return false;
		}
		
		private bool updatePosWalk (DirectionEnum input) {
			bool inControl = false;
			if (input == DirectionEnum.None) {
				input = this.headDirection;
			} else {
				// this.headDirection = input;
				inControl = true;
			}
			Point2 prevPos = this.PosList[0];
			Point2 nextHeadPos = prevPos + Directions.DirVectors[(int) input];
			nextHeadPos = Point2.Clamp (
				nextHeadPos, this.snakeMap.Width, this.snakeMap.Height);
			var x = nextHeadPos.x;
			var y = nextHeadPos.y;

			if (this.collisionMap.IsCollision (x, y)) {
				if (inControl) {
					// prevent turn into collition
					input = this.headDirection;
					nextHeadPos = prevPos + Directions.DirVectors[(int) input];
					nextHeadPos = Point2.Clamp (
						nextHeadPos, this.snakeMap.Width, this.snakeMap.Height);
					x = nextHeadPos.x;
					y = nextHeadPos.y;
					if (this.collisionMap.IsCollision (x, y)) {
						// still hit
						if (this.OnDeath != null)
							this.OnDeath ();
						this.status = Status.Dead;
						return false;
					}
				} else {
					if (this.OnDeath != null)
						this.OnDeath ();
					this.status = Status.Dead;
					return false;
				}
			}
			this.headDirection = input;

			var itemType = this.itemMap.GetItem (x, y);
			if (itemType != 0) {
				if ((itemType % 2) == this.type) {
					this.itemMap.SetItem (x, y, 0);
					this.ChangeLength (this.Length + 1);
				} else {
					if (this.OnDeath != null)
						this.OnDeath ();
					this.status = Status.Dead;
					return false;
				}
			}

			int inTargetCount = 0;
			bool inTarget = this.targetMap.IsTarget (x, y);
			if (inTarget)
				++inTargetCount;
			this.inTargetList[0] = inTarget;
			this.PosList[0] = nextHeadPos;
			this.snakeMap.AddCollision (x, y, 1);
			for (int i = 1; i < this.Length; ++i) {
				Point2 tmpPos = this.PosList[i];
				this.PosList[i] = prevPos;
				inTarget = this.targetMap.IsTarget (prevPos.x, prevPos.y);
				if (prevPos != tmpPos && inTarget)
					++inTargetCount;
				this.inTargetList[i] = inTarget;
				prevPos = tmpPos;
				Debug.Log (string.Format ("inTargetList {0} {1} ({2})", i, inTarget, this.type)); 
			}

			this.snakeMap.AddCollision (prevPos.x, prevPos.y, -1);
			return inTargetCount == this.targetMap.TargetCount;
		}
		
		private bool updatePosDead (DirectionEnum input) {	
			Debug.Log ("updatePosDead");
			return false;
		}
		
		public bool UpdatePos (DirectionEnum input) {
			switch (this.status) {
			case Status.Begin:
				return this.updatePosBegin (input);
			case Status.Walk:
				return this.updatePosWalk (input);
			case Status.Dead:
				return this.updatePosDead (input);
			default:
				throw new NotImplementedException ();
			}
		}
		
		private void ChangeLength (int length) {
			int newCount = length - this.PosList.Count;
			if (newCount > 0) {
				var oldTailIndex = this.PosList.Count - 1;
				var tailPos = this.PosList[oldTailIndex];
				for (int i = 0; i < newCount; ++i) {
					this.PosList.Add (tailPos);
					this.inTargetList.Add (false);
					this.snakeMap.AddCollision (tailPos.x, tailPos.y, 1);
				}
			} else if (newCount < 0) {
				for (int i = this.PosList.Count - 1; i >= length; --i) {
					var pos = this.PosList[i];
					this.PosList.RemoveAt (i);
					this.inTargetList.RemoveAt (i);
					this.snakeMap.AddCollision (pos.x, pos.y, -1);
				}
			}
		}
	}
}
