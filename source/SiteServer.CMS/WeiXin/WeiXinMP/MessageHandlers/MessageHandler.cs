﻿using System;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using SiteServer.CMS.WeiXin.WeiXinMP.Context;
using SiteServer.CMS.WeiXin.WeiXinMP.Entities.Request;
using SiteServer.CMS.WeiXin.WeiXinMP.Entities.Response;
using SiteServer.CMS.WeiXin.WeiXinMP.Exceptions;
using SiteServer.CMS.WeiXin.WeiXinMP.Helpers;

namespace SiteServer.CMS.WeiXin.WeiXinMP.MessageHandlers
{
    public interface IMessageHandler
    {
        /// <summary>
        /// 发送者用户名（OpenId）
        /// </summary>
        string WeixinOpenId { get; }

        /// <summary>
        /// 取消执行Execute()方法。一般在OnExecuting()中用于临时阻止执行Execute()。
        /// 默认为False。
        /// 如果在执行OnExecuting()执行前设为True，则所有OnExecuting()、Execute()、OnExecuted()代码都不会被执行。
        /// 如果在执行OnExecuting()执行过程中设为True，则后续Execute()及OnExecuted()代码不会被执行。
        /// 建议在设为True的时候，给ResponseMessage赋值，以返回友好信息。
        /// </summary>
        bool CancelExcute { get; set; }

        /// <summary>
        /// 在构造函数中转换得到原始XML数据
        /// </summary>
        XDocument RequestDocument { get; set; }

        /// <summary>
        /// 根据ResponseMessageBase获得转换后的ResponseDocument
        /// 注意：这里每次请求都会根据当前的ResponseMessageBase生成一次，如需重用此数据，建议使用缓存或局部变量
        /// </summary>
        XDocument ResponseDocument { get; }

        /// <summary>
        /// 请求实体
        /// </summary>
        IRequestMessageBase RequestMessage { get; set; }
        /// <summary>
        /// 响应实体
        /// 只有当执行Execute()方法后才可能有值
        /// </summary>
        IResponseMessageBase ResponseMessage { get; set; }

        /// <summary>
        /// 是否使用了MessageAgent代理
        /// </summary>
        bool UsedMessageAgent { get; set; }
    }

    /// <summary>
    /// 微信请求的集中处理方法
    /// 此方法中所有过程，都基于Senparc.Weixin.MP的基础功能，只为简化代码而设。
    /// </summary>
    public abstract class MessageHandler<TC> : IMessageHandler where TC : class, IMessageContext, new()
    {
        /// <summary>
        /// 上下文
        /// </summary>
        public static WeixinContext<TC> GlobalWeixinContext = new WeixinContext<TC>();

        /// <summary>
        /// 全局消息上下文
        /// </summary>
        public WeixinContext<TC> WeixinContext
        {
            get { return GlobalWeixinContext; }
        }

        /// <summary>
        /// 当前用户消息上下文
        /// </summary>
        public TC CurrentMessageContext
        {
            get { return WeixinContext.GetMessageContext(RequestMessage); }
        }

        /// <summary>
        /// 发送者用户名（OpenId）
        /// </summary>
        public string WeixinOpenId
        {
            get
            {
                if (RequestMessage != null)
                {
                    return RequestMessage.FromUserName;
                }
                return null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [Obsolete("UserName属性从v0.6起已过期，建议使用WeixinOpenId")]
        public string UserName
        {
            get
            {
                return WeixinOpenId;
            }
        }

        /// <summary>
        /// 取消执行Execute()方法。一般在OnExecuting()中用于临时阻止执行Execute()。
        /// 默认为False。
        /// 如果在执行OnExecuting()执行前设为True，则所有OnExecuting()、Execute()、OnExecuted()代码都不会被执行。
        /// 如果在执行OnExecuting()执行过程中设为True，则后续Execute()及OnExecuted()代码不会被执行。
        /// 建议在设为True的时候，给ResponseMessage赋值，以返回友好信息。
        /// </summary>
        public bool CancelExcute { get; set; }

        /// <summary>
        /// 在构造函数中转换得到原始XML数据
        /// </summary>
        public XDocument RequestDocument { get; set; }

        /// <summary>
        /// 根据ResponseMessageBase获得转换后的ResponseDocument
        /// 注意：这里每次请求都会根据当前的ResponseMessageBase生成一次，如需重用此数据，建议使用缓存或局部变量
        /// </summary>
        public XDocument ResponseDocument
        {
            get
            {
                if (ResponseMessage == null)
                {
                    return null;
                }
                return EntityHelper.ConvertEntityToXml(ResponseMessage as ResponseMessageBase);
            }
        }

        //protected Stream InputStream { get; set; }
        /// <summary>
        /// 请求实体
        /// </summary>
        public IRequestMessageBase RequestMessage { get; set; }
        /// <summary>
        /// 响应实体
        /// 正常情况下只有当执行Execute()方法后才可能有值。
        /// 也可以结合Cancel，提前给ResponseMessage赋值。
        /// </summary>
        public IResponseMessageBase ResponseMessage { get; set; }

        /// <summary>
        /// 是否使用了MessageAgent代理
        /// </summary>
        public bool UsedMessageAgent { get; set; }

        public MessageHandler(Stream inputStream, int maxRecordCount = 0)
        {
            WeixinContext.MaxRecordCount = maxRecordCount;
            using (var xr = XmlReader.Create(inputStream))
            {
                RequestDocument = XDocument.Load(xr);
                Init(RequestDocument);
            }
        }

        public MessageHandler(XDocument requestDocument, int maxRecordCount = 0)
        {
            WeixinContext.MaxRecordCount = maxRecordCount;
            Init(requestDocument);
        }

        private void Init(XDocument requestDocument)
        {
            RequestDocument = requestDocument;
            RequestMessage = RequestMessageFactory.GetRequestEntity(RequestDocument);

            //记录上下文
            if (WeixinContextGlobal.UseWeixinContext)
            {
                WeixinContext.InsertMessage(RequestMessage);
            }
        }

        /// <summary>
        /// 根据当前的RequestMessage创建指定类型的ResponseMessage
        /// </summary>
        /// <typeparam name="TR">基于ResponseMessageBase的响应消息类型</typeparam>
        /// <returns></returns>
        public TR CreateResponseMessage<TR>() where TR : ResponseMessageBase
        {
            if (RequestMessage == null)
            {
                return null;
            }

            return RequestMessage.CreateResponseMessage<TR>();
        }

        /// <summary>
        /// 执行微信请求
        /// </summary>
        public void Execute()
        {
            if (CancelExcute)
            {
                return;
            }

            OnExecuting();

            if (CancelExcute)
            {
                return;
            }

            try
            {
                if (RequestMessage == null)
                {
                    return;
                }

                switch (RequestMessage.MsgType)
                {
                    case RequestMsgType.Text:
                        ResponseMessage = OnTextRequest(RequestMessage as RequestMessageText);
                        break;
                    case RequestMsgType.Location:
                        ResponseMessage = OnLocationRequest(RequestMessage as RequestMessageLocation);
                        break;
                    case RequestMsgType.Image:
                        ResponseMessage = OnImageRequest(RequestMessage as RequestMessageImage);
                        break;
                    case RequestMsgType.Voice:
                        ResponseMessage = OnVoiceRequest(RequestMessage as RequestMessageVoice);
                        break;
                    case RequestMsgType.Video:
                        ResponseMessage = OnVideoRequest(RequestMessage as RequestMessageVideo);
                        break;
                    case RequestMsgType.Event:
                        ResponseMessage = OnEventRequest(RequestMessage as RequestMessageEventBase);
                        break;
                    default:
                        throw new UnknownRequestMsgTypeException("未知的MsgType请求类型", null);
                }

                //记录上下文
                if (WeixinContextGlobal.UseWeixinContext && ResponseMessage != null)
                {
                    WeixinContext.InsertMessage(ResponseMessage);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                OnExecuted();
            }
        }

        public virtual void OnExecuting()
        {
        }

        public virtual void OnExecuted()
        {
        }

        /// <summary>
        /// 默认返回消息（当任何OnXX消息没有被重写，都将自动返回此默认消息）
        /// </summary>
        public abstract IResponseMessageBase DefaultResponseMessage(IRequestMessageBase requestMessage);
        //{
        //    例如可以这样实现：
        //    var responseMessage = this.CreateResponseMessage<ResponseMessageText>();
        //    responseMessage.Content = "您发送的消息类型暂未被识别。";
        //    return responseMessage;
        //}

        /// <summary>
        /// 文字类型请求
        /// </summary>
        public virtual IResponseMessageBase OnTextRequest(RequestMessageText requestMessage)
        {
            return DefaultResponseMessage(requestMessage);
        }

        /// <summary>
        /// 位置类型请求
        /// </summary>
        public virtual IResponseMessageBase OnLocationRequest(RequestMessageLocation requestMessage)
        {
            return DefaultResponseMessage(requestMessage);
        }

        /// <summary>
        /// 图片类型请求
        /// </summary>
        public virtual IResponseMessageBase OnImageRequest(RequestMessageImage requestMessage)
        {
            return DefaultResponseMessage(requestMessage);
        }

        /// <summary>
        /// 语音类型请求
        /// </summary>
        public virtual IResponseMessageBase OnVoiceRequest(RequestMessageVoice requestMessage)
        {
            return DefaultResponseMessage(requestMessage);
        }


        /// <summary>
        /// 视频类型请求
        /// </summary>
        public virtual IResponseMessageBase OnVideoRequest(RequestMessageVideo requestMessage)
        {
            return DefaultResponseMessage(requestMessage);
        }


        /// <summary>
        /// 链接消息类型请求
        /// </summary>
        public virtual IResponseMessageBase OnLinkRequest(RequestMessageLink requestMessage)
        {
            return DefaultResponseMessage(requestMessage);
        }

        /// <summary>
        /// Event事件类型请求
        /// </summary>
        public virtual IResponseMessageBase OnEventRequest(RequestMessageEventBase requestMessage)
        {
            var strongRequestMessage = RequestMessage as IRequestMessageEventBase;
            IResponseMessageBase responseMessage = null;
            switch (strongRequestMessage.Event)
            {
                case Event.ENTER:
                    responseMessage = OnEvent_EnterRequest(RequestMessage as RequestMessageEvent_Enter);
                    break;
                case Event.LOCATION:
                    responseMessage = OnEvent_LocationRequest(RequestMessage as RequestMessageEvent_Location);//目前实际无效
                    break;
                case Event.subscribe://订阅
                    responseMessage = OnEvent_SubscribeRequest(RequestMessage as RequestMessageEvent_Subscribe);
                    break;
                case Event.unsubscribe://退订
                    responseMessage = OnEvent_UnsubscribeRequest(RequestMessage as RequestMessageEvent_Unsubscribe);
                    break;
                case Event.CLICK://菜单点击
                    responseMessage = OnEvent_ClickRequest(RequestMessage as RequestMessageEvent_Click);
                    break;
                case Event.scan://二维码
                    responseMessage = OnEvent_ScanRequest(RequestMessage as RequestMessageEvent_Scan);
                    break;
                default:
                    throw new UnknownRequestMsgTypeException("未知的Event下属请求信息", null);
            }
            return responseMessage;
        }

        #region Event 下属分类

        /// <summary>
        /// Event事件类型请求之ENTER
        /// </summary>
        public virtual IResponseMessageBase OnEvent_EnterRequest(RequestMessageEvent_Enter requestMessage)
        {
            return DefaultResponseMessage(requestMessage);
        }

        /// <summary>
        /// Event事件类型请求之LOCATION
        /// </summary>
        public virtual IResponseMessageBase OnEvent_LocationRequest(RequestMessageEvent_Location requestMessage)
        {
            return DefaultResponseMessage(requestMessage);
        }

        /// <summary>
        /// Event事件类型请求之subscribe
        /// </summary>
        public virtual IResponseMessageBase OnEvent_SubscribeRequest(RequestMessageEvent_Subscribe requestMessage)
        {
            return DefaultResponseMessage(requestMessage);
        }

        /// <summary>
        /// Event事件类型请求之unsubscribe
        /// </summary>
        public virtual IResponseMessageBase OnEvent_UnsubscribeRequest(RequestMessageEvent_Unsubscribe requestMessage)
        {
            return DefaultResponseMessage(requestMessage);
        }

        /// <summary>
        /// Event事件类型请求之CLICK
        /// </summary>
        public virtual IResponseMessageBase OnEvent_ClickRequest(RequestMessageEvent_Click requestMessage)
        {
            return DefaultResponseMessage(requestMessage);
        }

        /// <summary>
        /// Event事件类型请求之scan
        /// </summary>
        public virtual IResponseMessageBase OnEvent_ScanRequest(RequestMessageEvent_Scan requestMessage)
        {
            return DefaultResponseMessage(requestMessage);
        }

        #endregion
    }
}
