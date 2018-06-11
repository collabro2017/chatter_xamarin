using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using ImageType = Android.Graphics.Bitmap;
using Android.Graphics;
using System.Threading.Tasks;
using System.IO;
using Android.Media;
using Chadder.Data.Util;

namespace Chadder.Client.Data
{
    public partial class ChadderPicture
    {
        static public async Task<ImageType> LoadPictureFromFile(string uri)
        {
            if (uri.StartsWith("drawable:"))
            {
                var resId = int.Parse(uri.Substring(uri.IndexOf(":") + 1));
                return BitmapFactory.DecodeResource(Android.App.Application.Context.Resources, resId);
            }
            else
            {
                return await BitmapFactory.DecodeFileAsync(uri);
            }
        }
        static public async Task<ImageType> LoadPictureFromData(byte[] data, bool thumbnail)
        {
            Bitmap bmp = null;

            if (thumbnail)
            {
                using (var stream = new MemoryStream(data))
                {
                    using (var first = await BitmapFactory.DecodeStreamAsync(stream))
                        bmp = ThumbnailUtils.ExtractThumbnail(first, THUMBNAIL_WIDTH, THUMBNAIL_HEIGHT);
                }
            }
            else
            {
                using (var stream = new MemoryStream(data))
                    bmp = await BitmapFactory.DecodeStreamAsync(stream);
            }
            if (bmp == null)
                Insight.Track(string.Format("LoadPictureFromData returning NULL dataSize:{0}, thumbnail:{1}", data.Length, thumbnail));
            return bmp;
        }
    }
}