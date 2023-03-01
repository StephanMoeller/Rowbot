using System;
using System.Collections.Generic;
using System.Data;
using System.IO;

namespace Rowbot
{
    public static class RowbotConvert
    {
        public static void DataTableToCsv(DataTable dataTable, Stream outputStream) { }
        public static void DataTableToExcel(DataTable dataTable, Stream outputStream) { }
        public static IEnumerable<TObjectType> DataTableToObject<TObjectType>(DataTable dataTable) where TObjectType : new() { throw new NotImplementedException(); }
        public static IEnumerable<dynamic> DataTableToDynamic(DataTable dataTable) { throw new NotImplementedException(); }

        public static void DataReaderToCsv(IDataReader dataReader, Stream outputStream) { }
        public static void DataReaderToExcel(IDataReader dataReader, Stream outputStream) { }
        public static IEnumerable<TObjectType> DataReaderToObject<TObjectType>(IDataReader dataReader) where TObjectType : new() { throw new NotImplementedException(); }
        public static IEnumerable<dynamic> DataReaderToDynamic(IDataReader dataReader) { throw new NotImplementedException(); }

        public static void ObjectsToCsv<TObjectType>(IEnumerable<TObjectType> objects, Stream outputStream) { }
        public static void ObjectsToExcel<TObjectType>(IEnumerable<TObjectType> objects, Stream outputStream) { }
        public static IEnumerable<dynamic> ObjectsToDynamic<TObjectType>(IEnumerable<TObjectType> objects) { throw new NotImplementedException(); }
        public static IEnumerable<dynamic> ObjectsToDataTable<TObjectType>(IEnumerable<TObjectType> objects) { throw new NotImplementedException(); }
    }
}
