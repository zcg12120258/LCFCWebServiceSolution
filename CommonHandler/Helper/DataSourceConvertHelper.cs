using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Data;
using System.Reflection;
using CommonHandler.Model;
using System.ComponentModel;

namespace CommonHandler.Helper
{
    public class DataSourceConvertHelper
    {
        public static ArrayList GetPropertyInfo(Type objType)
        {

            // Use the cache because the reflection used later is expensive
            ArrayList objProperties = null;

            if (objProperties == null)
            {
                objProperties = new ArrayList();
                foreach (PropertyInfo objProperty in objType.GetProperties())
                {
                    objProperties.Add(objProperty);
                }
            }

            return objProperties;

        }
        private static int[] GetOrdinals(ArrayList objProperties, IDataReader dr)
        {

            int[] arrOrdinals = new int[objProperties.Count + 1];
            int intProperty;

            if ((dr != null))
            {
                for (intProperty = 0; intProperty <= objProperties.Count - 1; intProperty++)
                {
                    arrOrdinals[intProperty] = -1;
                    try
                    {
                        arrOrdinals[intProperty] = dr.GetOrdinal(((PropertyInfo)objProperties[intProperty]).Name);
                    }
                    catch
                    {
                        // property does not exist in datareader
                    }
                }
            }

            return arrOrdinals;

        }

        /// <summary>
        /// Return object based on parameters.
        /// </summary>
        /// <param name="objType">Type od datatype.</param>
        /// <param name="dr">The DataReader</param>
        /// <param name="objProperties">ArrayList</param>
        /// <param name="arrOrdinals">Array of integer.</param>
        /// <returns>Object</returns>
        private static object CreateObject(Type objType, IDataReader dr, ArrayList objProperties, int[] arrOrdinals)
        {

            PropertyInfo objPropertyInfo;
            object objValue;
            Type objPropertyType = null;
            int intProperty;

            //objPropertyInfo.ToString() == BuiltyNumber
            object objObject = Activator.CreateInstance(objType);

            // fill object with values from datareader
            for (intProperty = 0; intProperty <= objProperties.Count - 1; intProperty++)
            {
                objPropertyInfo = (PropertyInfo)objProperties[intProperty];
                if (objPropertyInfo.CanWrite)
                {
                    objValue = Null.SetNull(objPropertyInfo);
                    if (arrOrdinals[intProperty] != -1)
                    {
                        if (System.Convert.IsDBNull(dr.GetValue(arrOrdinals[intProperty])))
                        {
                            // translate Null value
                            objPropertyInfo.SetValue(objObject, objValue, null);
                        }
                        else
                        {
                            try
                            {
                                // try implicit conversion first
                                objPropertyInfo.SetValue(objObject, dr.GetValue(arrOrdinals[intProperty]), null);
                            }
                            catch
                            {
                                // business object info class member data type does not match datareader member data type
                                try
                                {
                                    objPropertyType = objPropertyInfo.PropertyType;
                                    //need to handle enumeration conversions differently than other base types
                                    if (objPropertyType.BaseType.Equals(typeof(System.Enum)))
                                    {
                                        // check if value is numeric and if not convert to integer ( supports databases like Oracle )
                                        int test = 0;
                                        if (test.GetType() == dr.GetValue(arrOrdinals[intProperty]).GetType())
                                        {
                                            ((PropertyInfo)objProperties[intProperty]).SetValue(objObject, System.Enum.ToObject(objPropertyType, Convert.ToInt32(dr.GetValue(arrOrdinals[intProperty]))), null);
                                        }
                                        else
                                        {
                                            ((PropertyInfo)objProperties[intProperty]).SetValue(objObject, System.Enum.ToObject(objPropertyType, dr.GetValue(arrOrdinals[intProperty])), null);
                                        }
                                    }
                                    else if (objPropertyType.FullName.Equals("System.Guid"))
                                    {
                                        // guid is not a datatype common across all databases ( ie. Oracle )
                                        objPropertyInfo.SetValue(objObject, Convert.ChangeType(new Guid(dr.GetValue(arrOrdinals[intProperty]).ToString()), objPropertyType), null);
                                    }
                                    else
                                    {
                                        // try explicit conversion
                                        objPropertyInfo.SetValue(objObject, Convert.ChangeType(dr.GetValue(arrOrdinals[intProperty]), objPropertyType), null);
                                    }
                                }
                                catch
                                {
                                    objPropertyInfo.SetValue(objObject, Convert.ChangeType(dr.GetValue(arrOrdinals[intProperty]), objPropertyType), null);
                                }
                            }
                        }
                    }
                    else
                    {
                        // property does not exist in datareader
                    }
                }
            }

            return objObject;

        }

        public static ArrayList FillCollection(IDataReader dr, Type objType)
        {

            ArrayList objFillCollection = new ArrayList();
            object objFillObject;

            // get properties for type
            ArrayList objProperties = GetPropertyInfo(objType);

            // get ordinal positions in datareader
            int[] arrOrdinals = GetOrdinals(objProperties, dr);

            // iterate datareader
            while (dr.Read())
            {
                // fill business object
                objFillObject = CreateObject(objType, dr, objProperties, arrOrdinals);
                // add to collection
                objFillCollection.Add(objFillObject);
            }

            // close datareader
            if ((dr != null))
            {
                dr.Close();
            }

            return objFillCollection;

        }

        private static T CreateObject<T>(IDataReader dr)
        {
            T objObject = Activator.CreateInstance<T>();

            // fill object with values from datareader

            List<string> field = new List<string>(dr.FieldCount);
            for (int i = 0; i < dr.FieldCount; i++)
            {
                field.Add(dr.GetName(i).ToLower());
            }

            foreach (PropertyInfo objPropertyInfo in objObject.GetType().GetProperties())
            {

                if (objPropertyInfo.CanWrite)
                {
                    object objValue = Null.SetNull(objPropertyInfo);

                    if (field.Contains(objPropertyInfo.Name.ToLower()))
                    {
                        if (System.Convert.IsDBNull(dr[objPropertyInfo.Name]))
                        {
                            // translate Null value
                            objPropertyInfo.SetValue(objObject, Convert.ChangeType(objValue, objPropertyInfo.PropertyType), null);
                        }
                        else
                        {
                            objPropertyInfo.SetValue(objObject, Convert.ChangeType(dr[objPropertyInfo.Name], objPropertyInfo.PropertyType), null);

                        }
                    }
                    else
                    {
                        objPropertyInfo.SetValue(objObject, Convert.ChangeType(objValue, objPropertyInfo.PropertyType), null);
                    }

                }
            }
            return objObject;
        }

        public static List<T> FillCollection<T>(IDataReader dr)
        {

            List<T> objFillCollection = new List<T>();

            // iterate datareader
            while (dr.Read())
            {
                // add to collection
                objFillCollection.Add(CreateObject<T>(dr));
            }

            // close datareader
            if ((dr != null))
            {
                dr.Close();
            }

            return objFillCollection;
        }

        #region List转换DataTable
        /// <summary>
        /// 将泛类型集合List类转换成DataTable
        /// </summary>
        /// <param name="list">泛类型集合</param>
        /// <returns></returns>
        public static DataTable ListToDataTable<T>(List<T> objFillCollection)
        {
            //检查实体集合不能为空
            if (objFillCollection == null || objFillCollection.Count < 1)
            {
                throw new Exception("需转换的集合为空");
            }

            //取出T所有Propertie
            Type objObjectType = typeof(T);

            PropertyInfo[] objProperties = objObjectType.GetProperties();

            //生成DataTable的结构
            DataTable dt = new DataTable("Table");
            for (int i = 0; i < objProperties.Length; i++)
            {
                var descAttribute = (DescriptionAttribute[])objProperties[i].GetCustomAttributes(typeof(DescriptionAttribute), false);

                if (descAttribute != null && descAttribute.Length > 0)
                {
                    string descriptionName = descAttribute[0].Description;
                    dt.Columns.Add(descriptionName, objProperties[i].PropertyType);
                }
                else
                {
                    dt.Columns.Add(objProperties[i].Name, objProperties[i].PropertyType);
                }
            }
            //将所有objObject添加到DataTable中
            foreach (object objObject in objFillCollection)
            {
                //检查所有的的实体都为同一类型
                if (objObject.GetType() != objObjectType)
                {
                    throw new Exception("要转换的集合元素类型不一致");
                }
                object[] objObjectValues = new object[objProperties.Length];
                for (int i = 0; i < objProperties.Length; i++)
                {
                    objObjectValues[i] = objProperties[i].GetValue(objObject, null);
                }
                dt.Rows.Add(objObjectValues);
            }
            return dt;
        }
        #endregion

        #region DataTable 转化为List
        /// <summary>
        /// DataTable 转化为List
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static List<T> DataTableToListCollection<T>(DataTable dt)
        {
            List<T> objListCollection = new List<T>();
            foreach (DataRow row in dt.Rows)
            {
                objListCollection.Add(CreateObject<T>(row));
            }
            return objListCollection;
        }
        /// <summary>
        /// DataRow转化为T对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dr"></param>
        /// <returns></returns>
        private static T CreateObject<T>(DataRow row)
        {
            T objObject = Activator.CreateInstance<T>();
            foreach (PropertyInfo pi in objObject.GetType().GetProperties())
            {
                if (pi.CanWrite)
                {
                    //获取属性的初始化值
                    var objValue = Null.SetNull(pi);
                    if (row.Table.Columns.Contains(pi.Name))
                    {

                        if (System.Convert.IsDBNull(row[pi.Name]))
                        {
                            row[pi.Name] = objValue;
                        }
                        pi.SetValue(objObject, Convert.ChangeType(row[pi.Name], pi.PropertyType), null);
                    }
                    else
                    {
                        pi.SetValue(objObject, Convert.ChangeType(objValue, pi.PropertyType), null);
                    }
                }
            }
            return objObject;
        }
        #endregion

    }
}
