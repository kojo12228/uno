#pragma warning disable 108 // new keyword hiding
#pragma warning disable 114 // new keyword hiding
namespace Windows.UI.Xaml.Controls
{
	#if __ANDROID__ || __IOS__ || NET461 || __WASM__ || __SKIA__ || __NETSTD_REFERENCE__ || __MACOS__
	[global::Uno.NotImplemented]
	#endif
	public  partial class SearchBoxQuerySubmittedEventArgs 
	{
		#if __ANDROID__ || __IOS__ || NET461 || __WASM__ || __SKIA__ || __NETSTD_REFERENCE__ || __MACOS__
		[global::Uno.NotImplemented("__ANDROID__", "__IOS__", "NET461", "__WASM__", "__SKIA__", "__NETSTD_REFERENCE__", "__MACOS__")]
		public  global::Windows.System.VirtualKeyModifiers KeyModifiers
		{
			get
			{
				throw new global::System.NotImplementedException("The member VirtualKeyModifiers SearchBoxQuerySubmittedEventArgs.KeyModifiers is not implemented. For more information, visit https://aka.platform.uno/notimplemented?m=VirtualKeyModifiers%20SearchBoxQuerySubmittedEventArgs.KeyModifiers");
			}
		}
		#endif
		#if __ANDROID__ || __IOS__ || NET461 || __WASM__ || __SKIA__ || __NETSTD_REFERENCE__ || __MACOS__
		[global::Uno.NotImplemented("__ANDROID__", "__IOS__", "NET461", "__WASM__", "__SKIA__", "__NETSTD_REFERENCE__", "__MACOS__")]
		public  string Language
		{
			get
			{
				throw new global::System.NotImplementedException("The member string SearchBoxQuerySubmittedEventArgs.Language is not implemented. For more information, visit https://aka.platform.uno/notimplemented?m=string%20SearchBoxQuerySubmittedEventArgs.Language");
			}
		}
		#endif
		#if __ANDROID__ || __IOS__ || NET461 || __WASM__ || __SKIA__ || __NETSTD_REFERENCE__ || __MACOS__
		[global::Uno.NotImplemented("__ANDROID__", "__IOS__", "NET461", "__WASM__", "__SKIA__", "__NETSTD_REFERENCE__", "__MACOS__")]
		public  global::Windows.ApplicationModel.Search.SearchQueryLinguisticDetails LinguisticDetails
		{
			get
			{
				throw new global::System.NotImplementedException("The member SearchQueryLinguisticDetails SearchBoxQuerySubmittedEventArgs.LinguisticDetails is not implemented. For more information, visit https://aka.platform.uno/notimplemented?m=SearchQueryLinguisticDetails%20SearchBoxQuerySubmittedEventArgs.LinguisticDetails");
			}
		}
		#endif
		#if __ANDROID__ || __IOS__ || NET461 || __WASM__ || __SKIA__ || __NETSTD_REFERENCE__ || __MACOS__
		[global::Uno.NotImplemented("__ANDROID__", "__IOS__", "NET461", "__WASM__", "__SKIA__", "__NETSTD_REFERENCE__", "__MACOS__")]
		public  string QueryText
		{
			get
			{
				throw new global::System.NotImplementedException("The member string SearchBoxQuerySubmittedEventArgs.QueryText is not implemented. For more information, visit https://aka.platform.uno/notimplemented?m=string%20SearchBoxQuerySubmittedEventArgs.QueryText");
			}
		}
		#endif
		// Forced skipping of method Windows.UI.Xaml.Controls.SearchBoxQuerySubmittedEventArgs.QueryText.get
		// Forced skipping of method Windows.UI.Xaml.Controls.SearchBoxQuerySubmittedEventArgs.Language.get
		// Forced skipping of method Windows.UI.Xaml.Controls.SearchBoxQuerySubmittedEventArgs.LinguisticDetails.get
		// Forced skipping of method Windows.UI.Xaml.Controls.SearchBoxQuerySubmittedEventArgs.KeyModifiers.get
	}
}
