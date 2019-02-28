﻿using System;
using System.Reflection;
using System.Runtime.InteropServices;
using SteamworksSharp.Native.Libs;

namespace SteamworksSharp.Native.Linux_x86
{
	public class NativeLibrary : INativeLibrary
	{
		public OSPlatform Platform { get; } = OSPlatform.Linux;

		public Architecture Architecture { get; } = Architecture.X86;

		public Lazy<byte[]> LibraryBytes { get; } = new Lazy<byte[]>(() =>
			LibUtils.ReadResourceBytes(typeof(NativeLibrary).GetTypeInfo().Assembly, "XIVLauncher.SteamworksSharp.Native.Linux_x86.libsteam_api.so"));

		public string Extension { get; } = "so";
	}
}