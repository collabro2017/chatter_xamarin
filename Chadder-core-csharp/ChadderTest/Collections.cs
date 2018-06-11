using ChadderLib.DataModel;
using ChadderLib.Util;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChadderTest
{
    public class CollectionsTestItem : MyNotifyChanged, OrderableObject<CollectionsTestItem>
    {
        public CollectionsTestItem(int i, bool displayed = true)
        {
            Order = i;
            Displayed = displayed;
        }
        private int _order;
        public int Order { get { return _order; } set { SetField(ref _order, value); } }
        private bool _displayed;
        public bool Displayed { get { return _displayed; } set { SetField(ref _displayed, value); } }
        public int compareTo(CollectionsTestItem obj)
        {
            return Order.CompareTo(obj.Order);
        }
    }
    public class TestFilteredCollection : MyFilteredCollection<CollectionsTestItem>
    {
        public TestFilteredCollection(MyObservableCollection<CollectionsTestItem> _lst) : base(_lst) { }
        public override bool isDisplayed(CollectionsTestItem item)
        {
            return item.Displayed;
        }
    }
    [TestFixture]
    public class Collections : BaseTest
    {
        [Test]
        [MaxTime(3000)]
        public void SortedCollection()
        {
            var lst = new MyOrderedCollection<CollectionsTestItem>();
            var filtered = new TestFilteredCollection(lst);

            int size = 1000;
            int copied = 20;
            int update = 500;
            int remove = 150;
            int addMore = 200;
            var random = new Random();
            for (int i = 0; i < size; ++i)
            {
                lst.Add(new CollectionsTestItem(random.Next(), random.Next() % 2 == 0));
            }
            for (int i = 0; i < copied; ++i)
            {
                lst.Add(new CollectionsTestItem(lst[i].Order, lst[i].Displayed));
            }

            int current = -1;
            foreach (var item in lst)
            {
                Assert.False(current.CompareTo(item.Order) > 0);
                current = item.Order;
                if(item.Displayed)
                    Assert.True(filtered.Contains(item));
                else
                    Assert.False(filtered.Contains(item));
            }
            for (int i = 0; i < update; ++i)
            {
                var item = lst[random.Next(lst.Count - 1)];
                item.Order = random.Next();
                item.Displayed = random.Next() % 2 == 0;
            }
            for (int i = 0; i < addMore; ++i)
            {
                lst.Add(new CollectionsTestItem(random.Next(), random.Next() % 2 == 0));
            }
            for (int i = 0; i < remove; ++i)
            {
                lst.RemoveAt(random.Next(lst.Count - 1));
            }
            current = -1;
            foreach (var item in lst)
            {
                Assert.False(current.CompareTo(item.Order) > 0);
                current = item.Order;
                if (item.Displayed)
                    Assert.True(filtered.Contains(item));
                else
                    Assert.False(filtered.Contains(item));
            }
        }
    }
}
