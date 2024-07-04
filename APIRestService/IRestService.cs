 using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

namespace APIRestServiceRestaurant
{ 
    [ServiceContract]
    public interface IRestService
    {
        [OperationContract]
        void DoWork();
       
        [OperationContract]
        [WebInvoke(Method = "POST",
           RequestFormat = WebMessageFormat.Json,
           ResponseFormat = WebMessageFormat.Json,
           BodyStyle = WebMessageBodyStyle.WrappedRequest,
           UriTemplate = "EnquireMasterData")]
        Result_MasterDataEnquiry EnquireMasterData(Param_MasterDataEnquiry param);
         
        [OperationContract]
        [WebInvoke(Method = "POST",
           RequestFormat = WebMessageFormat.Json,
           ResponseFormat = WebMessageFormat.Json,
           BodyStyle = WebMessageBodyStyle.WrappedRequest,
           UriTemplate = "UpdateMasterData")]
        Result_Update_MasterDataDetail UpdateMasterData(Update_MasterDataDetail param);

        [OperationContract]
        [WebInvoke(Method = "POST",
          RequestFormat = WebMessageFormat.Json,
          ResponseFormat = WebMessageFormat.Json,
          BodyStyle = WebMessageBodyStyle.WrappedRequest,
          UriTemplate = "DeleteMasterData")]
        Result_Update_MasterDataDetail DeleteMasterData(Update_MasterDataDetail param);
         
        [OperationContract]
        [WebInvoke(Method = "POST",
           RequestFormat = WebMessageFormat.Json,
           ResponseFormat = WebMessageFormat.Json,
           BodyStyle = WebMessageBodyStyle.WrappedRequest,
           UriTemplate = "AuthenUser")]
        AuthenUser_Result AuthenUser(AuthenUser_Param param);
         
        [OperationContract]
        [WebInvoke(Method = "POST",
           RequestFormat = WebMessageFormat.Json,
           ResponseFormat = WebMessageFormat.Json,
           BodyStyle = WebMessageBodyStyle.WrappedRequest,
           UriTemplate = "EnquireMenu")]
        EnquireMenu_Result EnquireMenu(EnquireMenu_Param param);

        [OperationContract]
        [WebInvoke(Method = "POST",
           RequestFormat = WebMessageFormat.Json,
           ResponseFormat = WebMessageFormat.Json,
           BodyStyle = WebMessageBodyStyle.WrappedRequest,
           UriTemplate = "UpdateNewOrder")]
        UpdateOrder_Result UpdateNewOrder(Data_ORDER_HEAD_Param param);

        [OperationContract]
        [WebInvoke(Method = "POST",
           RequestFormat = WebMessageFormat.Json,
           ResponseFormat = WebMessageFormat.Json,
           BodyStyle = WebMessageBodyStyle.WrappedRequest,
           UriTemplate = "UpdateCookAckOrder")]
        UpdateOrder_Result UpdateCookAckOrder(Data_ORDER_HEAD_Param param);

        [OperationContract]
        [WebInvoke(Method = "POST",
           RequestFormat = WebMessageFormat.Json,
           ResponseFormat = WebMessageFormat.Json,
           BodyStyle = WebMessageBodyStyle.WrappedRequest,
           UriTemplate = "UpdateFinishCookItemOrder")]
        UpdateOrder_Result UpdateFinishCookItemOrder(Data_ORDER_DETAIL_Param param);

        [OperationContract]
        [WebInvoke(Method = "POST",
           RequestFormat = WebMessageFormat.Json,
           ResponseFormat = WebMessageFormat.Json,
           BodyStyle = WebMessageBodyStyle.WrappedRequest,
           UriTemplate = "UpdateServeCookItemOrder")]
        UpdateOrder_Result UpdateServeCookItemOrder(Data_ORDER_DETAIL_Param param);
         
        [OperationContract]
        [WebInvoke(Method = "POST",
           RequestFormat = WebMessageFormat.Json,
           ResponseFormat = WebMessageFormat.Json,
           BodyStyle = WebMessageBodyStyle.WrappedRequest,
           UriTemplate = "UpdateCancelOrder")]
        UpdateOrder_Result UpdateCancelOrder(Data_ORDER_HEAD_Param param);

        [OperationContract]
        [WebInvoke(Method = "POST",
           RequestFormat = WebMessageFormat.Json,
           ResponseFormat = WebMessageFormat.Json,
           BodyStyle = WebMessageBodyStyle.WrappedRequest,
           UriTemplate = "UpdateCancelItemOrder")]
        UpdateOrder_Result UpdateCancelItemOrder(Data_ORDER_DETAIL_Param param);

        [OperationContract]
        [WebInvoke(Method = "POST",
           RequestFormat = WebMessageFormat.Json,
           ResponseFormat = WebMessageFormat.Json,
           BodyStyle = WebMessageBodyStyle.WrappedRequest,
           UriTemplate = "EnquireOrderSummary")]
        EnquireOrderSummary_Result EnquireOrderSummary(EnquireOrderSummary_Param param);

        [OperationContract]
        [WebInvoke(Method = "POST",
           RequestFormat = WebMessageFormat.Json,
           ResponseFormat = WebMessageFormat.Json,
           BodyStyle = WebMessageBodyStyle.WrappedRequest,
           UriTemplate = "EnquireOrderCooking")]
        EnquireOrderCooking_Result EnquireOrderCooking(EnquireOrderCooking_Param param);

        [OperationContract]
        [WebInvoke(Method = "POST",
           RequestFormat = WebMessageFormat.Json,
           ResponseFormat = WebMessageFormat.Json,
           BodyStyle = WebMessageBodyStyle.WrappedRequest,
           UriTemplate = "UpdatePayment")]
        UpdateOrder_Result UpdatePayment(Data_ORDER_HEAD_Param param);

    }



}
