using System;
using System.Threading.Tasks;
using Quartz;
using AutoMapper;
using ClickersAPI.Controllers;
using ClickersAPI.DTO;
using FirebaseAdmin;

namespace ClickersAPI.Jobs
{
    public class SendFcmNotificationEveryWeekJob : IJob
    {
        public async Task Execute(IJobExecutionContext _Context)
        {
            var jobDataMap      = _Context.JobDetail.JobDataMap;
            var context         = (ApplicationDbContext) jobDataMap.Get("context");
            var mapper          = (IMapper) jobDataMap.Get("mapper");
            var serviceProvider = (IServiceProvider) jobDataMap.Get("service_provider");
            var firebase        = (FirebaseApp) jobDataMap.Get("firebase");
            var notification    = (NotificationDtoScheduled)jobDataMap.Get("notification");
            var pushNotificationsController = new PushNotificationsController(context, mapper, serviceProvider, firebase);
            await pushNotificationsController.SendNotification(notification);
        }
    }
}