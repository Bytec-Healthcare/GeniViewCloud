using CsvHelper;
using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Geni_View_SettingTool.Common
{
    public class DeviceList
    {
        public string SNCode { get; set; }
    }

    // 定義 CSV 結構的映射
    public sealed class DeviceListMap : ClassMap<DeviceList>
    {
        public DeviceListMap()
        {
            Map(m => m.SNCode); // 將 Name 屬性映射到 CSV 檔案的 Name 欄位
        }
    }

    class CSVHelper
    {

        public List<string> Read(string path)
        {
            List<string> result = new List<string>();

            using (var reader = new StreamReader(path))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HasHeaderRecord = true, 
                };
                csv.Context.RegisterClassMap<DeviceListMap>(); 

                var records = csv.GetRecords<DeviceList>();

                result = records.Select(x => x.SNCode).ToList();

            }

            return result;
        }
    }
}
