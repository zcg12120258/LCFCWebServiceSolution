using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CommonHandler.Log;
using CommonHandler.Model;
using KittingToLineWebService.DataGateway;
using System.Data;
using KittingToLineWebService.Beans;
using CommonHandler.Helper;
using System.Reflection;

namespace KittingToLineWebService.Core
{
    public class DataProcessHelper
    {
        private DataProviderService dataProviderService;
        //日志对象
        private LogHandler logHandler;

        public DataProcessHelper(string conectionString)
        {
            dataProviderService = new DataProviderService(conectionString);
            logHandler = new LogHandler(this.GetType().Name);
        }

        #region 从SAP获取Lot号，更新R_WIP_LOT_MO_LOG_T表的Lot号、插入Lot号和Lot_Seq数据到r_wip_lot_tracking_t表
        public ReturnResult Get_PPM_LotFromSap_Process(List<LotMoEntity> lotMoList)//LotEntity
        {
            ReturnResult result = new ReturnResult();
            if (lotMoList != null && lotMoList.Count > 0)
            {
                var monumberList = lotMoList.Select(x => x.MO_ORDER).ToArray();
                List<LotMoEntity> toSapList = dataProviderService.GetMoEntityListByMoOrders(string.Join("','", monumberList));
                if (toSapList != null && toSapList.Count > 0)
                {
                    int lotSeq = 0;
                    //在lotMoList==[SAp根据toSapList生成lot后，回传给Mes的数据列表]存在，
                    //而在R_WIP_LOT_MO_LOG_T中不存的值
                    var IsEqualList = lotMoList.Where(x => !toSapList.Exists(y => x.MO_ORDER.Contains(y.MO_ORDER))).ToList();
                    if (IsEqualList.Count == 0)
                    {
                        lotMoList.ForEach(m =>
                        {
                            var item = toSapList.FirstOrDefault(n => n.MO_ORDER == m.MO_ORDER);
                            if (item != null)
                            {
                                m.LINE_NAME = item.LINE_NAME;
                                m.SHIFT = item.SHIFT;
                                m.CREATE_TIME = item.CREATE_TIME;
                            }
                        });

                        result = dataProviderService.UpdateBatch_R_WIP_LOT_MO_LOG_T(lotMoList);
                        List<LotMoEntity> lotMoListSort = lotMoList.OrderBy(x => int.Parse(x.MO_NO)).GroupBy(y => y.LOT_NO).Select(z => z.FirstOrDefault()).ToList();//根据mo顺序排序，并按lot号分组，得到唯一lot的最小mo序列值

                        List<LotMoEntity> lotSeqList = new List<LotMoEntity>();
                        lotSeqList.Clear();
                        lotMoListSort.Select(x =>
                        {
                            //LotMoEntity lotMoEntity = dataProviderService.GetMoEntityByMoOrder(x.MO_ORDER);
                            if (lotSeqList != null && lotSeqList.Count > 0)
                            {
                                var temp = lotSeqList.OrderByDescending(t => int.Parse(t.LOT_SEQ)).Where(t => t.SHIFT == x.SHIFT && t.CREATE_TIME == x.CREATE_TIME && t.LINE_NAME == x.LINE_NAME).ToList();
                                if (temp != null && temp.Count > 0)
                                {
                                    lotSeq = int.Parse(temp.FirstOrDefault().LOT_SEQ) + 1;
                                }
                                else
                                {
                                    lotSeq = dataProviderService.GetMaxLotSeq(x.SHIFT, x.CREATE_TIME, x.LINE_NAME);
                                }
                            }
                            else
                            {
                                lotSeq = dataProviderService.GetMaxLotSeq(x.SHIFT, x.CREATE_TIME, x.LINE_NAME);
                            }

                            lotSeqList.Add(new LotMoEntity { LOT_SEQ = lotSeq.ToString(), LINE_NAME = x.LINE_NAME, CREATE_TIME = x.CREATE_TIME, SHIFT = x.SHIFT });
                            x.LOT_SEQ = lotSeq.ToString();
                            // var entity = dataProviderService.GetMoEntityByMoOrder(x.MO_ORDER);
                            
                            return x;
                        }).ToList();

                        if (result.Status)
                        {
                            List<LotMoEntityGroup> LotMoEntityGroupList = lotMoList.GroupBy(x => new { x.LINE_NAME, x.SHIFT, x.CREATE_TIME }).Select(g =>
                                 new LotMoEntityGroup {
                                     LINE_NAME = g.Key.LINE_NAME,
                                     CREATE_TIME=g.Key.CREATE_TIME,
                                     SHIFT=g.Key.SHIFT
                            }).ToList();
                            LotMoEntityGroupList.ForEach(x=>
                            {
                                x.LotMoEntityList = lotMoList.Where(y => x.LINE_NAME == y.LINE_NAME && x.CREATE_TIME == y.CREATE_TIME && x.SHIFT == y.SHIFT).ToList();
                                x.LotMoEntityListSort= lotMoListSort.Where(y => x.LINE_NAME == y.LINE_NAME && x.CREATE_TIME == y.CREATE_TIME && x.SHIFT == y.SHIFT).ToList();
                                result = dataProviderService.UpdateLogAndInsertTracking(x.LotMoEntityList, x.LotMoEntityListSort);
                            });
                             //批量插入Lot数据到r_wip_lot_tracking_t表
                        }
                    }
                    else
                    {
                        var Mo_OrderList = IsEqualList.Select(x => x.MO_ORDER).ToArray();
                        result.Status = false;
                        result.Message = "工单号:" + string.Join(",", Mo_OrderList) + " 在SFISM4.R_WIP_LOT_MO_LOG_T表中不存在";
                    }
                }
                else
                {
                    result.Status = false;
                    result.Message = "SFISM4.R_WIP_LOT_MO_LOG_T表没有ToSap的数据";
                }
            }
            else
            {
                result.Status = false;
                result.Message = "lotMoList数据为空";
            }
            if (!result.Status)
            {
                this.logHandler.Error(MethodBase.GetCurrentMethod().Name, "Call Get_PPM_LotFromSap_Process Fail: " + result.Message);
            }
            return result;
        }

        public ReturnResult Update_Add_LotFromSap_Process(List<LotMoEntity> lotMoList)//LotEntity
        {
            int lotSeq = 0;
            ReturnResult result = new ReturnResult();
            result = dataProviderService.UpdateBatch_R_WIP_LOT_MO_LOG_T(lotMoList);

            List<LotMoEntity> lotMoListSort = lotMoList.OrderBy(x => int.Parse(x.MO_NO)).GroupBy(y => y.LOT_NO).Select(z => z.FirstOrDefault()).ToList();//根据mo顺序排序，并按lot号分组，得到唯一lot的最小mo序列值

            List<LotMoEntity> lotSeqList = new List<LotMoEntity>();
            lotSeqList.Clear();
            lotMoListSort.Select(x =>
            {
                LotMoEntity lotMoEntity = dataProviderService.GetMoEntityByMoOrder(x.MO_ORDER);
                if (lotSeqList != null && lotSeqList.Count > 0)
                {
                    var temp = lotSeqList.OrderByDescending(t => int.Parse(t.LOT_SEQ)).Where(t => t.SHIFT == lotMoEntity.SHIFT && t.CREATE_TIME == lotMoEntity.CREATE_TIME && t.LINE_NAME == lotMoEntity.LINE_NAME).ToList();
                    if (temp != null && temp.Count > 0)
                    {
                        lotSeq = int.Parse(temp.FirstOrDefault().LOT_SEQ) + 1;
                    }
                    else
                    {
                        lotSeq = dataProviderService.GetMaxLotSeq(lotMoEntity.SHIFT, lotMoEntity.CREATE_TIME, lotMoEntity.LINE_NAME);

                    }
                }
                else
                {
                    lotSeq = dataProviderService.GetMaxLotSeq(lotMoEntity.SHIFT, lotMoEntity.CREATE_TIME, lotMoEntity.LINE_NAME);
                }

                lotSeqList.Add(new LotMoEntity { LOT_SEQ = lotSeq.ToString(), LINE_NAME = lotMoEntity.LINE_NAME, CREATE_TIME = lotMoEntity.CREATE_TIME, SHIFT = lotMoEntity.SHIFT });
                x.LOT_SEQ = lotSeq.ToString();
                // var entity = dataProviderService.GetMoEntityByMoOrder(x.MO_ORDER);
                x.LINE_NAME = lotMoEntity.LINE_NAME;
                x.CREATE_TIME = lotMoEntity.CREATE_TIME;
                x.SHIFT = lotMoEntity.SHIFT;
                return x;
            }).ToList();

            var s = lotMoList.GroupBy(x=> new { x.LINE_NAME,x.CREATE_TIME,x.SHIFT});
            //批量插入Lot数据到r_wip_lot_tracking_t表
            //result = dataProviderService.InsertBatch_R_WIP_LOT_TRACKING_T(lotMoListSort);
            result = dataProviderService.UpdateLogAndInsertTracking(lotMoList, lotMoListSort);

            return result;
        }

            #endregion

            #region By Lot上下架数据同步到MES
            public DataTable SXJ_Process(DataTable dt)
        {
            DataTable resultDt = new DataTable();
            List<SXJ_CELLNOEntity> cellList = dataProviderService.Get_C_PICK_CONFIG_T();
            List<SXJ_WMSEntity> wmsList = DataSourceConvertHelper.DataTableToListCollection<SXJ_WMSEntity>(dt);
            if (wmsList != null && wmsList.Count > 0)
            {
                wmsList.Select(x =>
                {
                    SXJ_Process_Select(x, cellList);
                    return x;
                }).ToList();
                resultDt = DataSourceConvertHelper.ListToDataTable<SXJ_WMSEntity>(wmsList); 
            }
            return resultDt;
        }
        private void SXJ_Process_Select(SXJ_WMSEntity item, List<SXJ_CELLNOEntity> cellList)
        {
            ReturnResult result = new ReturnResult();
            SXJ_R_KITT_PN_TEntity r_kitt_pn_Entity = new SXJ_R_KITT_PN_TEntity();
            r_kitt_pn_Entity.MOGP = item.MOGP;
            r_kitt_pn_Entity.MATERIAL = item.MATERIAL;
            r_kitt_pn_Entity.VENDORCODE = item.VENDORCODE;
            r_kitt_pn_Entity.QTY = item.QTY;
            r_kitt_pn_Entity.ONRACKQTY = item.ONRACKQTY;
            r_kitt_pn_Entity.RACKNO = item.RACKNO;
            r_kitt_pn_Entity.CREATE_NAME = item.USER;
            r_kitt_pn_Entity.DESC1 = r_kitt_pn_Entity.DESC2 = r_kitt_pn_Entity.DESC3 = string.Empty;

            SXJ_R_KITT_PN_LOG_TEntity r_kitt_pn_log_Entity = new SXJ_R_KITT_PN_LOG_TEntity();
            r_kitt_pn_log_Entity.MOGP = item.MOGP;
            r_kitt_pn_log_Entity.MATERIAL = item.MATERIAL;
            r_kitt_pn_log_Entity.VENDORCODE = item.VENDORCODE;
            r_kitt_pn_log_Entity.QTY = item.QTY;
            r_kitt_pn_log_Entity.ONRACKQTY = item.ONRACKQTY;
            r_kitt_pn_log_Entity.RACKNO = item.RACKNO;
            r_kitt_pn_log_Entity.CREATE_NAME = item.USER;
            r_kitt_pn_log_Entity.DESC1 = r_kitt_pn_log_Entity.DESC2 = r_kitt_pn_log_Entity.DESC3 = string.Empty;

            //判断WMS传过来的储位号，在MES中存在
            if (cellList.Any(x => x.CELL_NO == item.RACKNO))
            {
                string line_Name = cellList.Where(x => x.CELL_NO == item.RACKNO).FirstOrDefault().LINE_NAME;
                SXJ_R_KITT_PN_TEntity r_kitt_pn_t_Entity = dataProviderService.Get_SXJ_R_KITT_PN_TEntityByWMSEntity(item);
                //判断Lot号、储位号、物料号和供应商编号，在SXJ_R_KITT_PN_T表中是否存在
                if (r_kitt_pn_t_Entity != null)
                {
                    
                    //Lot号、储位号、物料号和供应商编号，在SXJ_R_KITT_PN_T表中存在：更新SXJ_R_KITT_PN_T表数据
                    if (r_kitt_pn_t_Entity.ONRACKQTY + item.ONRACKQTY > 0)//储位库存+上下架数>0
                    {
                        result = dataProviderService.Update_SXJ_R_KITT_PN_TEntity(r_kitt_pn_Entity);//更新SXJ_R_KITT_PN_T数据
                        if (result.Status)
                        {
                            r_kitt_pn_log_Entity.LINE_NAME = line_Name;
                            item.STATUS = r_kitt_pn_log_Entity.STATUS = 2;
                            item.STATUS_INFO = r_kitt_pn_log_Entity.STATUS_INFO = "更新SXJ_R_KITT_PN_T数据";
                            result = dataProviderService.Insert_SXJ_R_KITT_PN_LOG_T(r_kitt_pn_log_Entity);
                        }
                    }
                    else if (r_kitt_pn_t_Entity.ONRACKQTY + item.ONRACKQTY == 0) //储位库存+上下架数=0
                    {
                        result = dataProviderService.Delete_SXJ_R_KITT_PN_TEntity(r_kitt_pn_Entity);//删除SXJ_R_KITT_PN_T数据"
                        if (result.Status)
                        {
                            r_kitt_pn_log_Entity.LINE_NAME = line_Name;
                            item.STATUS = r_kitt_pn_log_Entity.STATUS = 8;
                            item.STATUS_INFO = r_kitt_pn_log_Entity.STATUS_INFO = "下架数量和库存数相等，删除SXJ_R_KITT_PN_T数据";
                            result = dataProviderService.Insert_SXJ_R_KITT_PN_LOG_T(r_kitt_pn_log_Entity);
                        }
                    }
                    else //储位库存+上下架数<0,直接告警
                    {
                        r_kitt_pn_log_Entity.LINE_NAME = line_Name;
                        item.STATUS = r_kitt_pn_log_Entity.STATUS = 7;
                        item.STATUS_INFO = r_kitt_pn_log_Entity.STATUS_INFO = "下架数量:" + Math.Abs(item.ONRACKQTY) + ";大于现有库存:" + r_kitt_pn_t_Entity.ONRACKQTY;
                        result = dataProviderService.Insert_SXJ_R_KITT_PN_LOG_T(r_kitt_pn_log_Entity);
                    }
                }
                else
                {
                    //RAckNO是否存在
                    SXJ_R_KITT_PN_TEntity r_kitt_pn_t_Entity_byRackNO = dataProviderService.Get_SXJ_R_KITT_PN_TEntityByRackNo(item.RACKNO);
                    if (r_kitt_pn_t_Entity_byRackNO != null)//rackno 存在
                    {
                        SXJ_R_KITT_PN_TEntity r_kitt_pn_t_Entity_byPnVcRack = dataProviderService.Get_SXJ_R_KITT_PN_TEntityByPNAndVCAndRack(item.RACKNO, item.MATERIAL, item.VENDORCODE);
                        if (r_kitt_pn_t_Entity_byPnVcRack != null)//pn,vc,rackno存在
                        {
                            r_kitt_pn_Entity.LINE_NAME = line_Name;
                            result = dataProviderService.Insert_SXJ_R_KITT_PN_T(r_kitt_pn_Entity);
                            if (result.Status)
                            {
                                r_kitt_pn_log_Entity.LINE_NAME = line_Name;
                                item.STATUS = r_kitt_pn_log_Entity.STATUS = 4;
                                item.STATUS_INFO = r_kitt_pn_log_Entity.STATUS_INFO = "pn,vc,rackno相同，lot不同，插入数据到SXJ_R_KITT_PN_T表";
                                result = dataProviderService.Insert_SXJ_R_KITT_PN_LOG_T(r_kitt_pn_log_Entity);
                            }
                        }
                        else
                        {
                            r_kitt_pn_log_Entity.LINE_NAME = line_Name;
                            item.STATUS = r_kitt_pn_log_Entity.STATUS = 3;
                            item.STATUS_INFO = r_kitt_pn_log_Entity.STATUS_INFO = "相同的RackNO，不能插入不同的pn和vc";
                            result = dataProviderService.Insert_SXJ_R_KITT_PN_LOG_T(r_kitt_pn_log_Entity);
                        }
                    }
                    else //rackno 不存在
                    {
                        SXJ_R_KITT_PN_TEntity r_kitt_pn_t_Entity_byLotPnVc = dataProviderService.Get_SXJ_R_KITT_PN_TEntityByLotAndPNAndVC(item.MOGP, item.MATERIAL, item.VENDORCODE);
                        if (r_kitt_pn_t_Entity_byLotPnVc != null)
                        {
                            r_kitt_pn_log_Entity.LINE_NAME = line_Name;
                            item.STATUS = r_kitt_pn_log_Entity.STATUS = 5;
                            item.STATUS_INFO = r_kitt_pn_log_Entity.STATUS_INFO = "相同的：Lot,pn,vc，不能存在不同的储位";
                            result = dataProviderService.Insert_SXJ_R_KITT_PN_LOG_T(r_kitt_pn_log_Entity);
                        }
                        else
                        {
                            //线体，物料号、供应商不同时存在不同的货架，插入数据到SXJ_R_KITT_PN_T表中
                            r_kitt_pn_Entity.LINE_NAME = line_Name;
                            result = dataProviderService.Insert_SXJ_R_KITT_PN_T(r_kitt_pn_Entity);
                            if (result.Status)
                            {
                                r_kitt_pn_log_Entity.LINE_NAME = line_Name;
                                item.STATUS = r_kitt_pn_log_Entity.STATUS = 6;
                                item.STATUS_INFO = r_kitt_pn_log_Entity.STATUS_INFO = "不同的rackno，不同的pn或vc或lot，插入数据到SXJ_R_KITT_PN_T表";
                                result = dataProviderService.Insert_SXJ_R_KITT_PN_LOG_T(r_kitt_pn_log_Entity);
                            }
                        }
                    }
                }
            }
            else
            {
                //WMS传过来的储位号，在MES中不存在，写日志
                r_kitt_pn_log_Entity.LINE_NAME = "";
                item.STATUS = r_kitt_pn_log_Entity.STATUS = 1;
                item.STATUS_INFO = r_kitt_pn_log_Entity.STATUS_INFO = "储位号在MES中不存在";
                result = dataProviderService.Insert_SXJ_R_KITT_PN_LOG_T(r_kitt_pn_log_Entity);
            }
        }
        #endregion
        

        #region 调用库存(WMS从MES获取RACK架位信息)
        /// <summary>
        /// 根据料号、客户别获取料号对应站别
        /// </summary>
        /// <param name="cust_No"></param>
        /// <param name="key_Part_No"></param>
        /// <returns></returns>
        public string ZB_Process(string cust_No, string key_Part_No)
        {
            string result = string.Empty;
            try
            {
                DataSet ds = dataProviderService.Get_C_KEYPARTS_MODE_T(cust_No, key_Part_No);
                if (ds != null && ds.Tables[0].Rows.Count > 0)
                {
                    result = ds.Tables[0].Rows[0][1].ToString();
                }
            }
            catch (Exception ex)
            {
                this.logHandler.Error(MethodBase.GetCurrentMethod().Name, "Call ZB_Process Fail: " + ex.Message);
            }

            return result;
        }
        #endregion


        #region 同步MO线体信息
        /// <summary>
        /// 同步MO线体信息
        /// </summary>
        /// <param name="sID"></param>
        /// <returns></returns>
        public DataTable MO_Synchronization(string sID)
        {
            try
            {
                return dataProviderService.MO_Synchronization(sID);
            }
            catch (Exception ex)
            {
                this.logHandler.Error(MethodBase.GetCurrentMethod().Name, "Call MO_Synchronization [Service] Fail: " + ex.Message);
            }

            return null;
        }
        #endregion

        #region 3.3 同步料架库存及水位信息
        /// <summary>
        /// 3.3 同步料架库存及水位信息
        /// </summary>
        /// <param name="lineName">线体名称</param>
        /// <returns></returns>
        public DataTable MO_RackStockAndWaterLevel(string lineName)
        {
            try
            {
                return dataProviderService.MO_RackStockAndWaterLevel(lineName);
            }
            catch (Exception ex)
            {
                this.logHandler.Error(MethodBase.GetCurrentMethod().Name, "Call MO_RackStockAndWaterLevel [Service] Fail: " + ex.Message);
            }

            return null;
        }
        #endregion
    }
}