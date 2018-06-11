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
using Android.Graphics;
using System.Threading.Tasks;
using ChadderLib.Util;
using Chadder.Client.Data;

namespace Chadder.Droid.Views.Media
{
    public class CropImageFragment : BaseFragment
    {
        static readonly int AXIS_X = 0;
        static readonly int AXIS_Y = 1;
        static readonly int MIN_AREA = 185 * 185;

        ImageView _image;
        ImageView _canvasTop, _canvasBottom, _canvasRight, _canvasLeft;
        ImageView _cropResizer;
        Button _button;

        private Bitmap _picture;

        bool cropResizing;
        float _offsetX, _offsetY;

        public TaskCompletionSource<Bitmap> completion;

        public static Task<Bitmap> CropImage(Android.Support.V7.App.AppCompatActivity activity, Bitmap picture)
        {

            var fragment = new CropImageFragment();
            fragment._picture = picture;
            fragment.completion = new TaskCompletionSource<Bitmap>();
            activity.SupportFragmentManager.BeginTransaction()
            .Replace(Resource.Id.content_frame, fragment)
            .AddToBackStack(null)
            .Commit();

            return fragment.completion.Task;
        }
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {

            var view = inflater.Inflate(Resource.Layout.activity_crop_image, container, false);

            _image = view.FindViewById<ImageView>(Resource.Id.cropimage_imageview);
            _cropResizer = view.FindViewById<ImageView>(Resource.Id.crop_resizer);
            _canvasTop = view.FindViewById<ImageView>(Resource.Id.canvasTop);
            _canvasBottom = view.FindViewById<ImageView>(Resource.Id.canvasBottom);
            _canvasLeft = view.FindViewById<ImageView>(Resource.Id.canvasLeft);
            _canvasRight = view.FindViewById<ImageView>(Resource.Id.canvasRight);
            _button = view.FindViewById<Button>(Resource.Id.btnCrop);

            view.FindViewById<Button>(Resource.Id.cropBtnCancel).Click += (sender, e) =>
            {
                _picture = null;
                SupportFragmentManager.PopBackStack();
            };

            _button.Click += OnCropClick;

            _image.Touch += OnImageTouchEvent;
            _cropResizer.Touch += OnResizerTouchEvent;


            _image.SetImageBitmap(_picture);
            _image.RequestLayout();

            _image.Post(() =>
            {
                var imageWidth = _image.Width;
                var imageHeight = _image.Height;
                var minDimen = Math.Min(imageWidth, imageHeight);
                var square = minDimen;


                _canvasTop.LayoutParameters.Height = 0;
                _canvasLeft.LayoutParameters.Width = 0;

                _canvasBottom.LayoutParameters.Height = (imageHeight - square);
                _canvasRight.LayoutParameters.Width = (imageWidth - square);

                _canvasTop.RequestLayout();
                _canvasBottom.RequestLayout();
                _canvasRight.RequestLayout();
                _canvasLeft.RequestLayout();
            });
            return view;
        }

        void OnCropClick(object sender, EventArgs e)
        {
            var pic = _picture;
            if (pic != null)
            {
                var imageCoordinates = GetCoordinates(_image);
                int imageStartX = imageCoordinates[AXIS_X];
                int imageStartY = imageCoordinates[AXIS_Y];

                int imageWidth = pic.Width;
                int imageHeight = pic.Height;

                int drawableWidth = _image.Width;
                int drawableHeight = _image.Height;

                float ratioX = (float)drawableWidth / (float)imageWidth; // Drawed Pixel / Image Pixel
                float ratioY = (float)drawableHeight / (float)imageHeight;

                // Crop coordinates

                var cropBorder = View.FindViewById<View>(Resource.Id.crop_border);
                var coordinates = GetCoordinates(cropBorder);

                int cropStartX = coordinates[AXIS_X] - imageStartX;
                int cropLengthX = cropBorder.Width;

                int cropStartY = coordinates[AXIS_Y] - imageStartY;
                int cropLengthY = cropBorder.Height;


                int realStartX = (int)(cropStartX / ratioX);
                int realStartY = (int)(cropStartY / ratioY);

                int realLengthX = (int)(cropLengthX / ratioX);
                int realLengthY = (int)(cropLengthY / ratioY);

                var bmp = Bitmap.CreateBitmap(pic, realStartX, realStartY, realLengthX, realLengthY);


                var im = bmp.Resize(ChadderPicture.PROFILE_PIC_WIDTH, ChadderPicture.PROFILE_PIC_HEIGHT, false);
                bmp.Dispose();

                completion.SetResult(im);
                completion = null;

                SupportFragmentManager.PopBackStack();
            }
        }

        protected int[] GetCoordinates(View view)
        {
            int[] coordinates = new int[2];
            view.GetLocationOnScreen(coordinates);
            return coordinates;
        }

        public int CalculateFreeArea(int delta = 0)
        {
            var canvasTopArea = _canvasTop.Height * _canvasTop.Width;
            var canvasBottomArea = (_canvasBottom.Height + delta) * _canvasBottom.Width;
            var canvasLeftArea = (_canvasLeft.Height - delta) * (_canvasLeft.Width + delta); // Left and Right canvas Height relies on top and bottom canvas
            var canvasRightArea = (_canvasRight.Height - delta) * (_canvasRight.Width + delta);

            var canvasArea = canvasTopArea + canvasBottomArea + canvasLeftArea + canvasRightArea;
            var imageArea = _image.Height * _image.Width;

            //_button.Text = "ImageArea: " + imageArea + " - CanvasArea: " + canvasArea + " - FreeArea: " + (imageArea - canvasArea);

            return imageArea - canvasArea;
        }

        public void OnResizerTouchEvent(object sender, View.TouchEventArgs t)
        {
            var e = t.Event;

            switch (e.Action)
            {
                case MotionEventActions.Down:
                    cropResizing = true;
                    _offsetX = e.RawX;
                    _offsetY = e.RawY;

                    break;

                case MotionEventActions.Move:
                    int deltaX = (int)(_offsetX - e.RawX);
                    int deltaY = (int)(_offsetY - e.RawY);
                    _offsetX = e.RawX;
                    _offsetY = e.RawY;

                    var deltaXY = Math.Max(deltaX, deltaY);

                    if (CalculateFreeArea(deltaXY) < MIN_AREA)
                        return;


                    if (_canvasBottom.Height + deltaXY > 0 &&
                        _canvasRight.Width + deltaXY > 0)
                    {
                        _canvasBottom.LayoutParameters.Height += deltaXY;
                        _canvasRight.LayoutParameters.Width += deltaXY;
                        _canvasBottom.RequestLayout();
                        _canvasRight.RequestLayout();
                    }
                    else
                        return;

                    break;


                case MotionEventActions.Cancel:
                case MotionEventActions.Up:
                    cropResizing = false;
                    break;
            }
        }

        public void OnImageTouchEvent(object sender, View.TouchEventArgs t)
        {
            var e = t.Event;

            var touchX = (int)e.RawX;
            var touchY = (int)e.RawY;


            switch (e.Action)
            {
                case MotionEventActions.Down:
                    _offsetX = e.RawX;
                    _offsetY = e.RawY;
                    break;

                case MotionEventActions.Move:
                    int deltaX = (int)(_offsetX - e.RawX);
                    int deltaY = (int)(_offsetY - e.RawY);
                    _offsetX = e.RawX;
                    _offsetY = e.RawY;


                    if (cropResizing)
                        return;


                    if (_canvasRight.Width + deltaX > 0 && _canvasLeft.Width - deltaX > 0)
                    {
                        _canvasLeft.LayoutParameters.Width -= deltaX;
                        _canvasLeft.RequestLayout();

                        _canvasRight.LayoutParameters.Width += deltaX;
                        _canvasRight.RequestLayout();
                    }


                    if (_canvasTop.Height - deltaY > 0 && _canvasBottom.Height + deltaY > 0)
                    {
                        _canvasTop.LayoutParameters.Height -= deltaY;
                        _canvasTop.RequestLayout();

                        _canvasBottom.LayoutParameters.Height += deltaY;
                        _canvasBottom.RequestLayout();
                    }

                    break;

                case MotionEventActions.Cancel:
                case MotionEventActions.Up:
                    break;

            }

            //_button.Text = "X: " + e.GetX() + " -  Y: " + e.GetY();
            //return base.OnTouchEvent (e);
        }
    }
}