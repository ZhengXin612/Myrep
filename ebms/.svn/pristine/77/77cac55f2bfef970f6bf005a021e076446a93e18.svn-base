
using EBMS.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using System.Web.Mvc;
using System.Web.Script.Serialization;


namespace EBMS.App_Code
{
    public class Com
    {
        //接收JSON 反序列化
        public static List<T> Deserialize<T>(string text)
        {
            try
            {
                JavaScriptSerializer js = new JavaScriptSerializer();
                List<T> list = (List<T>)js.Deserialize(text, typeof(List<T>));
                return list;

            }
            catch (Exception)
            {

                return null;
            }
        }

        /// <summary>
        /// 获得补发货名字
        /// </summary>
        /// <returns></returns>
        public static string GetReissueName(string code, string reson)
        {
            EBMSEntities db = new EBMSEntities();
            if (reson.Equals("呼吸机专用"))
                return "阿奎";
            else
            {
                T_ShopFromGY shop = db.T_ShopFromGY.SingleOrDefault(s => s.code.Equals(code));
               
                if (shop.type_name.Equals("天猫"))
                    return "紫荆";
                //else if (shop.type_name.Equals("淘宝"))
                //    return "欧阳";
                else if (shop.type_name.Equals("京东"))
                    return "丹丹";
                else if (shop.type_name.Equals("Ebay"))
                    return "叮当";
                else if (shop.type_name.Equals("贝贝网"))
                    return "辉驮";
                else
                    return "成风";
            }
        }


        /// <summary>
        /// 根据用户ID获得花名
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static string GetNickName(int id)
        {
            EBMSEntities db = new EBMSEntities();
            string Nickname = "";
            T_User user = db.T_User.Find(id);
            if (user != null)
                Nickname = user.Nickname;
            return Nickname;

        }

        /// <summary>
        /// 支付方式无默认值
        /// </summary>
        /// <returns></returns>
        public static List<SelectListItem> PaymentType()
        {
            EBMSEntities db = new EBMSEntities();
            var list = db.T_PaymentType.AsQueryable();
            var selectList = new SelectList(list, "Code", "Code");
            List<SelectListItem> selecli = new List<SelectListItem>();
            selecli.Add(new SelectListItem { Text = "==请选择==", Value = "" });
            selecli.AddRange(selectList);
            return selecli;
        }
        /// <summary>
        /// 换货原因无默认值
        /// </summary>
        /// <returns></returns>
        public static List<SelectListItem> ExchangeReson()
        {
            EBMSEntities db = new EBMSEntities();
            var list = db.T_ExchangeReson.AsQueryable();
            var selectList = new SelectList(list, "Reson", "Reson");
            List<SelectListItem> selecli = new List<SelectListItem>();
            selecli.Add(new SelectListItem { Text = "==请选择==", Value = "" });
            selecli.AddRange(selectList);
            return selecli;
        }
        ///// <summary>
        ///// 绑定报销支出账号
        ///// </summary>
        ///// <returns></returns>
        //public static List<SelectListItem> ExpenseAcount()
        //{
        //    EBMSEntities db = new EBMSEntities();
        //    var list = db.T_ExpenseAcount.AsQueryable();
        //    var selectList = new SelectList(list, "type", "Number");
        //    List<SelectListItem> selecli = new List<SelectListItem>();
        //    selecli.Add(new SelectListItem { Text = "==请选择==", Value = "" });
        //    selecli.AddRange(selectList);
        //    return selecli;
        //}

        /// <summary>
        /// 绑定报销支出公司
        /// </summary>
        /// <returns></returns>
        public static List<SelectListItem> ExpenseCompany()
        {
            EBMSEntities db = new EBMSEntities();
            var list = db.T_ExpenseAcount.Where(s => s.ComPany != null).AsQueryable();
            var selectList = new SelectList(list, "type", "ComPany");
            List<SelectListItem> selecli = new List<SelectListItem>();
            selecli.Add(new SelectListItem { Text = "==请选择==", Value = "" });
            selecli.Add(new SelectListItem { Text = "==请输入==", Value = "请输入" });
            selecli.AddRange(selectList);
            return selecli;
        }
        /// <summary>
        /// 绑定报销状态
        /// </summary>
        /// <returns></returns>
        public static List<SelectListItem> ExpenseStatus()
        {
            EBMSEntities db = new EBMSEntities();
            var list = db.T_ExpenseState.AsQueryable();
            var selectList = new SelectList(list, "name", "name");
            List<SelectListItem> selecli = new List<SelectListItem>();
            selecli.Add(new SelectListItem { Text = "==请选择==", Value = "" });
            selecli.AddRange(selectList);
            return selecli;
        }

        /// <summary>
        /// 线下绑定报销支出帐号
        /// </summary>
        /// <returns></returns>
        public static List<SelectListItem> LineExpenseAcount()
        {
            EBMSEntities db = new EBMSEntities();
            var list = db.T_LineExpenseAcount.AsQueryable();
            var selectList = new SelectList(list, "ComPany", "Number");
            List<SelectListItem> selecli = new List<SelectListItem>();
            selecli.Add(new SelectListItem { Text = "==请选择==", Value = "" });
            selecli.AddRange(selectList);
            return selecli;
        }
        /// <summary>
        /// 换货原因有默认值
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static List<SelectListItem> ExchangeReson(object obj)
        {
            EBMSEntities db = new EBMSEntities();
            var list = db.T_ExchangeReson.AsQueryable();
            var selectList = new SelectList(list, "Reson", "Reson", obj);
            List<SelectListItem> selecli = new List<SelectListItem>();
            selecli.Add(new SelectListItem { Text = "==请选择==", Value = "" });
            selecli.AddRange(selectList);
            return selecli;
        }

        /// <summary>
        /// 补发原因无默认值
        /// </summary>
        /// <returns></returns>
        public static List<SelectListItem> ReissueReson()
        {
            EBMSEntities db = new EBMSEntities();
            var list = db.T_ReissueReson.AsQueryable();
            var selectList = new SelectList(list, "Reson", "Reson");
            List<SelectListItem> selecli = new List<SelectListItem>();
            selecli.Add(new SelectListItem { Text = "==请选择==", Value = "" });
            selecli.AddRange(selectList);
            return selecli;
        }

        /// <summary>
        /// 补发原因有默认值
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static List<SelectListItem> ReissueReson(object obj)
        {
            EBMSEntities db = new EBMSEntities();
            var list = db.T_ReissueReson.AsQueryable();
            var selectList = new SelectList(list, "Reson", "Reson", obj);
            List<SelectListItem> selecli = new List<SelectListItem>();
            selecli.Add(new SelectListItem { Text = "==请选择==", Value = "" });
            selecli.AddRange(selectList);
            return selecli;
        }
        /// <summary>
        /// 供应商
        /// </summary>
        /// <returns></returns>
        public static List<SelectListItem> SuppliersReson()
        {
            EBMSEntities db = new EBMSEntities();
            var list = db.T_Suppliers.AsQueryable();
            var selectList = new SelectList(list, "SuppliersName", "SuppliersName");
            List<SelectListItem> selecli = new List<SelectListItem>();
            selecli.Add(new SelectListItem { Text = "==请选择==", Value = "" });
            selecli.AddRange(selectList);
            return selecli;
        }
        /// <summary>
        /// 供应商管易
        /// </summary>
        /// <returns></returns>
        public static List<SelectListItem> SuppliersResonGy()
        {
            EBMSEntities db = new EBMSEntities();
            var list = db.T_SupplierGY.AsQueryable();
            var selectList = new SelectList(list, "code", "name");
            List<SelectListItem> selecli = new List<SelectListItem>();
            selecli.Add(new SelectListItem { Text = "==请选择==", Value = "" });
            selecli.AddRange(selectList);
            return selecli;
        }
        /// <summary>
        /// 拦截原因无默认值
        /// </summary>
        /// <returns></returns>
        public static List<SelectListItem> InterceptReson()
        {
            EBMSEntities db = new EBMSEntities();
            var list = db.T_InterceptReson.AsQueryable();
            var selectList = new SelectList(list, "Reson", "Reson");
            List<SelectListItem> selecli = new List<SelectListItem>();
            selecli.Add(new SelectListItem { Text = "==请选择==", Value = "" });
            selecli.AddRange(selectList);
            return selecli;
        }

        /// <summary>
        /// 拦截原因有默认值
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static List<SelectListItem> InterceptReson(object obj)
        {
            EBMSEntities db = new EBMSEntities();
            var list = db.T_InterceptReson.AsQueryable();
            var selectList = new SelectList(list, "Reson", "Reson", obj);
            List<SelectListItem> selecli = new List<SelectListItem>();
            selecli.Add(new SelectListItem { Text = "==请选择==", Value = "" });
            selecli.AddRange(selectList);
            return selecli;
        }

        /// <summary>
        /// 资金调拨调入单位
        /// </summary>
        /// <returns></returns>
        public static List<SelectListItem> Companyin()
        {
            EBMSEntities db = new EBMSEntities();
            var list = db.T_FundAcount.Where(s => s.Name != null).AsQueryable();
            var selectList = new SelectList(list, "ID", "Name");
            List<SelectListItem> selecli = new List<SelectListItem>();
            selecli.Add(new SelectListItem { Text = "==请选择==", Value = "" });
            selecli.AddRange(selectList);
            return selecli;
        }

        /// <summary>
        /// 资金调拨收款账号
        /// </summary>
        /// <returns></returns>
        public static List<SelectListItem> AcountNumber()
        {
            EBMSEntities db = new EBMSEntities();
            var list = db.T_FundAcount.Where(s => s.Number != null).AsQueryable();
            var selectList = new SelectList(list, "ID", "Number");
            List<SelectListItem> selecli = new List<SelectListItem>();
            selecli.Add(new SelectListItem { Text = "==请选择==", Value = "" });
            selecli.AddRange(selectList);
            return selecli;
        }

        /// <summary>
        /// 资金调拨调出单位
        /// </summary>
        /// <returns></returns>
        public static List<SelectListItem> CompanyOut()
        {
            EBMSEntities db = new EBMSEntities();
            var list = db.T_FundOutAllot.AsQueryable();
            var selectList = new SelectList(list, "ID", "Name");
            List<SelectListItem> selecli = new List<SelectListItem>();
            selecli.Add(new SelectListItem { Text = "==请选择==", Value = "" });
            selecli.AddRange(selectList);
            return selecli;
        }

        /// <summary>
        /// 资金调拨付款账号
        /// </summary>
        /// <returns></returns>
        public static List<SelectListItem> PayNumber()
        {
            EBMSEntities db = new EBMSEntities();
            var list = db.T_FundOutAllot.AsQueryable();
            var selectList = new SelectList(list, "ID", "Number");
            List<SelectListItem> selecli = new List<SelectListItem>();
            selecli.Add(new SelectListItem { Text = "==请选择==", Value = "" });
            selecli.AddRange(selectList);
            return selecli;
        }

        /// <summary>
        /// 借贷方向
        /// </summary>
        /// <returns></returns>
        public static List<SelectListItem> diRection()
        {
            var list = new List<SelectListItem>  {
            new SelectListItem { Text = "==请选择==", Value = "" },
               new SelectListItem { Text = "借", Value = "0" },
               new SelectListItem { Text = "贷", Value = "1" }
            };
            return list;
        }

        /// <summary>
        /// 凭证科目
        /// </summary>
        /// <returns></returns>
        public static List<SelectListItem> subject()
        {
            EBMSEntities db = new EBMSEntities();
            var list = db.T_AccountantProject.Where(s => s.ID != 1).AsQueryable();
            var selectList = new SelectList(list, "Name", "Name");
            List<SelectListItem> selecli = new List<SelectListItem>();
            selecli.Add(new SelectListItem { Text = "==请选择==", Value = "" });
            selecli.AddRange(selectList);
            return selecli;
        }

        /// <summary>
        /// 凭证部门
        /// </summary>
        /// <returns></returns>
        public static List<SelectListItem> depart()
        {
            EBMSEntities db = new EBMSEntities();
            var list = db.T_PZ_DePartMent.AsQueryable();
            var selectList = new SelectList(list, "Name", "Name");
            List<SelectListItem> selecli = new List<SelectListItem>();
            selecli.Add(new SelectListItem { Text = "==请选择==", Value = "" });
            selecli.AddRange(selectList);
            return selecli;
        }

        /// <summary>
        /// 绑定店铺无默认值
        /// </summary>
        /// <returns></returns>
        public static List<SelectListItem> Shop()
        {

            EBMSEntities db = new EBMSEntities();
            var list = db.T_ShopFromGY.AsQueryable();
            var selectList = new SelectList(list, "name", "name");
            List<SelectListItem> selecli = new List<SelectListItem>();
            selecli.Add(new SelectListItem { Text = "==请选择==", Value = "" });
            selecli.AddRange(selectList);
            return selecli;
        }
        class CashBackFrom
        {
      
            public string Name { get; set; }
        }
        class RetreatFrom
        {

            public string PaymentAccounts { get; set; }
        }
        /// <summary>
        /// 绑定支付帐号无默认值
        /// </summary>
        /// <returns></returns>
        public static List<SelectListItem> BackFrom()
        {

            EBMSEntities db = new EBMSEntities();
           List<CashBackFrom> list = db.Database.SqlQuery<CashBackFrom>("select Name from T_CashBackFrom group by Name").ToList();
            var selectList = new SelectList(list, "Name", "Name");
            List<SelectListItem> selecli = new List<SelectListItem>();
            selecli.Add(new SelectListItem { Text = "==请选择==", Value = "" });
            selecli.AddRange(selectList);
            return selecli;
        }
        //退款
        public static List<SelectListItem> RetreatBackFrom()
        {

            EBMSEntities db = new EBMSEntities();
            List<RetreatFrom> list = db.Database.SqlQuery<RetreatFrom>("select PaymentAccounts from  T_RetreatPayment  group by PaymentAccounts").ToList();
            var selectList = new SelectList(list, "PaymentAccounts", "PaymentAccounts");
            List<SelectListItem> selecli = new List<SelectListItem>();
            selecli.Add(new SelectListItem { Text = "==请选择==", Value = "" });
            selecli.AddRange(selectList);
            return selecli;
        }
        /// <summary>
        /// 绑定店铺有默认值
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static List<SelectListItem> Shop(object obj)
        {
            EBMSEntities db = new EBMSEntities();
            var list = db.T_ShopFromGY.AsQueryable();
            var selectList = new SelectList(list, "name", "name", obj);
            List<SelectListItem> selecli = new List<SelectListItem>();
            selecli.Add(new SelectListItem { Text = "==请选择==", Value = "" });
            selecli.AddRange(selectList);
            return selecli;
        }
        /// <summary>
        /// 绑定下线部门无默认值
        /// </summary>
        /// <returns></returns>
        public static List<SelectListItem> LinDepartMent()
        {
            EBMSEntities db = new EBMSEntities();
            var list = db.T_LineDepatment.Where(s => s.parentId != -1).AsQueryable();
            var selectList = new SelectList(list, "Name", "Name");
            List<SelectListItem> selecli = new List<SelectListItem>();
            selecli.Add(new SelectListItem { Text = "==请选择==", Value = "" });
            selecli.AddRange(selectList);
            return selecli;
        }
        /// <summary>
        /// 绑定部门无默认值
        /// </summary>
        /// <returns></returns>
        public static List<SelectListItem> DepartMent()
        {
            EBMSEntities db = new EBMSEntities();
            var list = db.T_Department.Where(s => s.parentId != -1).AsQueryable();
            var selectList = new SelectList(list, "ID", "Name");
            List<SelectListItem> selecli = new List<SelectListItem>();
            selecli.Add(new SelectListItem { Text = "==请选择==", Value = "" });
            selecli.AddRange(selectList);
            return selecli;
        }
        /// <summary>
        /// 绑定部门有默认值
        /// </summary>
        /// <returns></returns>
        public static List<SelectListItem> DepartMent(object obj)
        {
            EBMSEntities db = new EBMSEntities();
            var list = db.T_Department.Where(s => s.parentId != -1).AsQueryable();
            var selectList = new SelectList(list, "ID", "Name", obj);
            List<SelectListItem> selecli = new List<SelectListItem>();
            selecli.Add(new SelectListItem { Text = "==请选择==", Value = "" });
            selecli.AddRange(selectList);
            return selecli;
        }
        /// <summary>
        /// 支付方式无默认
        /// </summary>
        /// <returns></returns>
        public static List<SelectListItem> BlendingType()
        {
            EBMSEntities db = new EBMSEntities();
            var list = db.T_ReturnToStorageBlendingType.AsQueryable();
            var selectList = new SelectList(list, "TypeCode", "TypeName");
            List<SelectListItem> selecli = new List<SelectListItem>();
            selecli.Add(new SelectListItem { Text = "==请选择==", Value = "" });
            selecli.AddRange(selectList);
            return selecli;
        }
        /// <summary>
        /// 绑定仓库无默认值 code-name
        /// </summary>
        /// <returns></returns>
        public static List<SelectListItem> Warehouses()
        {
            EBMSEntities db = new EBMSEntities();
            var list = db.T_Warehouses.AsQueryable();
            var selectList = new SelectList(list, "code", "name");
            List<SelectListItem> selecli = new List<SelectListItem>();
            selecli.Add(new SelectListItem { Text = "==请选择==", Value = "" });
            selecli.AddRange(selectList);
            return selecli;
        }
        /// <summary>
        /// 绑定仓库无默认值 code-name
        /// </summary>
        /// <returns></returns>
        public static List<SelectListItem> Warehousesz()
        {
            EBMSEntities db = new EBMSEntities();
            var list = db.T_Warehouses.AsQueryable();
            var selectList = new SelectList(list, "code", "name");
            List<SelectListItem> selecli = new List<SelectListItem>();
          
            selecli.AddRange(selectList);
            return selecli;
        }
        /// <summary>
        /// 绑定仓库无默认值根据仓库名称获取仓库 name-name
        /// </summary>
        /// <returns></returns>
        public static List<SelectListItem> Warehouse()
        {
            EBMSEntities db = new EBMSEntities();
            var list = db.T_Warehouses.AsQueryable();
            var selectList = new SelectList(list, "name", "name");
            List<SelectListItem> selecli = new List<SelectListItem>();
            selecli.Add(new SelectListItem { Text = "==请选择==", Value = "" });
            selecli.AddRange(selectList);
            return selecli;
        }
        /// <summary>
        /// 绑定仓库有默认值
        /// </summary>
        /// <returns></returns>
        public static List<SelectListItem> Warehouses(object obj)
        {
            EBMSEntities db = new EBMSEntities();
            var list = db.T_Warehouses.AsQueryable();
            var selectList = new SelectList(list, "code", "name", obj);
            List<SelectListItem> selecli = new List<SelectListItem>();
            selecli.Add(new SelectListItem { Text = "==请选择==", Value = "" });
            selecli.AddRange(selectList);
            return selecli;
        }

        /// <summary>
        /// 绑定快递公司
        /// </summary>
        /// <returns></returns>
        public static List<SelectListItem> ExpressName()
        {
            EBMSEntities db = new EBMSEntities();
            var list = db.T_Express.Where(a=>a.Isdelete=="0").AsQueryable();
            var selectList = new SelectList(list, "Code", "Name");
            List<SelectListItem> selecli = new List<SelectListItem>();
            selecli.Add(new SelectListItem { Text = "==请选择==", Value = "" });
            selecli.AddRange(selectList);
            return selecli;
        }
        /// <summary>
        /// 绑定WDT快递公司
        /// </summary>
        /// <returns></returns>
        public static List<SelectListItem> WDTExpressName()
        {
            EBMSEntities db = new EBMSEntities();
            var list = db.T_WDTExpress.Where(a => a.Isdelete == "0").AsQueryable();
            var selectList = new SelectList(list, "Code", "Name");
            List<SelectListItem> selecli = new List<SelectListItem>();
            selecli.Add(new SelectListItem { Text = "==请选择==", Value = "" });
            selecli.AddRange(selectList);
            return selecli;
        }
        /// <summary>
        /// 绑定快递公司有默认值
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static List<SelectListItem> ExpressName(object obj)
        {
            EBMSEntities db = new EBMSEntities();
            var list = db.T_Express.AsQueryable();
            var selectList = new SelectList(list, "Code", "Name", obj);
            List<SelectListItem> selecli = new List<SelectListItem>();
            //selecli.Add(new SelectListItem { Text = "==请选择==", Value = "" });
            selecli.AddRange(selectList);
            return selecli;
        }
        public static List<SelectListItem> WDTExpressName(object obj)
        {
            EBMSEntities db = new EBMSEntities();
            var list = db.T_WDTExpress.AsQueryable();
            var selectList = new SelectList(list, "Code", "Name", obj);
            List<SelectListItem> selecli = new List<SelectListItem>();
            selecli.Add(new SelectListItem { Text = "==请选择==", Value = "" });
            selecli.AddRange(selectList);
            return selecli;
        }
        /// <summary>
        /// 绑定订单类型无默认值
        /// </summary>
        /// <returns></returns>
        public static List<SelectListItem> OrderType()
        {
            EBMSEntities db = new EBMSEntities();
            var list = db.T_OrderType.AsQueryable();
            var selectList = new SelectList(list, "Code", "Name");
            List<SelectListItem> selecli = new List<SelectListItem>();
            selecli.Add(new SelectListItem { Text = "==请选择==", Value = "" });
            selecli.AddRange(selectList);
            return selecli;
        }

        /// <summary>
        /// 绑定订单类型有默认值
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static List<SelectListItem> OrderType(object obj)
        {
            EBMSEntities db = new EBMSEntities();
            var list = db.T_OrderType.AsQueryable();
            var selectList = new SelectList(list, "Code", "Name", obj);
            List<SelectListItem> selecli = new List<SelectListItem>();
            selecli.Add(new SelectListItem { Text = "==请选择==", Value = "" });
            selecli.AddRange(selectList);
            return selecli;
        }

        /// <summary>
        /// 根据店铺代码获得店铺名称
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static string GetShopName(string code)
        {
            EBMSEntities db = new EBMSEntities();
            string ShopName = "";
            T_ShopFromGY shop = db.T_ShopFromGY.SingleOrDefault(a => a.code == code);
            if (shop != null)
            {
                ShopName = shop.name;
            }
            return ShopName;

        }
        /// <summary>
        /// 根据快递代码获得快递名称
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static string GetExpressName(string code)
        {
            EBMSEntities db = new EBMSEntities();
            string ExpressName = "";
            T_Express express = db.T_Express.SingleOrDefault(a => a.Code == code);
            if (express != null)
            {
                ExpressName = express.Name;
            }
            return ExpressName;

        }
        /// <summary>
        /// 绑定快递公司无默认值
        /// </summary>
        /// <returns></returns>
        public static List<SelectListItem> GetExpressName()
        {

            EBMSEntities db = new EBMSEntities();
            var list = db.T_Express.AsQueryable();
            var selectList = new SelectList(list, "name", "name");
            List<SelectListItem> selecli = new List<SelectListItem>();
            selecli.Add(new SelectListItem { Text = "==请选择==", Value = "" });
            selecli.AddRange(selectList);
            return selecli;
        }
        /// <summary>
        /// 根据仓库代码获得仓库名称
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static string GetWarehouseName(string code)
        {
            EBMSEntities db = new EBMSEntities();
            string WarehouseName = "";
            T_Warehouses Warehouse = db.T_Warehouses.SingleOrDefault(a => a.code == code);
            if (Warehouse != null)
            {
                WarehouseName = Warehouse.name;
            }
            return WarehouseName;

        }
        /// <summary>
        /// 根据部门ID获取部门名称
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static string GetDepartmentName(int ID)
        {
            EBMSEntities db = new EBMSEntities();
            string DepartmentName = "";
            T_Department Department = db.T_Department.Find(ID);
            if (Department != null)
            {
                DepartmentName = Department.Name;
            }
            return DepartmentName;

        }

        /// <summary>
        /// 是否选择  0否 1是
        /// </summary>
        public static List<SelectListItem> BoolList = new List<SelectListItem> 
        {
             new SelectListItem { Text = "是", Value = "1" },
             new SelectListItem { Text = "否", Value = "0" },
        };
        /// <summary>
        /// 花名下拉列表 text为花名,value 为花名
        /// </summary>
        public static List<SelectListItem> UserList()
        {
            EBMSEntities db = new EBMSEntities();
            IEnumerable list = db.T_User.AsEnumerable();
            SelectList selectList = new SelectList(list, "NickName", "NickName");
            List<SelectListItem> ListUser = new List<SelectListItem>();
            ListUser.AddRange(selectList);
            return ListUser;
        }
		public static List<SelectListItem> DirectoryList(string KeyType)
		{
			EBMSEntities db = new EBMSEntities();
			IEnumerable list = db.T_Directory.Where(a=>a.KeyType==KeyType).AsEnumerable();
			SelectList selectList = new SelectList(list, "KeyValue", "KeyName");
			List<SelectListItem> ListItem = new List<SelectListItem>();
			ListItem.Add(new SelectListItem { Text = "==请选择==", Value = "" });
			ListItem.AddRange(selectList);
			return ListItem;
		}

		public static void ModularUncheckCount(string PendingAuditName,string ModularName, int Qty=0)
		{
			try
			{

				EBMSEntities db = new EBMSEntities();
				T_ModularNotaudited model = db.T_ModularNotaudited.FirstOrDefault(a => a.ModularName == ModularName && a.PendingAuditName == PendingAuditName);
				model.ToupdateDate = DateTime.Now;
				model.NotauditedNumber = model.NotauditedNumber + Qty;
				db.Entry<T_ModularNotaudited>(model).State = System.Data.Entity.EntityState.Modified;
				db.SaveChanges();
			}
			catch (Exception e)
			{

			}
		

		}

		public static void ModularRejectCount(string PendingAuditName, string ModularName, int Qty = 0)
		{
			try {
				EBMSEntities db = new EBMSEntities();
				T_ModularNotaudited model = db.T_ModularNotaudited.FirstOrDefault(a => a.ModularName == ModularName && a.PendingAuditName == PendingAuditName);
				model.ToupdateDate = DateTime.Now;
				model.RejectNumber = model.RejectNumber + Qty;
				db.Entry<T_ModularNotaudited>(model).State = System.Data.Entity.EntityState.Modified;
				db.SaveChanges();
			}
			catch (Exception e)
			{

			}
			

		}
	}
}