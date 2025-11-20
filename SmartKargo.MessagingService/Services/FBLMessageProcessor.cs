#region FBL Message Processor Class Description
/* FBL Message Processor Class Description.
      * Company              :   QID SmartKargo(I)	Pvt. Ltd.
      * Copyright              :   Copyright © 2015 QID SmartKargo(I)	Pvt. Ltd.
      * Purpose                : 
      * Created By           :   Badiuzzaman Khan
      * Created On           :   2016-03-04
      * Approved By         :
      * Approved Date      :
      * Modified By          :  
      * Modified On          :   
      * Description           :   
     */
#endregion
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using SmartKargo.MessagingService.Data.Dao.Interfaces;
using SmartKargo.MessagingService.Services;
using System.Data;
using System.Text.RegularExpressions;
namespace QidWorkerRole
{
    public class FBLMessageProcessor
    {
        //#region :: Variable Declaration ::
        string unloadingportsequence = string.Empty;
        string uldsequencenum = string.Empty;
        string awbref = string.Empty;
        //static string strConnection = ConfigurationManager.ConnectionStrings["ConStr"].ToString();
        //const string PAGE_NAME = "FBLMessageProcessor";
        //SCMExceptionHandlingWorkRole scm = new SCMExceptionHandlingWorkRole();
        //#endregion

        #region :: Constructor

        private readonly ISqlDataHelperDao _readWriteDao;
        private readonly ILogger<FBLMessageProcessor> _logger;
        private readonly GenericFunction _genericFunction;
        private readonly FNAMessageProcessor _fNAMessageProcessor;

        public FBLMessageProcessor(
            ISqlDataHelperFactory sqlDataHelperFactory,
            ILogger<FBLMessageProcessor> logger,
            GenericFunction genericFunction,
            FNAMessageProcessor fNAMessageProcessor)
        {
            _readWriteDao = sqlDataHelperFactory.Create(readOnly: false);
            _logger = logger;
            _genericFunction = genericFunction;
            _fNAMessageProcessor = fNAMessageProcessor;
        }
        #endregion

        #region :: Private Methods ::

        /**Not in use*/
        private bool decodereceiveFBL(string fblmsg, ref MessageData.fblinfo fbldata, ref MessageData.unloadingport[] unloadingport, ref MessageData.dimensionnfo[] dimensioinfo, ref MessageData.ULDinfo[] uld, ref MessageData.otherserviceinfo othinfo, ref MessageData.consignmentorigininfo[] consorginfo, ref MessageData.consignmnetinfo[] consinfo)
        {
            bool flag = false;
            try
            {
                string lastrec = "NA";

                try
                {
                    if (fblmsg.StartsWith("FBL", StringComparison.OrdinalIgnoreCase))
                    {

                        string[] str = Regex.Split(fblmsg, "$");
                        if (str.Length > 3)
                        {
                            for (int i = 0; i < str.Length; i++)
                            {

                                flag = true;
                                #region Line 1
                                if (str[i].StartsWith("FBL", StringComparison.OrdinalIgnoreCase))
                                {
                                    try
                                    {
                                        string[] msg = str[i].Split('/');
                                        fbldata.fblversion = msg[1];
                                    }
                                    catch (Exception ex)
                                    {
                                        // clsLog.WriteLogAzure(ex);
                                        _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                    }
                                }
                                #endregion

                                #region line 2 flight data
                                if (i == 1)
                                {
                                    try
                                    {
                                        string[] msg = str[i].Split('/');
                                        if (msg.Length > 1)
                                        {
                                            fbldata.messagesequencenum = msg[0];
                                            fbldata.carriercode = msg[1].Substring(0, 2);
                                            fbldata.fltnum = msg[1].Substring(2);
                                            fbldata.date = msg[2].Substring(0, 2);
                                            fbldata.month = msg[2].Substring(2);
                                            fbldata.fltairportcode = msg[3];
                                            fbldata.aircraftregistration = msg[4];
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        // clsLog.WriteLogAzure(ex);
                                        _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                    }
                                }
                                #endregion

                                #region line 3 point of unloading
                                if (i >= 2)
                                {
                                    MessageData.unloadingport unloading = new MessageData.unloadingport("");
                                    if (str[i].Contains('/'))
                                    {
                                        string[] msg = str[i].Split('/');
                                        if (msg.Length == 2)
                                        {
                                            if (msg[0].Length > 0 && !msg[0].Equals("SSR", StringComparison.OrdinalIgnoreCase) && !msg[0].Equals("OSI", StringComparison.OrdinalIgnoreCase))
                                            {
                                                unloading.unloadingairport = msg[0];
                                                unloading.nilcargocode = msg[1];
                                                Array.Resize(ref unloadingport, unloadingport.Length + 1);
                                                unloadingport[unloadingport.Length - 1] = unloading;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (str[i].Trim().Length == 3)
                                        {
                                            unloading.unloadingairport = str[i];
                                            Array.Resize(ref unloadingport, unloadingport.Length + 1);
                                            unloadingport[unloadingport.Length - 1] = unloading;
                                        }
                                    }
                                }
                                #endregion

                                #region  line 4 onwards check consignment details
                                if (i > 1)
                                {
                                    try
                                    {
                                        string[] msg = str[i].Split('/');
                                        //0th element
                                        if (msg[0].Contains('-'))
                                        {
                                            //   DecodeConsigmentDetails(str[i], ref consinfo,"","");
                                        }

                                    }
                                    catch (Exception ex)
                                    {
                                        // clsLog.WriteLogAzure(ex);
                                        _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                        continue;
                                    }
                                }
                                #endregion

                                #region Line 5 Dimendion info
                                if (str[i].StartsWith("DIM", StringComparison.OrdinalIgnoreCase))
                                {
                                    try
                                    {
                                        string[] msg = str[i].Split('/');
                                        int total = msg.Length / 3;
                                        Array.Resize(ref dimensioinfo, dimensioinfo.Length + total + 1);
                                        for (int cnt = 0; cnt < total; cnt++)
                                        {
                                            int place = 3 * cnt;
                                            MessageData.dimensionnfo dimension = new MessageData.dimensionnfo("");
                                            dimension.weightcode = msg[place + 1].Substring(0, 1);
                                            dimension.weight = msg[place + 1].Substring(1);
                                            if (msg.Length > 0)
                                            {
                                                string[] dimstr = msg[place + 2].Split('-');
                                                dimension.mesurunitcode = dimstr[0].Substring(0, 3);
                                                dimension.length = dimstr[0].Substring(3);
                                                dimension.weight = dimstr[1];
                                                dimension.height = dimstr[2];
                                            }
                                            dimension.piecenum = msg[place + 3];
                                            dimensioinfo[cnt] = dimension;
                                        }

                                    }
                                    catch (Exception ex)
                                    {
                                        // clsLog.WriteLogAzure(ex);
                                        _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                    }
                                }
                                #endregion

                                #region Line 7 ULD Specification
                                if (str[i].StartsWith("ULD", StringComparison.OrdinalIgnoreCase))
                                {
                                    try
                                    {
                                        int uldnum = 0;
                                        string[] msg = str[i].Split('/');
                                        if (msg.Length > 1)
                                        {
                                            fbldata.noofuld = msg[1];
                                            if (int.Parse(msg[1]) > 0)
                                            {
                                                Array.Resize(ref uld, uld.Length + 1 + int.Parse(msg[1]));
                                                for (int k = 2; k < msg.Length; k += 2)
                                                {
                                                    MessageData.ULDinfo ulddata = new MessageData.ULDinfo("");
                                                    string[] splitstr = msg[k].Split('-');
                                                    ulddata.uldno = splitstr[0];
                                                    ulddata.uldtype = splitstr[0].Substring(0, 3);
                                                    ulddata.uldsrno = splitstr[0].Substring(3, splitstr[0].Length - 6);
                                                    ulddata.uldowner = splitstr[0].Substring(splitstr[0].Length - 3, 3);
                                                    ulddata.uldloadingindicator = splitstr[1];
                                                    ulddata.uldweightcode = msg[k + 1].Substring(0, 1);
                                                    ulddata.uldweight = msg[k + 1].Substring(1);
                                                    uld[uldnum++] = ulddata;
                                                }
                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        // clsLog.WriteLogAzure(ex);
                                        _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                    }
                                }
                                #endregion

                                #region Line 8 Special Service request
                                if (str[i].StartsWith("SSR", StringComparison.OrdinalIgnoreCase))
                                {
                                    try
                                    {
                                        string[] msg = str[i].Split('/');
                                        lastrec = msg[0];
                                        if (msg[1].Length > 0)
                                        {
                                            fbldata.specialservicereq1 = msg[1];
                                        }

                                    }
                                    catch (Exception ex)
                                    {
                                        // clsLog.WriteLogAzure(ex);
                                        _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                    }
                                }
                                #endregion

                                #region Line 9 Other service info
                                if (str[i].StartsWith("OSI", StringComparison.OrdinalIgnoreCase))
                                {
                                    try
                                    {
                                        string[] msg = str[i].Split('/');
                                        lastrec = msg[0];
                                        if (msg[1].Length > 0)
                                        {
                                            othinfo.otherserviceinfo1 = msg[1];
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        // clsLog.WriteLogAzure(ex); 
                                        _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                    }
                                }
                                #endregion

                                #region Last line
                                if (i > str.Length - 2)
                                {
                                    if (str[i].Trim().Length == 4 || str[i].Trim().Equals("LAST", StringComparison.OrdinalIgnoreCase) || str[i].Trim().Equals("CONT", StringComparison.OrdinalIgnoreCase))
                                    {
                                        fbldata.endmesgcode = str[i].Trim();
                                    }

                                }
                                #endregion

                                #region Other Info
                                if (str[i].StartsWith("/"))
                                {
                                    string[] msg = str[i].Split('/');
                                    try
                                    {
                                        #region line 6 consigment origin info
                                        if (msg.Length > 0 && msg[0].Length == 0 && lastrec == "NA")
                                        {
                                            MessageData.consignmentorigininfo consorg = new MessageData.consignmentorigininfo();
                                            try
                                            {
                                                consorg.abbrivatedname = msg[1];
                                                consorg.carriercode = msg[2].Length > 0 ? msg[2].Substring(0, 2) : "";
                                                consorg.flightnum = msg[2].Length > 0 ? msg[2].Substring(2) : "";
                                                consorg.day = msg[3].Length > 0 ? msg[3].Substring(0, 2) : "";
                                                consorg.month = msg[3].Length > 0 ? msg[3].Substring(2) : "";
                                                consorg.airportcode = msg[4];
                                                consorg.movementprioritycode = msg[5];
                                            }
                                            catch (Exception ex)
                                            {
                                                // clsLog.WriteLogAzure(ex); 
                                                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                            }
                                            Array.Resize(ref consorginfo, consorginfo.Length + 1);
                                            consorginfo[consorginfo.Length - 1] = consorg;
                                        }
                                        #endregion

                                        #region SSR 2
                                        if (lastrec == "SSR")
                                        {
                                            fbldata.specialservicereq2 = msg[1].Length > 0 ? msg[1] : "";
                                            lastrec = "NA";
                                        }
                                        #endregion

                                        #region OSI 2
                                        if (lastrec == "OSI")
                                        {
                                            othinfo.otherserviceinfo2 = msg[1].Length > 0 ? msg[1] : "";
                                            lastrec = "NA";
                                        }
                                        #endregion
                                    }
                                    catch (Exception ex)
                                    {
                                        // clsLog.WriteLogAzure(ex); 
                                        _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                    }
                                }
                                #endregion
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    // clsLog.WriteLogAzure(ex);
                    _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                    flag = false;
                }
            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                flag = false;
            }
            return flag;
        }

        private void DecodeConsigmentDetails(string inputstr, ref MessageData.consignmnetinfo[] consinfo, ref string awbprefix, ref string awbnumber)
        {
            try
            {
                MessageData.consignmnetinfo consig = new MessageData.consignmnetinfo("");
                string[] msg = inputstr.Split('/');
                string[] decmes = msg[0].Split('-');

                consig.airlineprefix = decmes[0];
                string[] sptarr = StringSplitter(decmes[1]);
                if (sptarr.Length > 0)
                {
                    try
                    {
                        consig.awbnum = sptarr[0];
                        if (sptarr[1].Length == 3)
                        {
                            consig.origin = "";
                            consig.dest = sptarr[1];
                        }
                        else if (sptarr[1].Length == 6)
                        {
                            consig.origin = sptarr[1].Substring(0, 3);
                            consig.dest = sptarr[1].Substring(3); ;
                        }
                    }
                    catch (Exception ex)
                    {
                        // clsLog.WriteLogAzure(ex);
                        _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                    }
                }
                else
                {
                    try
                    {
                        consig.awbnum = decmes[1].Substring(0, decmes[1].Length - 6);
                        consig.origin = decmes[1].Substring(decmes[1].Length - 6, 3);
                        consig.dest = decmes[1].Substring(decmes[1].Length - 3, 3);
                    }
                    catch (Exception ex)
                    {
                        // clsLog.WriteLogAzure(ex);
                        _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                    }
                }
                //1
                if (msg[1].Length > 0)
                {
                    try
                    {
                        int k = 0;
                        char lastchr = 'A';
                        char[] arr = msg[1].ToCharArray();
                        string[] strarr = new string[arr.Length];
                        for (int j = 0; j < arr.Length; j++)
                        {
                            if ((char.IsNumber(arr[j])) || (arr[j].Equals('.')))
                            {//number                            
                                if (lastchr == 'N')
                                    k--;
                                strarr[k] = strarr[k] + arr[j].ToString();
                                lastchr = 'N';
                            }
                            if (char.IsLetter(arr[j]))
                            {//letter
                                if (lastchr == 'L')
                                    k--;
                                strarr[k] = strarr[k] + arr[j].ToString();
                                lastchr = 'L';
                            }
                            k++;
                        }
                        consig.consigntype = strarr[0];
                        consig.pcscnt = strarr[1];//int.Parse(strarr[1]);
                        consig.weightcode = strarr[2];
                        consig.weight = strarr[3];//float.Parse(strarr[3]);
                        if (consig.consigntype.Equals("T"))
                        {//total pieces
                            consig.numshp = consig.pcscnt;
                        }
                        for (k = 4; k < strarr.Length; k += 2)
                        {
                            if (strarr[k] != null)
                            {
                                if (strarr[k] == "T")
                                {
                                    consig.shpdesccode = strarr[k];
                                    consig.numshp = strarr[k + 1];
                                    k = strarr.Length + 1;
                                    //if (consig.consigntype.Equals("S")) 
                                    //{
                                    //    consig.pcscnt = consig.numshp;
                                    //}
                                }
                                else if (strarr[k] == "DG")
                                {
                                    consig.densityindicator = strarr[k];
                                    consig.densitygrp = strarr[k + 1];
                                }
                                else
                                {
                                    consig.volumecode = strarr[k];
                                    consig.volumeamt = strarr[k + 1];
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        // clsLog.WriteLogAzure(ex);
                        _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                    }
                }
                if (msg.Length > 2)
                {
                    try
                    {

                        consig.manifestdesc = msg[2];
                    }
                    catch (Exception ex)
                    {
                        // clsLog.WriteLogAzure(ex);
                        _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                    }

                }
                if (msg.Length > 3)
                {
                    try
                    {
                        consig.splhandling = "";
                        for (int j = 3; j < msg.Length; j++)
                            consig.splhandling = consig.splhandling + msg[j] + ",";
                    }
                    catch (Exception ex)
                    {
                        // clsLog.WriteLogAzure(ex);
                        _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                    }
                }
                try
                {
                    if (unloadingportsequence.Length > 0)
                        consig.portsequence = unloadingportsequence;
                    if (uldsequencenum.Length > 0)
                        consig.uldsequence = uldsequencenum;
                }
                catch (Exception ex)
                {
                    // clsLog.WriteLogAzure(ex);
                    _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                }
                awbprefix = consig.airlineprefix;
                awbnumber = consig.awbnum;
                Array.Resize(ref consinfo, consinfo.Length + 1);
                consinfo[consinfo.Length - 1] = consig;
                awbref = consinfo.Length.ToString();
            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
            }
        }

        private string[] StringSplitter(string str)
        {
            char[] arr = str.ToCharArray();
            string[] strarr = new string[arr.Length];

            try
            {
                if (str.Length > 0)
                {
                    int k = 0;
                    char lastchr = 'A';
                    for (int j = 0; j < arr.Length; j++)
                    {
                        if ((char.IsNumber(arr[j])) || (arr[j].Equals('.')))
                        {//number                            
                            if (lastchr == 'N')
                                k--;
                            strarr[k] = strarr[k] + arr[j].ToString();
                            lastchr = 'N';
                        }
                        if (char.IsLetter(arr[j]))
                        {//letter
                            if (lastchr == 'L')
                                k--;
                            strarr[k] = strarr[k] + arr[j].ToString();
                            lastchr = 'L';
                        }
                        k++;
                    }
                }
            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                strarr = null;
            }
            return strarr;
        }

        private void ReProcessConsigment(ref MessageData.consignmnetinfo[] FFMConsig, ref MessageData.consignmnetinfo[] consinfo)
        {
            try
            {
                bool AWBMatch = false;
                Array.Resize(ref consinfo, consinfo.Length + 1);
                Array.Copy(FFMConsig, consinfo, 1);
                for (int i = 1; i < FFMConsig.Length; i++)
                {
                    AWBMatch = false;
                    for (int j = 0; j < consinfo.Length; j++)
                    {
                        if (consinfo[j].awbnum.Equals(FFMConsig[i].awbnum) && consinfo[j].origin.Equals(FFMConsig[i].origin) && consinfo[j].dest.Equals(FFMConsig[i].dest))
                        {
                            AWBMatch = true;
                            consinfo[j].weight = (Convert.ToDecimal(consinfo[j].weight) + Convert.ToDecimal(FFMConsig[i].weight)).ToString();
                            consinfo[j].pcscnt = (Convert.ToDecimal(consinfo[j].pcscnt) + Convert.ToDecimal(FFMConsig[i].pcscnt)).ToString();
                            //consinfo[j].numshp = (Convert.ToDecimal(consinfo[j].numshp) + Convert.ToDecimal(FFMConsig[i].numshp)).ToString();

                        }
                    }
                    if (!AWBMatch)
                    {
                        Array.Resize(ref consinfo, consinfo.Length + 1);
                        Array.Copy(FFMConsig, i, consinfo, consinfo.Length - 1, 1);
                    }
                }
            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
            }
        }

        private async Task<DataSet?> GenertateAWBDimensions(string AWBNumber, int AWBPieces, DataSet Dimensions, decimal AWBWt, string UserName, DateTime TimeStamp, bool IsCreate, string AWBPrefix)
        {
            //SQLServer da = new SQLServer();
            DataSet? ds = null;
            try
            {
                System.Text.StringBuilder strDimensions = new System.Text.StringBuilder();

                if (Dimensions != null && Dimensions.Tables.Count > 0 && Dimensions.Tables[0].Rows.Count > 0)
                {
                    for (int intCount = 0; intCount < Dimensions.Tables[0].Rows.Count; intCount++)
                    {
                        strDimensions.Append("Insert into #tblPieceInfo(PieceNo, IdentificationNo, Length, Breath, Height, Vol, Wt, Units, PieceType, BagNo, ULDNo, Location, FlightNo, FlightDate) values (");
                        strDimensions.Append(Dimensions.Tables[0].Rows[intCount]["PieceNo"]);
                        strDimensions.Append(",'");
                        strDimensions.Append(Dimensions.Tables[0].Rows[intCount]["IdentificationNo"]);
                        strDimensions.Append("',");
                        strDimensions.Append(Dimensions.Tables[0].Rows[intCount]["Length"]);
                        strDimensions.Append(",");
                        strDimensions.Append(Dimensions.Tables[0].Rows[intCount]["Breadth"]);
                        strDimensions.Append(",");
                        strDimensions.Append(Dimensions.Tables[0].Rows[intCount]["Height"]);
                        strDimensions.Append(",'");
                        strDimensions.Append(Dimensions.Tables[0].Rows[intCount]["Vol"] == DBNull.Value ? 0 : Dimensions.Tables[0].Rows[intCount]["Vol"]);
                        strDimensions.Append("',");
                        strDimensions.Append(Dimensions.Tables[0].Rows[intCount]["Wt"]);
                        strDimensions.Append(",'");
                        strDimensions.Append(Dimensions.Tables[0].Rows[intCount]["Units"]);
                        strDimensions.Append("','");
                        strDimensions.Append(Dimensions.Tables[0].Rows[intCount]["PieceType"]);
                        strDimensions.Append("','");
                        strDimensions.Append(Dimensions.Tables[0].Rows[intCount]["BagNo"]);
                        strDimensions.Append("','");
                        strDimensions.Append(Dimensions.Tables[0].Rows[intCount]["ULDNo"]);
                        strDimensions.Append("','");
                        strDimensions.Append(Dimensions.Tables[0].Rows[intCount]["Location"]);
                        strDimensions.Append("','");
                        strDimensions.Append(Dimensions.Tables[0].Rows[intCount]["FlightNo"]);
                        strDimensions.Append("','");
                        strDimensions.Append(Dimensions.Tables[0].Rows[intCount]["FlightDate"]);
                        strDimensions.Append("'); ");
                    }
                }

                //string[] PName = new string[] { "AWBNumber", "Pieces", "PieceInfo", "UserName", "TimeStamp", "IsCreate", "AWBWeight", "AWBPrefix" };
                //object[] PValue = new object[] { AWBNumber, AWBPieces, strDimensions.ToString(), UserName, TimeStamp, IsCreate, AWBWt, AWBPrefix };
                //SqlDbType[] PType = new SqlDbType[] { SqlDbType.VarChar, SqlDbType.Int, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.DateTime, SqlDbType.Bit, SqlDbType.Decimal, SqlDbType.VarChar };

                //ds = da.SelectRecords("sp_StoreCourierDetails", PName, PValue, PType);

                SqlParameter[] parameters =
                {
                    new SqlParameter("@AWBNumber", SqlDbType.VarChar) { Value = AWBNumber },
                    new SqlParameter("@Pieces", SqlDbType.Int) { Value = AWBPieces },
                    new SqlParameter("@PieceInfo", SqlDbType.VarChar) { Value = strDimensions.ToString() },
                    new SqlParameter("@UserName", SqlDbType.VarChar) { Value = UserName },
                    new SqlParameter("@TimeStamp", SqlDbType.DateTime) { Value = TimeStamp },
                    new SqlParameter("@IsCreate", SqlDbType.Bit) { Value = IsCreate },
                    new SqlParameter("@AWBWeight", SqlDbType.Decimal) { Value = AWBWt },
                    new SqlParameter("@AWBPrefix", SqlDbType.VarChar) { Value = AWBPrefix }
                };

                ds = await _readWriteDao.SelectRecords("sp_StoreCourierDetails", parameters);

                //PName = null;
                //PValue = null;
                //PType = null;
            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                ds = null;
            }
            //finally
            //{
            //    da = null;
            //}
            return ds;
        }
        #endregion Private Methods

        #region :: Public Methods ::
        public async Task GenerateAutoFBLMessage()
        {
            try
            {
                //SQLServer db = new SQLServer(); ;
                DataSet? ds = null;
                bool flag = false;
                do
                {
                    flag = false;

                    //ds = db.SelectRecords("Messaging.uspGetFlightsForFBL");

                    ds = await _readWriteDao.SelectRecords("Messaging.uspGetFlightsForFBL");
                    if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                    {
                        flag = true;
                        for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                        {
                            DataRow dr = ds.Tables[0].Rows[i];
                            //FBRMessageProcessor Fbr = new FBRMessageProcessor();
                            await GenerateFBLMessage(dr["Source"].ToString(), dr["Dest"].ToString(), dr["FlightID"].ToString(), dr["Date"].ToString());
                        }


                    }
                } while (flag);
                //db = null;
                //GC.Collect();
            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
            }
        }

        //public bool SaveandUpdagteFBLMessageinDatabase(ref MessageData.fblinfo fbldata, ref MessageData.unloadingport[] unloadingport, ref MessageData.dimensionnfo[] objDimension, ref MessageData.ULDinfo[] uld, ref MessageData.otherserviceinfo[] othinfoarray, ref MessageData.consignmentorigininfo[] consigmnentOrigin, ref MessageData.consignmnetinfo[] consinfo, int RefNo, string strMessage, string strmessageFrom, string strFromID, string strStatus, out string ErrorMsg)
        public async Task<(
            bool success,
            MessageData.fblinfo fbldata,
            MessageData.unloadingport[] unloadingport,
            MessageData.dimensionnfo[] objDimension,
            MessageData.ULDinfo[] uld,
            MessageData.otherserviceinfo[] othinfoarray,
            MessageData.consignmentorigininfo[] consigmnentOrigin,
            MessageData.consignmnetinfo[] consinfo,
            string ErrorMsg)>
            SaveandUpdagteFBLMessageinDatabase(
            MessageData.fblinfo fbldata,
            MessageData.unloadingport[] unloadingport,
            MessageData.dimensionnfo[] objDimension,
            MessageData.ULDinfo[] uld,
            MessageData.otherserviceinfo[] othinfoarray,
            MessageData.consignmentorigininfo[] consigmnentOrigin,
            MessageData.consignmnetinfo[] consinfo,
            int RefNo, string strMessage, string strmessageFrom, string strFromID, string strStatus,
            string ErrorMsg)
        {
            bool flag = false;
            try
            {

                ErrorMsg = string.Empty;

                //SQLServer dtb = new SQLServer();
                //GenericFunction gf = new GenericFunction();

                string flightnum = fbldata.carriercode + fbldata.fltnum;
                DateTime flightdate = new DateTime();
                DateTime date = new DateTime();
                flightdate = DateTime.Parse(DateTime.Parse("1." + fbldata.month + " 2008").Month.ToString().PadLeft(2, '0') + "/" + fbldata.date.PadLeft(2, '0') + "/" + System.DateTime.Today.Year);
                date = DateTime.Parse(DateTime.Parse("1." + fbldata.month + " 2008").Month.ToString().PadLeft(2, '0') + "/" + fbldata.date.PadLeft(2, '0') + "/" + System.DateTime.Today.Year);
                string AWBOriginAirportCode = string.Empty, AWBDestAirportCode = string.Empty;

                string source = string.Empty, dest = string.Empty;

                //string[] PName = new string[] { "flightnum", "date" };
                //SqlDbType[] PType = new SqlDbType[] { SqlDbType.NVarChar, SqlDbType.VarChar };
                //object[] PValue = new object[] { flightnum, flightdate };

                //DataSet ds = dtb.SelectRecords("spGetDestCodeForFFM", PName, PValue, PType);


                SqlParameter[] parameters =
                {
                    new SqlParameter("@flightnum", SqlDbType.NVarChar) { Value = flightnum },
                    new SqlParameter("@date", SqlDbType.VarChar) { Value = flightdate }
                };

                DataSet? ds = await _readWriteDao.SelectRecords("spGetDestCodeForFFM", parameters);

                if (ds != null)
                {
                    if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                    {
                        source = ds.Tables[0].Rows[0]["source"].ToString();
                        dest = ds.Tables[0].Rows[0]["Dest"].ToString();

                    }
                }
                if (source.Length < 1)
                {
                    source = fbldata.fltairportcode;
                }




                await _genericFunction.UpdateInboxFromMessageParameter(RefNo, string.Empty, flightnum, source, dest, "FBL", strmessageFrom == "" ? strFromID : strmessageFrom, flightdate);

                #region Reprocess the Consigment Info--commented for GHA logic
                for (int k = 0; k < unloadingport.Length; k++)
                {
                    dest = unloadingport[k].unloadingairport;
                    //Reprocess ConsigmentInfo
                    MessageData.consignmnetinfo[] FFMConsig = new MessageData.consignmnetinfo[consinfo.Length];
                    Array.Copy(consinfo, FFMConsig, consinfo.Length);
                    consinfo = new MessageData.consignmnetinfo[0];
                    ReProcessConsigment(ref FFMConsig, ref consinfo);
                }
                #endregion

                //priyanka:-check pcs,wt,volume not for 0.

                //FNAMessageProcessor FNA = new FNAMessageProcessor();

                int invalidAWBS = 0;
                string strErrorMessage = string.Empty, AWBPcszero = string.Empty, AllAWBS = string.Empty, AWBWtzero = string.Empty, AWBVolzero = string.Empty;
                if (consinfo.Length > 0)
                {
                    for (int i = 0; i < consinfo.Length; i++)
                    {
                        bool bookawb = true;
                        if (consinfo[i].pcscnt == "" || consinfo[i].pcscnt == "0")
                        {
                            AWBPcszero = AWBPcszero.Length > 0 ? (AWBPcszero + consinfo[i].airlineprefix + "-" + consinfo[i].awbnum + (i == (consinfo.Length - 1) ? "" : ",")) : "AWB PCS should be greater than zero for AWBs:-" + consinfo[i].airlineprefix + "-" + consinfo[i].awbnum + (i == (consinfo.Length - 1) ? "" : ",");
                            strErrorMessage = string.Empty;
                            AllAWBS = AllAWBS + consinfo[i].airlineprefix + "-" + consinfo[i].awbnum + ",";
                            bookawb = false;
                            invalidAWBS++;
                            await _fNAMessageProcessor.GenerateFNAMessage(strMessage, strErrorMessage, consinfo[i].airlineprefix, consinfo[i].awbnum, strFromID);

                        }
                        if (consinfo[i].weight == "" || consinfo[i].weight == "0")
                        {
                            AWBWtzero = AWBWtzero.Length > 0 ? (AWBWtzero + consinfo[i].airlineprefix + "-" + consinfo[i].awbnum + (i == (consinfo.Length - 1) ? "" : ",")) : "AWB Weight should be greater than zero for AWBs:-" + consinfo[i].airlineprefix + "-" + consinfo[i].awbnum + (i == (consinfo.Length - 1) ? "" : ",");
                            strErrorMessage = string.Empty;
                            bookawb = false;
                            invalidAWBS++;
                            AllAWBS = AllAWBS + consinfo[i].airlineprefix + "-" + consinfo[i].awbnum + ",";
                            await _fNAMessageProcessor.GenerateFNAMessage(strMessage, strErrorMessage, consinfo[i].airlineprefix, consinfo[i].awbnum, strFromID);
                        }
                        if (consinfo[i].volumecode != "" && (consinfo[i].volumeamt == "" || decimal.Parse(consinfo[i].volumeamt) == 0))
                        {

                            AWBVolzero = AWBVolzero.Length > 0 ? (AWBVolzero + consinfo[i].airlineprefix + "-" + consinfo[i].awbnum + (i == (consinfo.Length - 1) ? "" : ",")) : "AWB Volume should be greater than zero for AWBs:-" + consinfo[i].airlineprefix + "-" + consinfo[i].awbnum + (i == (consinfo.Length - 1) ? "" : ",");
                            strErrorMessage = string.Empty;
                            bookawb = false;
                            AllAWBS = AllAWBS + consinfo[i].airlineprefix + "-" + consinfo[i].awbnum + ",";
                            invalidAWBS++;
                            await _fNAMessageProcessor.GenerateFNAMessage(strMessage, strErrorMessage, consinfo[i].airlineprefix, consinfo[i].awbnum, strFromID);

                        }

                        if (bookawb)
                        {
                            string AWBNum = consinfo[i].awbnum;
                            string AWBPrefix = consinfo[i].airlineprefix;

                            #region Check AWB is present or not
                            bool isAWBPresent = false;
                            DataSet? dsCheck = new DataSet();

                            //dtb = new SQLServer();
                            //string[] parametername = new string[] { "AWBNumber", "AWBPrefix" };
                            //object[] AWBvalues = new object[] { AWBNum, AWBPrefix };
                            //SqlDbType[] ptype = new SqlDbType[] { SqlDbType.VarChar, SqlDbType.VarChar };
                            //dsCheck = dtb.SelectRecords("sp_getawbdetails", parametername, AWBvalues, ptype);

                            SqlParameter[] parameters1 =
                            {
                                new SqlParameter("@AWBNumber", SqlDbType.VarChar) { Value = AWBNum },
                                new SqlParameter("@AWBPrefix", SqlDbType.VarChar) { Value = AWBPrefix }
                            };

                            dsCheck = await _readWriteDao.SelectRecords("sp_getawbdetails", parameters1);

                            if (dsCheck != null)
                            {
                                if (dsCheck.Tables.Count > 0)
                                {
                                    if (dsCheck.Tables[0].Rows.Count > 0)
                                    {
                                        if (dsCheck.Tables[0].Rows[0]["AWBNumber"].ToString().Equals(AWBNum, StringComparison.OrdinalIgnoreCase))
                                        {
                                            isAWBPresent = true;
                                        }
                                    }
                                }
                            }
                            #endregion Check AWB is present or not

                            #region Add AWB details

                            //string[] pcname = new string[] { "AWBnumber", "AWBPrefix", "RefNo", "MessageType", "AWBOrigin", "AWBDestination", "FlightOrigin", "FlightDest" };
                            //object[] pcvalues = new object[] { AWBNum, AWBPrefix, RefNo, "FBL", consinfo[i].origin, consinfo[i].dest, source, dest };
                            //SqlDbType[] pctypes = new SqlDbType[] { SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.Int, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar };
                            //DataSet dscheck = dtb.SelectRecords("spCheckStatusofAWB", pcname, pcvalues, pctypes);

                            SqlParameter[] parameters2 =
                           {
                                new SqlParameter("@AWBnumber", SqlDbType.VarChar) { Value = AWBNum },
                                new SqlParameter("@AWBPrefix", SqlDbType.VarChar) { Value = AWBPrefix },
                                new SqlParameter("@RefNo", SqlDbType.Int) { Value = RefNo },
                                new SqlParameter("@MessageType", SqlDbType.VarChar) { Value = "FBL" },
                                new SqlParameter("@AWBOrigin", SqlDbType.VarChar) { Value = consinfo[i].origin },
                                new SqlParameter("@AWBDestination", SqlDbType.VarChar) { Value = consinfo[i].dest },
                                new SqlParameter("@FlightOrigin", SqlDbType.VarChar) { Value = source },
                                new SqlParameter("@FlightDest", SqlDbType.VarChar) { Value = dest }
                            };

                            DataSet? dscheck = await _readWriteDao.SelectRecords("spCheckStatusofAWB", parameters2);

                            if (dscheck != null && dscheck.Tables != null && dscheck.Tables.Count > 0 && dscheck.Tables[0].Rows.Count > 0)
                            {
                                if (dscheck.Tables[0].Columns.Count == 1 && dscheck.Tables[0].Rows[0]["Status"].ToString().ToUpper() == "FALSE")
                                {
                                    continue;
                                }

                                if (dscheck.Tables[0].Rows[0]["Status"].ToString().ToUpper() == "TRUE" && dscheck.Tables[0].Rows[0]["AWBStatus"].ToString().ToUpper() != "B")
                                    continue;
                            }

                            if (dscheck != null && dscheck.Tables.Count > 0 && dscheck.Tables[1].Rows.Count > 0 && dscheck.Tables[1].Columns.Contains("AWBOriginAirportCode"))
                            {
                                AWBOriginAirportCode = dscheck.Tables[1].Rows[0]["AWBOriginAirportCode"].ToString();
                                AWBDestAirportCode = dscheck.Tables[1].Rows[0]["AWBDestAirportCode"].ToString();
                            }


                            //dtb = new SQLServer();

                            decimal VolumeWt = 0;
                            if (consinfo[i].volumecode != "" && Convert.ToDecimal(consinfo[i].volumeamt == "" ? "0" : consinfo[i].volumeamt) > 0)
                            {
                                switch (consinfo[i].volumecode.ToUpper())
                                {
                                    case "MC":
                                        VolumeWt = decimal.Parse(string.Format("{0:0.00}", Convert.ToDecimal(Convert.ToDecimal(consinfo[i].volumeamt == "" ? "0" : consinfo[i].volumeamt) * decimal.Parse("166.67"))));
                                        break;
                                    case "CI":
                                        VolumeWt =
                                            decimal.Parse(string.Format("{0:0.00}", Convert.ToDecimal((Convert.ToDecimal(consinfo[i].volumeamt == "" ? "0" : consinfo[i].volumeamt) /
                                                                                          decimal.Parse("366")))));
                                        break;
                                    case "CF":
                                        VolumeWt =
                                            decimal.Parse(string.Format("{0:0.00}",
                                                                        Convert.ToDecimal(Convert.ToDecimal(consinfo[i].volumeamt == "" ? "0" : consinfo[i].volumeamt) *
                                                                                          decimal.Parse("4.7194"))));
                                        break;
                                    case "CC":
                                        VolumeWt =
                                           decimal.Parse(string.Format("{0:0.00}",
                                                                       Convert.ToDecimal(((Convert.ToDecimal(consinfo[i].volumeamt == "" ? "0" : consinfo[i].volumeamt) /
                                                                                          decimal.Parse("6000"))))));
                                        break;
                                    default:
                                        VolumeWt = Convert.ToDecimal(Convert.ToDecimal(consinfo[i].volumeamt == "" ? "0" : consinfo[i].volumeamt));
                                        break;
                                }
                            }
                            decimal ChargeableWeight = 0;
                            if (VolumeWt > 0)
                            {
                                if (Convert.ToDecimal(consinfo[i].weight == "" ? "0" : consinfo[i].weight) > VolumeWt)
                                    ChargeableWeight = Convert.ToDecimal(consinfo[i].weight == "" ? "0" : consinfo[i].weight);
                                else
                                    ChargeableWeight = VolumeWt;
                            }
                            else
                                ChargeableWeight = Convert.ToDecimal(consinfo[i].weight == "" ? "0" : consinfo[i].weight);

                            //if origin is not present in FBL message for consignmennt, then take flt origin present in message as origin by priyanka
                            consinfo[i].origin = consinfo[i].origin.Length < 3 ? fbldata.fltairportcode : consinfo[i].origin;

                            //string[] paramname = new string[] { "AirlinePrefix", "AWBNum", "Origin", "Dest", "PcsCount", "Weight", "Volume", "ComodityCode", "ComodityDesc", "CarrierCode", "FlightNum", "FlightDate", "FlightOrigin", "FlightDest", "ShipperName", "ShipperAddr", "ShipperPlace", "ShipperState", "ShipperCountryCode", "ShipperContactNo", "ConsName", "ConsAddr", "ConsPlace", "ConsState", "ConsCountryCode", "ConsContactNo", "CustAccNo", "IATACargoAgentCode", "CustName", "SystemDate", "MeasureUnit", "Length", "Breadth", "Height", "PartnerStatus", "REFNo", "UpdatedBy", "ChargeableWeight" };

                            //object[] paramvalue = new object[] { consinfo[i].airlineprefix,consinfo[i].awbnum,consinfo[i].origin,consinfo[i].dest,consinfo[i].pcscnt,consinfo[i].weight,consinfo[i].volumeamt ,"",consinfo[i].manifestdesc , "", flightnum, flightdate, source,dest,"",
                            //                             "","","", "","", "","", "","","","","", "", "", DateTime.Now.ToString("yyyy-MM-dd"),"", "", "", "", "" ,RefNo,"FBL",ChargeableWeight};

                            //SqlDbType[] paramtype = new SqlDbType[] { SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.DateTime,
                            //                                  SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar,
                            //                                  SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.DateTime, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar,SqlDbType.Int,SqlDbType.VarChar,SqlDbType.Decimal };

                            //if (!dtb.InsertData(procedure, paramname, paramtype, paramvalue))
                            //    clsLog.WriteLogAzure("Error in FBL AWB Add Error for:" + consinfo[i].awbnum);

                            string procedure = "spInsertBookingDataFromFFR";
                            SqlParameter[] parameters3 =
                            {
                                new SqlParameter("@AirlinePrefix", SqlDbType.VarChar) { Value = consinfo[i].airlineprefix },
                                new SqlParameter("@AWBNum", SqlDbType.VarChar) { Value = consinfo[i].awbnum },
                                new SqlParameter("@Origin", SqlDbType.VarChar) { Value = consinfo[i].origin },
                                new SqlParameter("@Dest", SqlDbType.VarChar) { Value = consinfo[i].dest },
                                new SqlParameter("@PcsCount", SqlDbType.VarChar) { Value = consinfo[i].pcscnt },
                                new SqlParameter("@Weight", SqlDbType.VarChar) { Value = consinfo[i].weight },
                                new SqlParameter("@Volume", SqlDbType.VarChar) { Value = consinfo[i].volumeamt },
                                new SqlParameter("@ComodityCode", SqlDbType.VarChar) { Value = "" },
                                new SqlParameter("@ComodityDesc", SqlDbType.VarChar) { Value = consinfo[i].manifestdesc },
                                new SqlParameter("@CarrierCode", SqlDbType.VarChar) { Value = "" },
                                new SqlParameter("@FlightNum", SqlDbType.VarChar) { Value = flightnum },
                                new SqlParameter("@FlightDate", SqlDbType.DateTime) { Value = flightdate },
                                new SqlParameter("@FlightOrigin", SqlDbType.VarChar) { Value = source },
                                new SqlParameter("@FlightDest", SqlDbType.VarChar) { Value = dest },
                                new SqlParameter("@ShipperName", SqlDbType.VarChar) { Value = "" },
                                new SqlParameter("@ShipperAddr", SqlDbType.VarChar) { Value = "" },
                                new SqlParameter("@ShipperPlace", SqlDbType.VarChar) { Value = "" },
                                new SqlParameter("@ShipperState", SqlDbType.VarChar) { Value = "" },
                                new SqlParameter("@ShipperCountryCode", SqlDbType.VarChar) { Value = "" },
                                new SqlParameter("@ShipperContactNo", SqlDbType.VarChar) { Value = "" },
                                new SqlParameter("@ConsName", SqlDbType.VarChar) { Value = "" },
                                new SqlParameter("@ConsAddr", SqlDbType.VarChar) { Value = "" },
                                new SqlParameter("@ConsPlace", SqlDbType.VarChar) { Value = "" },
                                new SqlParameter("@ConsState", SqlDbType.VarChar) { Value = "" },
                                new SqlParameter("@ConsCountryCode", SqlDbType.VarChar) { Value = "" },
                                new SqlParameter("@ConsContactNo", SqlDbType.VarChar) { Value = "" },
                                new SqlParameter("@CustAccNo", SqlDbType.VarChar) { Value = "" },
                                new SqlParameter("@IATACargoAgentCode", SqlDbType.VarChar) { Value = "" },
                                new SqlParameter("@CustName", SqlDbType.VarChar) { Value = "" },
                                new SqlParameter("@SystemDate", SqlDbType.DateTime) { Value =DateTime.Now.ToString("yyyy-MM-dd") },
                                new SqlParameter("@MeasureUnit", SqlDbType.VarChar) { Value = "" },
                                new SqlParameter("@Length", SqlDbType.VarChar) { Value = "" },
                                new SqlParameter("@Breadth", SqlDbType.VarChar) { Value = "" },
                                new SqlParameter("@Height", SqlDbType.VarChar) { Value = "" },
                                new SqlParameter("@PartnerStatus", SqlDbType.VarChar) { Value = "" },
                                new SqlParameter("@REFNo", SqlDbType.Int) { Value = RefNo },
                                new SqlParameter("@UpdatedBy", SqlDbType.VarChar) { Value = "FBL" },
                                new SqlParameter("@ChargeableWeight", SqlDbType.Decimal) { Value = ChargeableWeight }
                            };

                            var dbRes = await _readWriteDao.ExecuteNonQueryAsync(procedure, parameters3);

                            if (!dbRes)
                            {
                                // clsLog.WriteLogAzure("Error in FBL AWB Add Error for:" + consinfo[i].awbnum);
                                _logger.LogWarning("Error in FBL AWB Add Error for:{0}", consinfo[i].awbnum);
                            }
                            int row = 0;
                            if (objDimension.Length > 0 || uld.Length > 0)
                            {
                                DataSet? dsDimension = await GenertateAWBDimensions(AWBNum, Convert.ToInt16(consinfo[i].pcscnt), null, Convert.ToDecimal(consinfo[i].weight), "MSG", System.DateTime.Now, false, AWBPrefix);

                                for (int j = 0; j < objDimension.Length; j++)
                                {
                                    if (objDimension[j].AWBPrefix == consinfo[i].airlineprefix && objDimension[j].AWBNumber == consinfo[i].awbnum)
                                    {
                                        if (objDimension[j].mesurunitcode.Trim() != "")
                                        {
                                            if (objDimension[j].mesurunitcode.Trim().ToUpper() == "CMT")
                                            {
                                                objDimension[j].mesurunitcode = "Cms";
                                            }
                                            else if (objDimension[j].mesurunitcode.Trim().ToUpper() == "INH")
                                            {
                                                objDimension[j].mesurunitcode = "Inches";
                                            }
                                        }
                                        if (objDimension[j].length.Trim() == "")
                                        {
                                            objDimension[j].length = "0";
                                        }
                                        if (objDimension[j].width.Trim() == "")
                                        {
                                            objDimension[j].width = "0";
                                        }
                                        if (objDimension[j].height.Trim() == "")
                                        {
                                            objDimension[j].height = "0";
                                        }


                                        if (dsDimension.Tables[0].Rows.Count == 0)
                                        {
                                            DataRow dr = dsDimension.Tables[0].NewRow();
                                            dsDimension.Tables[0].Rows.Add(dr);
                                        }
                                        dsDimension.Tables[0].Rows[j]["Length"] = Convert.ToInt16(objDimension[j].length);
                                        dsDimension.Tables[0].Rows[j]["Breadth"] = Convert.ToInt16(objDimension[j].width);
                                        dsDimension.Tables[0].Rows[j]["Height"] = Convert.ToInt16(objDimension[j].height);
                                        dsDimension.Tables[0].Rows[j]["Units"] = objDimension[j].mesurunitcode;
                                        dsDimension.Tables[0].Rows[j]["PieceNo"] = objDimension[j].piecenum;
                                        dsDimension.Tables[0].Rows[j]["Wt"] = objDimension[j].weight;
                                        row = row + Convert.ToInt16(objDimension[j].piecenum);

                                    }
                                }
                                row = 0;
                                if (uld.Length > 0)
                                {
                                    for (int u = 0; u < uld.Length; u++)
                                    {
                                        if (uld[u].AWBNumber == AWBNum && uld[u].AWBPrefix == AWBPrefix)
                                        {
                                            dsDimension.Tables[0].Rows[row]["ULDNo"] = uld[u].uldno;
                                            dsDimension.Tables[0].Rows[row]["PieceType"] = "ULD";
                                        }
                                    }
                                }
                                await GenertateAWBDimensions(AWBNum, Convert.ToInt16(consinfo[i].pcscnt), dsDimension, Convert.ToDecimal(consinfo[i].weight), "FBL", System.DateTime.Now, true, AWBPrefix);
                            }

                            #endregion

                            #region MakeAWBRoute through FBL Message
                            bool isRouteUpdate = false;

                            var updateRouteThroughFBL = ConfigCache.Get("UpdateRouteThroughFBL");
                            isRouteUpdate = Convert.ToBoolean(updateRouteThroughFBL == string.Empty ? "false" : updateRouteThroughFBL);

                            if ((isRouteUpdate && isAWBPresent) || !isAWBPresent)
                            {
                                //string[] paramnm = new string[] { "AWBNum", "AWBPrefix" };
                                //object[] paramobj = new object[] { AWBNum, AWBPrefix };
                                //SqlDbType[] paramtyp = new SqlDbType[] { SqlDbType.VarChar, SqlDbType.VarChar };
                                //if (dtb.ExecuteProcedure("spDeleteAWBRouteFFR", paramnm, paramtyp, paramobj))

                                SqlParameter[] parameters4 =
                                {
                                    new SqlParameter("@AWBNum", SqlDbType.VarChar) { Value = AWBNum },
                                    new SqlParameter("@AWBPrefix", SqlDbType.VarChar) { Value = AWBPrefix }
                                };

                                var dbRes1 = await _readWriteDao.ExecuteNonQueryAsync("spDeleteAWBRouteFFR", parameters4);
                                if (dbRes1)
                                {
                                    MessageData.FltRoute[] fltroute = new MessageData.FltRoute[0];
                                    MessageData.FltRoute flight = new MessageData.FltRoute("");
                                    if (!(AWBOriginAirportCode.ToUpper() == source && dest == AWBDestAirportCode.ToUpper()))
                                    {
                                        if (AWBOriginAirportCode.ToUpper() == source.ToUpper())
                                        {

                                            flight.carriercode = flightnum.Substring(0, 2);
                                            flight.fltnum = flightnum;
                                            flight.date = flightdate.ToString("MM/dd/yyyy");
                                            //flight.fltdept = consinfo[i].origin.ToUpper();
                                            flight.fltdept = AWBDestAirportCode;
                                            flight.fltarrival = dest;

                                            Array.Resize(ref fltroute, fltroute.Length + 1);
                                            fltroute[fltroute.Length - 1] = flight;

                                            if (dest.ToUpper() != AWBDestAirportCode.ToUpper())
                                            {
                                                flight.carriercode = string.Empty;
                                                flight.fltnum = string.Empty;
                                                flight.date = flightdate.ToString("MM/dd/yyyy");
                                                flight.fltdept = dest;
                                                flight.fltarrival = AWBDestAirportCode;
                                                Array.Resize(ref fltroute, fltroute.Length + 1);
                                                fltroute[fltroute.Length - 1] = flight;
                                            }
                                        }

                                        if (AWBOriginAirportCode.ToUpper() != source.ToUpper())
                                        {
                                            flight.carriercode = flightnum.Substring(0, 2);
                                            flight.fltnum = flightnum;
                                            flight.date = flightdate.ToString("MM/dd/yyyy");
                                            flight.fltdept = AWBOriginAirportCode;
                                            flight.fltarrival = source;
                                            Array.Resize(ref fltroute, fltroute.Length + 1);
                                            fltroute[fltroute.Length - 1] = flight;

                                            if (source.ToUpper() != AWBDestAirportCode.ToUpper())
                                            {
                                                flight.carriercode = flightnum.Substring(0, 2);
                                                flight.fltnum = flightnum;
                                                flight.date = flightdate.ToString("MM/dd/yyyy");
                                                flight.fltdept = source;
                                                flight.fltarrival = dest;
                                                Array.Resize(ref fltroute, fltroute.Length + 1);
                                                fltroute[fltroute.Length - 1] = flight;
                                            }

                                            if (dest.ToUpper() != AWBDestAirportCode.ToUpper())
                                            {
                                                flight.carriercode = string.Empty;
                                                flight.fltnum = string.Empty;
                                                flight.date = flightdate.ToString("MM/dd/yyyy");
                                                flight.fltdept = dest;
                                                flight.fltarrival = AWBDestAirportCode;
                                                Array.Resize(ref fltroute, fltroute.Length + 1);
                                                fltroute[fltroute.Length - 1] = flight;
                                            }
                                        }

                                    }
                                    else
                                    {
                                        if (AWBOriginAirportCode.ToUpper() == source.ToUpper() && dest.ToUpper() == AWBDestAirportCode.ToUpper())
                                        {
                                            flight.carriercode = flightnum.Substring(0, 2);
                                            flight.fltnum = flightnum;
                                            flight.date = flightdate.ToString("MM/dd/yyyy");
                                            flight.fltdept = AWBOriginAirportCode;
                                            flight.fltarrival = dest;
                                            Array.Resize(ref fltroute, fltroute.Length + 1);
                                            fltroute[fltroute.Length - 1] = flight;
                                        }
                                    }
                                    if (fltroute.Length > 0)
                                    {
                                        for (int route = 0; route < fltroute.Length; route++)
                                        {
                                            //dtb = new SQLServer();
                                            //string[] RName = new string[]
                                            //{
                                            //      "AWBNumber", "FltOrigin",  "FltDestination",  "FltNumber", "FltDate",  "Status", "UpdatedBy", "UpdatedOn",
                                            //      "IsFFR","REFNo", "date", "AWBPrefix"
                                            //};
                                            //SqlDbType[] RType = new SqlDbType[]
                                            //{
                                            //     SqlDbType.VarChar, SqlDbType.VarChar,  SqlDbType.VarChar, SqlDbType.VarChar,SqlDbType.DateTime, SqlDbType.VarChar, SqlDbType.VarChar,
                                            //     SqlDbType.DateTime,  SqlDbType.Bit, SqlDbType.Int,  SqlDbType.DateTime, SqlDbType.VarChar
                                            //};
                                            //object[] RValues = new object[]
                                            // {
                                            // AWBNum,fltroute[route].fltdept, fltroute[route].fltarrival,fltroute[route].fltnum, flightdate, "Q", "FBL",DateTime.Now,1, 0, date,AWBPrefix
                                            // };
                                            //if (!dtb.UpdateData("spSaveFFRAWBRoute", RName, RType, RValues))
                                            //    clsLog.WriteLogAzure("Error in Save AWB Route FBL " + dtb.LastErrorDescription);

                                            SqlParameter[] parameters5 =
                                            {
                                                new SqlParameter("@AWBNumber", SqlDbType.VarChar) { Value = AWBNum },
                                                new SqlParameter("@FltOrigin", SqlDbType.VarChar) { Value = fltroute[route].fltdept },
                                                new SqlParameter("@FltDestination", SqlDbType.VarChar) { Value = fltroute[route].fltarrival },
                                                new SqlParameter("@FltNumber", SqlDbType.VarChar) { Value = fltroute[route].fltnum },
                                                new SqlParameter("@FltDate", SqlDbType.DateTime) { Value = flightdate },
                                                new SqlParameter("@Status", SqlDbType.VarChar) { Value = "Q" },
                                                new SqlParameter("@UpdatedBy", SqlDbType.VarChar) { Value = "FBL" },
                                                new SqlParameter("@UpdatedOn", SqlDbType.DateTime) { Value = DateTime.Now },
                                                new SqlParameter("@IsFFR", SqlDbType.Bit) { Value = 1 },
                                                new SqlParameter("@REFNo", SqlDbType.Int) { Value = 0 },
                                                new SqlParameter("@date", SqlDbType.DateTime) { Value = date },
                                                new SqlParameter("@AWBPrefix", SqlDbType.VarChar) { Value = AWBPrefix }
                                            };

                                            var dbRes3 = await _readWriteDao.ExecuteNonQueryAsync("spSaveFFRAWBRoute", parameters5);
                                            if (!dbRes3)
                                            {
                                                // clsLog.WriteLogAzure("Error in Save AWB Route FBL");
                                                _logger.LogWarning("Error in Save AWB Route FBL");
                                            }

                                            #region Save AWBNo On Audit Log
                                            //string[] CNname = new string[] { "AWBPrefix", "AWBNumber", "Origin", "Destination", "Pieces", "Weight", "FlightNo", "FlightDate", "FlightOrigin", "FlightDestination", "Action", "Message", "Description", "UpdatedBy", "UpdatedOn", "Public" };
                                            //SqlDbType[] CType = new SqlDbType[] { SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.DateTime, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.Bit };
                                            //object[] CValues = new object[] { AWBPrefix, AWBNum, consinfo[i].origin, consinfo[i].dest, consinfo[i].pcscnt, consinfo[i].weight, flightnum, flightdate, fltroute[route].fltdept, fltroute[route].fltarrival, "Booked", "FBL", "AWB Flight Information", "FBL", DateTime.Today.ToString(), 1 };

                                            SqlParameter[] parameters6 =
                                            {
                                               new SqlParameter("@AWBPrefix", SqlDbType.VarChar) { Value = AWBPrefix },
                                               new SqlParameter("@AWBNumber", SqlDbType.VarChar) { Value = AWBNum },
                                               new SqlParameter("@Origin", SqlDbType.VarChar) { Value = consinfo[i].origin },
                                               new SqlParameter("@Destination", SqlDbType.VarChar) { Value = consinfo[i].dest },
                                               new SqlParameter("@Pieces", SqlDbType.VarChar) { Value = consinfo[i].pcscnt },
                                               new SqlParameter("@Weight", SqlDbType.VarChar) { Value = consinfo[i].weight },
                                               new SqlParameter("@FlightNo", SqlDbType.VarChar) { Value = flightnum },
                                               new SqlParameter("@FlightDate", SqlDbType.DateTime) { Value = flightdate },
                                               new SqlParameter("@FlightOrigin", SqlDbType.VarChar) { Value = fltroute[route].fltdept },
                                               new SqlParameter("@FlightDestination", SqlDbType.VarChar) { Value = fltroute[route].fltarrival },
                                               new SqlParameter("@Action", SqlDbType.VarChar) { Value = "Booked" },
                                               new SqlParameter("@Message", SqlDbType.VarChar) { Value = "FBL" },
                                               new SqlParameter("@Description", SqlDbType.VarChar) { Value = "AWB Flight Information" },
                                               new SqlParameter("@UpdatedBy", SqlDbType.VarChar) { Value = "FBL" },
                                               new SqlParameter("@UpdatedOn", SqlDbType.VarChar) { Value = DateTime.Today.ToString() },
                                               new SqlParameter("@Public", SqlDbType.Bit) { Value = 1 }
                                            };

                                            var dbRes4 = await _readWriteDao.ExecuteNonQueryAsync("SPAddAWBAuditLog", parameters6);
                                            //if (!dtb.ExecuteProcedure("SPAddAWBAuditLog", CNname, CType, CValues))

                                            if (!dbRes4)
                                            {
                                                // clsLog.WriteLog("AWB Audit log  for:" + AWBNum + Environment.NewLine);
                                                _logger.LogWarning("AWB Audit log  for:{0}", AWBNum + Environment.NewLine);
                                                //clsLog.WriteLog("AWB Audit log  for:" + AWBNum + Environment.NewLine + "Error: " + dtb.LastErrorDescription);
                                            }
                                            #endregion
                                        }
                                    }
                                }
                            }
                            #endregion
                        }
                    }

                    if (AWBVolzero.Length > 0 || AWBWtzero.Length > 0 || AWBPcszero.Length > 0)
                    {
                        //string[] QueryNames = { "ErrorMessage", "MessageSNo" };
                        //SqlDbType[] QueryTypes = { SqlDbType.VarChar, SqlDbType.Int };
                        //object[] QueryValues = { "AWB PCS/WT/Vol should be greater than zero for AWBs:-" + AllAWBS.TrimEnd(','), RefNo };
                        //dtb.UpdateData("spUpdateStatusformessage", QueryNames, QueryTypes, QueryValues);
                        ErrorMsg = "AWB PCS/WT/Vol should be greater than zero for AWBs:-" + AllAWBS.TrimEnd(',');
                    }
                    else
                    {
                        await _fNAMessageProcessor.GenerateFNAMessage(strMessage, "We will book AWBS shortly.", "", "", strFromID);

                    }
                }
            }
            catch (Exception ex)
            {
                //clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on SaveandUpdagteFBLMessageinDatabase");
                ErrorMsg = string.Empty;
                flag = false;

            }
            return (flag, fbldata, unloadingport, objDimension, uld, othinfoarray, consigmnentOrigin, consinfo, ErrorMsg);
        }

        public bool DecodeReceiveFBLMessage(string fblmsg, ref MessageData.fblinfo fbldata, ref MessageData.unloadingport[] unloadingport, ref MessageData.dimensionnfo[] dimensioinfo, ref MessageData.ULDinfo[] uld, ref MessageData.otherserviceinfo[] othinfo, ref MessageData.consignmentorigininfo[] consorginfo, ref MessageData.consignmnetinfo[] consinfo, ref MessageData.otherserviceinfo[] othinfoarray)
        {
            bool flag = false;
            try
            {
                string lastrec = "NA";
                string AWBPrefix = "", AWBNumber = "";
                try
                {
                    if (fblmsg.StartsWith("FBL", StringComparison.OrdinalIgnoreCase))
                    {

                        string[] str = fblmsg.Split('$');
                        if (str.Length > 3)
                        {
                            for (int i = 0; i < str.Length; i++)
                            {

                                flag = true;
                                #region Line 1
                                if (str[i].StartsWith("FBL", StringComparison.OrdinalIgnoreCase))
                                {
                                    try
                                    {
                                        string[] msg = str[i].Split('/');
                                        fbldata.fblversion = msg[1];
                                    }
                                    catch (Exception ex)
                                    {
                                        // clsLog.WriteLogAzure(ex);
                                        _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                    }
                                }
                                #endregion

                                #region line 2 flight data
                                if (i == 1)
                                {
                                    try
                                    {
                                        string[] msg = str[i].Split('/');
                                        if (msg.Length > 1)
                                        {
                                            fbldata.messagesequencenum = msg[0];
                                            fbldata.carriercode = msg[1].Substring(0, 2);
                                            fbldata.fltnum = msg[1].Substring(2);
                                            fbldata.date = msg[2].Substring(0, 2);
                                            fbldata.month = msg[2].Substring(2);
                                            fbldata.fltairportcode = msg[3];
                                            fbldata.aircraftregistration = msg[4];
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        // clsLog.WriteLogAzure(ex);
                                        _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                    }
                                }
                                #endregion

                                #region line 3 point of unloading
                                if (i >= 2)
                                {
                                    MessageData.unloadingport unloading = new MessageData.unloadingport("");
                                    if (str[i].Contains('/') && (!str[i].StartsWith("/")) && (!str[i].Contains(":")) && (!str[i].Contains("-")))
                                    {
                                        string[] msg = str[i].Split('/');
                                        if (msg.Length == 2)
                                        {
                                            if (msg[0].Length > 0 && !msg[0].StartsWith("/") && !msg[0].Contains('-')
                                                && !msg[0].Equals("SSR", StringComparison.OrdinalIgnoreCase)
                                                && !msg[0].Equals("SCI", StringComparison.OrdinalIgnoreCase)
                                                && !msg[0].Equals("OSI", StringComparison.OrdinalIgnoreCase)
                                                && !msg[0].Equals("ULD", StringComparison.OrdinalIgnoreCase)
                                                && !msg[0].Equals("COR", StringComparison.OrdinalIgnoreCase)
                                                 && !msg[0].Equals("OCI", StringComparison.OrdinalIgnoreCase))
                                            {
                                                unloading.unloadingairport = msg[0];
                                                unloading.nilcargocode = msg[1];
                                                Array.Resize(ref unloadingport, unloadingport.Length + 1);
                                                unloadingport[unloadingport.Length - 1] = unloading;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (str[i].Trim().Length == 3 && (!str[i].Contains(":")) && (!str[i].StartsWith("/")) && (!str[i].Contains("-")))
                                        {
                                            unloading.unloadingairport = str[i];
                                            Array.Resize(ref unloadingport, unloadingport.Length + 1);
                                            unloadingport[unloadingport.Length - 1] = unloading;
                                        }
                                    }
                                }
                                #endregion

                                #region  line 4 onwards check consignment details
                                if (i > 1)
                                {
                                    try
                                    {
                                        string[] msg = str[i].Split('/');
                                        //0th element
                                        if (msg[0].Contains('-'))
                                        {
                                            AWBPrefix = "";
                                            AWBNumber = "";
                                            lastrec = "";
                                            DecodeConsigmentDetails(str[i], ref consinfo, ref AWBPrefix, ref AWBNumber);
                                        }

                                    }
                                    catch (Exception ex)
                                    {
                                        // clsLog.WriteLogAzure(ex);
                                        _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                        continue;
                                    }
                                }
                                #endregion

                                #region Line 5 Dimendion info
                                if (str[i].StartsWith("DIM", StringComparison.OrdinalIgnoreCase))
                                {
                                    try
                                    {
                                        lastrec = "DIM";
                                        string[] msg = str[i].Split('/');
                                        int place = 0;
                                        MessageData.dimensionnfo dimension = new MessageData.dimensionnfo("");
                                        try
                                        {
                                            dimension.weightcode = msg[place + 1].Substring(0, 1);
                                            dimension.weight = msg[place + 1].Substring(1);
                                            if (msg.Length > 0)
                                            {
                                                string[] dimstr = msg[place + 2].Split('-');
                                                dimension.mesurunitcode = dimstr[0].Substring(0, 3);
                                                dimension.length = dimstr[0].Substring(3);
                                                dimension.width = dimstr[1];
                                                dimension.height = dimstr[2];
                                            }
                                            dimension.piecenum = msg[place + 3];
                                        }
                                        catch (Exception ex)
                                        {
                                            // clsLog.WriteLogAzure(ex);
                                            _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                        }
                                        dimension.AWBPrefix = AWBPrefix;
                                        dimension.AWBNumber = AWBNumber;
                                        Array.Resize(ref dimensioinfo, dimensioinfo.Length + 1);
                                        dimensioinfo[dimensioinfo.Length - 1] = dimension;


                                    }
                                    catch (Exception ex)
                                    {
                                        // clsLog.WriteLogAzure(ex); 
                                        _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                    }
                                }
                                #endregion

                                #region Line 7 ULD Specification
                                if (str[i].StartsWith("ULD", StringComparison.OrdinalIgnoreCase))
                                {
                                    try
                                    {
                                        lastrec = "ULD";
                                        int uldnum = 0;
                                        string[] msg = str[i].Split('/');
                                        if (msg.Length > 1)
                                        {
                                            fbldata.noofuld = msg[1];
                                            if (int.Parse(msg[1]) > 0)
                                            {
                                                Array.Resize(ref uld, uld.Length + 1 + int.Parse(msg[1]));
                                                for (int k = 2; k < msg.Length; k += 2)
                                                {
                                                    MessageData.ULDinfo ulddata = new MessageData.ULDinfo("");
                                                    string[] splitstr = msg[k].Split('-');
                                                    ulddata.uldno = splitstr[0];
                                                    ulddata.uldtype = splitstr[0].Substring(0, 3);
                                                    ulddata.uldsrno = splitstr[0].Substring(3, splitstr[0].Length - 6);
                                                    ulddata.uldowner = splitstr[0].Substring(splitstr[0].Length - 3, 3);
                                                    ulddata.uldloadingindicator = splitstr[1];
                                                    ulddata.uldweightcode = msg[k + 1].Substring(0, 1);
                                                    ulddata.uldweight = msg[k + 1].Substring(1);
                                                    ulddata.AWBPrefix = AWBPrefix;
                                                    ulddata.AWBNumber = AWBNumber;
                                                    uld[uldnum++] = ulddata;
                                                }
                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        // clsLog.WriteLogAzure(ex);
                                        _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                    }
                                }
                                #endregion

                                #region Line 8 Special Service request
                                if (str[i].StartsWith("SSR", StringComparison.OrdinalIgnoreCase))
                                {
                                    try
                                    {
                                        string[] msg = str[i].Split('/');
                                        lastrec = msg[0];
                                        if (msg[1].Length > 0)
                                        {
                                            fbldata.specialservicereq1 = msg[1];
                                        }

                                    }
                                    catch (Exception ex)
                                    {
                                        // clsLog.WriteLogAzure(ex);
                                        _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                    }
                                }
                                #endregion

                                #region Line 9 Other service info
                                if (str[i].StartsWith("OSI", StringComparison.OrdinalIgnoreCase))
                                {
                                    try
                                    {
                                        string[] msg = str[i].Split('/');
                                        lastrec = msg[0];
                                        if (msg[1].Length > 0)
                                        {
                                            Array.Resize(ref othinfoarray, othinfoarray.Length + 1);
                                            othinfoarray[othinfoarray.Length - 1].otherserviceinfo1 = msg[1];
                                            othinfoarray[othinfoarray.Length - 1].consigref = awbref;

                                        }

                                    }
                                    catch (Exception ex)
                                    {
                                        // clsLog.WriteLogAzure(ex); 
                                        _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                    }
                                }
                                #endregion

                                #region Last line
                                if (i > str.Length - 2)
                                {
                                    if (str[i].Trim().Length == 4 || str[i].Trim().Equals("LAST", StringComparison.OrdinalIgnoreCase) || str[i].Trim().Equals("CONT", StringComparison.OrdinalIgnoreCase))
                                    {
                                        fbldata.endmesgcode = str[i].Trim();
                                    }

                                }
                                #endregion

                                #region Other Info
                                if (str[i].StartsWith("/"))
                                {
                                    string[] msg = str[i].Split('/');
                                    try
                                    {
                                        #region line 6 consigment origin info
                                        if (msg.Length > 0 && msg[0].Length == 0 && lastrec == "NA")
                                        {
                                            MessageData.consignmentorigininfo consorg = new MessageData.consignmentorigininfo();
                                            try
                                            {
                                                consorg.abbrivatedname = msg[1];
                                                consorg.carriercode = msg[2].Length > 0 ? msg[2].Substring(0, 2) : "";
                                                consorg.flightnum = msg[2].Length > 0 ? msg[2].Substring(2) : "";
                                                consorg.day = msg[3].Length > 0 ? msg[3].Substring(0, 2) : "";
                                                consorg.month = msg[3].Length > 0 ? msg[3].Substring(2) : "";
                                                consorg.airportcode = msg[4];
                                                consorg.movementprioritycode = msg[5];
                                            }
                                            catch (Exception ex)
                                            {
                                                // clsLog.WriteLogAzure(ex);
                                                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                            }
                                            Array.Resize(ref consorginfo, consorginfo.Length + 1);
                                            consorginfo[consorginfo.Length - 1] = consorg;
                                        }
                                        #endregion

                                        #region SSR 2
                                        if (lastrec == "SSR")
                                        {
                                            fbldata.specialservicereq2 = msg[1].Length > 0 ? msg[1] : "";
                                            lastrec = "NA";
                                        }
                                        #endregion

                                        #region OSI 2
                                        if (lastrec == "OSI")
                                        {
                                            othinfoarray[othinfoarray.Length - 1].otherserviceinfo2 = msg[1].Length > 0 ? msg[1] : "";
                                            lastrec = "NA";
                                        }
                                        #endregion

                                        #region SSR 2
                                        if (lastrec == "DIM")
                                        {
                                            MessageData.dimensionnfo dimension = new MessageData.dimensionnfo("");
                                            try
                                            {
                                                lastrec = "DIM";
                                                int place = 0;

                                                dimension.weightcode = msg[place + 1].Substring(0, 1);
                                                dimension.weight = msg[place + 1].Substring(1);
                                                if (msg.Length > 0)
                                                {
                                                    string[] dimstr = msg[place + 2].Split('-');
                                                    dimension.mesurunitcode = dimstr[0].Substring(0, 3);
                                                    dimension.length = dimstr[0].Substring(3);
                                                    dimension.width = dimstr[1];
                                                    dimension.height = dimstr[2];
                                                }
                                                dimension.piecenum = msg[place + 3];
                                                dimension.AWBPrefix = AWBPrefix;
                                                dimension.AWBNumber = AWBNumber;


                                            }
                                            catch (Exception ex)
                                            {
                                                // clsLog.WriteLogAzure(ex); 
                                                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                            }
                                            Array.Resize(ref dimensioinfo, dimensioinfo.Length + 1);
                                            dimensioinfo[dimensioinfo.Length - 1] = dimension;
                                        }
                                        #endregion
                                    }
                                    catch (Exception ex)
                                    {
                                        // clsLog.WriteLogAzure(ex);
                                        _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                    }
                                }
                                #endregion
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    // clsLog.WriteLogAzure(ex);
                    _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                    flag = false;
                }
            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                flag = false;
            }
            return flag;
        }


        public async Task GenerateFBLMessage(string strFlightOrigin, string strFlightDestination, string FlightNo, string FlightDate, bool isAutoSendOnTriggerTime = false, string messageType = "FBL")
        {
            try
            {
                string SitaMessageHeader = string.Empty, FblMessageversion = string.Empty, Emailaddress = string.Empty, SFTPMessageHeader = string.Empty;

                //GenericFunction gf = new GenericFunction();
                MessageData.fblinfo objFBLInfo = new MessageData.fblinfo("");
                MessageData.unloadingport[] objUnloadingPort = new MessageData.unloadingport[0];
                MessageData.consignmnetinfo[] objConsInfo = new MessageData.consignmnetinfo[0];
                int count1 = 0;
                int count2 = 0;
                DataSet? dsData = await GetRecordforGenerateFBLMessage(strFlightOrigin, strFlightDestination, FlightNo, FlightDate);
                if (dsData != null && dsData.Tables.Count > 1 && dsData.Tables[0].Rows.Count > 0)
                {
                    DataSet dsmessage = await _genericFunction.GetSitaAddressandMessageVersionForAutoMessage(FlightNo.Substring(0, 2), messageType, "AIR", strFlightOrigin, strFlightDestination, FlightNo, string.Empty, string.Empty, string.Empty, isAutoSendOnTriggerTime: isAutoSendOnTriggerTime);
                    if (dsmessage != null && dsmessage.Tables[0].Rows.Count > 0)
                    {
                        Emailaddress = dsmessage.Tables[0].Rows[0]["PartnerEmailiD"].ToString();
                        string MessageCommunicationType = dsmessage.Tables[0].Rows[0]["MsgCommType"].ToString();
                        FblMessageversion = dsmessage.Tables[0].Rows[0]["MessageVersion"].ToString();
                        if (dsmessage.Tables[0].Rows[0]["PatnerSitaID"].ToString().Length > 0)
                            SitaMessageHeader = _genericFunction.MakeMailMessageFormat(dsmessage.Tables[0].Rows[0]["PatnerSitaID"].ToString(), dsmessage.Tables[0].Rows[0]["OriginSenderAddress"].ToString(), dsmessage.Tables[0].Rows[0]["MessageID"].ToString());
                        if (dsmessage.Tables[0].Rows[0]["SFTPHeaderSITAddress"].ToString().Length > 0)
                            if (dsmessage.Tables[0].Rows[0]["SFTPHeaderSITAddress"].ToString().ToUpper() != "WITHOUT SFTP HEADER")
                                SFTPMessageHeader = _genericFunction.MakeMailMessageFormat(dsmessage.Tables[0].Rows[0]["SFTPHeaderSITAddress"].ToString(), dsmessage.Tables[0].Rows[0]["OriginSenderAddress"].ToString(), dsmessage.Tables[0].Rows[0]["MessageID"].ToString(), dsmessage.Tables[0].Rows[0]["SFTPHeaderType"].ToString());
                            else
                                SFTPMessageHeader = dsmessage.Tables[0].Rows[0]["SFTPHeaderSITAddress"].ToString().ToUpper();
                    }
                    if (Emailaddress.Trim() != string.Empty || SitaMessageHeader.Trim() != string.Empty || SFTPMessageHeader.Trim() != string.Empty)
                    {
                        DateTime dtFlight = DateTime.Now;
                        objFBLInfo.date = FlightDate.Substring(3, 2);
                        try
                        {
                            dtFlight = DateTime.ParseExact(FlightDate, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);//, "dd/MM/yyyy", null);
                            objFBLInfo.date = DateTime.ParseExact(FlightDate, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture).Day.ToString().PadLeft(2, '0');
                        }
                        catch (Exception ex)
                        {
                            // clsLog.WriteLogAzure(ex.Message);
                            _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                        }

                        objFBLInfo.month = dtFlight.ToString("MMM").ToUpper();
                        objFBLInfo.fltairportcode = strFlightOrigin;
                        objFBLInfo.endmesgcode = "LAST";
                        objFBLInfo.fblversion = FblMessageversion;
                        objFBLInfo.messagesequencenum = "1";
                        //flight details
                        if (dsData.Tables[0].Rows.Count > 0)
                        {
                            count1 = 1;
                            foreach (DataRow dr in dsData.Tables[0].Rows)
                            {
                                MessageData.unloadingport objTempUnloadingPort = new MessageData.unloadingport("");
                                objTempUnloadingPort.unloadingairport = dr[2].ToString().ToUpper();
                                Array.Resize(ref objUnloadingPort, count1);
                                objUnloadingPort[count1 - 1] = objTempUnloadingPort;
                                objFBLInfo.carriercode = dsData.Tables[0].Rows[0]["FlightID"].ToString().Substring(0, 2);
                                objFBLInfo.fltnum = dsData.Tables[0].Rows[0]["FlightID"].ToString().Substring(2);
                                count1++;
                                if (dsData.Tables[1].Rows.Count > 0)
                                {
                                    bool isNil = true;
                                    foreach (DataRow drAWB in dsData.Tables[1].Rows)
                                    {
                                        if (objTempUnloadingPort.unloadingairport.Trim() == drAWB["DestinationCode"].ToString().Trim())
                                        {
                                            isNil = false;
                                            break;
                                        }
                                    }
                                    if (isNil)
                                    {
                                        objUnloadingPort[count1 - 2].nilcargocode = "NIL";
                                    }
                                }
                                else
                                {
                                    objUnloadingPort[count1 - 2].nilcargocode = "NIL";
                                }
                            }
                        }
                        //awb details
                        if (dsData.Tables[1].Rows.Count > 0)
                        {
                            objFBLInfo.carriercode = dsData.Tables[1].Rows[0]["CarrierCode"].ToString();
                            objFBLInfo.fltnum = dsData.Tables[1].Rows[0]["FlightNo"].ToString();

                            count2 = 1;
                            foreach (DataRow dr in dsData.Tables[1].Rows)
                            {
                                MessageData.consignmnetinfo objTempConsInfo = new MessageData.consignmnetinfo("");
                                string AWBNumber = dr[0].ToString().Trim();
                                objTempConsInfo.airlineprefix = dr["Prefix"].ToString().Trim();//Prefix
                                objTempConsInfo.awbnum = dr["AWBNumber"].ToString().Trim();
                                objTempConsInfo.origin = dr["OrginCode"].ToString().Trim().ToUpper();
                                objTempConsInfo.dest = dr["DestinationCode"].ToString().Trim().ToUpper();
                                if (dr["Piececode"].ToString() == "P")
                                {
                                    objTempConsInfo.consigntype = dr["Piececode"].ToString();
                                    objTempConsInfo.pcscnt = dr["FlightPcs"].ToString();
                                    objTempConsInfo.weightcode = dr["UOM"].ToString().Trim();
                                    objTempConsInfo.weight = dr["AWBGwt"].ToString();

                                    objTempConsInfo.TotalConsignmentType = "T";
                                    objTempConsInfo.AWBPieces = dr["AWBPcs"].ToString();
                                }
                                else
                                {
                                    objTempConsInfo.consigntype = dr["Piececode"].ToString();
                                    objTempConsInfo.pcscnt = dr["FlightPcs"].ToString();
                                    objTempConsInfo.weightcode = dr["UOM"].ToString().Trim();
                                    objTempConsInfo.weight = dr["AWBGwt"].ToString();
                                }

                                objTempConsInfo.volumecode = dr["VolumeCode"].ToString();
                                objTempConsInfo.volumeamt = dr["Volume"].ToString();

                                objTempConsInfo.manifestdesc = dr["CommDesc"].ToString().Trim().ToUpper();
                                objTempConsInfo.splhandling = dr["SHCCodes"].ToString().Trim().ToUpper();

                                Array.Resize(ref objConsInfo, count2);
                                objConsInfo[count2 - 1] = objTempConsInfo;
                                count2++;
                            }
                        }
                        if (count1 > 0)
                        {
                            MessageData.dimensionnfo[] objDimenInfo = new MessageData.dimensionnfo[0];
                            MessageData.consignmentorigininfo[] objConsOriginInfo = new MessageData.consignmentorigininfo[0];
                            MessageData.ULDinfo[] objULDInfo = new MessageData.ULDinfo[0];
                            MessageData.otherserviceinfo objOtherInfo = new MessageData.otherserviceinfo("");

                            //Cls_BL cls_BL = new Cls_BL();

                            string FBLMsg = cls_Encode_Decode.EncodeFBLforsend(objFBLInfo, objUnloadingPort, objConsInfo, objDimenInfo, objConsOriginInfo, objULDInfo, objOtherInfo);
                            if (FBLMsg != null)
                            {
                                if (FBLMsg.Trim() != "")
                                {
                                    //Multipart FBL
                                    if (FBLMsg.Contains("#"))
                                    {
                                        string[] MulitpartFBL = FBLMsg.Split('#');

                                        foreach (string FBLMessage in MulitpartFBL)
                                        {
                                            if (SitaMessageHeader != "")
                                                await _genericFunction.SaveMessageOutBox(messageType, SitaMessageHeader + "\r\n" + FBLMessage, "SITAFTP", "SITAFTP", strFlightOrigin, strFlightDestination, FlightNo, FlightDate, String.Empty, "Auto", messageType);
                                            if (Emailaddress != "")
                                                await _genericFunction.SaveMessageOutBox(messageType, FBLMessage, string.Empty, Emailaddress, strFlightOrigin, strFlightDestination, FlightNo, FlightDate, String.Empty, "Auto", messageType);
                                            if (SFTPMessageHeader.Trim().Length > 0)
                                                await _genericFunction.SaveMessageOutBox(messageType, SFTPMessageHeader + "\r\n" + FBLMessage, "SFTP", "SFTP", strFlightOrigin, strFlightDestination, FlightNo, FlightDate, String.Empty, "Auto", messageType);
                                        }
                                    }
                                    else
                                    {
                                        if (SitaMessageHeader != "")
                                            await _genericFunction.SaveMessageOutBox(messageType, SitaMessageHeader + "\r\n" + FBLMsg, "SITAFTP", "SITAFTP", strFlightOrigin, strFlightDestination, FlightNo, FlightDate, String.Empty, "Auto", messageType);
                                        if (Emailaddress != "")
                                            await _genericFunction.SaveMessageOutBox(messageType, FBLMsg, string.Empty, Emailaddress, strFlightOrigin, strFlightDestination, FlightNo, FlightDate, String.Empty, "Auto", messageType);
                                        if (SFTPMessageHeader.Trim().Length > 0)
                                            if (SFTPMessageHeader.Trim() == "WITHOUT SFTP HEADER")
                                                await _genericFunction.SaveMessageOutBox(messageType, FBLMsg, "SFTP", "SFTP", strFlightOrigin, strFlightDestination, FlightNo, FlightDate, String.Empty, "Auto", messageType);
                                            else
                                                await _genericFunction.SaveMessageOutBox(messageType, SFTPMessageHeader + "\r\n" + FBLMsg, "SFTP", "SFTP", strFlightOrigin, strFlightDestination, FlightNo, FlightDate, String.Empty, "Auto", messageType);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
            }
        }

        private async Task<DataSet?> GetRecordforGenerateFBLMessage(string strFlightOrigin, string strFlightDestination, string FlightNo, string FlightDate)
        {
            DataSet? dsData = new DataSet();
            try
            {
                //SQLServer dtb = new SQLServer(true);
                string procedure = "spGetFBLDataForSend";

                //string[] paramname = new string[] { "FlightNo", "FlightOrigin", "FlightDestination", "FltDate" };

                //object[] paramvalue = new object[] { FlightNo, strFlightOrigin, strFlightDestination, FlightDate };

                //SqlDbType[] paramtype = new SqlDbType[] { SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar };
                //dsData = dtb.SelectRecords(procedure, paramname, paramvalue, paramtype);

                SqlParameter[] parameters =
                {
                    new SqlParameter("@FlightNo", SqlDbType.VarChar) { Value = FlightNo },
                    new SqlParameter("@FlightOrigin", SqlDbType.VarChar) { Value = strFlightOrigin },
                    new SqlParameter("@FlightDestination", SqlDbType.VarChar) { Value = strFlightDestination },
                    new SqlParameter("@FltDate", SqlDbType.VarChar) { Value = FlightDate }
                };

                dsData = await _readWriteDao.SelectRecords(procedure, parameters);
            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
            }

            return dsData;
        }
        #endregion Public Methods
    }
}
