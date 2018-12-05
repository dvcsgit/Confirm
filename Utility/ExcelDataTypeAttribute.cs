using System;
using System.ComponentModel.DataAnnotations;

namespace Utility
{
    public class ExcelDataTypeAttribute : Attribute
    {

        public ExcelDataTypeAttribute(DataType dataType)
        {
            this.DataType = dataType;
        }

        public DataType DataType { get; set; }
    }
}
