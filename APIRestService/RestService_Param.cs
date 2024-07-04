 
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

namespace APIRestServiceRestaurant
{
    //public enum Enum_QuotationLogType : byte
    //{
    //    None = 0,
    //    RequestApprove = 1,
    //    CancelApprove = 2,
    //    RejectQuotation = 3,
    //    ApproveQuotation = 4,
    //    AcceptQuotation = 5, //แจ้งได้งาน
    //    ApproveWin = 6,      //Approve เริ่มงาน
    //    Working = 7,         //
    //    RequestFinishWorking = 8, //แจ้งเสร็๋จงาน
    //    ApprovalComplete = 9,    //Approve By Gm2        
    //    CreateInvoice = 10,
    //    ApprovalInvoice = 11,
    //    Received = 12,
    //    ApproveCompleteClosed = 13,
    //    ApproveLostClosed = 14,
    //    ApproveQuotationLv1 = 15,
    //    ViewQuotationPDF = 16,
    //    ViewOrEditQuotation = 17,
    //} 

    public static class TableName
    {
        public static string ORDER_HEAD = "ORDER_HEAD";
        public static string ORDER_HEAD_Field_OrderNo = "OrderNo";
        public static string ORDER_HEAD_Field_TableID = "TableID";
        public static string ORDER_HEAD_Field_OrderDateTime = "OrderDateTime";
        public static string ORDER_HEAD_Field_OrderByUserID = "OrderByUserID";
        public static string ORDER_HEAD_Field_CxlDateTime = "CxlDateTime";
        public static string ORDER_HEAD_Field_CxlUserID = "CxlUserID";
        public static string ORDER_HEAD_Field_CookAckDateTime = "CookAckDateTim";
        public static string ORDER_HEAD_Field_CookAckUserID = "CookAckUserID";
        public static string ORDER_HEAD_Field_ReceiveDateTime = "ReceiveDateTime";
        public static string ORDER_HEAD_Field_ReceiveUserID = "ReceiveUserID";

        public static string ORDER_DETAIL="ORDER_DETAIL";
        public static string ORDER_DETAIL_Field_OrderNo = "OrderNo";
        public static string ORDER_DETAIL_Field_Suffix = "Suffix";
        public static string ORDER_DETAIL_Field_MenuID = "MenuID";
        public static string ORDER_DETAIL_Field_MenuMemo = "MenuMemo";
        public static string ORDER_DETAIL_Field_CookAckDateTime = "CookAckDateTime";
        public static string ORDER_DETAIL_Field_CookAckUserID = "CookAckUserID";
        public static string ORDER_DETAIL_Field_CxlDateTime = "CxlDateTime";
        public static string ORDER_DETAIL_Field_CxlUserID = "CxlUserID";
        public static string ORDER_DETAIL_Field_ChargeAmt = "ChargeAmt";        
        public static string ORDER_DETAIL_Field_FinishCookDateTime = "FinishCookDateTime";
        public static string ORDER_DETAIL_Field_FinisCookUserID = "FinisCookUserID";
        public static string ORDER_DETAIL_Field_ServeCookDateTime = "ServeCookDateTime";
        public static string ORDER_DETAIL_Field_ServeCookUserID = "ServeCookUserID";

        public static string RECEIVE_HEAD = "RECEIVE_HEAD";
        public static string RECEIVE_HEAD_Field_RecevieNo = "RecevieNo";
        public static string RECEIVE_HEAD_Field_TableID = "TableID";
        public static string RECEIVE_HEAD_Field_CxlDateTime = "CxlDateTime";
        public static string RECEIVE_HEAD_Field_CxlUserID = "CxlUserID";
        public static string RECEIVE_HEAD_Field_ReceiveDateTime = "ReceiveDateTime";
        public static string RECEIVE_HEAD_Field_ReceiveUserID = "ReceiveUserID";
        public static string RECEIVE_HEAD_Field_ChargeAmt = "ChargeAmt";
        public static string RECEIVE_HEAD_Field_DiscountAmt = "DiscountAmt";
        public static string RECEIVE_HEAD_Field_TotalAmt = "TotalAmt";
         
        public static string RECEIVE_DETAIL = "RECEIVE_DETAIL";
        public static string RECEIVE_DETAIL_Field_RecevieNo = "RecevieNo";
        public static string RECEIVE_DETAIL_Field_Suffix = "Suffix";
        public static string RECEIVE_DETAIL_Field_OrderNo = "OrderNo";
        public static string RECEIVE_DETAIL_Field_ChargeAmt = "ChargeAmt";

    }

    public class Data_ORDER_HEAD_Param
    {
        public string OrderNo { get; set; }
        public string TableID { get; set; }
        public string OrderDateTime { get; set; }
        public string OrderByUserID { get; set; }
        public string CxlDateTime { get; set; }
        public string CxlUserID { get; set; }
        public string CookAckDateTime { get; set; }
        public string CookAckUserID { get; set; }
        public string ReceiveDateTime { get; set; }
        public string ReceiveUserID { get; set; }
        public string ChargeAmt { get; set; }
        public string TotalAmt { get; set; }
        public string DiscountAmt { get; set; }
        public List<Data_ORDER_DETAIL_Param> ListOfItem { get; set; }

        public Data_ORDER_HEAD_Param()
        {
            OrderNo = string.Empty;
            TableID = string.Empty;
            OrderDateTime = string.Empty;
            OrderByUserID = string.Empty;
            CxlDateTime = string.Empty;
            CxlUserID = string.Empty;
            CookAckDateTime = string.Empty;
            CookAckUserID = string.Empty;
            ReceiveDateTime = string.Empty;
            ReceiveUserID = string.Empty;
            ChargeAmt = string.Empty;
            TotalAmt = string.Empty;
            DiscountAmt = string.Empty;
            ListOfItem = new List<Data_ORDER_DETAIL_Param>();
        } 
    }

    public class Data_ORDER_DETAIL_Param
    {
        public string OrderNo { get; set; }
        public string MenuID { get; set; }
        public string MenuMemo { get; set; }
        public string CookAckDateTime { get; set; }
        public string CookAckUserID { get; set; }
        public string CxlDateTime { get; set; }
        public string CxlUserID { get; set; }
        public string ChargeAmt { get; set; }
        public string Suffix { get; set; }
        public string FinishCookDateTime { get; set; }
        public string FinisCookUserID { get; set; }
        public string ServeCookDateTime { get; set; }
        public string ServeCookUserID { get; set; }

        public Data_ORDER_DETAIL_Param()
        {
            OrderNo = string.Empty;
            MenuID = string.Empty;
            MenuMemo = string.Empty;
            CookAckDateTime = string.Empty;
            CookAckUserID = string.Empty;
            CxlDateTime = string.Empty;
            CxlUserID = string.Empty;
            ChargeAmt = string.Empty;
            Suffix = string.Empty;
            ///
            FinishCookDateTime = string.Empty;
            FinisCookUserID = string.Empty;
            ServeCookDateTime = string.Empty;
            ServeCookUserID = string.Empty;
        } 
    }
     
    public class UpdateOrder_Result
    {
        public string OrderNo { get; set; }
        public string ErrorMessage { get; set; }
        public bool ResultStatus { get; set; }

        public UpdateOrder_Result()
        {
            OrderNo = string.Empty;
            ErrorMessage = string.Empty;
            ResultStatus = false;
        }
    }


    public class EnquireOrderSummary_Param
    {
        public string TableID { get; set; }

        public EnquireOrderSummary_Param()
        {
            TableID = string.Empty;
        }
    }

    public class EnquireOrderSummary_Result
    { 
        public string ErrorMessage { get; set; }
        public bool ResultStatus { get; set; }
        public string TotalChargeAmt { get; set; }
        public string DiscountChargeAmt { get; set; }
        public string ChargeAmt { get; set; }
        public List<Data_ORDER_HEAD_Param> ListOfSummaryOrder { get; set; }

        public EnquireOrderSummary_Result()
        {
            TotalChargeAmt = string.Empty;
            DiscountChargeAmt = string.Empty;
            ChargeAmt = string.Empty;
            ErrorMessage = string.Empty;
            ResultStatus = false;
            ListOfSummaryOrder = new List<Data_ORDER_HEAD_Param>();
        }
    }


    public class EnquireOrderCooking_Param
    {
        public string TableID { get; set; }

        public EnquireOrderCooking_Param()
        {
            TableID = string.Empty;
        }
    }

    public class EnquireOrderCooking_Result
    {
        public string ErrorMessage { get; set; }
        public bool ResultStatus { get; set; }
        public List<Data_ORDER_DETAIL_Param> ListOfItemWaitAck { get; set; }
        public List<Data_ORDER_DETAIL_Param> ListOfItemAcked { get; set; }
        public List<Data_ORDER_DETAIL_Param> ListOfItemFinish { get; set; }

        public EnquireOrderCooking_Result()
        {
            ErrorMessage = string.Empty;
            ResultStatus = false;
            ListOfItemWaitAck = new List<Data_ORDER_DETAIL_Param>();
            ListOfItemAcked = new List<Data_ORDER_DETAIL_Param>();
            ListOfItemFinish = new List<Data_ORDER_DETAIL_Param>();
        }
    }


    


    public class Param_MasterDataEnquiry
    {
        public string ConnectKey { get; set; }
        public string MasterType { get; set; }
        public string MasterID { get; set; }
        public string MasterIDLike { get; set; }
        public string MasterNameThai { get; set; }
        public string MasterNameEnglish { get; set; }
        public string MasterData { get; set; }
    }

    public class Result_MasterDataEnquiry
    {
        public string ErrorMessage { get; set; }
        public bool Status { get; set; }
        public List<MasterDataDetail> ListOfData { get; set; }
    }

    public class MasterDataDetail
    {
        public string MasterType { get; set; }
        public string MasterID { get; set; }
        public string MasterNameThai { get; set; }
        public string MasterNameEnglish { get; set; }
        public string MasterData { get; set; }
        public string ImageData { get; set; }
        public string ItemImageData { get; set; }
    }

    public class Update_MasterDataDetail
    {
        public string MasterType { get; set; }
        public string MasterID { get; set; }
        public string MasterNameThai { get; set; }
        public string MasterNameEnglish { get; set; }
        public string MasterData { get; set; }
        public string ImageData { get; set; }
        public string ItemImageData { get; set; } //ProfileUser
    }

    public class Result_Update_MasterDataDetail
    {
        public string ErrorMessage { get; set; }
        public bool Status { get; set; }
    }
     
       
    public class MasterDataUser_Result
    {
        public string UserID { get; set; }
        public string UserName { get; set; }
        public string UserNameThai { get; set; }
        public string UserEmail { get; set; }
        public string UserIDType { get; set; }
        public byte[] ImageData { get; set; }

    }

    public class MasterData_UserInfo
    {
        public string UserID { get; set; }
        public string UserPassword { get; set; }
        public string UserIDType { get; set; }

        public MasterData_UserInfo()
        {
            UserID = string.Empty;
            UserPassword = string.Empty;
            UserIDType = string.Empty;
        }
    }

    public class MasterData_MenuInfo
    {
        public string MenuCode { get; set; }
        public string MenuNameShow { get; set; }
        public string MenuNameShowThai { get; set; }

        public string MenuType { get; set; }
        public string MenuDescrption { get; set; }
        public string MenuImage { get; set; }
        public string Price { get; set; }
        public string Cost { get; set; }
        public string StockIngredient1 { get; set; }
        public string StockIngredient2 { get; set; }
        public string StockIngredient3 { get; set; }
        public string StockIngredient4 { get; set; }
        public string StockIngredient5 { get; set; }

        public MasterData_MenuInfo()
        {
            MenuCode = string.Empty;
            MenuNameShow = string.Empty;
            MenuNameShowThai = string.Empty;
            MenuType = string.Empty;
            MenuDescrption = string.Empty;
            MenuImage = string.Empty;
            Price = string.Empty;
            Cost = string.Empty;
            StockIngredient1 = string.Empty;
            StockIngredient2 = string.Empty;
            StockIngredient3 = string.Empty;
            StockIngredient4 = string.Empty;
            StockIngredient5 = string.Empty;
        }
    }

    public class MasterData_StockInfo
    { 
        public string StockDescrption { get; set; } 
        public string Price { get; set; }
        public string Cost { get; set; } 

        public MasterData_StockInfo()
        {
            StockDescrption = string.Empty; 
            Price = string.Empty;
            Cost = string.Empty; 
        }
    }
     
    public class EnquireMenu_Param
    {
        [DataMember]
        public string ContextKey { get; set; }
        [DataMember]
        public string MenuType { get; set; }
        [DataMember]
        public string MasterID { get; set; }
        [DataMember]
        public string MenuName { get; set; } 

        public EnquireMenu_Param()
        {
            ContextKey = string.Empty;
            MasterID = string.Empty;
            MenuType = string.Empty;
            MenuName = string.Empty; 
        }
    }

    public class EnquireMenu_Result
    {
        [DataMember]
        public bool ResultStatus { get; set; }
        [DataMember]
        public string ErrorMessage { get; set; }
        [DataMember]
        public List<MasterData_MenuInfo> ListOfMasterData { get; set; }

        public EnquireMenu_Result()
        {
            ResultStatus = false;
            ErrorMessage = string.Empty;
            ListOfMasterData = new List<MasterData_MenuInfo>();
        }
    }



    [DataContract]
    public class AuthenUser_Result
    {
        [DataMember]
        public bool ResultStatus { get; set; }
        [DataMember]
        public string ErrorMessage { get; set; }
        [DataMember]
        public MasterData_UserInfo UserInfo { get; set; }

        public AuthenUser_Result()
        {
            ResultStatus = false;
            ErrorMessage = string.Empty;
            UserInfo = new MasterData_UserInfo();
        }
    }


    [DataContract]
    public class AuthenUser_Param
    {
        [DataMember]
        public string ContextKey { get; set; }
        [DataMember]
        public string UserID { get; set; }
        [DataMember]
        public string UserPassword { get; set; }

        public AuthenUser_Param()
        {
            ContextKey = string.Empty;
            UserID = string.Empty;
            UserPassword = string.Empty;
        }
    }



    public class MasterDataSystem
    {
        public string FormatNumberQuatation { get; set; } //RUNQTA
        public string FormatNumberInvoice { get; set; }  //RUNINV
        public string OfficerName { get; set; }
        public string OfficerAddress { get; set; }
        public string OfficerEmail { get; set; }
        public string OfficerTax { get; set; }
        public string OfficerTelePhone { get; set; }
        public string OfficerMobilePhone { get; set; }
        public string CreditDay { get; set; }
        public string BankAccount { get; set; }
        public string BankAccountName { get; set; }
        public string BankBrance { get; set; }
        public string BankName { get; set; }
    }
       


    public class PushMessage_Result
    {
        private bool x_ResultStatus;
        private string x_Message;

        public PushMessage_Result()
        {
            x_ResultStatus = false;
            x_Message = string.Empty;
        }
        public bool ResultStatus { get { return x_ResultStatus; } set { x_ResultStatus = value; } }
        public string Message { get { return x_Message; } set { x_Message = value; } }
    }

    public class PushMessage_Param
    {
        private string x_ContextKey;
        private string x_SenderName;
        private List<string> x_ListTokenIDClient;
        private string x_Title;
        private string x_TextMsg;
        private string x_TextInternalData;
        private string x_ImageBase64;

        public PushMessage_Param()
        {
            x_ContextKey = string.Empty;
            x_SenderName = string.Empty;
            x_ListTokenIDClient = new List<string>();
            x_Title = string.Empty;
            x_TextMsg = string.Empty;
            x_TextInternalData = string.Empty;
            x_ImageBase64 = string.Empty;
        }

        public string ContextKey { get { return x_ContextKey; } set { x_ContextKey = value; } }
        public string SenderName { get { return x_SenderName; } set { x_SenderName = value; } }
        public List<string> ListTokenIDClient { get { return x_ListTokenIDClient; } set { x_ListTokenIDClient = value; } }
        public string Title { get { return x_Title; } set { x_Title = value; } }
        public string TextMsg { get { return x_TextMsg; } set { x_TextMsg = value; } }
        public string TextInternalData { get { return x_TextInternalData; } set { x_TextInternalData = value; } }
        public string ImageBase64 { get { return x_ImageBase64; } set { x_ImageBase64 = value; } }

    }
     
      


}




