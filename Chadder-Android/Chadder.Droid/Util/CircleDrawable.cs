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
using Android.Graphics.Drawables;
using Android.Graphics;
using Android.Util;
using Chadder.Client.Data;
using Chadder.Data.Util;

namespace Chadder.Droid.Util
{
    public class ImageViewEx : ImageView
    {
        public ImageViewEx(Context c)
            : base(c) { }
        public ImageViewEx(Context c, IAttributeSet a)
            : base(c, a) { }
        public ImageViewEx(Context c, IAttributeSet a, int d)
            : base(c, a, d) { }

        public bool IsThumbnail { get; set; }
        public bool IsCircle { get; set; }
        private ChadderPicture _picture;
        public ChadderPicture Picture
        {
            get
            {
                return _picture;
            }
            set
            {
                if (_picture != null)
                    _picture.PropertyChanged -= Picture_PropertyChanged;
                _picture = value;
                if (_picture != null)
                    _picture.PropertyChanged += Picture_PropertyChanged;
                UpdatePicture();
            }
        }

        public void UpdatePicture()
        {
            try
            {
                if (Picture != null)
                {
                    Bitmap img = null;
                    if (IsThumbnail)
                        img = Picture.Thumbnail;
                    else
                        img = Picture.Image;
                    if (IsCircle)
                        SetImageDrawable(new CircleDrawable(img));
                    else
                        SetImageBitmap(img);
                }
            }
            catch (Exception ex)
            {
                Insight.Track("ImageViewEx.UpdatePicture Exception: " + ex.Message);
            }
        }

        public void Picture_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            UpdatePicture();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            Picture = null;
        }
    }
    public class CircleDrawable : Drawable
    {
        Bitmap bmp;
        BitmapShader bmpShader;
        Paint paint;
        RectF oval;

        public CircleDrawable(Bitmap bmp)
        {
            this.bmp = bmp;
            this.bmpShader = new BitmapShader(bmp, Shader.TileMode.Clamp, Shader.TileMode.Clamp);
            this.paint = new Paint() { AntiAlias = true };
            this.paint.SetShader(bmpShader);
            this.oval = new RectF();
        }

        public override void Draw(Canvas canvas)
        {
            canvas.DrawOval(oval, paint);
        }

        protected override void OnBoundsChange(Rect bounds)
        {
            base.OnBoundsChange(bounds);
            oval.Set(0, 0, bounds.Width(), bounds.Height());
        }

        public override int IntrinsicWidth
        {
            get
            {
                return bmp.Width;
            }
        }

        public override int IntrinsicHeight
        {
            get
            {
                return bmp.Height;
            }
        }

        public override void SetAlpha(int alpha)
        {

        }

        public override int Opacity
        {
            get
            {
                return (int)Format.Opaque;
            }
        }

        public override void SetColorFilter(ColorFilter cf)
        {

        }
    }
}