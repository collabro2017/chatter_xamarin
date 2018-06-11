using Chadder.Client.Source;
using Chadder.Client.Util;
using Chadder.Data;
using Chadder.Data.Keys;
using Chadder.Data.Response;
using SQLite;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Chadder.Client.Data
{
    public class ChadderContact : SQLRecordBase, IChadderUserProfile, Chadder.Data.Base.IIdentity
    {
        public bool IsTemporary { get; set; }
        public string UserId { get; set; }

        private string _name;
        public string Name
        {
            get { return _name; }
            set
            {
                if (SetField(ref _name, value))
                    NotifyPropertyChanged("DisplayName");
            }
        }

        private string _username;
        public string Username { get { return _username; } set { SetField(ref _username, value); } }

        public byte[] PublicKeyBookData { get; set; }
        public string DisplayName { get { return Name; } }
        public string PictureId { get; set; }

        private RelationshipType _type;
        public RelationshipType Type { get { return _type; } set { SetField(ref _type, value); } }

        private PublicKeyStatus _keyStatus;
        public PublicKeyStatus KeyStatus { get { return _keyStatus; } set { SetField(ref _keyStatus, value); } }

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
        public PublicKeyBook PublicKeyBook
        {
            get { return Chadder.Data.Base.Content.Deserialize<PublicKeyBook>(PublicKeyBookData); }
        }

        public ChadderContact() { }
        public ChadderContact(ContactInfo info, ChadderSource Source)
        {
            UserId = info.Id;
            Update(info, Source);
        }

        public static ChadderContact Create(ContactInfo info, ChadderSource Source, bool isTemp = false)
        {
            var contact = new ChadderContact()
            {
                UserId = info.Id,
                IsTemporary = isTemp
            };
            contact.Update(info, Source);
            return contact;
        }


        public void Update(ContactInfo info, ChadderSource Source)
        {
            Name = info.Name;
            UpdatePublicKey(info.PublicKey);
            PictureId = info.ProfilePicture;
            Picture = Source.db.GetPicture(PictureId);
        }

        public void UpdatePublicKey(byte[] PublicKey)
        {
            PublicKeyBookData = PublicKey;
        }

        public async Task<PublicKeyBook> GetPublicKeyBook()
        {
            return PublicKeyBook;
        }
    }
}
