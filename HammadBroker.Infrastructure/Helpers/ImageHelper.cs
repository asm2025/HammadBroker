using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using essentialMix.Extensions;
using essentialMix.Helpers;
using HammadBroker.Model;
using JetBrains.Annotations;
using emImageHelper = essentialMix.Drawing.Helpers.ImageHelper;

namespace HammadBroker.Infrastructure.Helpers;

public static class ImageHelper
{
	[NotNull]
	public static string SaveAsJpeg([NotNull] Image image, [NotNull] string fileName, bool renameOnExists = false)
	{
		fileName = PathHelper.Trim(fileName);
		if (string.IsNullOrEmpty(fileName)) throw new ArgumentNullException(nameof(fileName));

		string path = Path.GetDirectoryName(fileName);

		if (path == null)
			path = Directory.GetCurrentDirectory();
		else
			fileName = Path.GetFileName(fileName);

		if (!DirectoryHelper.Ensure(path)) throw new DirectoryNotFoundException();

		if (renameOnExists)
		{
			fileName = Path.GetFileNameWithoutExtension(fileName);

			string combinedPath;
			int i = 0;

			do
			{
				combinedPath = Path.Combine(path, fileName +
												(i++ > 0
													? $" ({i})"
													: string.Empty) +
												".jpg");
			}
			while (PathHelper.Exists(combinedPath));

			fileName = combinedPath;
		}
		else
		{
			fileName = Path.Combine(path, Path.GetFileNameWithoutExtension(fileName) + ".jpg");
		}

		FileStream fileStream = null;

		try
		{
			fileStream = File.OpenWrite(fileName);
			ImageCodecInfo codecInfo = ImageCodecInfo.GetImageEncoders().First(e => e.FormatID == ImageFormat.Jpeg.Guid);
			EncoderParameters encoderParameter = new EncoderParameters(1);
			encoderParameter.Param[0] = new EncoderParameter(Encoder.Quality, Constants.Images.JPEGQuality);
			image.Save(fileStream, codecInfo, encoderParameter);
		}
		finally
		{
			ObjectHelper.Dispose(fileStream);
		}

		return fileName;
	}

	[NotNull]
	public static Image FixImageSize([NotNull] Image image, int newWidth, int newHeight)
	{
		ValidateDimensions(ref newWidth, ref newHeight);
		// resize the image to the new width and height keeping aspect ratio
		return Resize(image, newWidth, newHeight);
	}

	[NotNull]
	public static Image Resize([NotNull] Image image, int maxWidth, int maxHeight)
	{
		if (maxWidth <= 0) throw new ArgumentOutOfRangeException(nameof(maxWidth));
		if (maxHeight <= 0) throw new ArgumentOutOfRangeException(nameof(maxHeight));

		Size size = emImageHelper.GetThumbnailSize(image.Width, image.Height, maxWidth, maxHeight);
		if (size.IsEmpty || (image.Width == size.Width && image.Height == size.Height)) return image;

		int x = Convert.ToInt32((maxWidth - size.Width) / 2.0d);
		int y = Convert.ToInt32((maxHeight - size.Height) / 2.0d);
		Rectangle destRect = new Rectangle(x, y, size.Width, size.Height);
		Bitmap bitmap = null;
		Graphics graphics = null;
		ImageAttributes imageAttributes = null;

		try
		{
			bitmap = new Bitmap(maxWidth, maxHeight);
			bitmap.SetResolution(image.HorizontalResolution.NotAbove(96), image.VerticalResolution.NotAbove(96));

			imageAttributes = new ImageAttributes();
			imageAttributes.SetWrapMode(WrapMode.TileFlipXY);

			graphics = Graphics.FromImage(bitmap);
			graphics.CompositingMode = CompositingMode.SourceCopy;
			graphics.CompositingQuality = CompositingQuality.HighQuality;
			graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
			graphics.SmoothingMode = SmoothingMode.HighQuality;
			graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
			graphics.Clear(Color.White);
			graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, imageAttributes);

			return bitmap;
		}
		catch
		{
			ObjectHelper.Dispose(ref bitmap);
			throw;
		}
		finally
		{
			ObjectHelper.Dispose(ref graphics);
			ObjectHelper.Dispose(ref imageAttributes);
		}
	}

	[NotNull]
	public static Image Crop([NotNull] Image image, int width, int height)
	{
		if (width <= 0) throw new ArgumentOutOfRangeException(nameof(width));
		if (height <= 0) throw new ArgumentOutOfRangeException(nameof(height));
		if (image.Width <= width && image.Height <= height) return image;

		int x = Convert.ToInt32((image.Width - width) / 2.0d);
		int y = Convert.ToInt32((image.Height - height) / 2.0d);
		Rectangle rcSource = new Rectangle(x, y, width, height);
		Rectangle rcDestination = new Rectangle(0, 0, width, height);
		Bitmap bitmap = null;
		Graphics graphics = null;

		try
		{
			bitmap = new Bitmap(rcDestination.Width, rcDestination.Height);
			bitmap.SetResolution(image.HorizontalResolution.NotAbove(96), image.VerticalResolution.NotAbove(96));

			graphics = Graphics.FromImage(bitmap);
			graphics.CompositingMode = CompositingMode.SourceCopy;
			graphics.CompositingQuality = CompositingQuality.HighQuality;
			graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
			graphics.SmoothingMode = SmoothingMode.HighQuality;
			graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
			graphics.DrawImage(image, rcDestination, rcSource, GraphicsUnit.Pixel);
			return bitmap;
		}
		catch
		{
			ObjectHelper.Dispose(ref bitmap);
			throw;
		}
		finally
		{
			ObjectHelper.Dispose(ref graphics);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.ForwardRef)]
	private static void ValidateDimensions(ref int newWidth, ref int newHeight)
	{
		if (newWidth < 16) newWidth = 16;
		if (newHeight < 16) newHeight = 16;
	}
}