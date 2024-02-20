///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;

using Xunit;

namespace GriffinPlus.Lib.Imaging;

/// <summary>
/// Unit tests targeting the <see cref="Colors"/> class.
/// </summary>
public class ColorsTests
{
	/// <summary>
	/// Tests getting the <see cref="Colors.AliceBlue"/> property.
	/// </summary>
	[Fact]
	public void AliceBlue()
	{
		CheckColor(Colors.AliceBlue, 0xFFF0F8FFu);
	}

	/// <summary>
	/// Tests getting the <see cref="Colors.AntiqueWhite"/> property.
	/// </summary>
	[Fact]
	public void AntiqueWhite()
	{
		CheckColor(Colors.AntiqueWhite, 0xFFFAEBD7u);
	}

	/// <summary>
	/// Tests getting the <see cref="Colors.Aqua"/> property.
	/// </summary>
	[Fact]
	public void Aqua()
	{
		CheckColor(Colors.Aqua, 0xFF00FFFFu);
	}

	/// <summary>
	/// Tests getting the <see cref="Colors.Aquamarine"/> property.
	/// </summary>
	[Fact]
	public void Aquamarine()
	{
		CheckColor(Colors.Aquamarine, 0xFF7FFFD4u);
	}

	/// <summary>
	/// Tests getting the <see cref="Colors.Azure"/> property.
	/// </summary>
	[Fact]
	public void Azure()
	{
		CheckColor(Colors.Azure, 0xFFF0FFFFu);
	}

	/// <summary>
	/// Tests getting the <see cref="Colors.Beige"/> property.
	/// </summary>
	[Fact]
	public void Beige()
	{
		CheckColor(Colors.Beige, 0xFFF5F5DCu);
	}

	/// <summary>
	/// Tests getting the <see cref="Colors.Bisque"/> property.
	/// </summary>
	[Fact]
	public void Bisque()
	{
		CheckColor(Colors.Bisque, 0xFFFFE4C4u);
	}

	/// <summary>
	/// Tests getting the <see cref="Colors.Black"/> property.
	/// </summary>
	[Fact]
	public void Black()
	{
		CheckColor(Colors.Black, 0xFF000000u);
	}

	/// <summary>
	/// Tests getting the <see cref="Colors.BlanchedAlmond"/> property.
	/// </summary>
	[Fact]
	public void BlanchedAlmond()
	{
		CheckColor(Colors.BlanchedAlmond, 0xFFFFEBCDu);
	}

	/// <summary>
	/// Tests getting the <see cref="Colors.Blue"/> property.
	/// </summary>
	[Fact]
	public void Blue()
	{
		CheckColor(Colors.Blue, 0xFF0000FFu);
	}

	/// <summary>
	/// Tests getting the <see cref="Colors.BlueViolet"/> property.
	/// </summary>
	[Fact]
	public void BlueViolet()
	{
		CheckColor(Colors.BlueViolet, 0xFF8A2BE2u);
	}

	/// <summary>
	/// Tests getting the <see cref="Colors.Brown"/> property.
	/// </summary>
	[Fact]
	public void Brown()
	{
		CheckColor(Colors.Brown, 0xFFA52A2Au);
	}

	/// <summary>
	/// Tests getting the <see cref="Colors.BurlyWood"/> property.
	/// </summary>
	[Fact]
	public void BurlyWood()
	{
		CheckColor(Colors.BurlyWood, 0xFFDEB887u);
	}

	/// <summary>
	/// Tests getting the <see cref="Colors.CadetBlue"/> property.
	/// </summary>
	[Fact]
	public void CadetBlue()
	{
		CheckColor(Colors.CadetBlue, 0xFF5F9EA0u);
	}

	/// <summary>
	/// Tests getting the <see cref="Colors.Chartreuse"/> property.
	/// </summary>
	[Fact]
	public void Chartreuse()
	{
		CheckColor(Colors.Chartreuse, 0xFF7FFF00u);
	}

	/// <summary>
	/// Tests getting the <see cref="Colors.Chocolate"/> property.
	/// </summary>
	[Fact]
	public void Chocolate()
	{
		CheckColor(Colors.Chocolate, 0xFFD2691Eu);
	}

	/// <summary>
	/// Tests getting the <see cref="Colors.Coral"/> property.
	/// </summary>
	[Fact]
	public void Coral()
	{
		CheckColor(Colors.Coral, 0xFFFF7F50u);
	}

	/// <summary>
	/// Tests getting the <see cref="Colors.CornflowerBlue"/> property.
	/// </summary>
	[Fact]
	public void CornflowerBlue()
	{
		CheckColor(Colors.CornflowerBlue, 0xFF6495EDu);
	}

	/// <summary>
	/// Tests getting the <see cref="Colors.Cornsilk"/> property.
	/// </summary>
	[Fact]
	public void Cornsilk()
	{
		CheckColor(Colors.Cornsilk, 0xFFFFF8DCu);
	}

	/// <summary>
	/// Tests getting the <see cref="Colors.Crimson"/> property.
	/// </summary>
	[Fact]
	public void Crimson()
	{
		CheckColor(Colors.Crimson, 0xFFDC143Cu);
	}

	/// <summary>
	/// Tests getting the <see cref="Colors.Cyan"/> property.
	/// </summary>
	[Fact]
	public void Cyan()
	{
		CheckColor(Colors.Cyan, 0xFF00FFFFu);
	}

	/// <summary>
	/// Tests getting the <see cref="Colors.DarkBlue"/> property.
	/// </summary>
	[Fact]
	public void DarkBlue()
	{
		CheckColor(Colors.DarkBlue, 0xFF00008Bu);
	}

	/// <summary>
	/// Tests getting the <see cref="Colors.DarkCyan"/> property.
	/// </summary>
	[Fact]
	public void DarkCyan()
	{
		CheckColor(Colors.DarkCyan, 0xFF008B8Bu);
	}

	/// <summary>
	/// Tests getting the <see cref="Colors.DarkGoldenrod"/> property.
	/// </summary>
	[Fact]
	public void DarkGoldenrod()
	{
		CheckColor(Colors.DarkGoldenrod, 0xFFB8860Bu);
	}

	/// <summary>
	/// Tests getting the <see cref="Colors.DarkGray"/> property.
	/// </summary>
	[Fact]
	public void DarkGray()
	{
		CheckColor(Colors.DarkGray, 0xFFA9A9A9u);
	}

	/// <summary>
	/// Tests getting the <see cref="Colors.DarkGreen"/> property.
	/// </summary>
	[Fact]
	public void DarkGreen()
	{
		CheckColor(Colors.DarkGreen, 0xFF006400u);
	}

	/// <summary>
	/// Tests getting the <see cref="Colors.DarkKhaki"/> property.
	/// </summary>
	[Fact]
	public void DarkKhaki()
	{
		CheckColor(Colors.DarkKhaki, 0xFFBDB76Bu);
	}

	/// <summary>
	/// Tests getting the <see cref="Colors.DarkMagenta"/> property.
	/// </summary>
	[Fact]
	public void DarkMagenta()
	{
		CheckColor(Colors.DarkMagenta, 0xFF8B008Bu);
	}

	/// <summary>
	/// Tests getting the <see cref="Colors.DarkOliveGreen"/> property.
	/// </summary>
	[Fact]
	public void DarkOliveGreen()
	{
		CheckColor(Colors.DarkOliveGreen, 0xFF556B2Fu);
	}

	/// <summary>
	/// Tests getting the <see cref="Colors.DarkOrange"/> property.
	/// </summary>
	[Fact]
	public void DarkOrange()
	{
		CheckColor(Colors.DarkOrange, 0xFFFF8C00u);
	}

	/// <summary>
	/// Tests getting the <see cref="Colors.DarkOrchid"/> property.
	/// </summary>
	[Fact]
	public void DarkOrchid()
	{
		CheckColor(Colors.DarkOrchid, 0xFF9932CCu);
	}

	/// <summary>
	/// Tests getting the <see cref="Colors.DarkRed"/> property.
	/// </summary>
	[Fact]
	public void DarkRed()
	{
		CheckColor(Colors.DarkRed, 0xFF8B0000u);
	}

	/// <summary>
	/// Tests getting the <see cref="Colors.DarkSalmon"/> property.
	/// </summary>
	[Fact]
	public void DarkSalmon()
	{
		CheckColor(Colors.DarkSalmon, 0xFFE9967Au);
	}

	/// <summary>
	/// Tests getting the <see cref="Colors.DarkSeaGreen"/> property.
	/// </summary>
	[Fact]
	public void DarkSeaGreen()
	{
		CheckColor(Colors.DarkSeaGreen, 0xFF8FBC8Fu);
	}

	/// <summary>
	/// Tests getting the <see cref="Colors.DarkSlateBlue"/> property.
	/// </summary>
	[Fact]
	public void DarkSlateBlue()
	{
		CheckColor(Colors.DarkSlateBlue, 0xFF483D8Bu);
	}

	/// <summary>
	/// Tests getting the <see cref="Colors.DarkSlateGray"/> property.
	/// </summary>
	[Fact]
	public void DarkSlateGray()
	{
		CheckColor(Colors.DarkSlateGray, 0xFF2F4F4Fu);
	}

	/// <summary>
	/// Tests getting the <see cref="Colors.DarkTurquoise"/> property.
	/// </summary>
	[Fact]
	public void DarkTurquoise()
	{
		CheckColor(Colors.DarkTurquoise, 0xFF00CED1u);
	}

	/// <summary>
	/// Tests getting the <see cref="Colors.DarkViolet"/> property.
	/// </summary>
	[Fact]
	public void DarkViolet()
	{
		CheckColor(Colors.DarkViolet, 0xFF9400D3u);
	}

	/// <summary>
	/// Tests getting the <see cref="Colors.DeepPink"/> property.
	/// </summary>
	[Fact]
	public void DeepPink()
	{
		CheckColor(Colors.DeepPink, 0xFFFF1493u);
	}

	/// <summary>
	/// Tests getting the <see cref="Colors.DeepSkyBlue"/> property.
	/// </summary>
	[Fact]
	public void DeepSkyBlue()
	{
		CheckColor(Colors.DeepSkyBlue, 0xFF00BFFFu);
	}

	/// <summary>
	/// Tests getting the <see cref="Colors.DimGray"/> property.
	/// </summary>
	[Fact]
	public void DimGray()
	{
		CheckColor(Colors.DimGray, 0xFF696969u);
	}

	/// <summary>
	/// Tests getting the <see cref="Colors.DodgerBlue"/> property.
	/// </summary>
	[Fact]
	public void DodgerBlue()
	{
		CheckColor(Colors.DodgerBlue, 0xFF1E90FFu);
	}

	/// <summary>
	/// Tests getting the <see cref="Colors.Firebrick"/> property.
	/// </summary>
	[Fact]
	public void Firebrick()
	{
		CheckColor(Colors.Firebrick, 0xFFB22222u);
	}

	/// <summary>
	/// Tests getting the <see cref="Colors.FloralWhite"/> property.
	/// </summary>
	[Fact]
	public void FloralWhite()
	{
		CheckColor(Colors.FloralWhite, 0xFFFFFAF0u);
	}

	/// <summary>
	/// Tests getting the <see cref="Colors.ForestGreen"/> property.
	/// </summary>
	[Fact]
	public void ForestGreen()
	{
		CheckColor(Colors.ForestGreen, 0xFF228B22u);
	}

	/// <summary>
	/// Tests getting the <see cref="Colors.Fuchsia"/> property.
	/// </summary>
	[Fact]
	public void Fuchsia()
	{
		CheckColor(Colors.Fuchsia, 0xFFFF00FFu);
	}

	/// <summary>
	/// Tests getting the <see cref="Colors.Gainsboro"/> property.
	/// </summary>
	[Fact]
	public void Gainsboro()
	{
		CheckColor(Colors.Gainsboro, 0xFFDCDCDCu);
	}

	/// <summary>
	/// Tests getting the <see cref="Colors.GhostWhite"/> property.
	/// </summary>
	[Fact]
	public void GhostWhite()
	{
		CheckColor(Colors.GhostWhite, 0xFFF8F8FFu);
	}

	/// <summary>
	/// Tests getting the <see cref="Colors.Gold"/> property.
	/// </summary>
	[Fact]
	public void Gold()
	{
		CheckColor(Colors.Gold, 0xFFFFD700u);
	}

	/// <summary>
	/// Tests getting the <see cref="Colors.Goldenrod"/> property.
	/// </summary>
	[Fact]
	public void Goldenrod()
	{
		CheckColor(Colors.Goldenrod, 0xFFDAA520u);
	}

	/// <summary>
	/// Tests getting the <see cref="Colors.Gray"/> property.
	/// </summary>
	[Fact]
	public void Gray()
	{
		CheckColor(Colors.Gray, 0xFF808080u);
	}

	/// <summary>
	/// Tests getting the <see cref="Colors.Green"/> property.
	/// </summary>
	[Fact]
	public void Green()
	{
		CheckColor(Colors.Green, 0xFF008000u);
	}

	/// <summary>
	/// Tests getting the <see cref="Colors.GreenYellow"/> property.
	/// </summary>
	[Fact]
	public void GreenYellow()
	{
		CheckColor(Colors.GreenYellow, 0xFFADFF2Fu);
	}

	/// <summary>
	/// Tests getting the <see cref="Colors.Honeydew"/> property.
	/// </summary>
	[Fact]
	public void Honeydew()
	{
		CheckColor(Colors.Honeydew, 0xFFF0FFF0u);
	}

	/// <summary>
	/// Tests getting the <see cref="Colors.HotPink"/> property.
	/// </summary>
	[Fact]
	public void HotPink()
	{
		CheckColor(Colors.HotPink, 0xFFFF69B4u);
	}

	/// <summary>
	/// Tests getting the <see cref="Colors.IndianRed"/> property.
	/// </summary>
	[Fact]
	public void IndianRed()
	{
		CheckColor(Colors.IndianRed, 0xFFCD5C5Cu);
	}

	/// <summary>
	/// Tests getting the <see cref="Colors.Indigo"/> property.
	/// </summary>
	[Fact]
	public void Indigo()
	{
		CheckColor(Colors.Indigo, 0xFF4B0082u);
	}

	/// <summary>
	/// Tests getting the <see cref="Colors.Ivory"/> property.
	/// </summary>
	[Fact]
	public void Ivory()
	{
		CheckColor(Colors.Ivory, 0xFFFFFFF0u);
	}

	/// <summary>
	/// Tests getting the <see cref="Colors.Khaki"/> property.
	/// </summary>
	[Fact]
	public void Khaki()
	{
		CheckColor(Colors.Khaki, 0xFFF0E68Cu);
	}

	/// <summary>
	/// Tests getting the <see cref="Colors.Lavender"/> property.
	/// </summary>
	[Fact]
	public void Lavender()
	{
		CheckColor(Colors.Lavender, 0xFFE6E6FAu);
	}

	/// <summary>
	/// Tests getting the <see cref="Colors.LavenderBlush"/> property.
	/// </summary>
	[Fact]
	public void LavenderBlush()
	{
		CheckColor(Colors.LavenderBlush, 0xFFFFF0F5u);
	}

	/// <summary>
	/// Tests getting the <see cref="Colors.LawnGreen"/> property.
	/// </summary>
	[Fact]
	public void LawnGreen()
	{
		CheckColor(Colors.LawnGreen, 0xFF7CFC00u);
	}

	/// <summary>
	/// Tests getting the <see cref="Colors.LemonChiffon"/> property.
	/// </summary>
	[Fact]
	public void LemonChiffon()
	{
		CheckColor(Colors.LemonChiffon, 0xFFFFFACDu);
	}

	/// <summary>
	/// Tests getting the <see cref="Colors.LightBlue"/> property.
	/// </summary>
	[Fact]
	public void LightBlue()
	{
		CheckColor(Colors.LightBlue, 0xFFADD8E6u);
	}

	/// <summary>
	/// Tests getting the <see cref="Colors.LightCoral"/> property.
	/// </summary>
	[Fact]
	public void LightCoral()
	{
		CheckColor(Colors.LightCoral, 0xFFF08080u);
	}

	/// <summary>
	/// Tests getting the <see cref="Colors.LightCyan"/> property.
	/// </summary>
	[Fact]
	public void LightCyan()
	{
		CheckColor(Colors.LightCyan, 0xFFE0FFFFu);
	}

	/// <summary>
	/// Tests getting the <see cref="Colors.LightGoldenrodYellow"/> property.
	/// </summary>
	[Fact]
	public void LightGoldenrodYellow()
	{
		CheckColor(Colors.LightGoldenrodYellow, 0xFFFAFAD2u);
	}

	/// <summary>
	/// Tests getting the <see cref="Colors.LightGray"/> property.
	/// </summary>
	[Fact]
	public void LightGray()
	{
		CheckColor(Colors.LightGray, 0xFFD3D3D3u);
	}

	/// <summary>
	/// Tests getting the <see cref="Colors.LightGreen"/> property.
	/// </summary>
	[Fact]
	public void LightGreen()
	{
		CheckColor(Colors.LightGreen, 0xFF90EE90u);
	}

	/// <summary>
	/// Tests getting the <see cref="Colors.LightPink"/> property.
	/// </summary>
	[Fact]
	public void LightPink()
	{
		CheckColor(Colors.LightPink, 0xFFFFB6C1u);
	}

	/// <summary>
	/// Tests getting the <see cref="Colors.LightSalmon"/> property.
	/// </summary>
	[Fact]
	public void LightSalmon()
	{
		CheckColor(Colors.LightSalmon, 0xFFFFA07Au);
	}

	/// <summary>
	/// Tests getting the <see cref="Colors.LightSeaGreen"/> property.
	/// </summary>
	[Fact]
	public void LightSeaGreen()
	{
		CheckColor(Colors.LightSeaGreen, 0xFF20B2AAu);
	}

	/// <summary>
	/// Tests getting the <see cref="Colors.LightSkyBlue"/> property.
	/// </summary>
	[Fact]
	public void LightSkyBlue()
	{
		CheckColor(Colors.LightSkyBlue, 0xFF87CEFAu);
	}

	/// <summary>
	/// Tests getting the <see cref="Colors.LightSlateGray"/> property.
	/// </summary>
	[Fact]
	public void LightSlateGray()
	{
		CheckColor(Colors.LightSlateGray, 0xFF778899u);
	}

	/// <summary>
	/// Tests getting the <see cref="Colors.LightSteelBlue"/> property.
	/// </summary>
	[Fact]
	public void LightSteelBlue()
	{
		CheckColor(Colors.LightSteelBlue, 0xFFB0C4DEu);
	}

	/// <summary>
	/// Tests getting the <see cref="Colors.LightYellow"/> property.
	/// </summary>
	[Fact]
	public void LightYellow()
	{
		CheckColor(Colors.LightYellow, 0xFFFFFFE0u);
	}

	/// <summary>
	/// Tests getting the <see cref="Colors.Lime"/> property.
	/// </summary>
	[Fact]
	public void Lime()
	{
		CheckColor(Colors.Lime, 0xFF00FF00u);
	}

	/// <summary>
	/// Tests getting the <see cref="Colors.LimeGreen"/> property.
	/// </summary>
	[Fact]
	public void LimeGreen()
	{
		CheckColor(Colors.LimeGreen, 0xFF32CD32u);
	}

	/// <summary>
	/// Tests getting the <see cref="Colors.Linen"/> property.
	/// </summary>
	[Fact]
	public void Linen()
	{
		CheckColor(Colors.Linen, 0xFFFAF0E6u);
	}

	/// <summary>
	/// Tests getting the <see cref="Colors.Magenta"/> property.
	/// </summary>
	[Fact]
	public void Magenta()
	{
		CheckColor(Colors.Magenta, 0xFFFF00FFu);
	}

	/// <summary>
	/// Tests getting the <see cref="Colors.Maroon"/> property.
	/// </summary>
	[Fact]
	public void Maroon()
	{
		CheckColor(Colors.Maroon, 0xFF800000u);
	}

	/// <summary>
	/// Tests getting the <see cref="Colors.MediumAquamarine"/> property.
	/// </summary>
	[Fact]
	public void MediumAquamarine()
	{
		CheckColor(Colors.MediumAquamarine, 0xFF66CDAAu);
	}

	/// <summary>
	/// Tests getting the <see cref="Colors.MediumBlue"/> property.
	/// </summary>
	[Fact]
	public void MediumBlue()
	{
		CheckColor(Colors.MediumBlue, 0xFF0000CDu);
	}

	/// <summary>
	/// Tests getting the <see cref="Colors.MediumOrchid"/> property.
	/// </summary>
	[Fact]
	public void MediumOrchid()
	{
		CheckColor(Colors.MediumOrchid, 0xFFBA55D3u);
	}

	/// <summary>
	/// Tests getting the <see cref="Colors.MediumPurple"/> property.
	/// </summary>
	[Fact]
	public void MediumPurple()
	{
		CheckColor(Colors.MediumPurple, 0xFF9370DBu);
	}

	/// <summary>
	/// Tests getting the <see cref="Colors.MediumSeaGreen"/> property.
	/// </summary>
	[Fact]
	public void MediumSeaGreen()
	{
		CheckColor(Colors.MediumSeaGreen, 0xFF3CB371u);
	}

	/// <summary>
	/// Tests getting the <see cref="Colors.MediumSlateBlue"/> property.
	/// </summary>
	[Fact]
	public void MediumSlateBlue()
	{
		CheckColor(Colors.MediumSlateBlue, 0xFF7B68EEu);
	}

	/// <summary>
	/// Tests getting the <see cref="Colors.MediumSpringGreen"/> property.
	/// </summary>
	[Fact]
	public void MediumSpringGreen()
	{
		CheckColor(Colors.MediumSpringGreen, 0xFF00FA9Au);
	}

	/// <summary>
	/// Tests getting the <see cref="Colors.MediumTurquoise"/> property.
	/// </summary>
	[Fact]
	public void MediumTurquoise()
	{
		CheckColor(Colors.MediumTurquoise, 0xFF48D1CCu);
	}

	/// <summary>
	/// Tests getting the <see cref="Colors.MediumVioletRed"/> property.
	/// </summary>
	[Fact]
	public void MediumVioletRed()
	{
		CheckColor(Colors.MediumVioletRed, 0xFFC71585u);
	}

	/// <summary>
	/// Tests getting the <see cref="Colors.MidnightBlue"/> property.
	/// </summary>
	[Fact]
	public void MidnightBlue()
	{
		CheckColor(Colors.MidnightBlue, 0xFF191970u);
	}

	/// <summary>
	/// Tests getting the <see cref="Colors.MintCream"/> property.
	/// </summary>
	[Fact]
	public void MintCream()
	{
		CheckColor(Colors.MintCream, 0xFFF5FFFAu);
	}

	/// <summary>
	/// Tests getting the <see cref="Colors.MistyRose"/> property.
	/// </summary>
	[Fact]
	public void MistyRose()
	{
		CheckColor(Colors.MistyRose, 0xFFFFE4E1u);
	}

	/// <summary>
	/// Tests getting the <see cref="Colors.Moccasin"/> property.
	/// </summary>
	[Fact]
	public void Moccasin()
	{
		CheckColor(Colors.Moccasin, 0xFFFFE4B5u);
	}

	/// <summary>
	/// Tests getting the <see cref="Colors.NavajoWhite"/> property.
	/// </summary>
	[Fact]
	public void NavajoWhite()
	{
		CheckColor(Colors.NavajoWhite, 0xFFFFDEADu);
	}

	/// <summary>
	/// Tests getting the <see cref="Colors.Navy"/> property.
	/// </summary>
	[Fact]
	public void Navy()
	{
		CheckColor(Colors.Navy, 0xFF000080u);
	}

	/// <summary>
	/// Tests getting the <see cref="Colors.OldLace"/> property.
	/// </summary>
	[Fact]
	public void OldLace()
	{
		CheckColor(Colors.OldLace, 0xFFFDF5E6u);
	}

	/// <summary>
	/// Tests getting the <see cref="Colors.Olive"/> property.
	/// </summary>
	[Fact]
	public void Olive()
	{
		CheckColor(Colors.Olive, 0xFF808000u);
	}

	/// <summary>
	/// Tests getting the <see cref="Colors.OliveDrab"/> property.
	/// </summary>
	[Fact]
	public void OliveDrab()
	{
		CheckColor(Colors.OliveDrab, 0xFF6B8E23u);
	}

	/// <summary>
	/// Tests getting the <see cref="Colors.Orange"/> property.
	/// </summary>
	[Fact]
	public void Orange()
	{
		CheckColor(Colors.Orange, 0xFFFFA500u);
	}

	/// <summary>
	/// Tests getting the <see cref="Colors.OrangeRed"/> property.
	/// </summary>
	[Fact]
	public void OrangeRed()
	{
		CheckColor(Colors.OrangeRed, 0xFFFF4500u);
	}

	/// <summary>
	/// Tests getting the <see cref="Colors.Orchid"/> property.
	/// </summary>
	[Fact]
	public void Orchid()
	{
		CheckColor(Colors.Orchid, 0xFFDA70D6u);
	}

	/// <summary>
	/// Tests getting the <see cref="Colors.PaleGoldenrod"/> property.
	/// </summary>
	[Fact]
	public void PaleGoldenrod()
	{
		CheckColor(Colors.PaleGoldenrod, 0xFFEEE8AAu);
	}

	/// <summary>
	/// Tests getting the <see cref="Colors.PaleGreen"/> property.
	/// </summary>
	[Fact]
	public void PaleGreen()
	{
		CheckColor(Colors.PaleGreen, 0xFF98FB98u);
	}

	/// <summary>
	/// Tests getting the <see cref="Colors.PaleTurquoise"/> property.
	/// </summary>
	[Fact]
	public void PaleTurquoise()
	{
		CheckColor(Colors.PaleTurquoise, 0xFFAFEEEEu);
	}

	/// <summary>
	/// Tests getting the <see cref="Colors.PaleVioletRed"/> property.
	/// </summary>
	[Fact]
	public void PaleVioletRed()
	{
		CheckColor(Colors.PaleVioletRed, 0xFFDB7093u);
	}

	/// <summary>
	/// Tests getting the <see cref="Colors.PapayaWhip"/> property.
	/// </summary>
	[Fact]
	public void PapayaWhip()
	{
		CheckColor(Colors.PapayaWhip, 0xFFFFEFD5u);
	}

	/// <summary>
	/// Tests getting the <see cref="Colors.PeachPuff"/> property.
	/// </summary>
	[Fact]
	public void PeachPuff()
	{
		CheckColor(Colors.PeachPuff, 0xFFFFDAB9u);
	}

	/// <summary>
	/// Tests getting the <see cref="Colors.Peru"/> property.
	/// </summary>
	[Fact]
	public void Peru()
	{
		CheckColor(Colors.Peru, 0xFFCD853Fu);
	}

	/// <summary>
	/// Tests getting the <see cref="Colors.Pink"/> property.
	/// </summary>
	[Fact]
	public void Pink()
	{
		CheckColor(Colors.Pink, 0xFFFFC0CBu);
	}

	/// <summary>
	/// Tests getting the <see cref="Colors.Plum"/> property.
	/// </summary>
	[Fact]
	public void Plum()
	{
		CheckColor(Colors.Plum, 0xFFDDA0DDu);
	}

	/// <summary>
	/// Tests getting the <see cref="Colors.PowderBlue"/> property.
	/// </summary>
	[Fact]
	public void PowderBlue()
	{
		CheckColor(Colors.PowderBlue, 0xFFB0E0E6u);
	}

	/// <summary>
	/// Tests getting the <see cref="Colors.Purple"/> property.
	/// </summary>
	[Fact]
	public void Purple()
	{
		CheckColor(Colors.Purple, 0xFF800080u);
	}

	/// <summary>
	/// Tests getting the <see cref="Colors.Red"/> property.
	/// </summary>
	[Fact]
	public void Red()
	{
		CheckColor(Colors.Red, 0xFFFF0000u);
	}

	/// <summary>
	/// Tests getting the <see cref="Colors.RosyBrown"/> property.
	/// </summary>
	[Fact]
	public void RosyBrown()
	{
		CheckColor(Colors.RosyBrown, 0xFFBC8F8Fu);
	}

	/// <summary>
	/// Tests getting the <see cref="Colors.RoyalBlue"/> property.
	/// </summary>
	[Fact]
	public void RoyalBlue()
	{
		CheckColor(Colors.RoyalBlue, 0xFF4169E1u);
	}

	/// <summary>
	/// Tests getting the <see cref="Colors.SaddleBrown"/> property.
	/// </summary>
	[Fact]
	public void SaddleBrown()
	{
		CheckColor(Colors.SaddleBrown, 0xFF8B4513u);
	}

	/// <summary>
	/// Tests getting the <see cref="Colors.Salmon"/> property.
	/// </summary>
	[Fact]
	public void Salmon()
	{
		CheckColor(Colors.Salmon, 0xFFFA8072u);
	}

	/// <summary>
	/// Tests getting the <see cref="Colors.SandyBrown"/> property.
	/// </summary>
	[Fact]
	public void SandyBrown()
	{
		CheckColor(Colors.SandyBrown, 0xFFF4A460u);
	}

	/// <summary>
	/// Tests getting the <see cref="Colors.SeaGreen"/> property.
	/// </summary>
	[Fact]
	public void SeaGreen()
	{
		CheckColor(Colors.SeaGreen, 0xFF2E8B57u);
	}

	/// <summary>
	/// Tests getting the <see cref="Colors.SeaShell"/> property.
	/// </summary>
	[Fact]
	public void SeaShell()
	{
		CheckColor(Colors.SeaShell, 0xFFFFF5EEu);
	}

	/// <summary>
	/// Tests getting the <see cref="Colors.Sienna"/> property.
	/// </summary>
	[Fact]
	public void Sienna()
	{
		CheckColor(Colors.Sienna, 0xFFA0522Du);
	}

	/// <summary>
	/// Tests getting the <see cref="Colors.Silver"/> property.
	/// </summary>
	[Fact]
	public void Silver()
	{
		CheckColor(Colors.Silver, 0xFFC0C0C0u);
	}

	/// <summary>
	/// Tests getting the <see cref="Colors.SkyBlue"/> property.
	/// </summary>
	[Fact]
	public void SkyBlue()
	{
		CheckColor(Colors.SkyBlue, 0xFF87CEEBu);
	}

	/// <summary>
	/// Tests getting the <see cref="Colors.SlateBlue"/> property.
	/// </summary>
	[Fact]
	public void SlateBlue()
	{
		CheckColor(Colors.SlateBlue, 0xFF6A5ACDu);
	}

	/// <summary>
	/// Tests getting the <see cref="Colors.SlateGray"/> property.
	/// </summary>
	[Fact]
	public void SlateGray()
	{
		CheckColor(Colors.SlateGray, 0xFF708090u);
	}

	/// <summary>
	/// Tests getting the <see cref="Colors.Snow"/> property.
	/// </summary>
	[Fact]
	public void Snow()
	{
		CheckColor(Colors.Snow, 0xFFFFFAFAu);
	}

	/// <summary>
	/// Tests getting the <see cref="Colors.SpringGreen"/> property.
	/// </summary>
	[Fact]
	public void SpringGreen()
	{
		CheckColor(Colors.SpringGreen, 0xFF00FF7Fu);
	}

	/// <summary>
	/// Tests getting the <see cref="Colors.SteelBlue"/> property.
	/// </summary>
	[Fact]
	public void SteelBlue()
	{
		CheckColor(Colors.SteelBlue, 0xFF4682B4u);
	}

	/// <summary>
	/// Tests getting the <see cref="Colors.Tan"/> property.
	/// </summary>
	[Fact]
	public void Tan()
	{
		CheckColor(Colors.Tan, 0xFFD2B48Cu);
	}

	/// <summary>
	/// Tests getting the <see cref="Colors.Teal"/> property.
	/// </summary>
	[Fact]
	public void Teal()
	{
		CheckColor(Colors.Teal, 0xFF008080u);
	}

	/// <summary>
	/// Tests getting the <see cref="Colors.Thistle"/> property.
	/// </summary>
	[Fact]
	public void Thistle()
	{
		CheckColor(Colors.Thistle, 0xFFD8BFD8u);
	}

	/// <summary>
	/// Tests getting the <see cref="Colors.Tomato"/> property.
	/// </summary>
	[Fact]
	public void Tomato()
	{
		CheckColor(Colors.Tomato, 0xFFFF6347u);
	}

	/// <summary>
	/// Tests getting the <see cref="Colors.Transparent"/> property.
	/// </summary>
	[Fact]
	public void Transparent()
	{
		CheckColor(Colors.Transparent, 0x00FFFFFFu);
	}

	/// <summary>
	/// Tests getting the <see cref="Colors.Turquoise"/> property.
	/// </summary>
	[Fact]
	public void Turquoise()
	{
		CheckColor(Colors.Turquoise, 0xFF40E0D0u);
	}

	/// <summary>
	/// Tests getting the <see cref="Colors.Violet"/> property.
	/// </summary>
	[Fact]
	public void Violet()
	{
		CheckColor(Colors.Violet, 0xFFEE82EEu);
	}

	/// <summary>
	/// Tests getting the <see cref="Colors.Wheat"/> property.
	/// </summary>
	[Fact]
	public void Wheat()
	{
		CheckColor(Colors.Wheat, 0xFFF5DEB3u);
	}

	/// <summary>
	/// Tests getting the <see cref="Colors.White"/> property.
	/// </summary>
	[Fact]
	public void White()
	{
		CheckColor(Colors.White, 0xFFFFFFFFu);
	}

	/// <summary>
	/// Tests getting the <see cref="Colors.WhiteSmoke"/> property.
	/// </summary>
	[Fact]
	public void WhiteSmoke()
	{
		CheckColor(Colors.WhiteSmoke, 0xFFF5F5F5u);
	}

	/// <summary>
	/// Tests getting the <see cref="Colors.Yellow"/> property.
	/// </summary>
	[Fact]
	public void Yellow()
	{
		CheckColor(Colors.Yellow, 0xFFFFFF00u);
	}

	/// <summary>
	/// Tests getting the <see cref="Colors.YellowGreen"/> property.
	/// </summary>
	[Fact]
	public void YellowGreen()
	{
		CheckColor(Colors.YellowGreen, 0xFF9ACD32u);
	}

	/// <summary>
	/// Checks whether the specified <see cref="Color"/> instance reflects the specified color.
	/// </summary>
	/// <param name="color"><see cref="Color"/> instance to check.</param>
	/// <param name="argb">Color as sRGB (from MSB to LSB: A, R, G, B).</param>
	private static void CheckColor(Color color, uint argb)
	{
		byte rgbA = (byte)((argb & 0xFF000000U) >> 24);
		byte rgbR = (byte)((argb & 0x00FF0000U) >> 16);
		byte rgbG = (byte)((argb & 0x0000FF00U) >> 8);
		byte rgbB = (byte)(argb & 0x000000FF);

		Assert.Equal(rgbA, color.A);
		Assert.Equal(rgbR, color.R);
		Assert.Equal(rgbG, color.G);
		Assert.Equal(rgbB, color.B);

		float scRgbA = rgbA / 255.0f;
		float scRgbR = SRgbToScRgb(rgbR);
		float scRgbG = SRgbToScRgb(rgbG);
		float scRgbB = SRgbToScRgb(rgbB);

		Assert.Equal(scRgbA, color.ScA);
		Assert.Equal(scRgbR, color.ScR);
		Assert.Equal(scRgbG, color.ScG);
		Assert.Equal(scRgbB, color.ScB);
	}

	/// <summary>
	/// Converts a channel value from sRGB to scRGB.
	/// </summary>
	/// <param name="value">The sRGB channel value to convert to scRGB.</param>
	/// <returns>The corresponding scRGB channel value.</returns>
	private static float SRgbToScRgb(byte value)
	{
		float num = value / 255.0f;

		return num switch
		{
			<= 0.0f     => 0.0f,
			<= 0.04045f => num / 12.92f,
			var _       => num < 1.0f ? (float)Math.Pow((num + 0.055) / 1.055, 2.4) : 1.0f
		};
	}
}
