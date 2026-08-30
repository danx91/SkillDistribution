using EFT.Communications;

namespace SkillDistribution.Helpers
{
    internal static class NotificationHelper
    {
        public static void ShowNotification(string message, ENotificationIconType notificationType = ENotificationIconType.Quest)
        {
            NotificationManager.DisplayNotification(new CustomNotification(
                message,
                ENotificationDurationType.Long,
                notificationType
            ));
        }
    }
}
