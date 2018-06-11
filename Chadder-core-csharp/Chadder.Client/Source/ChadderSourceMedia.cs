using Chadder.Client.Data;
using Chadder.Data;
using Chadder.Data.Base;
using Chadder.Data.Response;
using Chadder.Data.Util;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Chadder.Client.Source
{
    public partial class ChadderSource
    {
        public async Task<ChadderError> DownloadPicture(ChadderPicture picture)
        {
            var response = await Session.PostRequestAPI<BasicResponse<byte[]>>(Urls.DownloadBlob, picture.Id);
            if (response.Error == ChadderError.OK)
            {
                var packed = Content.Deserialize(response.Extra);
                if (packed == null)
                {
                    Insight.Track("Not a valid content");
                    return ChadderError.INVALID_INPUT;
                }
                if (packed is ImageContent)
                {
                    var record = new ChadderSQLPicture()
                    {
                        PictureId = picture.Id,
                        Bin = (packed as ImageContent).BinData,
                        ToBeUploaded = false
                    };
                    await sqlDB.InsertAsync(record);
                    picture.RecordId = record.recordId;
                    return ChadderError.OK;
                }
                else
                {
                    Insight.Track("Not a ImageContent");
                    return ChadderError.INVALID_INPUT;
                }
            }
            return response.Error;
        }
        public async Task<BasicResponse<string>> UploadPicture(byte[] data)
        {
            var packed = new ImageContent()
            {
                BinData = data
            }.Serialize();

            var response = await Session.PostRequestAPI<BasicResponse<string>>(Urls.UploadBlob, packed);
            if (response.Error == ChadderError.OK)
            {
                var record = new ChadderSQLPicture()
                {
                    PictureId = response.Extra,
                    Bin = data,
                    ToBeUploaded = false
                };
                await sqlDB.InsertAsync(record);
                await db.LoadPicture(record, true);
            }
            return response;
        }
        public async Task<BasicResponse<string>> UploadPicture(ChadderSQLPicture record)
        {
            var packed = new ImageContent()
            {
                BinData = record.Bin
            }.Serialize();

            var response = await Session.PostRequestAPI<BasicResponse<string>>(Urls.UploadBlob, packed);
            if (response.Error == ChadderError.OK)
            {
                var picture = db.GetPicture(record.PictureId);
                picture.Id = response.Extra;
                picture.ToBeUploaded = false;
                record.PictureId = response.Extra;
                record.ToBeUploaded = false;
                await sqlDB.UpdateAsync(record);
            }
            return response;
        }
    }
}
