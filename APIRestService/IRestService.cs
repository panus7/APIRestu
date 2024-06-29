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
         
    }


}
