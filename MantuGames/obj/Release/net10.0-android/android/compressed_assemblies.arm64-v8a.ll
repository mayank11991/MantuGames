; ModuleID = 'compressed_assemblies.arm64-v8a.ll'
source_filename = "compressed_assemblies.arm64-v8a.ll"
target datalayout = "e-m:e-i8:8:32-i16:16:32-i64:64-i128:128-n32:64-S128"
target triple = "aarch64-unknown-linux-android21"

%struct.CompressedAssemblyDescriptor = type {
	i32, ; uint32_t uncompressed_file_size
	i1, ; bool loaded
	i32 ; uint32_t buffer_offset
}

@compressed_assembly_count = dso_local local_unnamed_addr constant i32 136, align 4

@compressed_assembly_descriptors = dso_local local_unnamed_addr global [136 x %struct.CompressedAssemblyDescriptor] [
	%struct.CompressedAssemblyDescriptor {
		i32 15624, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 0; uint32_t buffer_offset
	}, ; 0: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15632, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 15624; uint32_t buffer_offset
	}, ; 1: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15624, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 31256; uint32_t buffer_offset
	}, ; 2: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15624, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 46880; uint32_t buffer_offset
	}, ; 3: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15632, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 62504; uint32_t buffer_offset
	}, ; 4: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15632, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 78136; uint32_t buffer_offset
	}, ; 5: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15632, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 93768; uint32_t buffer_offset
	}, ; 6: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15624, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 109400; uint32_t buffer_offset
	}, ; 7: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15624, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 125024; uint32_t buffer_offset
	}, ; 8: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15632, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 140648; uint32_t buffer_offset
	}, ; 9: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15624, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 156280; uint32_t buffer_offset
	}, ; 10: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15624, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 171904; uint32_t buffer_offset
	}, ; 11: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15624, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 187528; uint32_t buffer_offset
	}, ; 12: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15624, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 203152; uint32_t buffer_offset
	}, ; 13: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15624, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 218776; uint32_t buffer_offset
	}, ; 14: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15624, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 234400; uint32_t buffer_offset
	}, ; 15: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15624, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 250024; uint32_t buffer_offset
	}, ; 16: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15624, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 265648; uint32_t buffer_offset
	}, ; 17: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15632, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 281272; uint32_t buffer_offset
	}, ; 18: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15664, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 296904; uint32_t buffer_offset
	}, ; 19: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15624, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 312568; uint32_t buffer_offset
	}, ; 20: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15632, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 328192; uint32_t buffer_offset
	}, ; 21: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15632, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 343824; uint32_t buffer_offset
	}, ; 22: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15632, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 359456; uint32_t buffer_offset
	}, ; 23: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15672, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 375088; uint32_t buffer_offset
	}, ; 24: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15632, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 390760; uint32_t buffer_offset
	}, ; 25: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15664, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 406392; uint32_t buffer_offset
	}, ; 26: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15624, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 422056; uint32_t buffer_offset
	}, ; 27: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15624, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 437680; uint32_t buffer_offset
	}, ; 28: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15624, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 453304; uint32_t buffer_offset
	}, ; 29: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15624, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 468928; uint32_t buffer_offset
	}, ; 30: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15664, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 484552; uint32_t buffer_offset
	}, ; 31: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15624, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 500216; uint32_t buffer_offset
	}, ; 32: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15632, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 515840; uint32_t buffer_offset
	}, ; 33: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 6144, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 531472; uint32_t buffer_offset
	}, ; 34: _Microsoft.Android.Resource.Designer
	%struct.CompressedAssemblyDescriptor {
		i32 14848, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 537616; uint32_t buffer_offset
	}, ; 35: Microsoft.Extensions.Configuration
	%struct.CompressedAssemblyDescriptor {
		i32 6656, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 552464; uint32_t buffer_offset
	}, ; 36: Microsoft.Extensions.Configuration.Abstractions
	%struct.CompressedAssemblyDescriptor {
		i32 46592, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 559120; uint32_t buffer_offset
	}, ; 37: Microsoft.Extensions.DependencyInjection
	%struct.CompressedAssemblyDescriptor {
		i32 26624, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 605712; uint32_t buffer_offset
	}, ; 38: Microsoft.Extensions.DependencyInjection.Abstractions
	%struct.CompressedAssemblyDescriptor {
		i32 8192, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 632336; uint32_t buffer_offset
	}, ; 39: Microsoft.Extensions.Diagnostics.Abstractions
	%struct.CompressedAssemblyDescriptor {
		i32 7168, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 640528; uint32_t buffer_offset
	}, ; 40: Microsoft.Extensions.FileProviders.Abstractions
	%struct.CompressedAssemblyDescriptor {
		i32 6144, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 647696; uint32_t buffer_offset
	}, ; 41: Microsoft.Extensions.Hosting.Abstractions
	%struct.CompressedAssemblyDescriptor {
		i32 17920, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 653840; uint32_t buffer_offset
	}, ; 42: Microsoft.Extensions.Logging
	%struct.CompressedAssemblyDescriptor {
		i32 19456, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 671760; uint32_t buffer_offset
	}, ; 43: Microsoft.Extensions.Logging.Abstractions
	%struct.CompressedAssemblyDescriptor {
		i32 16896, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 691216; uint32_t buffer_offset
	}, ; 44: Microsoft.Extensions.Options
	%struct.CompressedAssemblyDescriptor {
		i32 9216, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 708112; uint32_t buffer_offset
	}, ; 45: Microsoft.Extensions.Primitives
	%struct.CompressedAssemblyDescriptor {
		i32 1928504, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 717328; uint32_t buffer_offset
	}, ; 46: Microsoft.Maui.Controls
	%struct.CompressedAssemblyDescriptor {
		i32 135432, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 2645832; uint32_t buffer_offset
	}, ; 47: Microsoft.Maui.Controls.Xaml
	%struct.CompressedAssemblyDescriptor {
		i32 861696, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 2781264; uint32_t buffer_offset
	}, ; 48: Microsoft.Maui
	%struct.CompressedAssemblyDescriptor {
		i32 58880, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 3642960; uint32_t buffer_offset
	}, ; 49: Microsoft.Maui.Essentials
	%struct.CompressedAssemblyDescriptor {
		i32 208696, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 3701840; uint32_t buffer_offset
	}, ; 50: Microsoft.Maui.Graphics
	%struct.CompressedAssemblyDescriptor {
		i32 40448, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 3910536; uint32_t buffer_offset
	}, ; 51: Plugin.InAppBilling
	%struct.CompressedAssemblyDescriptor {
		i32 52736, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 3950984; uint32_t buffer_offset
	}, ; 52: Plugin.Maui.Audio
	%struct.CompressedAssemblyDescriptor {
		i32 100864, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 4003720; uint32_t buffer_offset
	}, ; 53: Plugin.MauiMtAdmob
	%struct.CompressedAssemblyDescriptor {
		i32 124928, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 4104584; uint32_t buffer_offset
	}, ; 54: Xamarin.Android.Google.BillingClient
	%struct.CompressedAssemblyDescriptor {
		i32 77312, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 4229512; uint32_t buffer_offset
	}, ; 55: Xamarin.AndroidX.Activity
	%struct.CompressedAssemblyDescriptor {
		i32 583680, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 4306824; uint32_t buffer_offset
	}, ; 56: Xamarin.AndroidX.AppCompat
	%struct.CompressedAssemblyDescriptor {
		i32 17920, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 4890504; uint32_t buffer_offset
	}, ; 57: Xamarin.AndroidX.AppCompat.AppCompatResources
	%struct.CompressedAssemblyDescriptor {
		i32 104960, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 4908424; uint32_t buffer_offset
	}, ; 58: Xamarin.AndroidX.Browser
	%struct.CompressedAssemblyDescriptor {
		i32 18944, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 5013384; uint32_t buffer_offset
	}, ; 59: Xamarin.AndroidX.CardView
	%struct.CompressedAssemblyDescriptor {
		i32 22528, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 5032328; uint32_t buffer_offset
	}, ; 60: Xamarin.AndroidX.Collection.Jvm
	%struct.CompressedAssemblyDescriptor {
		i32 78336, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 5054856; uint32_t buffer_offset
	}, ; 61: Xamarin.AndroidX.CoordinatorLayout
	%struct.CompressedAssemblyDescriptor {
		i32 626176, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 5133192; uint32_t buffer_offset
	}, ; 62: Xamarin.AndroidX.Core
	%struct.CompressedAssemblyDescriptor {
		i32 26624, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 5759368; uint32_t buffer_offset
	}, ; 63: Xamarin.AndroidX.CursorAdapter
	%struct.CompressedAssemblyDescriptor {
		i32 9728, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 5785992; uint32_t buffer_offset
	}, ; 64: Xamarin.AndroidX.CustomView
	%struct.CompressedAssemblyDescriptor {
		i32 47104, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 5795720; uint32_t buffer_offset
	}, ; 65: Xamarin.AndroidX.DrawerLayout
	%struct.CompressedAssemblyDescriptor {
		i32 236032, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 5842824; uint32_t buffer_offset
	}, ; 66: Xamarin.AndroidX.Fragment
	%struct.CompressedAssemblyDescriptor {
		i32 23552, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 6078856; uint32_t buffer_offset
	}, ; 67: Xamarin.AndroidX.Lifecycle.Common.Jvm
	%struct.CompressedAssemblyDescriptor {
		i32 18944, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 6102408; uint32_t buffer_offset
	}, ; 68: Xamarin.AndroidX.Lifecycle.LiveData.Core
	%struct.CompressedAssemblyDescriptor {
		i32 32768, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 6121352; uint32_t buffer_offset
	}, ; 69: Xamarin.AndroidX.Lifecycle.ViewModel.Android
	%struct.CompressedAssemblyDescriptor {
		i32 13824, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 6154120; uint32_t buffer_offset
	}, ; 70: Xamarin.AndroidX.Lifecycle.ViewModelSavedState.Android
	%struct.CompressedAssemblyDescriptor {
		i32 39424, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 6167944; uint32_t buffer_offset
	}, ; 71: Xamarin.AndroidX.Loader
	%struct.CompressedAssemblyDescriptor {
		i32 92672, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 6207368; uint32_t buffer_offset
	}, ; 72: Xamarin.AndroidX.Navigation.Common.Android
	%struct.CompressedAssemblyDescriptor {
		i32 19456, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 6300040; uint32_t buffer_offset
	}, ; 73: Xamarin.AndroidX.Navigation.Fragment
	%struct.CompressedAssemblyDescriptor {
		i32 65536, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 6319496; uint32_t buffer_offset
	}, ; 74: Xamarin.AndroidX.Navigation.Runtime.Android
	%struct.CompressedAssemblyDescriptor {
		i32 27136, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 6385032; uint32_t buffer_offset
	}, ; 75: Xamarin.AndroidX.Navigation.UI
	%struct.CompressedAssemblyDescriptor {
		i32 34816, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 6412168; uint32_t buffer_offset
	}, ; 76: Xamarin.AndroidX.NavigationEvent.Android
	%struct.CompressedAssemblyDescriptor {
		i32 457728, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 6446984; uint32_t buffer_offset
	}, ; 77: Xamarin.AndroidX.RecyclerView
	%struct.CompressedAssemblyDescriptor {
		i32 12288, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 6904712; uint32_t buffer_offset
	}, ; 78: Xamarin.AndroidX.SavedState.SavedState.Android
	%struct.CompressedAssemblyDescriptor {
		i32 41984, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 6917000; uint32_t buffer_offset
	}, ; 79: Xamarin.AndroidX.SwipeRefreshLayout
	%struct.CompressedAssemblyDescriptor {
		i32 62976, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 6958984; uint32_t buffer_offset
	}, ; 80: Xamarin.AndroidX.ViewPager
	%struct.CompressedAssemblyDescriptor {
		i32 40448, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 7021960; uint32_t buffer_offset
	}, ; 81: Xamarin.AndroidX.ViewPager2
	%struct.CompressedAssemblyDescriptor {
		i32 191488, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 7062408; uint32_t buffer_offset
	}, ; 82: Xamarin.Essentials
	%struct.CompressedAssemblyDescriptor {
		i32 675840, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 7253896; uint32_t buffer_offset
	}, ; 83: Xamarin.Google.Android.Material
	%struct.CompressedAssemblyDescriptor {
		i32 45056, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 7929736; uint32_t buffer_offset
	}, ; 84: Xamarin.Google.UserMessagingPlatform
	%struct.CompressedAssemblyDescriptor {
		i32 305152, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 7974792; uint32_t buffer_offset
	}, ; 85: Xamarin.GooglePlayServices.Ads.Lite
	%struct.CompressedAssemblyDescriptor {
		i32 10752, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 8279944; uint32_t buffer_offset
	}, ; 86: Xamarin.GooglePlayServices.Basement
	%struct.CompressedAssemblyDescriptor {
		i32 90624, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 8290696; uint32_t buffer_offset
	}, ; 87: Xamarin.Kotlin.StdLib
	%struct.CompressedAssemblyDescriptor {
		i32 28672, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 8381320; uint32_t buffer_offset
	}, ; 88: Xamarin.KotlinX.Coroutines.Core.Jvm
	%struct.CompressedAssemblyDescriptor {
		i32 91648, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 8409992; uint32_t buffer_offset
	}, ; 89: Xamarin.KotlinX.Serialization.Core.Jvm
	%struct.CompressedAssemblyDescriptor {
		i32 432128, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 8501640; uint32_t buffer_offset
	}, ; 90: MantuGames
	%struct.CompressedAssemblyDescriptor {
		i32 24576, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 8933768; uint32_t buffer_offset
	}, ; 91: System.Collections.Concurrent
	%struct.CompressedAssemblyDescriptor {
		i32 17408, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 8958344; uint32_t buffer_offset
	}, ; 92: System.Collections.NonGeneric
	%struct.CompressedAssemblyDescriptor {
		i32 10752, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 8975752; uint32_t buffer_offset
	}, ; 93: System.Collections.Specialized
	%struct.CompressedAssemblyDescriptor {
		i32 31744, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 8986504; uint32_t buffer_offset
	}, ; 94: System.Collections
	%struct.CompressedAssemblyDescriptor {
		i32 12288, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 9018248; uint32_t buffer_offset
	}, ; 95: System.ComponentModel.Primitives
	%struct.CompressedAssemblyDescriptor {
		i32 99328, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 9030536; uint32_t buffer_offset
	}, ; 96: System.ComponentModel.TypeConverter
	%struct.CompressedAssemblyDescriptor {
		i32 5120, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 9129864; uint32_t buffer_offset
	}, ; 97: System.ComponentModel
	%struct.CompressedAssemblyDescriptor {
		i32 12288, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 9134984; uint32_t buffer_offset
	}, ; 98: System.Console
	%struct.CompressedAssemblyDescriptor {
		i32 5120, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 9147272; uint32_t buffer_offset
	}, ; 99: System.Core
	%struct.CompressedAssemblyDescriptor {
		i32 40448, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 9152392; uint32_t buffer_offset
	}, ; 100: System.Diagnostics.DiagnosticSource
	%struct.CompressedAssemblyDescriptor {
		i32 18432, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 9192840; uint32_t buffer_offset
	}, ; 101: System.Diagnostics.TraceSource
	%struct.CompressedAssemblyDescriptor {
		i32 37376, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 9211272; uint32_t buffer_offset
	}, ; 102: System.Drawing.Primitives
	%struct.CompressedAssemblyDescriptor {
		i32 5120, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 9248648; uint32_t buffer_offset
	}, ; 103: System.Drawing
	%struct.CompressedAssemblyDescriptor {
		i32 60416, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 9253768; uint32_t buffer_offset
	}, ; 104: System.Formats.Asn1
	%struct.CompressedAssemblyDescriptor {
		i32 22016, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 9314184; uint32_t buffer_offset
	}, ; 105: System.IO.Compression.Brotli
	%struct.CompressedAssemblyDescriptor {
		i32 31744, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 9336200; uint32_t buffer_offset
	}, ; 106: System.IO.Compression
	%struct.CompressedAssemblyDescriptor {
		i32 6144, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 9367944; uint32_t buffer_offset
	}, ; 107: System.IO.Pipelines
	%struct.CompressedAssemblyDescriptor {
		i32 354816, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 9374088; uint32_t buffer_offset
	}, ; 108: System.Linq.Expressions
	%struct.CompressedAssemblyDescriptor {
		i32 67584, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 9728904; uint32_t buffer_offset
	}, ; 109: System.Linq
	%struct.CompressedAssemblyDescriptor {
		i32 14336, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 9796488; uint32_t buffer_offset
	}, ; 110: System.Memory
	%struct.CompressedAssemblyDescriptor {
		i32 126464, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 9810824; uint32_t buffer_offset
	}, ; 111: System.Net.Http
	%struct.CompressedAssemblyDescriptor {
		i32 38912, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 9937288; uint32_t buffer_offset
	}, ; 112: System.Net.Primitives
	%struct.CompressedAssemblyDescriptor {
		i32 7168, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 9976200; uint32_t buffer_offset
	}, ; 113: System.Net.Requests
	%struct.CompressedAssemblyDescriptor {
		i32 5120, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 9983368; uint32_t buffer_offset
	}, ; 114: System.Numerics.Vectors
	%struct.CompressedAssemblyDescriptor {
		i32 4608, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 9988488; uint32_t buffer_offset
	}, ; 115: System.Numerics
	%struct.CompressedAssemblyDescriptor {
		i32 17920, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 9993096; uint32_t buffer_offset
	}, ; 116: System.ObjectModel
	%struct.CompressedAssemblyDescriptor {
		i32 72704, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 10011016; uint32_t buffer_offset
	}, ; 117: System.Private.Uri
	%struct.CompressedAssemblyDescriptor {
		i32 396288, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 10083720; uint32_t buffer_offset
	}, ; 118: System.Private.Xml
	%struct.CompressedAssemblyDescriptor {
		i32 9216, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 10480008; uint32_t buffer_offset
	}, ; 119: System.Runtime.InteropServices
	%struct.CompressedAssemblyDescriptor {
		i32 5120, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 10489224; uint32_t buffer_offset
	}, ; 120: System.Runtime.Loader
	%struct.CompressedAssemblyDescriptor {
		i32 79360, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 10494344; uint32_t buffer_offset
	}, ; 121: System.Runtime.Numerics
	%struct.CompressedAssemblyDescriptor {
		i32 14336, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 10573704; uint32_t buffer_offset
	}, ; 122: System.Runtime
	%struct.CompressedAssemblyDescriptor {
		i32 133120, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 10588040; uint32_t buffer_offset
	}, ; 123: System.Security.Cryptography
	%struct.CompressedAssemblyDescriptor {
		i32 29696, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 10721160; uint32_t buffer_offset
	}, ; 124: System.Text.Encodings.Web
	%struct.CompressedAssemblyDescriptor {
		i32 378368, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 10750856; uint32_t buffer_offset
	}, ; 125: System.Text.Json
	%struct.CompressedAssemblyDescriptor {
		i32 330752, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 11129224; uint32_t buffer_offset
	}, ; 126: System.Text.RegularExpressions
	%struct.CompressedAssemblyDescriptor {
		i32 5120, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 11459976; uint32_t buffer_offset
	}, ; 127: System.Threading.Thread
	%struct.CompressedAssemblyDescriptor {
		i32 12288, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 11465096; uint32_t buffer_offset
	}, ; 128: System.Threading
	%struct.CompressedAssemblyDescriptor {
		i32 5120, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 11477384; uint32_t buffer_offset
	}, ; 129: System.Xml.ReaderWriter
	%struct.CompressedAssemblyDescriptor {
		i32 5120, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 11482504; uint32_t buffer_offset
	}, ; 130: System
	%struct.CompressedAssemblyDescriptor {
		i32 9216, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 11487624; uint32_t buffer_offset
	}, ; 131: mscorlib
	%struct.CompressedAssemblyDescriptor {
		i32 2017792, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 11496840; uint32_t buffer_offset
	}, ; 132: System.Private.CoreLib
	%struct.CompressedAssemblyDescriptor {
		i32 171520, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 13514632; uint32_t buffer_offset
	}, ; 133: Java.Interop
	%struct.CompressedAssemblyDescriptor {
		i32 22368, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 13686152; uint32_t buffer_offset
	}, ; 134: Mono.Android.Runtime
	%struct.CompressedAssemblyDescriptor {
		i32 2281984, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 13708520; uint32_t buffer_offset
	} ; 135: Mono.Android
], align 4

@uncompressed_assemblies_data_size = dso_local local_unnamed_addr constant i32 15990504, align 4

@uncompressed_assemblies_data_buffer = dso_local local_unnamed_addr global [15990504 x i8] zeroinitializer, align 1

; Metadata
!llvm.module.flags = !{!0, !1, !7, !8, !9, !10}
!0 = !{i32 1, !"wchar_size", i32 4}
!1 = !{i32 7, !"PIC Level", i32 2}
!llvm.ident = !{!2}
!2 = !{!".NET for Android remotes/origin/release/10.0.1xx @ d549e1dc4e2a083b08b4f24cb5495e81b99d79b5"}
!3 = !{!4, !4, i64 0}
!4 = !{!"any pointer", !5, i64 0}
!5 = !{!"omnipotent char", !6, i64 0}
!6 = !{!"Simple C++ TBAA"}
!7 = !{i32 1, !"branch-target-enforcement", i32 0}
!8 = !{i32 1, !"sign-return-address", i32 0}
!9 = !{i32 1, !"sign-return-address-all", i32 0}
!10 = !{i32 1, !"sign-return-address-with-bkey", i32 0}
