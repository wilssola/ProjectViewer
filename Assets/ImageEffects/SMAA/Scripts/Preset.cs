using UnityEngine;
using System;

namespace Smaa
{
	// Holds a set of settings to use with SMAA passes.
	[Serializable]
	public class Preset
	{
		// Enables/Disables diagonal processing.
		public bool DiagDetection = true;

		// Enables/Disables corner detection. Leave this on to avoid blurry corners.
		public bool CornerDetection = true;

		/// <summary>
		/// Specifies the threshold or sensitivity to edges. Lowering this value you will be able to detect more edges
		/// at the expense of performance.
		/// <c>0.1</c> is a reasonable value, and allows to catch most visible edges. <c>0.05</c> is a rather overkill
		/// value, that allows to catch 'em all.
		/// </summary>
		[Range(0f, 0.5f)]
		public float Threshold = 0.1f;

		/// <summary>
		/// Specifies the threshold for depth edge detection. Lowering this value you will be able to detect more edges
		/// at the expense of performance. Only used with <see cref="SMAAEdgeDetectionMethod.Depth"/>.
		/// </summary>
		[Min(0.0001f)]
		public float DepthThreshold = 0.01f;

		/// <summary>
		/// Specifies the maximum steps performed in the horizontal/vertical pattern searches, at each side of the
		/// pixel. In number of pixels, it's actually the double. So the maximum line length perfectly handled by, for
		/// example <c>16</c> is <c>64</c> (by perfectly, we meant that longer lines won't look as good, but still
		/// antialiased).
		/// </summary>
		[Range(0, 112)]
		public int MaxSearchSteps = 16;

		/// <summary>
		/// Specifies the maximum steps performed in the diagonal pattern searches, at each side of the pixel. In this
		/// case we jump one pixel at time, instead of two.
		/// 
		/// On high-end machines it is cheap (between a 0.8x and 0.9x slower for <c>16</c> steps), but it can have a
		/// significant impact on older machines.
		/// </summary>
		[Range(0, 20)]
		public int MaxSearchStepsDiag = 8;

		/// <summary>
		/// Specifies how much sharp corners will be rounded.
		/// </summary>
		[Range(0, 100)]
		public int CornerRounding = 25;

		/// <summary>
		/// If there is an neighbor edge that has a local contrast factor times bigger contrast than current edge,
		/// current edge will be discarded.
		/// 
		/// This allows to eliminate spurious crossing edges, and is based on the fact that, if there is too much
		/// contrast in a direction, that will hide perceptually contrast in the other neighbors.
		/// 
		/// Currently unused in OpenGL.
		/// </summary>
		[Min(0f)]
		public float LocalContrastAdaptationFactor = 2f;
	}
}
