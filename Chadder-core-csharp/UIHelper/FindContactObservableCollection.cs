using Chadder.Client.Data;
using Chadder.Client.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Text;

namespace UIHelper
{
    public class FindContactObservableCollection : IEnumerable<ChadderContact>, INotifyCollectionChanged, IList<ChadderContact>
    {
        private Chadder.Client.Source.ChadderSource Source;
        public class FindContactEnumerator : IEnumerator<ChadderContact>, IDisposable
        {
            public FindContactEnumerator(FindContactObservableCollection coll)
            {
                Collection = coll;
                n = -1;
            }
            private FindContactObservableCollection Collection;
            private int n;
            public ChadderContact Current
            {
                get
                {
                    return Collection[n];
                }
            }

            public bool MoveNext()
            {
                if (n + 1 < Collection.Count)
                {
                    n++;
                    return true;
                }
                return false;
            }

            public void Reset()
            {
                n = 0;
            }

            public void Dispose()
            {
            }

            object IEnumerator.Current
            {
                get { return this.Current; }
            }
        }
        public FindContactObservableCollection(Chadder.Client.Source.ChadderSource Source)
        {
            this.Source = Source;
            OnlineResult = new ChadderObservableCollection<ChadderContact>();
            Online = new ContactsFiltered(OnlineResult);
            OnlineResult.CollectionChanged += OnCollectionChanged;

            Contacts = new ContactsFiltered(Source.db.Contacts);
            Contacts.CollectionChanged += OnCollectionChanged;
        }

        bool searching = false;
        public string SearchContent = "";
        public async void SetSearch(string txt)
        {
            SearchContent = txt;
            Contacts.Filter = txt;
            Online.Filter = txt;
            if (searching == true)
                return;
            searching = true;
            string search = "";
            do
            {
                search = SearchContent;
                var results = await Source.FindUser(search);
                OnlineResult.Clear();
                if (results.Error == Chadder.Data.Util.ChadderError.OK)
                {
                    OnlineResult.AddItems(results.List);
                }
            } while (search != SearchContent);
            searching = false;
        }
        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (CollectionChanged != null)
                CollectionChanged(sender, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        public ChadderObservableCollection<ChadderContact> OnlineResult;
        public ContactsFiltered Contacts;
        public ContactsFiltered Online;

        public IEnumerator<ChadderContact> GetEnumerator()
        {
            return new FindContactEnumerator(this);
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return new FindContactEnumerator(this);
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public int IndexOf(ChadderContact item)
        {
            if (item == null)
            {
                if (Online.Count > 0)
                    return Contacts.Count;
                return -1;
            }
            var index = -1;
            if (item is ChadderContact)
                index = Contacts.IndexOf(item as ChadderContact);
            if (index == -1)
            {
                index = Online.IndexOf(item);
                if (index == -1)
                    return -1;
                return index + Contacts.Count;
            }
            else
                return index;
        }

        public void Insert(int index, ChadderContact item)
        {
            throw new NotImplementedException();
        }

        public void RemoveAt(int index)
        {
            throw new NotImplementedException();
        }

        public ChadderContact this[int n]
        {
            get
            {
                if (n < Contacts.Count)
                    return Contacts[n];
                else if (n == Contacts.Count)
                    return null;
                else
                {
                    var temp = n - Contacts.Count - 1;
                    if (Online.Count > temp)
                        return Online[temp];
                    return null;
                }
            }
            set
            {
                throw new NotImplementedException();
            }
        }


        public int Count
        {
            get
            {
                if (Online.Count > 0)
                    return Online.Count + Contacts.Count + 1;
                return Contacts.Count;
            }
        }

        public bool IsReadOnly
        {
            get { return true; }
        }

        public bool Remove(ChadderContact item)
        {
            throw new NotImplementedException();
        }
        public void Add(ChadderContact item)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Contains(ChadderContact item)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(ChadderContact[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }
    }
}