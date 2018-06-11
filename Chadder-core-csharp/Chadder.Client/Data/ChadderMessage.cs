using Chadder.Client.Util;
using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chadder.Client.Data
{
    public class ChadderMessage : SQLRecordBase
    {
        public static ChadderMessage Create(ChadderConversation conversation, IChadderUserProfile Sender, MESSAGE_TYPE type)
        {
            return new ChadderMessage()
            {
                MessageId = Guid.NewGuid().ToString(),
                Type = type,
                Status = ChadderMessage.MESSAGE_STATUS.SENDING,
                MyMessage = true,
                ConversationId = conversation.recordId,
                UserId = Sender.UserId,
                Sender = Sender,
                Expiration = DateTime.UtcNow.AddDays(7),
                TimeSent = DateTime.UtcNow,
            };
        }

        public enum MESSAGE_STATUS { NONE, SENDING, SENT, ERROR };
        public enum MESSAGE_TYPE { NONE, TEXT, PICTURE, SYSTEM };

        public string MessageId { get; set; }
        public string ReferenceId { get; set; }
        public bool MyMessage { get; set; }
        public string UserId { get; set; }
        [Ignore]
        public IChadderUserProfile Sender { get; set; }
        public int ConversationId { get; set; }

        private MESSAGE_STATUS _status;
        public MESSAGE_STATUS Status { get { return _status; } set { SetField(ref _status, value); } }
        private MESSAGE_TYPE _type;
        public MESSAGE_TYPE Type { get { return _type; } set { SetField(ref _type, value); } }

        public DateTime? TimeSent { get; set; }
        public DateTime? TimeServer { get; set; }
        public DateTime? TimeReceived { get; set; }
        public DateTime? TimeRead { get; set; }
        public DateTime? Expiration { get; set; }

        public string Body { get; set; }

        public string PictureId { get; set; }

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


        // Helper Functions
        public string Preview
        {
            get
            {
                if (Type == MESSAGE_TYPE.PICTURE)
                    return "Picture";
                return Body;
            }
        }
        public string TimeDisplay
        {
            get
            {
                if (MyMessage)
                    return ChadderHelper.GetNiceDateFormat(TimeSent.Value);
                else
                    return ChadderHelper.GetNiceDateFormat(TimeServer.Value);
            }
        }
        public string TimeDisplayCompact
        {
            get
            {
                if (MyMessage)
                    return ChadderHelper.GetNiceDateFormat(TimeSent.Value);
                else
                    return ChadderHelper.GetNiceDateFormat(TimeServer.Value);
            }
        }
    }
}
