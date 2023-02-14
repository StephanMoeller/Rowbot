using Rowbot.Sources;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rowbot.Test.Sources
{
    public class DataReaderSource_Test
    {
        [Fact]
        public void GetColumnsAndRows_NoRows_Test()
        {
            var dataTable = new DataTable();
            dataTable.Columns.Add(new DataColumn("Col1", typeof(string)));
            dataTable.Columns.Add(new DataColumn("Col2 æøå Φ", typeof(int)));
            dataTable.Columns.Add(new DataColumn("Col3", typeof(decimal)));
            dataTable.Columns.Add(new DataColumn(""));
            dataTable.Columns.Add(new DataColumn("Column number four"));
            dataTable.Columns.Add(new DataColumn(""));

            var source = new DataReaderSource(dataTable.CreateDataReader());
            var columns = source.InitAndGetColumns().ToArray();
            Assert.Equal(6, columns.Length);
            AssertColumn<string>(columns[0], "Col1");
            AssertColumn<int>(columns[1], "Col2 æøå Φ");
            AssertColumn<decimal>(columns[2], "Col3");
            AssertColumn<string>(columns[3], "Column1");
            AssertColumn<string>(columns[4], "Column number four");
            AssertColumn<string>(columns[5], "Column2");
            
            var rows = TestUtils.ReadAllLines(source, columnCount: columns.Length);
            Assert.Empty(rows);
        }

        private void AssertColumn<T>(ColumnInfo col, string expectedName)
        {
            Assert.NotNull(col);
            Assert.NotNull(expectedName);
            
            Assert.Equal(expectedName, col.Name);
            Assert.Equal(typeof(T), col.ValueType);
        }

        [Fact]
        public void GetColumnsAndRows_NoColumns_ExpectException_Test()
        {
            var dataTable = new DataTable();
            var source = new DataReaderSource(dataTable.CreateDataReader());
            Assert.Empty(source.InitAndGetColumns());
        }

        [Fact]
        public void GetColumnsAndRows_Test()
        {
            var dataTable = new DataTable();
            dataTable.Columns.Add(new DataColumn("Col1", typeof(string)));
            dataTable.Columns.Add(new DataColumn("Col2 æøå Φ", typeof(decimal)));
            dataTable.Columns.Add(new DataColumn("Col3"));
            dataTable.Columns.Add(new DataColumn(""));
            dataTable.Columns.Add(new DataColumn("Column number four"));
            dataTable.Columns.Add(new DataColumn(""));

            {
                var row = dataTable.NewRow();
                row["Col1"] = "string_val_1";
                row["Col2 æøå Φ"] = -12.34m;
                row["Col3"] = DBNull.Value;
                row["Column1"] = new DateTime(2001, 02, 03, 04, 05, 06);
                // left out on purpose: row["Column number four"]
                row["Column2"] = "";
                dataTable.Rows.Add(row);
            }

            {
                // Empty row
                var row = dataTable.NewRow();
                dataTable.Rows.Add(row);
            }

            {
                var row = dataTable.NewRow();
                row["Col1"] = 123;
                row["Col2 æøå Φ"] = 112;
                row["Col3"] = 1234;
                row["Column1"] = 11;
                row["Column number four"] = null;
                row["Column2"] = 22;
                dataTable.Rows.Add(row);
            }

            var source = new DataReaderSource(dataTable.CreateDataReader());

            // Ensure to call init event though we dont assert on the columns in this test
            var columns = source.InitAndGetColumns();

            // Assert row elements
            var rows = TestUtils.ReadAllLines(source, columnCount: columns.Length);

            Assert.Equal(3, rows.Length);
            AssertArraysEqual(new object?[] { "string_val_1", -12.34m, null, "03/02/2001 04.05.06", null, "" }, rows[0]);
            AssertArraysEqual(new object?[] { null, null, null, null, null, null }, rows[1]);
            AssertArraysEqual(new object?[] { "123", 112m, "1234", "11", null, "22" }, rows[2]);
        }

        private static void AssertArraysEqual(object?[] val1, object[] val2)
        {
            Assert.Equal(val1.Length, val2.Length);
            for (var i = 0; i < val1.Length; i++)
            {
                if (val1[i] == null && val2[i] == null)
                    return;

                if (!Equals(val1[i], val2[i]))
                {
                    Assert.True(false, $"Value at position {i} has value {val1[i]} ({val1[i]?.GetType().Name ?? "NULL"}) in val1, but value {val2[i]} ({val2[i]?.GetType().Name ?? "NULL"}) in val2");
                }
            }
        }
    }
}
