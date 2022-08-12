﻿using System;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Uno;

namespace Windows.UI.Xaml.Media.Imaging;

/// <summary>
/// Provides a source object for properties that use a Scalable Vector Graphics (SVG) source. You can define a SvgImageSource
/// by using a Uniform Resource Identifier (URI) that references a SVG file, or by calling SetSourceAsync(IRandomAccessStream)
/// and supplying a stream.
/// </summary>
public partial class SvgImageSource : ImageSource
{
	private SvgImageSourceLoadStatus? _lastStatus;

#if __NETSTD__
	private IRandomAccessStream _stream;
#endif

	/// <summary>
	/// Initializes a new instance of the SvgImageSource class.
	/// </summary>
	public SvgImageSource()
	{
		InitPartial();
	}

	/// <summary>
	/// Initializes a new instance of the SvgImageSource class, using the supplied Uniform Resource Identifier (URI).
	/// </summary>
	/// <param name="uriSource"></param>
	public SvgImageSource(Uri uriSource)
	{
		UriSource = uriSource;

		InitPartial();
	}
	
	private void OnUriSourceChanged(DependencyPropertyChangedEventArgs e)
	{
		if (!object.Equals(e.OldValue, e.NewValue))
		{
			UnloadImageData();
		}
		InitFromUri(e.NewValue as Uri);
#if __NETSTD__
		InvalidateSource();
#endif
	}

	public IAsyncOperation<SvgImageSourceLoadStatus> SetSourceAsync(IRandomAccessStream streamSource)
	{
		async Task<SvgImageSourceLoadStatus> SetSourceAsync(
			CancellationToken ct,
			AsyncOperation<SvgImageSourceLoadStatus> _)
		{
			if (streamSource == null)
			{
				//Same behavior as windows, although the documentation does not mention it!!!
				throw new ArgumentException(nameof(streamSource));
			}

			_lastStatus = null;

#if __NETSTD__
			_stream = streamSource.CloneStream();

			var tcs = new TaskCompletionSource<SvgImageSourceLoadStatus>();

			using var x = Subscribe(OnChanged);

			InvalidateSource();

			return await tcs.Task;

			void OnChanged(ImageData data)
			{
				tcs.TrySetResult(_lastStatus ?? SvgImageSourceLoadStatus.Other);
			}
#else
			Stream = streamSource.CloneStream().AsStream();
			return SvgImageSourceLoadStatus.Success;
#endif
		}

		return AsyncOperation<SvgImageSourceLoadStatus>.FromTask(SetSourceAsync);
	}

	partial void InitPartial();

	private void RaiseImageFailed(SvgImageSourceLoadStatus loadStatus)
	{
		_lastStatus = loadStatus;
		OpenFailed?.Invoke(this, new SvgImageSourceFailedEventArgs(loadStatus));
	}

	private void RaiseImageOpened()
	{
		_lastStatus = SvgImageSourceLoadStatus.Success;
		Opened?.Invoke(this, new SvgImageSourceOpenedEventArgs());
	}
}
