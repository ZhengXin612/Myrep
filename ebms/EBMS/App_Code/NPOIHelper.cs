using NPOI.HSSF.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;

namespace IPMS
{
	public class NPOIHelper
	{
		public static XSSFWorkbook BuildSwitchData<T>(string SheetName, List<T> list, Dictionary<string, string> FiedNames)
		{
			XSSFWorkbook wb = new XSSFWorkbook();
			XSSFSheet sheet = (XSSFSheet)wb.CreateSheet(SheetName); //创建工作表
			sheet.CreateFreezePane(0, 1); //冻结列头行
			XSSFRow row_Title = (XSSFRow)sheet.CreateRow(0); //创建列头行
			
			#region 生成列头
			int ii = 0;
			foreach (string key in FiedNames.Keys)
			{
				XSSFCell cell_Title = (XSSFCell)row_Title.CreateCell(ii); //创建单元格
				//cell_Title.CellStyle = cs_Title; //将样式绑定到单元格
				cell_Title.SetCellValue(key);
				//sheet.SetColumnWidth(ii, 25 * 256);//设置列宽
				ii++;
			}

			#endregion
			//获取 实体类 类型对象
			Type t = typeof(T); // model.GetType();
								//获取 实体类 所有的 公有属性
			List<PropertyInfo> proInfos = t.GetProperties(BindingFlags.Instance | BindingFlags.Public).ToList();
			//创建 实体属性 字典集合
			Dictionary<string, PropertyInfo> dictPros = new Dictionary<string, PropertyInfo>();
			//将 实体属性 中要修改的属性名 添加到 字典集合中 键：属性名  值：属性对象
			proInfos.ForEach(p =>
			{
				if (FiedNames.Values.Contains(p.Name))
				{
					dictPros.Add(p.Name, p);
				}
			});

			for (int i = 0; i < list.Count; i++)
			{

				XSSFRow row_Content = (XSSFRow)sheet.CreateRow(i + 1); //创建行
				row_Content.HeightInPoints = 20;
				int jj = 0;
				foreach (string proName in FiedNames.Values)
				{
					if (dictPros.ContainsKey(proName))
					{
						XSSFCell cell_Conent = (XSSFCell)row_Content.CreateCell(jj); //创建单元格
						

						//如果存在，则取出要属性对象
						PropertyInfo proInfo = dictPros[proName];
						//获取对应属性的值
						object value = proInfo.GetValue(list[i], null); //object newValue = model.uName;
						string cell_value = value == null ? "" : value.ToString();
						cell_Conent.SetCellValue(cell_value);
						jj++;
					}
				}
			}
			return wb;

		}
	}
}