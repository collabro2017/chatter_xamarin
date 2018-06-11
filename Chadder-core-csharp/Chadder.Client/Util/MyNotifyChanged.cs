using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace Chadder.Client.Util
{
    public class MyNotifyChanged : INotifyPropertyChanged
    {
        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            NotifyPropertyChanged(propertyName);
            return true;
        }

        protected bool SetNonEmpty(ref string field, string value, [CallerMemberName] string propertyName = null)
        {
            if (value != null && value.Length == 0)
                value = null;
            return SetField(ref field, value, propertyName);
        }
        public event PropertyChangedEventHandler PropertyChanged;
        public virtual void NotifyPropertyChanged(String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public void PropagateUIEvent(object sender, PropertyChangedEventArgs e)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(sender, e);
            }
        }

        public string getNullString(string s)
        {
            if (s == null || s.Length == 0)
                return null;
            return s;
        }

    }
}
