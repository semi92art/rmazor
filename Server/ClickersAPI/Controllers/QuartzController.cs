using System;
using System.Threading.Tasks;
using AutoMapper;
using ClickersAPI.DTO;
using ClickersAPI.Jobs;
using FirebaseAdmin;
using Microsoft.AspNetCore.Mvc;
using Quartz;

namespace ClickersAPI.Controllers
{
    [ApiController]
    [Route("api/quartz")]
    public class QuartzController : ControllerBaseImpl
    {
        private IScheduler  Scheduler   { get; }
        private FirebaseApp FirebaseApp { get; }
        
        public QuartzController(
            ApplicationDbContext _Context,
            IMapper              _Mapper,
            IServiceProvider     _Provider,
            IScheduler           _Scheduler,
            FirebaseApp          _FirebaseApp) 
            : base(_Context, _Mapper, _Provider)
        {
            Scheduler   = _Scheduler;
            FirebaseApp = _FirebaseApp;
        }

        [HttpPost("schedule_notification_weekly")]
        public async Task<IActionResult> StartScheduleNotificationJob(
            [FromBody] NotificationDtoScheduled _NotificationDto)
        {
            var job = JobBuilder
                .Create<SendFcmNotificationEveryWeekJob>()
                .WithIdentity(_NotificationDto.IdentityName)
                .Build();
            job.JobDataMap.Put("context",          Context);
            job.JobDataMap.Put("mapper",           Mapper);
            job.JobDataMap.Put("service_provider", Provider);
            job.JobDataMap.Put("firebase",         FirebaseApp);
            job.JobDataMap.Put("notification",     _NotificationDto);
            var trigger = TriggerBuilder
                .Create()
                .WithIdentity("default")
                .StartNow()
                .WithSchedule(
                    CronScheduleBuilder.AtHourAndMinuteOnGivenDaysOfWeek(
                        18, 
                        0,
                        DayOfWeek.Sunday))
                .Build();
            await Scheduler.ScheduleJob(job, trigger);
            return Ok($"Schedule notification job started, identity: {_NotificationDto.IdentityName}.");
        }
    }
}