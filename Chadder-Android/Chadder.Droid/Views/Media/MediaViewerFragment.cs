using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using System.ComponentModel;
using System.IO;
using Android.Graphics;
using Android.Graphics.Drawables;
using Chadder.Client.Data;

namespace Chadder.Droid.Views.Media
{
    public class MediaViewerFragment : BaseFragment
    {
        private static string EXTRA_PICTURE_ID = "EXTRA_PICTURE_ID";
        ImageView _imageView;
        public ChadderPicture _picture;
        public static MediaViewerFragment Create(ChadderPicture picture)
        {
            var result = new MediaViewerFragment();
            result.Arguments = new Bundle();
            result.Arguments.PutString(EXTRA_PICTURE_ID, picture.Id);
            return result;
        }
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            _picture = ChadderUI.Source.db.Pictures.FirstOrDefault(i => i.Id == Arguments.GetString(EXTRA_PICTURE_ID, null));
            if (_picture == null)
                SupportFragmentManager.PopBackStack();
        }
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            HasOptionsMenu = true;
            var view = inflater.Inflate(Resource.Layout.activity_view_media, container, false);
            _imageView = view.FindViewById<ImageView>(Resource.Id.media_viewer_image);
            return view;
        }

        public override void OnCreateOptionsMenu(IMenu menu, MenuInflater inflater)
        {
            base.OnCreateOptionsMenu(menu, inflater);
            inflater.Inflate(Resource.Menu.image_view_menu, menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            if (item.ItemId == Resource.Id.image_view_save)
            {
                SaveToGallery();
                return true;
            }
            return base.OnOptionsItemSelected(item);
        }


        private void SaveToGallery()
        {
            if (_picture != null && _picture.IsLoaded)
            {
                var fPath = DroidUtil.GetChadderTempFile();
                var image = File.Create(fPath);

                FileStream stream = image;

                Bitmap bmp = _picture.Image;
                bmp.Compress(Bitmap.CompressFormat.Jpeg, 100, stream);

                stream.Close();


                // Add file to Gallery
                Intent mediaScanIntent = new Intent(Intent.ActionMediaScannerScanFile);
                Android.Net.Uri contentUri = Android.Net.Uri.FromFile(new Java.IO.File(fPath));
                mediaScanIntent.SetData(contentUri);
                Activity.SendBroadcast(mediaScanIntent);

                DisplayToast("Image saved");
            }
            else
            {
                DisplayToast("Can\'t save this picture.");
            }
        }
        public override void OnResume()
        {
            base.OnResume();
            ShowImage();
        }
        public override void OnPause()
        {
            base.OnPause();
            _picture.PropertyChanged -= UpdateEvent;
        }

        protected void ShowImage()
        {
            if (_picture != null)
            {
                _picture.PropertyChanged += UpdateEvent;
                _imageView.SetImageBitmap(_picture.Image);
            }
        }

        protected void UpdateEvent(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "deleted")
            {
                _picture.PropertyChanged -= UpdateEvent;
                SupportFragmentManager.PopBackStack();
            }
            else if (e.PropertyName == "image" || e.PropertyName == null)
                _imageView.SetImageBitmap(_picture.Image);

        }
    }
}