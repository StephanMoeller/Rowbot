using System;
using System.Collections.Generic;
using System.Data;
using System.IO;

namespace Rowbot
{
    public static class RowbotConvert
    {
        public static void DataTable_Csv(DataTable dataTable, Stream outputStream) { }
        public static void DataTable_Excel(DataTable dataTable, Stream outputStream) { }
        public static IEnumerable<TObjectType> DataTable_Objects<TObjectType>(DataTable dataTable) where TObjectType : new() { throw new NotImplementedException(); }
        public static IEnumerable<dynamic> DataTable_Dynamic(DataTable dataTable) { throw new NotImplementedException(); }
        public static IDataReader DataTable_DataReader(DataTable dataTable) { throw new NotImplementedException(); }

        public static void DataReader_Csv(IDataReader dataReader, Stream outputStream) { }
        public static void DataReader_Excel(IDataReader dataReader, Stream outputStream) { }
        public static IEnumerable<TObjectType> DataReader_Objects<TObjectType>(IDataReader dataReader) where TObjectType : new() { throw new NotImplementedException(); }
        public static IEnumerable<dynamic> DataReader_Dynamic(IDataReader dataReader) { throw new NotImplementedException(); }
        public static DataTable DataReader_DataTable(IDataReader dataReader) { throw new NotImplementedException(); }

        public static void Objects_Csv<TObjectType>(IEnumerable<TObjectType> objects, Stream outputStream) { }
        public static void Objects_Excel<TObjectType>(IEnumerable<TObjectType> objects, Stream outputStream) { }
        public static IEnumerable<dynamic> Objects_Dynamic<TObjectType>(IEnumerable<TObjectType> objects) { throw new NotImplementedException(); }
        public static DataTable Objects_DataTable<TObjectType>(IEnumerable<TObjectType> objects) { throw new NotImplementedException(); }
        public static IDataReader Objects_DataReader<TObjectType>(IEnumerable<TObjectType> objects) { throw new NotImplementedException(); }

        public static void Csv_Excel(Stream csvInputSource, Stream excelOutputStream) { }
        public static IEnumerable<TObjectType> Csv_Objects<TObjectType>(Stream csvInputSource) where TObjectType : new() { throw new NotImplementedException(); }
        public static IEnumerable<dynamic> Csv_Dynamic<TObjectType>(Stream csvInputSource) { throw new NotImplementedException(); }
        public static DataTable Csv_DataTable<TObjectType>(Stream csvInputSource) { throw new NotImplementedException(); }
        public static IDataReader Csv_DataReader<TObjectType>(Stream csvInputSource) { throw new NotImplementedException(); }
    }
}
