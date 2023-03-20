using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.CompilerServices;
using essentialMix.Helpers;
using JetBrains.Annotations;
using emImageHelper = essentialMix.Drawing.Helpers.ImageHelper;

namespace HammadBroker.Infrastructure.Helpers;

public static class ImageHelper
{
	[NotNull]
	public static Image FixImageSize([NotNull] Image image, int newWidth, int newHeight)
	{
		ValidateDimensions(ref newWidth, ref newHeight);

		Size size = GetNewImageSize(image, newWidth, newHeight);
		if (size.IsEmpty || (image.Width == size.Width && image.Height == size.Height)) return image;

		// resize the image to the new width and height keeping aspect ratio
		Image newImage = emImageHelper.Resize(image, size.Width, size.Height);
		if (newImage.Width <= newWidth || newImage.Height <= newHeight) return newImage;

		// if the image width and height is larger, we'll need to crop it
		Image croppedImage = Crop(newImage, newWidth, newHeight);
		if (newImage != croppedImage) ObjectHelper.Dispose(ref newImage);
		return croppedImage;
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
			bitmap.SetResolution(image.HorizontalResolution, image.VerticalResolution);

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

	public static Size GetNewImageSize([NotNull] Image image, int newWidth, int newHeight)
	{
		if (image.Width == 0 || image.Height == 0) return Size.Empty;
		ValidateDimensions(ref newWidth, ref newHeight);
		if (image.Width == newWidth && image.Height == newHeight) return image.Size;

		double xRatio = image.Width / (double)newWidth;
		double yRatio = image.Height / (double)newHeight;
		double ratio = image.Width < newWidth || image.Height < newHeight
							// enlarge the image
							? Math.Max(xRatio, yRatio)
							// shrink the image
							: Math.Min(xRatio, yRatio);
		int xScale = Convert.ToInt32(image.Width * ratio);
		int yScale = Convert.ToInt32(image.Height * ratio);
		return new Size(xScale, yScale);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.ForwardRef)]
	private static void ValidateDimensions(ref int newWidth, ref int newHeight)
	{
		if (newWidth < 16) newWidth = 16;
		if (newHeight < 16) newHeight = 16;
	}
}