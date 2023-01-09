using System;
using System.Threading.Tasks;
using AutoMapper;
using ClickersAPI.DTO;
using ClickersAPI.Helpers;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Mvc;

namespace ClickersAPI.Controllers
{
    [ApiController]
    [Route("api/notifications")]
    public class PushNotificationsController : ControllerBaseImpl
    {
        private static FirebaseApp _firebaseApp;
        
        public PushNotificationsController(
            ApplicationDbContext _Context,
            IMapper              _Mapper,
            IServiceProvider     _Provider) 
            : base(_Context, _Mapper, _Provider) { }

        [HttpPost("init_firebase_admin_sdk")]
        public IActionResult InitFirebaseAdminSdk()
        {
            var credential = GoogleCredential.FromJson(Credentials.FirebaseAdmin);
            var appOptions = new AppOptions
            {
                Credential = credential, 
                ProjectId = "minigames-collection-299814"
            };
            _firebaseApp = FirebaseApp.Create(appOptions);
            const string message = "firebase admin sdk initialized";
            return Ok(new {message, DateTime.Now});
        }

        [HttpPost("send_notification")]
        public async Task<ActionResult<string>> SendNotification([FromBody] FirebaseNotificationDto _NotificationDto)
        {
            if (_firebaseApp == null)
                return Problem("firebase admin sdk was not initialized!");
            var messaging = FirebaseMessaging.GetMessaging(_firebaseApp);
            var androidNotification = new AndroidNotification
            {
                Title          = _NotificationDto.Title,
                Body           = _NotificationDto.Body,
                Icon           = _NotificationDto.SmallIcon,
                Priority       = NotificationPriority.LOW,
                ImageUrl       = _NotificationDto.LargeIcon,
                LocalOnly      = false,
                Sticky         = false,
                Tag            = null,
                Ticker         = _NotificationDto.Title,
                Visibility     = NotificationVisibility.SECRET,
            };
            if (_NotificationDto.TimeStampSpan.HasValue)
                androidNotification.EventTimestamp = DateTime.Now + _NotificationDto.TimeStampSpan.Value;
            var androidConfig = new AndroidConfig
            {
                Notification = androidNotification,
                Priority     = Priority.High,
                TimeToLive   = _NotificationDto.TimeToLive
            };
            var notification = new Notification
            {
                Title    = _NotificationDto.Title,
                Body     = _NotificationDto.Body,
                ImageUrl = _NotificationDto.LargeIcon
            };
            var message = new Message
            {
                Android      = androidConfig,
                Notification = notification,
                Topic        = _NotificationDto.Topic,
                Token        = _NotificationDto.Token,
                Condition    = _NotificationDto.Condition
            };
            string result;
            try
            {
                result = await messaging.SendAsync(message);
            }
            catch (Exception ex)
            {
                if (!(ex is FirebaseMessagingException firebaseMessagingException)) 
                    return Problem(ex.Message);
                string error =
                    firebaseMessagingException.Message 
                    + "; messaging error code: " + firebaseMessagingException.MessagingErrorCode
                    + "; error code: " + firebaseMessagingException.ErrorCode;
                return Problem(error);
            }
            return Ok(new {result, DateTime.Now});
        }
    }
}