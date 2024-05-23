using System.Collections.Generic;

namespace LiquidMapTestApp.Models
{
    public class JsonResult
    {
        public JsonResult(string fileName)
        {
            FileName = fileName;
            Rows = new List<object>();
        }

        public string FileName { get; set; }
        public List<object> Rows { get; set; }
    }
}
