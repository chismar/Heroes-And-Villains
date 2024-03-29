using System;
using Demiurg;
using UnityEngine;
using Demiurg.Core;
using System.Collections.Generic;


namespace CoreMod
{
	public class NoiseModule :  Demiurg.Core.Avatar
	{
		public enum EdgeSide
		{
			South,
			North,
			East,
			West}

		;

		class EdgeLevel
		{
			[AConfig (1)]
			public float StartValue { get; set; }

			[AConfig (2)]
			public float EndValue { get; set; }

			[AConfig (3)]
			public float Value { get; set; }

			public int EndTile;
			public int StartTile;
		}

		class CornerValues
		{
			[AConfig ("north_east")]
			public float NEValue { get; set; }

			[AConfig ("north_west")]
			public float NWValue { get; set; }

			[AConfig ("south_east")]
			public float SEValue { get; set; }

			[AConfig ("south_west")]
			public float SWValue { get; set; }
		}

		[AOutput ("main")]
		float[,] main;
		[AConfig ("scale")]
		int scale;

		[AConfig ("south_edge_levels")]
		List<EdgeLevel> southLevels;
		[AConfig ("north_edge_levels")]
		List<EdgeLevel> northLevels;
		[AConfig ("east_edge_levels")]
		List<EdgeLevel> eastLevels;
		[AConfig ("west_edge_levels")]
		List<EdgeLevel> westLevels;

		[AConfig ("corners")]
		CornerValues corners = new CornerValues ();

		void SetRow (string message, bool horizontal, int fixedTile, List<EdgeLevel> levels, double[,] initValues)
		{
			int size = horizontal ? sizeX : sizeY;
			EdgeLevel carriedLevel;
			if (levels.Count > 0)
			{
				carriedLevel = levels [0];
			
				carriedLevel.StartTile = (int)(carriedLevel.StartValue * (float)size);
				if (carriedLevel.StartTile == 0)
					carriedLevel.StartTile = 1;
				carriedLevel.EndTile = (int)(carriedLevel.EndValue * (float)size);
				for (int i = 1; i < levels.Count; i++)
				{
					EdgeLevel level = levels [i];
					level.StartTile = (int)(level.StartValue * (float)size);
					level.EndTile = (int)(level.EndValue * (float)size);
					if (level.StartTile == carriedLevel.EndTile)
						level.StartTile = carriedLevel.EndTile + 1;
					carriedLevel = level;
				}
			}

			if (horizontal)
			{
				foreach (var level in levels)
				{
					level.Value = level.Value * 2 - 1;
					Debug.LogWarningFormat (" {3} LEVEL {4} {5} | {0} {1} -> {2}", level.StartTile, level.EndTile, level.Value, message, level.StartValue, level.EndValue);
					for (int tile = level.StartTile; tile < level.EndTile; tile++)
						initValues [tile, fixedTile] = level.Value;
				}
			} else
			{
				foreach (var level in levels)
				{
					level.Value = level.Value * 2 - 1;
					Debug.LogWarningFormat (" {3} LEVEL {4} {5} | {0} {1} -> {2}", level.StartTile, level.EndTile, level.Value, message, level.StartValue, level.EndValue);
					for (int tile = level.StartTile; tile < level.EndTile; tile++)
						initValues [fixedTile, tile] = level.Value;
				}
			}

		}

		int sizeX;
		int sizeY;

		public override void Work ()
		{ 
			sizeX = Find.Root<TilesRoot> ().MapHandle.SizeX;
			sizeY = Find.Root<TilesRoot> ().MapHandle.SizeY;
			double[,] initValues = new double[sizeX, sizeY];

			for (int y = 0; y < sizeY; y += 1)
				for (int x = 0; x < sizeX; x += 1)
				{
					initValues [x, y] = Random.NextDouble () * 2.0 - 1.0;
				}
			#region levelsCopyPaste
			SetRow ("SOUTH", true, 0, southLevels, initValues);
			SetRow ("NORTH", true, sizeY - 1, northLevels, initValues);
			SetRow ("WEST", false, 0, westLevels, initValues);
			SetRow ("EAST", false, sizeX - 1, eastLevels, initValues);
			#endregion
			if (corners.NEValue >= 0)
				initValues [sizeX - 1, 0] = corners.NEValue;
			if (corners.NWValue >= 0)
				initValues [0, 0] = corners.NWValue;
			if (corners.SEValue >= 0)
				initValues [sizeX - 1, sizeY - 1] = corners.SEValue;
			if (corners.SWValue >= 0)
				initValues [0, sizeY - 1] = corners.SWValue;
			DiamondSquare ds = new DiamondSquare (initValues, sizeX / (int)Mathf.Pow (2, scale), Random.Next ());
			main = ds.GetNormalValues ();
			FinishWork ();

		}

		class DiamondSquare
		{
			double[] values;
			int featuresize;
			int height;
			int width;
			System.Random rand;
			double[,] initValues;

			public DiamondSquare (double[,] initValues, int featuresize, int seed)
			{
				rand = new System.Random (seed);
				this.height = initValues.GetLength (1);
				this.width = initValues.GetLength (0);
				this.initValues = initValues;
				values = new double[height * width];
				for (int y = 0; y < height; y += 1)
					for (int x = 0; x < width; x += 1)
					{
						SetSample (x, y, initValues [x, y]);
					}
				this.featuresize = featuresize;
				
				int samplesize = featuresize;
				
				double scale = 1.0;
				
				while (samplesize > 1)
				{
					
					DiamondSquarePass (samplesize, scale);
					
					samplesize /= 2;
					scale /= 2.0;
				}
				
			}

			public float[,] GetNormalValues ()
			{
				float[,] dimensionalValues = new float[width, height];
				double maxValue = int.MinValue;
				double minValue = int.MaxValue;
				for (int i = 0; i < width; i++)
					for (int j = 0; j < height; j++)
					{
						if (values [i + j * height] > maxValue)
							maxValue = values [i + j * height];
						if (values [i + j * height] < minValue)
							minValue = values [i + j * height];
					}
				
				float fMaxValue = (float)maxValue;
				float fMinValue = (float)minValue;
				for (int i = 0; i < width; i++)
				{
					
					for (int j = 0; j < height; j++)
					{
						dimensionalValues [i, j] = Mathf.InverseLerp (fMinValue, fMaxValue, (float)GetSample (i, j));
						
					}
				}
				
				
				return dimensionalValues;
			}

			public float[,] GetFloatValues ()
			{
				float[,] dimensionalValues = new float[width, height];
				for (int i = 0; i < width; i++)
				{
					for (int j = 0; j < height; j++)
					{
						dimensionalValues [i, j] = (float)GetSample (i, j);
						
					}
				}
				
				
				return dimensionalValues;
			}


			//			double GetSample (int x, int y)
			//			{
			//				x = x & (width - 1);
			//				if (y >= height)
			//					return values[x + ]
			//				else if(y < 0)
			//				return values [x + y * width];
			//			}
			//
			//			void SetSample (int x, int y, double value)
			//			{
			//				x = x & (width - 1);
			//				if (y >= height || y < 0)
			//					return;
			//				values [x + y * width] = value;
			//			}

			double GetSample (int x, int y)
			{
				bool edge = false;
				if (x >= width)
				{
					edge = true;
					x = width - 1;
				} else if (x < 0)
				{
					edge = true;
					x = 0;
				}
				if (y >= height)
				{
					edge = true;
					y = height - 1;
				} else if (y < 0)
				{
					edge = true;
					y = 0;
				}
				if (edge)
					return initValues [x, y];
				return values [x + y * width];
			}

			void SetSample (int x, int y, double value)
			{

				if (x >= width)
					return;
				if (x < 0)
					return;
				if (y >= height)
					return;
				if (y < 0)
					return;
				values [x + y * width] = value;
			}

			void ComputerSquare (int x, int y, int size, double value)
			{
				int hs = size / 2;
				
				// a     b 
				//
				//    x
				//
				// c     d
				
				double a = GetSample (x - hs, y - hs);
				double b = GetSample (x + hs, y - hs);
				double c = GetSample (x - hs, y + hs);
				double d = GetSample (x + hs, y + hs);
				
				SetSample (x, y, ((a + b + c + d) / 4.0) + value);
				
			}

			void ComputeDiamond (int x, int y, int size, double value)
			{
				int hs = size / 2;
				
				//   c
				//
				//a  x  b
				//
				//   d
				
				double a = GetSample (x - hs, y);
				double b = GetSample (x + hs, y);
				double c = GetSample (x, y - hs);
				double d = GetSample (x, y + hs);
				
				SetSample (x, y, ((a + b + c + d) / 4.0) + value);
			}

			void DiamondSquarePass (int stepsize, double scale)
			{
				
				int halfstep = stepsize / 2;
				
				for (int y = halfstep; y < height + halfstep; y += stepsize)
				{
					for (int x = halfstep; x < width + halfstep; x += stepsize)
					{
						ComputerSquare (x, y, stepsize, (rand.NextDouble () * 2.0 - 1.0) * scale);
					}
				}
				
				for (int y = 0; y < height; y += stepsize)
				{
					for (int x = 0; x < width; x += stepsize)
					{
						ComputeDiamond (x + halfstep, y, stepsize, (rand.NextDouble () * 2.0 - 1.0) * scale);
						ComputeDiamond (x, y + halfstep, stepsize, (rand.NextDouble () * 2.0 - 1.0) * scale);
					}
				}
				
			}
		}
	}

}

