// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Specialized;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Uno.Extensions;
using static Microsoft.UI.Xaml.Controls._Tracing;

namespace Microsoft.UI.Xaml.Controls
{
	public partial class StackLayout : VirtualizingLayout, IFlowLayoutAlgorithmDelegates
	{
		private double m_itemSpacing;

		#region IFlowLayout

		public StackLayout()
		{
			//__RP_Marker_ClassById(RuntimeProfiler.ProfId_StackLayout);
			LayoutId = "StackLayout";
		}

		#endregion

		private StackLayoutState GetAsStackState(object state)
			=> state as StackLayoutState;

		private void InvalidateLayout()
			=> base.InvalidateMeasure();

		FlowLayoutAlgorithm GetFlowAlgorithm(VirtualizingLayoutContext context)
			=> GetAsStackState(context.LayoutState).FlowAlgorithm;

		private bool DoesRealizationWindowOverlapExtent(Rect realizationWindow, Rect extent)
			=> MajorEnd(realizationWindow) >= MajorStart(extent) && MajorStart(realizationWindow) <= MajorEnd(extent);

		#region IVirtualizingLayoutOverrides
		protected internal override void InitializeForContextCore(VirtualizingLayoutContext context)
		{
			var state = context.LayoutState;
			StackLayoutState stackState = default;
			if (state != null)
			{
				stackState = GetAsStackState(state);
			}

			if (stackState == null)
			{
				if (state != null)
				{
					throw new InvalidOperationException("LayoutState must derive from StackLayoutState.");
				}

				// Custom deriving layouts could potentially be stateful.
				// If that is the case, we will just create the base state required by UniformGridLayout ourselves.
				stackState = new StackLayoutState();
			}

			stackState.InitializeForContext(context, this);
		}

		protected internal override void UninitializeForContextCore(VirtualizingLayoutContext context)
		{
			var stackState = GetAsStackState(context.LayoutState);
			stackState.UninitializeForContext(context);
		}

		protected internal override Size MeasureOverride(VirtualizingLayoutContext context, Size availableSize)
		{
			GetAsStackState(context.LayoutState).OnMeasureStart();

			var algo = GetFlowAlgorithm(context);
			var desiredSize = algo.Measure(
				availableSize,
				context,
				false, /* isWrapping*/
				0 /* minItemSpacing */,
				m_itemSpacing,
				uint.MaxValue /* maxItemsPerLine */,
				ScrollOrientation,
				DisableVirtualization,
				LayoutId);

			// Uno workaround [BEGIN]: Keep track of realized items count for viewport invalidation optimization
			_uno_lastKnownRealizedElementsCount = algo.RealizedElementCount;
			// Uno workaround [END]

			return desiredSize;
		}

		protected internal override Size ArrangeOverride(VirtualizingLayoutContext context, Size finalSize)
		{
			var value = GetFlowAlgorithm(context).Arrange(
				finalSize,
				context,
				false, /* isWraping */
				FlowLayoutLineAlignment.Start,
				LayoutId);

			return value;
		}

		protected internal override void OnItemsChangedCore(VirtualizingLayoutContext context, object source, NotifyCollectionChangedEventArgs args)
		{
			GetFlowAlgorithm(context).OnItemsSourceChanged(source, args, context);
			// Always invalidate layout to keep the view accurate.
			InvalidateLayout();
		}

		#endregion

		#region IStackLayoutOverrides

		FlowLayoutAnchorInfo GetAnchorForRealizationRect(Size availableSize, VirtualizingLayoutContext context)
		{

			int anchorIndex = -1;
			double offset = double.NaN;

			// Constants
			int itemsCount = context.ItemCount;
			if (itemsCount > 0)
			{
				var realizationRect = context.RealizationRect;
				var state = GetAsStackState(context.LayoutState);
				var lastExtent = state.FlowAlgorithm.LastExtent;

				double averageElementSize = GetAverageElementSize(availableSize, context, state) + m_itemSpacing;
				double realizationWindowOffsetInExtent = MajorStart(realizationRect) - MajorStart(lastExtent);
				double majorSize = MajorSize(lastExtent) == 0 ? Math.Max(0.0, averageElementSize * itemsCount - m_itemSpacing) : MajorSize(lastExtent);
				if (itemsCount > 0 &&
					MajorSize(realizationRect) >= 0 &&
					// MajorSize = 0 will account for when a nested repeater is outside the realization rect but still being measured. Also,
					// note that if we are measuring this repeater, then we are already realizing an element to figure out the size, so we could
					// just keep that element alive. It also helps in XYFocus scenarios to have an element realized for XYFocus to find a candidate
					// in the navigating direction.
					realizationWindowOffsetInExtent + MajorSize(realizationRect) >= 0 && realizationWindowOffsetInExtent <= majorSize)
				{
					anchorIndex = (int)(realizationWindowOffsetInExtent / averageElementSize);
					offset = anchorIndex * averageElementSize + MajorStart(lastExtent);
					anchorIndex = Math.Max(0, Math.Min(itemsCount - 1, anchorIndex));
				}
			}

			return new FlowLayoutAnchorInfo(anchorIndex, offset);
		}

		Rect GetExtent(
			Size availableSize,
			VirtualizingLayoutContext context,
			UIElement firstRealized,
			int firstRealizedItemIndex,
			Rect firstRealizedLayoutBounds,
			UIElement lastRealized,
			int lastRealizedItemIndex,
			Rect lastRealizedLayoutBounds)
		{
			var extent = default(Rect);

			// Constants
			int itemsCount = context.ItemCount;
			var stackState = GetAsStackState(context.LayoutState);
			double averageElementSize = GetAverageElementSize(availableSize, context, stackState) + m_itemSpacing;

			SetMinorSize(ref extent, (float)(stackState.MaxArrangeBounds));
			SetMajorSize(ref extent, Math.Max(0.0f, (float)(itemsCount * averageElementSize - m_itemSpacing)));
			if (itemsCount > 0)
			{
				if (firstRealized != null)
				{
					MUX_ASSERT(lastRealized != null);
					SetMajorStart(ref extent, (float)(MajorStart(firstRealizedLayoutBounds) - firstRealizedItemIndex * averageElementSize));
					var remainingItems = itemsCount - lastRealizedItemIndex - 1;
					SetMajorSize(ref extent, MajorEnd(lastRealizedLayoutBounds) - MajorStart(extent) + (float)(remainingItems * averageElementSize));
				}
				else
				{
					REPEATER_TRACE_INFO("%*s: \tEstimating extent with no realized elements.  \n",
						context.Indent,
						LayoutId);
				}
			}
			else
			{
				MUX_ASSERT(firstRealizedItemIndex == -1);
				MUX_ASSERT(lastRealizedItemIndex == -1);
			}

			REPEATER_TRACE_INFO("%*s: \tExtent is (%.0f,%.0f). Based on average %.0f. \n",
				context.Indent,
				LayoutId, extent.Width, extent.Height, averageElementSize);
			return extent;
		}

		void OnElementMeasured(
			UIElement element,
			int index,
			Size availableSize,
			Size measureSize,
			Size desiredSize,
			Size provisionalArrangeSize,
			VirtualizingLayoutContext context)
		{
			if (context is VirtualizingLayoutContext virtualContext)
			{
				var stackState = GetAsStackState(virtualContext.LayoutState);
				var provisionalArrangeSizeWinRt = provisionalArrangeSize;
				stackState.OnElementMeasured(
					index,
					Major(provisionalArrangeSizeWinRt),
					Minor(provisionalArrangeSizeWinRt));
			}
		}

		#endregion

		#region IFlowLayoutAlgorithmDelegates

		Size IFlowLayoutAlgorithmDelegates.Algorithm_GetMeasureSize(int index, Size availableSize, VirtualizingLayoutContext context)
		{
			return availableSize;
		}

		Size IFlowLayoutAlgorithmDelegates.Algorithm_GetProvisionalArrangeSize(int index, Size measureSize, Size desiredSize, VirtualizingLayoutContext context)
		{
			var measureSizeMinor = Minor(measureSize);
			return MinorMajorSize(
				(float)(measureSizeMinor.IsFinite() ? Math.Max(measureSizeMinor, Minor(desiredSize)) : Minor(desiredSize)),
				(float)Major(desiredSize));
		}

		bool IFlowLayoutAlgorithmDelegates.Algorithm_ShouldBreakLine(int index, double remainingSpace)
		{
			return true;
		}

		FlowLayoutAnchorInfo IFlowLayoutAlgorithmDelegates.Algorithm_GetAnchorForRealizationRect(Size availableSize, VirtualizingLayoutContext context)
		{
			return GetAnchorForRealizationRect(availableSize, context);
		}

		FlowLayoutAnchorInfo IFlowLayoutAlgorithmDelegates.Algorithm_GetAnchorForTargetElement(int targetIndex, Size availableSize, VirtualizingLayoutContext context)
		{
			double offset = double.NaN;
			int index = -1;
			int itemsCount = context.ItemCount;

			if (targetIndex >= 0 && targetIndex < itemsCount)
			{
				index = targetIndex;
				var state = GetAsStackState(context.LayoutState);
				double averageElementSize = GetAverageElementSize(availableSize, context, state) + m_itemSpacing;
				offset = index * averageElementSize + MajorStart(state.FlowAlgorithm.LastExtent);
			}

			return new FlowLayoutAnchorInfo(index, offset);
		}

		Rect IFlowLayoutAlgorithmDelegates.Algorithm_GetExtent(
			Size availableSize,
			VirtualizingLayoutContext context,
			UIElement firstRealized,
			int firstRealizedItemIndex,
			Rect firstRealizedLayoutBounds,
			UIElement lastRealized,
			int lastRealizedItemIndex,
			Rect lastRealizedLayoutBounds)
		{
			return GetExtent(
				availableSize,
				context,
				firstRealized,
				firstRealizedItemIndex,
				firstRealizedLayoutBounds,
				lastRealized,
				lastRealizedItemIndex,
				lastRealizedLayoutBounds);
		}

		void IFlowLayoutAlgorithmDelegates.Algorithm_OnElementMeasured(
			UIElement element,
			int index,
			Size availableSize,
			Size measureSize,
			Size desiredSize,
			Size provisionalArrangeSize,
			VirtualizingLayoutContext context)
		{
			OnElementMeasured(
				element,
				index,
				availableSize,
				measureSize,
				desiredSize,
				provisionalArrangeSize,
				context);
		}

		public void Algorithm_OnLineArranged(int startIndex, int countInLine, double lineSize, VirtualizingLayoutContext context)
		{
		}
		#endregion

		void OnPropertyChanged(DependencyPropertyChangedEventArgs args)
		{
			var property = args.Property;
			if (property == OrientationProperty)
			{
				var orientation = (Orientation)args.NewValue;

				//Note: For StackLayout Vertical Orientation means we have a Vertical ScrollOrientation.
				//Horizontal Orientation means we have a Horizontal ScrollOrientation.
				ScrollOrientation scrollOrientation = (orientation == Orientation.Horizontal) ? ScrollOrientation.Horizontal : ScrollOrientation.Vertical;
				ScrollOrientation = scrollOrientation;
			}
			else if (property == SpacingProperty)
			{
				m_itemSpacing = (double)args.NewValue;
			}

			InvalidateLayout();
		}

		#region private helpers

		double GetAverageElementSize(
			Size availableSize,
			VirtualizingLayoutContext context,
			StackLayoutState stackLayoutState)
		{
			double averageElementSize = 0;

			if (context.ItemCount > 0)
			{
				if (stackLayoutState.TotalElementsMeasured == 0)
				{
					var tmpElement = context.GetOrCreateElementAt(0, ElementRealizationOptions.ForceCreate | ElementRealizationOptions.SuppressAutoRecycle);
					stackLayoutState.FlowAlgorithm.MeasureElement(tmpElement, 0, availableSize, context);
					context.RecycleElement(tmpElement);
				}

				MUX_ASSERT(stackLayoutState.TotalElementsMeasured > 0);
				averageElementSize = Math.Round(stackLayoutState.TotalElementSize / stackLayoutState.TotalElementsMeasured);
			}

			return _uno_lastKnownAverageElementSize = averageElementSize;
		}



		#endregion

		#region Uno workaround
		private double _uno_lastKnownAverageElementSize;
		private double _uno_lastKnownRealizedElementsCount;

		/// <inheritdoc />
		protected internal override bool IsSignificantViewportChange(Rect oldViewport, Rect newViewport)
		{
			var elementSize = _uno_lastKnownAverageElementSize;
			if (elementSize <= 0)
			{
				return base.IsSignificantViewportChange(oldViewport, newViewport);
			}

			if (_uno_lastKnownRealizedElementsCount <= 1)
			{
				// Only the first item has been measured so far, this might be because the IR is within a SV and not visible yet.
				// Note: In that case we have to make sure to not only validate the Major axis since the parent SV could be vertical while local layout itself is horizontal.
				// Note2: Depending of the platform (Android), we might be invoked with empty viewport, make sure to consider out-of-bound in such case.
				// Test case: When_NestedInSVAndOutOfViewportOnInitialLoad_Then_MaterializedEvenWhenScrollingOnMinorAxis
				const int threshold = 100; // Allows 100px above and after to trigger loading even before IR is visible.
				var wasOutOfBounds = oldViewport is { Width: 0 } or { Height: 0 }
					|| MajorEnd(oldViewport) < -threshold
					|| MajorStart(oldViewport) > MajorSize(oldViewport) + threshold
					|| MinorEnd(oldViewport) < -threshold
					|| MinorStart(oldViewport) > MinorSize(oldViewport) + threshold;
				var isOutOfBounds = newViewport is { Width: 0 } or { Height: 0 }
					|| MinorEnd(newViewport) < -threshold
					|| MinorStart(newViewport) > MinorSize(newViewport) + threshold
					|| MajorEnd(newViewport) < -threshold
					|| MajorStart(newViewport) > MajorSize(newViewport) + threshold;

				return wasOutOfBounds && !isOutOfBounds;
			}

			var size = Math.Max(MajorSize(oldViewport), MajorSize(newViewport));
			var minDelta = Math.Min(elementSize * 5, size);

			return Math.Abs(MajorStart(oldViewport) - MajorStart(newViewport)) > minDelta
				|| Math.Abs(MajorEnd(oldViewport) - MajorEnd(newViewport)) > minDelta;
		}
		#endregion
	}
}
