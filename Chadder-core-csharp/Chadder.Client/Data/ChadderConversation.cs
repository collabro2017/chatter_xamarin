using Chadder.Client.Util;
using SQLite;
using System;
using System.Collections.Generic;
using System.Text;
using Chadder.Data;

namespace Chadder.Client.Data
{
    public class ChadderConversation : SQLRecordBase
    {
        public string ContactUserId { get; set; }
        private ChadderContact _contact;
        [Ignore]
        public ChadderContact Contact
        {
            get { return _contact; }
            set
            {
                if (_contact != null)
                    _contact.PropertyChanged -= _contact_PropertyChanged;
                _contact = value;
                if (_contact != null)
                    _contact.PropertyChanged += _contact_PropertyChanged;
            }
        }

        private void _contact_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            NotifyPropertyChanged("Contact");
        }
        private bool _hidden;
        public bool Hidden { get { return _hidden; } set { SetField(ref _hidden, value); } }

        private int _numNewMessages;
        public int NumNewMessages { get { return _numNewMessages; } set { SetField(ref _numNewMessages, value); } }

        private DateTime _lastUpdate;
        public DateTime LastUpdate { get { return _lastUpdate; } set { SetField(ref _lastUpdate, value); } }

        public string DisplayName { get { return Contact.DisplayName; } }
        public ChadderPicture Picture { get { return Contact.Picture; } }

        public PublicKeyStatus KeyStatus { get { return Contact.KeyStatus; } }

        public ChadderObservableCollection<ChadderMessage> Messages = new ChadderObservableCollection<ChadderMessage>();
    }
}
