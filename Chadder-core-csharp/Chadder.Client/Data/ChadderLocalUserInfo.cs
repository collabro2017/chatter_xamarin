using Chadder.Client.Util;
using Chadder.Data.Keys;
using SQLite;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Chadder.Client.Data
{
    public interface IChadderUserProfile
    {
        string UserId { get; }
        string DisplayName { get; }
        Task<PublicKeyBook> GetPublicKeyBook();
        string PictureId { get; }
        ChadderPicture Picture { get; }
    }
    public class ChadderLocalUserInfo : SQLSingleton, IChadderUserProfile
    {
        public string UserId { get; set; }

        private byte[] _privateKeyBookData;
        public byte[] PrivateKeyBookData { get { return _privateKeyBookData; } set { SetField(ref _privateKeyBookData, value); } }

        private string _name;
        public string Name { get { return _name; } set { SetField(ref _name, value); } }
        private bool _isNamePublic;
        public bool IsNamePublic { get { return _isNamePublic; } set { SetField(ref _isNamePublic, value); } }


        public string DisplayName { get { return Name; } }
        public long LastPackage { get; set; }
        public DateTime LastContactUpdate { get; set; }
        public string PictureId { get; set; }

        private string _email;
        public string Email { get { return _email; } set { SetField(ref _email, value); } }

        private bool _emailVerified;
        public bool EmailVerified { get { return _emailVerified; } set { SetField(ref _emailVerified, value); } }

        private string _phone;
        public string Phone { get { return _phone; } set { SetField(ref _phone, value); } }

        private bool _phoneVerified;
        public bool PhoneVerified { get { return _phoneVerified; } set { SetField(ref _phoneVerified, value); } }

        public ChadderObservableCollection<ChadderUserDevice> Devices = new ChadderObservableCollection<ChadderUserDevice>();

        public byte[] RefreshToken { get; set; }



        private ChadderPicture _picture;
        [Ignore]
        public ChadderPicture Picture
        {
            get { return _picture; }
            set
            {
                if (_picture != null)
                    _picture.PropertyChanged -= _picture_PropertyChanged;
                _picture = value;
                if (_picture != null)
                    _picture.PropertyChanged += _picture_PropertyChanged;
            }
        }

        private void _picture_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            NotifyPropertyChanged("Picture");
        }


        public PrivateKeyBook PrivateKeyBook { get { return Chadder.Data.Base.Content.Deserialize<PrivateKeyBook>(PrivateKeyBookData); } }
        public bool HasUserKey
        {
            get { return PrivateKeyBookData != null; }
        }


        public Task<PublicKeyBook> GetPublicKeyBook()
        {
            return PrivateKeyBook.GetPublicBook();
        }
    }
}
