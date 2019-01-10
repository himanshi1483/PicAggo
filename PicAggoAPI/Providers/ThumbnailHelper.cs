using SkiaSharp;
using System;
using System.Drawing;
using System.IO;

namespace PicAggoAPI.Providers
{
    public static class ThumbnailHelper
    {
        public static Size GetThumbnailSize(Image original)
        {
            // Maximum size of any dimension.
            const int maxPixels = 400;
          
            // Width and height.
            int originalWidth = original.Width;
            int originalHeight = original.Height;

            bool IsWideImage = original.Width > original.Height ? true : false;
            // Compute best factor to scale entire image based on larger dimension.
            //int startX = 0,
            //           startY = 0,
            //           size = 0;
            //if (IsWideImage)
            //{
            //    size = original.Height;
            //    startX = (original.Width - original.Height) / 2;
            //}
            //else
            //{
            //    size = original.Width;
            //    startY = (original.Height - original.Width) / 2;
            //}

            double factor;
            if (originalWidth > originalHeight)
            {
                factor = (double)maxPixels / originalWidth;
            }
            else
            {
                factor = (double)maxPixels / originalHeight;
            }

            // Return thumbnail size.
            return new Size((int)(originalWidth * factor), (int)(originalHeight * factor));
        }


        public static Stream getThumbnail(Stream file, int thumbSize = 1280)
        {
            try
            {
                using (var inputStream = new SKManagedStream(file))
                {
                    var original = SKBitmap.Decode(inputStream);
                    var croppedImage = SKImage.FromBitmap(original);
                    //SKCanvas can = new SKCanvas(SKBitmap.Decode(inputStream));
                    //can.Clear(SKColors.White);
                    //can.DrawImage(croppedImage, 0, 0);
                    bool IsWideImage = original.Width > original.Height ? true : false;

                    int startX = 0,
                        startY = 0,
                        size = 0;
                    if (IsWideImage)
                    {
                        size = original.Height;
                        startX = (original.Width - original.Height) / 2;
                    }
                    else
                    {
                        size = original.Width;
                        startY = (original.Height - original.Width) / 2;
                    }
                    croppedImage = croppedImage.Subset(SKRectI.Create(startX, startY, size, size));
                    SKBitmap croppedBitmap = SKBitmap.FromImage(croppedImage);



                    using (var resized = croppedBitmap.Resize(new SKImageInfo(thumbSize, thumbSize), SKBitmapResizeMethod.Lanczos3))
                    {

                        SKImage thumbImage = SKImage.FromBitmap(resized);

                        return (thumbImage.Encode(SKEncodedImageFormat.Png, 75).AsStream());
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static Stream getImage(Stream file, int size, bool widescreen)
        {
            try
            {
                using (var inputStream = new SKManagedStream(file))
                {
                    var original = SKBitmap.Decode(inputStream);

                    var croppedImage = SKImage.FromBitmap(original);
                    //SKCanvas can = new SKCanvas(SKBitmap.Decode(inputStream));
                    //can.Clear(SKColors.White);
                    //can.DrawImage(croppedImage, 0, 0);

                    bool IsWideImage = original.Width > original.Height ? true : false;
                    int height = 0;
                    int width = 0;
                    double ratio = 0;

                    if (widescreen)
                    {
                        int cropHeight = (int)(((double)original.Width * 9d) / 16d);
                        int cropWidth = original.Width;

                        if (original.Height < cropHeight)
                        {
                            cropHeight = original.Height;
                            cropWidth = (int)((double)original.Height * 16d / 9d);

                        }

                        croppedImage = croppedImage.Subset(SKRectI.Create(cropWidth, cropHeight));

                        ratio = 16d / 9d;
                        height = (int)((double)size / ratio);
                        width = size;
                    }
                    else
                    {
                        if (IsWideImage)
                        {
                            ratio = (double)original.Width / (double)original.Height;
                            height = (int)((double)size / ratio);
                            width = size;
                        }
                        else
                        {
                            ratio = original.Height / original.Width;
                            width = (int)(size / ratio);
                            height = size;
                        }
                    }

                    SKBitmap croppedBitmap = SKBitmap.FromImage(croppedImage);

                    using (var resized = croppedBitmap.Resize(new SKImageInfo(width, height), SKBitmapResizeMethod.Lanczos3))
                    {

                        SKImage thumbImage = SKImage.FromBitmap(resized);
                        return (thumbImage.Encode(SKEncodedImageFormat.Png, 85).AsStream());
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}