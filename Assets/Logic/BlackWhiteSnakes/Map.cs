using System.Collections.Generic;

namespace BlackWhiteSnakes {
	
	class Map {
		public enum LayerEnum {
			BlackSnake,
			WhiteSnake,
			Item,
			Target,
			EnumCount,
		}
		public byte[] Data { get; private set; }
		public int Width { get; private set; }
		public int Height { get; private set; }
		private int layerSize;
		private CItemMap itemMap;
		private CTargetMap blackTargetMap;
		private CTargetMap whiteTargetMap;
		
		public ISnakeMap BlackSnakeMap { get; private set; }
		public ISnakeMap WhiteSnakeMap { get; private set; }
		public IItemMap ItemMap { get { return this.itemMap; } }
		public ICollisionMap CollisionMap { get; private set; }
		public ITargetMap BlackTargetMap { get { return this.blackTargetMap; } }
		public ITargetMap WhiteTargetMap { get { return this.whiteTargetMap; } }
		public ITargetMapData BlackTargetMapData { get { return this.blackTargetMap; } }
		public ITargetMapData WhiteTargetMapData { get { return this.whiteTargetMap; } }

		public IItemMapData ItemMapData { get { return this.itemMap; }  }
		
		public Map (int width, int height) {
			int layerCount = (int)LayerEnum.EnumCount;
			this.Data = new byte[width * height * layerCount];
			this.Width = width;
			this.Height = height;
			this.layerSize = this.Width * this.Height;
			this.itemMap = new CItemMap (this, LayerEnum.Item);
			this.blackTargetMap = new CTargetMap (this, LayerEnum.Target, 2);
			this.whiteTargetMap = new CTargetMap (this, LayerEnum.Target, 3);
			this.BlackSnakeMap = new SnakeMap (this, LayerEnum.BlackSnake);
			this.WhiteSnakeMap = new SnakeMap (this, LayerEnum.WhiteSnake);
			this.CollisionMap = new CCollisionMap (
				this, new LayerEnum[] { LayerEnum.BlackSnake, LayerEnum.WhiteSnake});
		}

		public void Load (string levelData) {

			System.Array.Clear (this.Data, 0, this.Data.Length);
			if (levelData != null) {
				var lines = levelData.Split ('\n');
				for (int y = 0; y < this.Height; ++y) {
					var line = lines[this.Height - 1 - y];
					for (int x = 0; x < this.Width; ++x) {
						int x2 = x << 1;
						char c0 = line[x2];
						char c1 = line[x2 + 1];
						if (c0 == 'o' || c1 == 'o') {
							this.Set (LayerEnum.Item, x, y, 2);
						} else if (c0 == 'x' || c1 == 'x') {
							this.Set (LayerEnum.Item, x, y, 3);
						} else if (c0 == '-' || c1 == '-') {
							this.Set (LayerEnum.Target, x, y, 2);
						} else if (c0 == '+' || c1 == '+') {
							this.Set (LayerEnum.Target, x, y, 3);
						}
					}
				}
			}
			this.itemMap.Reset ();
			this.blackTargetMap.Reset ();
			this.whiteTargetMap.Reset ();
		}

		private int getIndex (LayerEnum layer, int x, int y) {
			return ((int)layer) * this.layerSize + y * this.Width + x;
		}
		void Set (LayerEnum layer, int x, int y, byte v) {
			int index = this.getIndex (layer, x, y);
			this.Data [index] = v;
		}
		void Add (LayerEnum layer, int x, int y, int v) {
			int index = this.getIndex (layer, x, y);
			v += (int) this.Data [index];
			this.Data [index] = (byte) v;
		}
		byte Get (LayerEnum layer, int x, int y) {
			int index = this.getIndex (layer, x, y);
			return this.Data[index];
		}
		
		class SnakeMap : ISnakeMap {
			private LayerEnum layer;
			private Map map;
			public SnakeMap (Map map, LayerEnum layer) {
				this.map = map;
				this.layer = layer;
			}
			public int Width { get { return this.map.Width; } }
			public int Height { get { return this.map.Height; } }
			public void AddCollision (int x, int y, int n) {
				this.map.Add (this.layer, x, y, n);
			}
			public int GetCollision (int x, int y) {
				return (int) this.map.Get (this.layer, x, y);
			}
		}
		
		class CItemMap : IItemMap, IItemMapData {
			private LayerEnum layer;
			private Map map;
			private List<Point2> posList;
			private bool dirty = true;

			public bool Dirty { get { return this.dirty; } set { this.dirty = value; } }
			public int Count { get { return this.posList.Count; } }

			public void GetItemAtIndex (int index, out int x, out int y, out int t) {
				var pos = this.posList[index];
				x = pos.x;
				y = pos.y;
				t = this.GetItem (x, y);
			}

			public CItemMap (Map map, LayerEnum layer) {
				this.map = map;
				this.layer = layer;
				this.Reset ();
			}

			public void Reset () {
				this.dirty = true;
				this.posList = new List<Point2> ();
				for (int x = 0; x < map.Width; ++x) {
					for (int y = 0; y < map.Height; ++y) {
						int t = this.map.Get (this.layer, x, y);
						if (t != 0) {
							this.posList.Add (new Point2(x, y));
						}
					}
				}
			}


			public void SetItem (int x, int y, int v) {
				this.dirty = true;
				this.map.Set (this.layer, x, y, (byte) v);
				var pos = new Point2 (x, y);
				if (v != 0) {
					var itemIndex = this.posList.IndexOf (pos);
					if (itemIndex < 0) {
						this.posList.Add (pos);
					}
				} else {
					this.posList.Remove (pos);
				}
			}
			public int GetItem (int x, int y) {
				return (int) this.map.Get (this.layer, x, y);
			}
			public int Width { get { return this.map.Width; } }
			public int Height { get { return this.map.Height; } }
		}

		class CCollisionMap : ICollisionMap {
			private LayerEnum[] layers;
			private Map map;
			public CCollisionMap (Map map, LayerEnum[] layers) {
				this.map = map;
				this.layers = layers;
			}
			public bool IsCollision (int x, int y) {
				for (int i = 0; i < this.layers.Length; ++i) {
					var layer = this.layers[i];
					if (this.map.Get (layer, x, y) != 0)
						return true;
				}
				return false;
			}
			public int Width { get { return this.map.Width; } }
			public int Height { get { return this.map.Height; } }
		}

		class CTargetMap : ITargetMap, ITargetMapData {
			private LayerEnum layer;
			private int targetValue;
			private Map map;
			private int targetCount;
			private List<Point2> posList;

			public int Count { get { return this.targetCount; } }
			public void GetTargetAtIndex (int index, out int x, out int y) {
				var pos = this.posList[index];
				x = pos.x;
				y = pos.y;
			}
			private bool dirty = true;
			public bool Dirty { get { return this.dirty; } set { this.dirty = value; } }

			public CTargetMap (Map map, LayerEnum layer, int targetValue) {
				this.map = map;
				this.layer = layer;
				this.targetValue = targetValue;
				this.Reset ();
			}
			public void Reset () {
				this.dirty = true;
				this.posList = new List<Point2> ();
				int targetCount = 0;
				for (int x = 0; x < map.Width; ++x) {
					for (int y = 0; y < map.Height; ++y) {
						if (this.map.Get (this.layer, x, y) == this.targetValue) {
							++targetCount;
							this.posList.Add (new Point2(x, y));
						}
					}
				}
				this.targetCount = targetCount;
			}
			public bool IsTarget (int x, int y) {
				return this.map.Get (this.layer, x, y) == this.targetValue;
			}
			public int TargetCount { get { return this.targetCount; } }
			public int Width { get { return this.map.Width; } }
			public int Height { get { return this.map.Height; } }
		}
	}
}
