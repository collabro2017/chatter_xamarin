using Chadder.Client.Util;
using SQLite;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Chadder.Data.Util;
using Chadder.Client.Source;
using System.Threading.Tasks;
using Android.Graphics;

namespace Chadder.Client.Data
{
#if __ANDROID__
    using ImageType = Android.Graphics.Bitmap;
#elif WINDOWS_DESKTOP
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using ImageType = System.Windows.Media.ImageSource;
#endif
    public class ChadderSQLPicture : SQLRecordBase
    {
        [Unique]
        public string PictureId { get; set; }
        public string Uri { get; set; }
        public byte[] Bin { get; set; }
        public bool ToBeUploaded { get; set; }
        public byte[] Signature { get; set; }
    }
    public partial class ChadderPicture : MyNotifyChanged, IDisposable
    {
        public const int MAX_IMAGE_WIDTH = 800;
        public const int MAX_IMAGE_HEIGHT = 600;
        public const int PROFILE_PIC_WIDTH = 512;
        public const int PROFILE_PIC_HEIGHT = 512;

        public const int THUMBNAIL_WIDTH = 128;
        public const int THUMBNAIL_HEIGHT = 128;

        public ChadderPicture(ChadderSource source)
        {
            this.Source = source;
        }
        public readonly ChadderSource Source;

        public int RecordId { get; set; }
        public string Id { get; set; }
        public bool Temporary { get; set; }
        public bool ToBeUploaded { get; set; }

        public bool IsLoaded { get; private set; }
        public bool IsThumbnailLoaded { get; private set; }

        private ImageType _image;
        private ImageType _thumbnail;
        public ImageType Image
        {
            get
            {
                if (_image == null)
                {
                    if (IsThumbnailLoaded)
                        _image = _thumbnail;
                    else
                        _image = defaultImage;
                    LoadPicture(false);
                }
                return _image;
            }
        }
        public ImageType Thumbnail
        {
            get
            {
                if (_thumbnail == null)
                {
                    _thumbnail = defaultImage;
                    LoadPicture(true);
                }
                return _thumbnail;
            }
        }

        public void Update(ImageType img, bool isThumbnail)
        {
            if (img == null)
            {
                Insight.Track("ChadderPicture:update called with NULL");
                return;
            }
            if (isThumbnail)
            {
                _thumbnail = img;
                IsThumbnailLoaded = true;
                NotifyPropertyChanged("Thumbnail");
            }
            else
            {
                _image = img;
                IsLoaded = true;
                NotifyPropertyChanged("Image");
            }
        }

        public bool IsAvailableOffline { get { return RecordId > 0; } }

        public bool _loading = false;
        public async void LoadPicture(bool thumbnail)
        {
            await LoadPictureAsync(thumbnail);
        }
        public async Task<bool> LoadPictureAsync(bool thumbnail, ChadderSQLPicture picture = null)
        {
            try
            {
                if (thumbnail && IsThumbnailLoaded || thumbnail == false && IsLoaded)
                    return true; // Already Loaded
                if (Id == null)
                    return true; // Invalid Picture
                if (_loading == true)
                    return true; // Already Loading
                _loading = true;
                if (picture == null && RecordId != 0)
                    picture = await Source.sqlDB.GetPicture(RecordId);
                if (picture == null)
                {
                    if (await Source.DownloadPicture(this) == ChadderError.OK)
                        picture = await Source.sqlDB.GetPicture(RecordId);
                }
                if (picture != null)
                {
                    LoadMetaDataFromRecord(picture);

                    var bmp = await LoadPictureFromData(picture.Bin, thumbnail);
                    Update(bmp, thumbnail);
                    _loading = false;
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                _loading = false;
                Insight.Report(ex);
                return false;
            }
        }

        /// <summary>
        /// Loads the picture data from the record
        /// </summary>
        /// <param name="picture">The picture record</param>
        public void LoadMetaDataFromRecord(ChadderSQLPicture picture)
        {
            if (picture == null)
                return;
            Id = picture.PictureId;
            ToBeUploaded = picture.ToBeUploaded;
            RecordId = picture.recordId;
        }
#if __ANDROID__
        public static ImageType defaultImage = BitmapFactory.DecodeResource(Android.App.Application.Context.Resources, ChadderLib.Droid.Resource.Drawable.ic_default_face);
#endif

        public void Dispose()
        {
            IsLoaded = false;
            IsThumbnailLoaded = false;
#if __ANDROID__ || __IOS__
            if (_image != null)
            {
                _image.Dispose();
            }
            if (_thumbnail != null)
            {
                _thumbnail.Dispose();
            }
#endif
            _image = null;
            _thumbnail = null;
        }
    }
}
