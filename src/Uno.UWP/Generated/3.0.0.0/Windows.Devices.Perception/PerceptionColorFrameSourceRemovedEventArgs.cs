#pragma warning disable 108 // new keyword hiding
#pragma warning disable 114 // new keyword hiding
namespace Windows.Devices.Perception
{
	#if __ANDROID__ || __IOS__ || NET461 || __WASM__ || __SKIA__ || __NETSTD_REFERENCE__ || __MACOS__
	[global::Uno.NotImplemented]
	#endif
	public  partial class PerceptionColorFrameSourceRemovedEventArgs 
	{
		#if __ANDROID__ || __IOS__ || NET461 || __WASM__ || __SKIA__ || __NETSTD_REFERENCE__ || __MACOS__
		[global::Uno.NotImplemented("__ANDROID__", "__IOS__", "NET461", "__WASM__", "__SKIA__", "__NETSTD_REFERENCE__", "__MACOS__")]
		public  global::Windows.Devices.Perception.PerceptionColorFrameSource FrameSource
		{
			get
			{
				throw new global::System.NotImplementedException("The member PerceptionColorFrameSource PerceptionColorFrameSourceRemovedEventArgs.FrameSource is not implemented. For more information, visit https://aka.platform.uno/notimplemented?m=PerceptionColorFrameSource%20PerceptionColorFrameSourceRemovedEventArgs.FrameSource");
			}
		}
		#endif
		// Forced skipping of method Windows.Devices.Perception.PerceptionColorFrameSourceRemovedEventArgs.FrameSource.get
	}
}
