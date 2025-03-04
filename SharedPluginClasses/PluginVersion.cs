﻿using System;

namespace SharedPluginClasses
{
#if NET
    readonly
#endif
    struct PluginVersion
    {
        public readonly uint Packed;

        public bool IsValid => Magic == MagicValue;

        uint Magic => (Packed & (0xFFFFu << 16)) >> 16;
        public uint Minor => (Packed & 0xFF00) >> 8;
        public uint Patch => Packed & 0xFF;

        const uint MagicValue = 0xABCD;

        public PluginVersion(uint packed)
        {
            Packed = packed;
        }

        public PluginVersion(uint minor, uint patch)
        {
#if NET8_0_OR_GREATER
        ArgumentOutOfRangeException.ThrowIfGreaterThan(minor, byte.MaxValue);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(patch, byte.MaxValue);
#elif NETSTANDARD2_0
            if (minor > byte.MaxValue) throw new ArgumentOutOfRangeException(nameof(minor));
            if (patch > byte.MaxValue) throw new ArgumentOutOfRangeException(nameof(patch));
#else
            if (minor > byte.MaxValue) throw new ArgumentException(null, nameof(minor));
            if (patch > byte.MaxValue) throw new ArgumentException(null, nameof(patch));
#endif

            // 16 bits of magic value, 8 bits of major, 8 bits of minor
            Packed = (MagicValue << 16) | (minor << 8) | patch;
        }

#if NET
        public (uint Minor, uint Patch) GetVersionNumbers() => (Minor, Patch);

        public override bool Equals(object? obj) => obj is PluginVersion version && Packed == version.Packed;
#else
        public override bool Equals(object obj) => obj is PluginVersion && Packed == ((PluginVersion)obj).Packed;
#endif

        public override int GetHashCode() => (int)Packed;

        public override string ToString() => IsValid ? $"1.{Minor}.{Patch}" : "Invalid Version";

        public static bool operator ==(PluginVersion a, PluginVersion b) => a.Packed == b.Packed;
        public static bool operator !=(PluginVersion a, PluginVersion b) => a.Packed != b.Packed;

        public static bool operator <(PluginVersion a, PluginVersion b)
        {
            if (a.Minor > b.Minor)
                return false;
            else
                return a.Minor < b.Minor || a.Patch < b.Patch;
        }

        public static bool operator >(PluginVersion a, PluginVersion b)
        {
            if (a.Minor < b.Minor)
                return false;
            else
                return a.Minor > b.Minor || a.Patch > b.Patch;
        }

        public static bool operator <=(PluginVersion a, PluginVersion b)
        {
            if (a.Minor > b.Minor)
                return false;
            else
                return a.Minor < b.Minor || a.Patch <= b.Patch;
        }

        public static bool operator >=(PluginVersion a, PluginVersion b)
        {
            if (a.Minor < b.Minor)
                return false;
            else
                return a.Minor > b.Minor || a.Patch >= b.Patch;
        }
    }
}
