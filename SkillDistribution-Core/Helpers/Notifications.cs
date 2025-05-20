using EFT.Communications;

namespace SkillDistribution.Helpers
{
    internal static class Notifications
    {
        public static void ShowNotification(string message, ENotificationIconType notificationType = ENotificationIconType.Quest)
        {
            NotificationManagerClass.DisplayNotification(new GClass2269(
                message,
                ENotificationDurationType.Long,
                notificationType
            ));
        }
    }
}
