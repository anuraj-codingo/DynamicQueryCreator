using ClosedXML.Excel;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using DynamicQueryCreator.Models;
using log4net;
using Microsoft.Ajax.Utilities;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web.Mvc;

namespace DynamicQueryCreator.Controllers
{
    public class HomeController : Controller
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(HomeController));
        string connectionString = ConfigurationManager.ConnectionStrings["DynamicQueryCreator"].ConnectionString;

        public ActionResult Index()
        {
            var model = new TableDetailsModel();
            List<TableDetailsModel> TableDetails =new List<TableDetailsModel>();
             using (SqlConnection con = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("select * from TableDetails order by [Table]", con))
                {
                    cmd.CommandType = CommandType.Text;
                    using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                    {
                        using (DataTable dt = new DataTable())
                        {
                            sda.Fill(dt);



                           List<Columndetails> x = new List<Columndetails>();
                            //List<string> y = new List<string>();
                            //List<string> z = new List<string>();
                            TableDetailsModel Table = new TableDetailsModel();
                            for (int i = 0; i < dt.Rows.Count; i++)
                            {
                                
                                //var x = new List<string>;
                                if (Table.TableName != dt.Rows[i]["table"].ToString())
                                {
                                    if (Table.TableName !=null)
                                    {
                                        TableDetails.Add(Table);
                                    }
                                    Table = new TableDetailsModel();
                                    x = new List<Columndetails>();
                                    //y = new List<string>();
                                    //z = new List<string>();
                                    Table.TableName = dt.Rows[i]["table"].ToString();
                                    Table.TableDisplayName = dt.Rows[i]["TableDisplayName"].ToString();
                                    Table.TableKeyColumnName = dt.Rows[i]["pk_column_name"].ToString();

                                }

                                x.Add(new Columndetails { column_name = dt.Rows[i]["column_name"].ToString(), Displaycolumnname = dt.Rows[i]["Displaycolumn_name"].ToString(), DataTypeName = dt.Rows[i]["DataTypeName"].ToString() });
                               
                                Table.Columndetail = x;
                               


                                //Table.ColumName.Add(newNode);

                            }

                            TableDetails.Add(Table);



                            //-------------------------------
                        }
                    }
                }
            }
         






            return View(TableDetails);
        }


        [HttpPost]
        public JsonResult GetQuery(List<Tabledata> Tabledetails, List<Codition> ConditionDetails)
        {
            string sqlquery = "select";
            try
            {
                if (Tabledetails != null)
                {
                    if (Tabledetails.Count > 0)
                    {
                        foreach (var child in Tabledetails)
                        {
                            sqlquery = sqlquery + " " + child.Tablename+"." +child.Columnname + ",";
                        }
                        sqlquery = sqlquery.Remove(sqlquery.Length - 1, 1);
                        sqlquery = sqlquery + " " + "From" + " ";
                        var Tablejoin = "";
                      var x= Tabledetails.Select(m => new { m.Tablename, m.TableKeyColumnName }).Distinct().ToList();
                        for (int i = 0; i < x.Count; i++)
                        {
                            if (i==0)
                            {
                                Tablejoin = Tablejoin + x[i].Tablename;
                            }
                            
                            if (i!=0)
                            {
                                if (x[i].Tablename != x[i-1].Tablename)
                                {
                                    Tablejoin = Tablejoin + " " + "inner join" + " " + x[i].Tablename + " " + "on" + " " + x[i].Tablename + "." + x[i].TableKeyColumnName + "=" + x[i - 1].Tablename + "." + x[i - 1].TableKeyColumnName;
                                }
                            }
                        }
                        sqlquery = sqlquery + " "+Tablejoin;
                        if (ConditionDetails != null)
                        {
                            if (ConditionDetails.Count > 0)
                            {
                                sqlquery = sqlquery + " " + "Where ";
                                foreach (var child in ConditionDetails)
                                {
                                    var conditiontype = "";
                                    conditiontype = Getcondition(child, conditiontype);

                                    sqlquery =sqlquery +" "+ conditiontype + "  " +"and";
                                }
                                if (sqlquery.Substring(sqlquery.Length - 3)== "and")
                                {
                                    sqlquery= sqlquery.Remove(sqlquery.Length - 3);
                                }
                            }

                        }
                    }

                }

            }
            catch (Exception ex)
            {

                throw;
            }
           
             
          return Json(sqlquery);
        }
      

        private static string Getcondition(Codition child, string conditiontype)
        {
            string[] split= { }; 
            switch (child.Condition)
            {
                case "Starts With":
                    conditiontype = child.Tablename + "." + child.Columnname + " " + "LIKE" + " " + "'" + child.Value + "%'";
                    break;
                case "contains":
                    conditiontype = child.Tablename + "." + child.Columnname + " " + "LIKE" + " " + "'%" + child.Value + "%'";
                    break;
                case "is equal to":
                    conditiontype = child.Tablename + "." + child.Columnname + " " + "=" + "'" + child.Value + "'";
                    break;
                case "is in list":

                    if (child.Value != null)
                    {
                        split = child.Value.Split(',');
                        if (split.Count() > 1)
                        {
                            var list = "";
                            foreach (var item in split)
                            {
                                list = list + "'" + item + "',";
                            }
                            list = list.Remove(list.Length - 1, 1);
                            conditiontype = child.Tablename + "." + child.Columnname + " " + "in (" + list + ")";
                        }
                    }
               
                    break;
                case "does not start with":
                    conditiontype = "NOT(" + child.Tablename + "." + child.Columnname + " " + "LIKE '" + child.Value + "%')";
                    break;
                case "does not contain":
                    conditiontype = "NOT(" + child.Tablename + "." + child.Columnname + " " + "LIKE '%" + child.Value + "%')";
                    break;
                case "is not equal to":
                    conditiontype = child.Tablename + "." + child.Columnname + " <> " + child.Value + "'";
                    break;
                case "is not in list":
                    if (child.Value != null)
                    {
                        split = child.Value.Split(',');
                        if (split.Count() > 1)
                        {
                            var list = "";
                            foreach (var item in split)
                            {
                                list = list + "'" + item + "',";
                            }
                            list = list.Remove(list.Length - 1, 1);
                            conditiontype = "NOT(" + child.Tablename + "." + child.Columnname + " in (" + list + "))";
                        }
                    }

                   
                    break;
                case "is null":
                    conditiontype = child.Tablename + "." + child.Columnname + " IS NULL";
                    break;
                case "is not null":
                    conditiontype = child.Tablename + "." + child.Columnname + " IS not NULL";
                    break;
                case "Between":
                    if (child.Value !=null)
                    {                     
                        split = child.Value.Split(',');
                        if (split.Count()>1)
                        {
                            conditiontype = child.Tablename + "." + child.Columnname + " " + "BetWeen" + " '" + split[0] + "' " + "and" + "  '" + split[1] + "'";
                        }
                   
                    }                    
                  
                    break;
                    
            }



            return conditiontype;
        }

        [HttpPost]
        public ActionResult GetData(string query = "")
        {
            try
            {
                DataTable dt = gettable(query, connectionString);

                return PartialView("~/Views/Shared/TableData.cshtml", dt);

            }
            catch (Exception ex)
            {
                //return Json(new { fileName = "error" });
                throw;
            }
           
        }

        private static DataTable gettable(string query,string connectionString)
        {
            DataTable dt = new DataTable();
            //string connection = @"Data Source=LAPTOP-SE2PMU7T;Initial Catalog=DynamicQueryCreator;User ID=sa;Password=1234";
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.CommandType = CommandType.Text;
                    using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                    {
                        using (dt = new DataTable())
                        {
                            sda.Fill(dt);

                        }
                    }
                }
            }

            return dt;
        }
      
        [HttpPost]
        public JsonResult ExportExcel(string query="")
        {
            var fileName = "Report" + ".xlsx";
            string fullPath = Path.Combine(Server.MapPath("~/Scripts/file"), fileName);           
            try
            {              

                DataTable dt = new DataTable();
                dt = gettable(query, connectionString);               

                using (SpreadsheetDocument document = SpreadsheetDocument.Create(fullPath, SpreadsheetDocumentType.Workbook))
                {
                    WorkbookPart workbookPart = document.AddWorkbookPart();
                    workbookPart.Workbook = new Workbook();

                    WorksheetPart worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
                    var sheetData = new SheetData();
                    worksheetPart.Worksheet = new Worksheet(sheetData);

                    Sheets sheets = workbookPart.Workbook.AppendChild(new Sheets());
                    Sheet sheet = new Sheet() { Id = workbookPart.GetIdOfPart(worksheetPart), SheetId = 1, Name = "Sheet1" };

                    sheets.Append(sheet);

                    Row headerRow = new Row();

                    List<String> columns = new List<string>();
                    foreach (System.Data.DataColumn column in dt.Columns)
                    {
                        columns.Add(column.ColumnName);

                        Cell cell = new Cell();
                        cell.DataType = CellValues.String;
                        cell.CellValue = new CellValue(column.ColumnName);
                        headerRow.AppendChild(cell);
                    }

                    sheetData.AppendChild(headerRow);

                    foreach (DataRow dsrow in dt.Rows)
                    {
                        Row newRow = new Row();
                        foreach (String col in columns)
                        {
                            Cell cell = new Cell();
                            cell.DataType = CellValues.String;
                            cell.CellValue = new CellValue(dsrow[col].ToString());
                            newRow.AppendChild(cell);
                        }

                        sheetData.AppendChild(newRow);
                    }

                    workbookPart.Workbook.Save();
                }

            }
            catch (Exception ex)
            {
                Log.Error(ex);
                Log.Fatal(ex);
                throw;
                throw;
            }
            return Json(new { fileName = fileName });
        }
       
        [HttpGet]
        public ActionResult Download(string fileName)
        {
            string fullPath = Path.Combine(Server.MapPath("~/Scripts/file"), fileName);
            byte[] fileByteArray = System.IO.File.ReadAllBytes(fullPath);
            System.IO.File.Delete(fullPath);
            return File(fileByteArray, "application/vnd.ms-excel", fileName);
        }
    }
}