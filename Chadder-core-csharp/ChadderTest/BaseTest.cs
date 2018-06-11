using ChadderCDM;
using ChadderLib.DataSource;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace ChadderTest
{
    public class BaseTest
    {
        private List<ChadderDataSource> _sources = new List<ChadderDataSource>();
        public ChadderDataSource CreateSource()
        {
            var source = new ChadderDataSource();
            _sources.Add(source);
            return source;
        }
        [TearDown]
        public async void CleanUp()
        {
            var sources = _sources;
            _sources = new List<ChadderDataSource>();
            foreach (var source in sources)
            {
                await source.CleanUp();
            }
        }
        public async Task<ChadderDataSource> CreateAccount(string username = null, string password = null, bool hide = true)
        {
            var source = CreateSource();
            var random = new Random();
            if (username == null)
                username = "autotest" + random.Next().ToString();
            if (password == null)
                password = random.Next().ToString("x8");
            // Valid account
            var first = await source.register("AutoTest", username, password);
            Assert.Null(first);
            if(hide)
                Assert.AreEqual(await source.ChangeShareName(false), ChadderError.OK);
            return source;
        }

        public async Task<List<ChadderDataSource>> CreateAccountsAndAdd(int n)
        {
            var sources = new List<ChadderDataSource>();

            for (var i = 0; i < n; ++i)
            {
                var source = await CreateAccount(hide:false);
                foreach (var other in sources)
                {
                    Ok((await source.addContact(other.database.profile.myId)).Item1);
                }
                sources.Add(source);
            }
            foreach (var source in sources)
                Assert.AreEqual(await source.ChangeShareName(false), ChadderError.OK);

            return sources;
        }

        public void Ok(ChadderError e)
        {
            Assert.AreEqual(ChadderError.OK, e);
        }
    }
}
