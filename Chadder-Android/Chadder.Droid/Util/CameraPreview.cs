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
using Android.Hardware;
using Android.Content.PM;
using System.Threading.Tasks;
using ChadderLib.Util;

namespace Chadder.Droid.Util
{
    public class CameraPreview : ViewGroup, ISurfaceHolderCallback, IDisposable, Android.Hardware.Camera.IPictureCallback
    {
        private SurfaceView mSurfaceView;
        Camera.Size mPreviewSize;
        Camera.Size mSize;
        IList<Camera.Size> mSupportedPreviewSizes;
        IList<Camera.Size> mSupportedSizes;
        Camera _camera;
        private int currentCamera = 0;
        private TaskCompletionSource<Android.Graphics.Bitmap> _captureComplete;
        private bool IsPreviewRunning { get; set; }
        private Camera PreviewCamera
        {
            get { return _camera; }
            set
            {
                _camera = value;
                if (_camera != null)
                {
                    mSupportedPreviewSizes = PreviewCamera.GetParameters().SupportedPreviewSizes;
                    mSupportedSizes = PreviewCamera.GetParameters().SupportedPictureSizes;
                }
            }
        }

        public void SwitchCamera()
        {
            SetCamera((currentCamera + 1) % NumberOfCameras);
        }
        public bool SetCamera(int num = 0)
        {
            if (PreviewCamera != null)
            {
                Dispose();
            }


            try
            {
                var camera = Camera.Open(num);
                currentCamera = num;
                PreviewCamera = camera;
                camera.SetDisplayOrientation(90);
                RefreshPreviewSize();
                camera.SetPreviewDisplay(mSurfaceView.Holder);

                RefreshParameters();
                return true;
            }
            catch { return false; }
        }

        public void RefreshParameters()
        {
            if (PreviewCamera != null)
            {
                if (IsPreviewRunning)
                    PreviewCamera.StopPreview();
                var parameters = PreviewCamera.GetParameters();
                if (parameters.SupportedFocusModes.Contains(Android.Hardware.Camera.Parameters.FocusModeContinuousPicture))
                    parameters.FocusMode = Android.Hardware.Camera.Parameters.FocusModeContinuousPicture;
                if (mPreviewSize != null)
                {
                    parameters.SetPreviewSize(mPreviewSize.Width, mPreviewSize.Height);
                    parameters.SetPictureSize(mSize.Width, mSize.Height);
                }

                PreviewCamera.SetParameters(parameters);
                if (IsPreviewRunning)
                    PreviewCamera.StartPreview();
            }
        }



        public CameraPreview(Context context)
            : base(context)
        {
            mSurfaceView = new SurfaceView(context);
            AddView(mSurfaceView);
            mSurfaceView.Holder.AddCallback(this);
            // deprecated setting, but required on Android versions prior to 3.0
            mSurfaceView.Holder.SetType(SurfaceType.PushBuffers);
        }

        public async void OnPictureTaken(byte[] data, Android.Hardware.Camera camera)
        {
            using (var bmp = await Android.Graphics.BitmapFactory.DecodeByteArrayAsync(data, 0, data.Length))
            {
                using (var resized = bmp.DefaultResize())
                {
                    var mat = new Android.Graphics.Matrix();

                    var info = new Android.Hardware.Camera.CameraInfo();
                    Android.Hardware.Camera.GetCameraInfo(currentCamera, info);

                    if (info.Facing == CameraFacing.Front)
                        mat.PostScale(-1, 1);

                    mat.PostRotate(90);
                    var correctBmp = Android.Graphics.Bitmap.CreateBitmap(resized, 0, 0, resized.Width, resized.Height, mat, true);
                    _captureComplete.SetResult(correctBmp);
                    _captureComplete = null;
                }
            }
        }


        public void SurfaceCreated(ISurfaceHolder Holder)
        {
            if(PreviewCamera != null)
                PreviewCamera.SetPreviewDisplay(mSurfaceView.Holder);
        }

        public void SurfaceDestroyed(ISurfaceHolder Holder)
        {
            StopPreview();
        }

        public void SurfaceChanged(ISurfaceHolder Holder, Android.Graphics.Format format, int width, int height)
        {
            RefreshParameters();
            StartPreview();
        }

        protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
        {
            // We purposely disregard child measurements because act as a
            // wrapper to a SurfaceView that centers the camera preview instead
            // of stretching it.
            int width = ResolveSize(SuggestedMinimumWidth, widthMeasureSpec);
            int height = ResolveSize(SuggestedMinimumHeight, heightMeasureSpec);
            SetMeasuredDimension(width, height);

            RefreshPreviewSize();
        }

        private Camera.Size GetOptimalPreviewSize(IList<Camera.Size> sizes, int w, int h)
        {
            const double ASPECT_TOLERANCE = 0.1;
            double targetRatio = (double)w / h;

            if (sizes == null)
                return null;

            Camera.Size optimalSize = null;
            double minDiff = Double.MaxValue;

            int targetHeight = h;

            // Try to find an size match aspect ratio and size
            foreach (Camera.Size size in sizes)
            {
                double ratio = (double)size.Width / size.Height;

                if (Math.Abs(ratio - targetRatio) > ASPECT_TOLERANCE)
                    continue;

                if (Math.Abs(size.Height - targetHeight) < minDiff)
                {
                    optimalSize = size;
                    minDiff = Math.Abs(size.Height - targetHeight);
                }
            }

            // Cannot find the one match the aspect ratio, ignore the requirement
            if (optimalSize == null)
            {
                minDiff = Double.MaxValue;
                foreach (Camera.Size size in sizes)
                {
                    if (Math.Abs(size.Height - targetHeight) < minDiff)
                    {
                        optimalSize = size;
                        minDiff = Math.Abs(size.Height - targetHeight);
                    }
                }
            }

            return optimalSize;
        }

        protected override void OnLayout(bool changed, int l, int t, int r, int b)
        {
            if (changed && ChildCount > 0)
            {
                View child = GetChildAt(0);

                int width = r - l;
                int height = b - t;

                int previewWidth = width;
                int previewHeight = height;
                if (mPreviewSize != null)
                {
                    // Switched because we want portrait not landscape
                    previewWidth = mPreviewSize.Height;
                    previewHeight = mPreviewSize.Width;
                }

                if (width * previewHeight < height * previewWidth)
                {
                    int scaledChildWidth = previewWidth * height / previewHeight;
                    child.Layout((width - scaledChildWidth) / 2, 0,
                                 (width + scaledChildWidth) / 2, height);
                }
                else
                {
                    int scaledChildHeight = previewHeight * width / previewWidth;
                    child.Layout(0, (height - scaledChildHeight) / 2,
                                 width, (height + scaledChildHeight) / 2);
                }
            }
        }
        public void RefreshPreviewSize()
        {
            if (mSupportedPreviewSizes != null)
            {
                // Switched because we want portrait not landscape
                mPreviewSize = GetOptimalPreviewSize(mSupportedPreviewSizes, MeasuredHeight, MeasuredWidth);
            }
            if (mSupportedPreviewSizes != null)
            {
                // Switched because we want portrait not landscape
                mSize = GetOptimalPreviewSize(mSupportedSizes, MeasuredHeight, MeasuredWidth);
            }
        }
        public Task<Android.Graphics.Bitmap> Capture()
        {
            _captureComplete = new TaskCompletionSource<Android.Graphics.Bitmap>();
            PreviewCamera.TakePicture(null, null, this);
            return _captureComplete.Task;
        }
        public void StartPreview()
        {
            if (PreviewCamera != null)
                PreviewCamera.StartPreview();
            IsPreviewRunning = true;
        }

        public void StopPreview()
        {
            if (PreviewCamera != null && IsPreviewRunning)
            {
                PreviewCamera.StopPreview();
            }
            IsPreviewRunning = false;
        }

        public void Dispose()
        {
            if (PreviewCamera != null)
            {
                StopPreview();
                PreviewCamera.Release();
                PreviewCamera = null;
            }
        }

        public static int NumberOfCameras
        {
            get
            {
                return Camera.NumberOfCameras;
            }
        }
        public static bool checkCameraHardware(Context context)
        {
            if (context.PackageManager.HasSystemFeature(PackageManager.FeatureCameraAny))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}