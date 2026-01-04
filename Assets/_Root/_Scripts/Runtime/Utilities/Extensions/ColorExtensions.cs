using System;
using System.Globalization;
using UnityEngine;

namespace PixelCiv.Utilities.Extensions
{
public static class ColorExtensions
{
	public static Color FromHex(this Color _, string hex)
	{
		if (string.IsNullOrWhiteSpace(hex))
			throw new ArgumentException("Hex string is null or empty.");

		hex = hex.Replace("#", string.Empty);

		if (hex.Length != 6 && hex.Length != 8)
			throw new ArgumentException("Hex string must be 6 or 8 characters long.");

		byte r = byte.Parse(hex[..2], NumberStyles.HexNumber);
		byte g = byte.Parse(hex.Substring(2, 2), NumberStyles.HexNumber);
		byte b = byte.Parse(hex.Substring(4, 2), NumberStyles.HexNumber);
		byte a = hex.Length == 8 ?
						 byte.Parse(hex.Substring(6, 2), NumberStyles.HexNumber) :
						 (byte)255;

		return FromRgb255(_, r, g, b, a);
	}

	public static Color FromRgb255(this Color _, int r, int g, int b, int a = 255)
	{
		return new Color(Mathf.Clamp01(r / 255f),
						 Mathf.Clamp01(g / 255f),
						 Mathf.Clamp01(b / 255f),
						 Mathf.Clamp01(a / 255f));
	}
}
}
