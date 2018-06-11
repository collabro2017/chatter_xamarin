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
using Android.Graphics;
using System.Threading.Tasks;
using ChadderLib.Util;
using System.IO;
using Android.Provider;


namespace Chadder.Droid
{
    using FragmentCompat = Android.Support.V4.App.Fragment;
    using Android.Content.PM;
    using Android.Media;
    using Chadder.Data.Util;
    using Chadder.Client.Data;

    public static class ChadderImage
    {
        const int IMAGE_QUALITY = 70;
        static public int getTargetWidth(int width, int height,
            int maxWidth = ChadderPicture.MAX_IMAGE_WIDTH, int maxHeight = ChadderPicture.MAX_IMAGE_HEIGHT)
        {
            float ratio = (float)width / height;
            float defaultRatio = (float)maxWidth / maxHeight;

            if (width > maxWidth && ratio >= defaultRatio)
                return maxWidth;
            if (height > maxHeight && ratio < defaultRatio)
                return (int)(width * ((float)maxHeight / (float)height));
            return width;
        }

        static public int getTargetHeight(int width, int height,
            int maxWidth = ChadderPicture.MAX_IMAGE_WIDTH, int maxHeight = ChadderPicture.MAX_IMAGE_HEIGHT)
        {
            float ratio = (float)width / height;
            float defaultRatio = (float)maxWidth / maxHeight;

            if (height > maxHeight && ratio < defaultRatio)
                return maxHeight;
            if (width > maxWidth && ratio >= defaultRatio)
                return (int)(height * ((float)maxWidth / (float)width));
            return height;
        }
        static public Bitmap LoadFrom(string realPath)
        {
            if (!File.Exists(realPath))
            {
                Insight.Track("ChadderImage: Invalid real path");
                return null;
            }

            ExifInterface exif = new ExifInterface(realPath);
            var a = exif.GetAttributeInt(ExifInterface.TagOrientation, -1);

            return SampledLoad(realPath, a);
        }

        static public Bitmap LoadFrom(Android.Net.Uri contentPath, Activity activity)
        {
            string filePath = DroidUtil.GetRealPathFromURI(contentPath, activity);
            if (string.IsNullOrEmpty(filePath))
            {
                Insight.Track("ChadderImage: Invalid real path");
                return null;
            }

            ExifInterface exif = new ExifInterface(filePath);
            var a = exif.GetAttributeInt(ExifInterface.TagOrientation, -1);

            return SampledLoad(filePath, a);
        }

        static public Bitmap SampledLoad(string filePath, int orientation)
        {
            BitmapFactory.Options opts = new BitmapFactory.Options();
            opts.InJustDecodeBounds = true;
            BitmapFactory.DecodeFile(filePath, opts);

            int imageWidth = opts.OutWidth;
            int imageHeight = opts.OutHeight;
            int inSampleSize = InSampleScale(imageWidth, imageHeight, ChadderPicture.MAX_IMAGE_WIDTH, ChadderPicture.MAX_IMAGE_HEIGHT);

            opts.InJustDecodeBounds = false;
            opts.InSampleSize = inSampleSize;

            var bmp = BitmapFactory.DecodeFile(filePath, opts);

            float degree = 0;

            if (orientation == (int)Orientation.Rotate180)
                degree = 180;
            else if (orientation == (int)Orientation.Rotate270)
                degree = 270;
            else if (orientation == (int)Orientation.Rotate90)
                degree = 90;

            if (degree != 0)
            {
                Matrix matrix = new Matrix();
                matrix.PostRotate(degree);
                bmp = Android.Graphics.Bitmap.CreateBitmap(bmp, 0, 0, bmp.Width, bmp.Height, matrix, true);
            }

            return bmp;
        }


        static public Bitmap DefaultResize(this Bitmap _image)
        {
            return Resize(_image, ChadderPicture.MAX_IMAGE_WIDTH, ChadderPicture.MAX_IMAGE_HEIGHT);
        }

        static public Bitmap Resize(this Bitmap _image, int maxWidth, int maxHeight, bool keepAspect = true)
        {
            int dstWidth = maxWidth;
            int dstHeight = maxHeight;

            if (keepAspect)
            {
                if (_image.Width > maxWidth || _image.Height > maxHeight)
                {
                    dstWidth = getTargetWidth(_image.Width, _image.Height, maxWidth, maxHeight);
                    dstHeight = getTargetHeight(_image.Width, _image.Height, maxWidth, maxHeight);
                }
                else
                    return _image;
            }

            Bitmap resized = Bitmap.CreateScaledBitmap(_image, dstWidth, dstHeight, true);
            return resized;
        }

        static public byte[] CompressToJPEG(this Bitmap _image)
        {
            if (_image != null)
            {
                MemoryStream stream = new MemoryStream();
                _image.Compress(Bitmap.CompressFormat.Jpeg, IMAGE_QUALITY, stream);
                return stream.ToArray();
            }

            return null;
        }

        public static int InSampleScale(int imageWidth, int imageHeight, int maxWidth, int maxHeight)
        {
            int inSampleSize = 1;

            if (imageWidth > maxWidth || imageHeight > maxHeight)
            {
                while ((imageWidth / inSampleSize) > maxWidth && (imageHeight / inSampleSize) > maxHeight)
                    inSampleSize *= 2;
            }

            return inSampleSize / 2; // It returns one value higher to avoid cropping more than intended
        }
    }

    public static class ChadderImagePicker
    {
        public static readonly int INTENT_PICK_IMAGE = 0x10;
        private static string TempFile;

        private static void MultiSourceDialog(Activity activity, FragmentCompat fragment = null)
        {
            List<Intent> cameraIntents = new List<Intent>();
            Intent captureIntent = new Intent(MediaStore.ActionImageCapture);
            PackageManager packageManager = activity.PackageManager;
            IList<ResolveInfo> listCam = packageManager.QueryIntentActivities(captureIntent, 0);
            foreach (ResolveInfo res in listCam)
            {
                String packageName = res.ActivityInfo.PackageName;
                Intent intent = new Intent(captureIntent);
                intent.SetComponent(new ComponentName(res.ActivityInfo.PackageName, res.ActivityInfo.Name));
                intent.SetPackage(packageName);
                TempFile = DroidUtil.GetChadderTempFile();
                intent.PutExtra(MediaStore.ExtraOutput, Android.Net.Uri.FromFile(new Java.IO.File(TempFile)));
                cameraIntents.Add(intent);
            }

            // Filesystem.
            Intent galleryIntent = new Intent();
            galleryIntent.SetType("image/*");
            galleryIntent.SetAction(Intent.ActionGetContent);

            // Chooser of filesystem options.
            Intent chooserIntent = Intent.CreateChooser(galleryIntent, "Select Source");

            // Add the camera options.
            chooserIntent.PutExtra(Intent.ExtraInitialIntents, cameraIntents.ToArray<IParcelable>());

            if (fragment == null)
                activity.StartActivityForResult(chooserIntent, INTENT_PICK_IMAGE);
            else
                fragment.StartActivityForResult(chooserIntent, INTENT_PICK_IMAGE);

        }

        public static void PickFromGallery(Activity activity = null, FragmentCompat fragment = null)
        {
            // Filesystem.
            Intent galleryIntent = new Intent();
            galleryIntent.SetType("image/*");
            galleryIntent.SetAction(Intent.ActionGetContent);

            // Chooser of filesystem options.
            Intent chooserIntent = Intent.CreateChooser(galleryIntent, "Select Source");

            if (activity != null)
                activity.StartActivityForResult(chooserIntent, INTENT_PICK_IMAGE);
            if (fragment != null)
                fragment.StartActivityForResult(chooserIntent, INTENT_PICK_IMAGE);
        }

        public static void Capture(Activity activity)
        {
            List<Intent> cameraIntents = new List<Intent>();
            Intent captureIntent = new Intent(MediaStore.ActionImageCapture);
            PackageManager packageManager = activity.PackageManager;
            IList<ResolveInfo> listCam = packageManager.QueryIntentActivities(captureIntent, 0);
            foreach (ResolveInfo res in listCam)
            {
                String packageName = res.ActivityInfo.PackageName;
                Intent intent = new Intent(captureIntent);
                intent.SetComponent(new ComponentName(res.ActivityInfo.PackageName, res.ActivityInfo.Name));
                intent.SetPackage(packageName);
                TempFile = DroidUtil.GetChadderTempFile();
                intent.PutExtra(MediaStore.ExtraOutput, Android.Net.Uri.FromFile(new Java.IO.File(TempFile)));
                cameraIntents.Add(intent);
            }

            if (cameraIntents.Count == 1)
                activity.StartActivityForResult(cameraIntents[0], INTENT_PICK_IMAGE);
            else
            {
                // Chooser of filesystem options.
                Intent chooserIntent = Intent.CreateChooser(cameraIntents[0], "Select Source");

                // Add the camera options.
                chooserIntent.PutExtra(Intent.ExtraInitialIntents, cameraIntents.Skip(1).ToArray<IParcelable>());

                activity.StartActivityForResult(chooserIntent, INTENT_PICK_IMAGE);
            }
        }

        public static void MultiSourcePick(FragmentCompat fragment)
        {
            MultiSourceDialog(fragment.Activity, fragment);
        }

        public static void MultiSourcePick(Activity activity)
        {
            MultiSourceDialog(activity);
        }

        static public Bitmap OnPickImageResult(int requestCode, int resultCode, Intent data, Activity activity)
        {
            if (resultCode == (int)Result.Ok)
            {
                if (requestCode == INTENT_PICK_IMAGE)
                {
                    if (data == null || data.Data == null || (TempFile != null && data.Data.ToString().EndsWith(Uri.EscapeUriString(TempFile))))
                    {
                        var image = ChadderImage.LoadFrom(TempFile);
                        File.Delete(TempFile);
                        return image;
                    }
                    else
                    {
                        Android.Net.Uri imageUri = data.Data;
                        return ChadderImage.LoadFrom(imageUri, activity);
                    }
                }
            }

            return null;
        }

    }
}