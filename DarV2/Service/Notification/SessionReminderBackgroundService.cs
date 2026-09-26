using DarV2.Context;
using DarV2.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DarV2.Service.Notification
{
    public class SessionReminderBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<SessionReminderBackgroundService> _logger;

        public SessionReminderBackgroundService(IServiceScopeFactory scopeFactory, ILogger<SessionReminderBackgroundService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("SessionReminderBackgroundService started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CheckAndSendSessionRemindersAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred in SessionReminderBackgroundService.");
                }

                // Check every 1 minute
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }

        private async Task CheckAndSendSessionRemindersAsync()
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<DarContext>();

            var now = DateTime.Now;
            var today = DateOnly.FromDateTime(now);

            // Fetch today's sessions
            var sessions = await context.Sessions
                .Include(s => s.Group)
                .Where(s => s.SessionDate == today)
                .ToListAsync();

            if (!sessions.Any()) return;

            foreach (var session in sessions)
            {
                var sessionStartTime = session.SessionDate.ToDateTime(TimeOnly.FromTimeSpan(session.StartTime));
                var reminderTime = sessionStartTime.AddHours(-2);

                if (now >= reminderTime && now < sessionStartTime)
                {
                    var alreadySent = await context.Notifications.AnyAsync(n =>
                        n.SessionId == session.Id && n.Type == "SessionReminder");

                    if (alreadySent) continue;

                    var isSubstitute = !string.IsNullOrEmpty(session.SubstituteTeacherId);
                    var recipientId = isSubstitute
                        ? session.SubstituteTeacherId
                        : session.Group?.TeacherId;

                    if (string.IsNullOrEmpty(recipientId)) continue;

                    var groupName = session.Group?.Name ?? "الحلقة";

                    var startDateTime = DateTime.Today.Add(session.StartTime);
                    var startPeriod = startDateTime.Hour >= 12 ? "مساءً" : "صباحاً";
                    var startHour = startDateTime.Hour % 12 == 0 ? 12 : startDateTime.Hour % 12;
                    var timeStr = $"{startHour:D2}:{startDateTime.Minute:D2} {startPeriod}";

                    var title = "تذكير بموعد الحصة";
                    var message = isSubstitute
                        ? $"تذكير: لديك حصة كمعلم بديل لحلقة '{groupName}' اليوم في تمام الساعة {timeStr} (خلال ساعتين)."
                        : $"تذكير: لديك حصة قادمة لحلقة '{groupName}' اليوم في تمام الساعة {timeStr} (خلال ساعتين).";

                    var notifService = scope.ServiceProvider.GetRequiredService<INotificationService>();
                    await notifService.CreateNotificationAsync(
                        recipientId,
                        title,
                        message,
                        "SessionReminder",
                        session.Id,
                        session.GroupId
                    );

                    _logger.LogInformation("Sent 2-hour reminder for session {SessionId} to teacher {TeacherId}.", session.Id, recipientId);
                }
            }
        }
    }
}