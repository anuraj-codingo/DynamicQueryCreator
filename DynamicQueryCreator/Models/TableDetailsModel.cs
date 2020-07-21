using System.Collections.Generic;

namespace DynamicQueryCreator.Models
{
    public class TableDetailsModel
    {

        public string TableName { get; set; }
        public string TableDisplayName { get; set; }
        public string TableKeyColumnName { get; set; }
        public List<Columndetails> Columndetail { get; set; }
        


    }
    public class Columndetails
    {
        public string column_name { get; set; }
        public string Displaycolumnname { get; set; }
        public string DataTypeName { get; set; }
        //public List<string> Displaycolumnname { get; set; }
        //public List<string> DataTypeName { get; set; }
    }
    public class Tabledata
    {
        public string Tablename { get; set; }
        public string Columnname { get; set; }
        public string TableKeyColumnName { get; set; }
    }
    public class Codition
    {
        public string Tablename { get; set; }
        public string Columnname { get; set; }
        public string Condition { get; set; }
        public string Value { get; set; }
    }
    public class innerjoin
    {
        public string Tablename { get; set; }       
        public string TableKeyColumnName { get; set; }
    }
}