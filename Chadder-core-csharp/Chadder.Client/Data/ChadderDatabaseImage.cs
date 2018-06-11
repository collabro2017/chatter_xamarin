using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

namespace Chadder.Client.Data
{
    public partial class ChadderDatabase
    {
        public ChadderPicture DefaultPicture;
        public List<ChadderPicture> Pictures = new List<ChadderPicture>();

        public async Task<ChadderPicture> LoadPicture(ChadderSQLPicture record, bool preload)
        {
            var picture = GetPicture(record.PictureId);
            picture.LoadMetaDataFromRecord(record);
            Pictures.Add(picture);

            if (preload)
            {
                await picture.LoadPictureAsync(true, record);
            }
            return picture;
        }

        public ChadderPicture GetPicture(string id)
        {
            if (id == null)
            {
                if (DefaultPicture == null)
                {
                    DefaultPicture = new ChadderPicture(Source);
                }
                return DefaultPicture;
            }
            var picture = Pictures.FirstOrDefault(i => i.Id == id);
            if (picture == null)
            {
                picture = new ChadderPicture(Source)
                {
                    Id = id,
                };
                Pictures.Add(picture);
            }
            return picture;
        }
    }
}
