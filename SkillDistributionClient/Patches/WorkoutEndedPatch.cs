using Comfort.Common;
using EFT;
using HarmonyLib;
using SkillDistribution.Features;
using SkillDistribution.Helpers;
using SPT.Reflection.Patching;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace SkillDistribution.Patches
{
    class WorkoutEndedPatch : ModulePatch
    {
        private static Task _lastSave = Task.CompletedTask;

        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(EftClientBackendSession), nameof(EftClientBackendSession.WorkoutEnded));
        }

        [PatchPrefix]
        private static bool Prefix(EftClientBackendSession __instance, IEnumerable<bool> results, string id, string aid, Callback callback)
        {
            WorkoutSession? series = WorkoutSession.Current;
            if (series == null || series.Workout._hideout._session != __instance ||
                series.Workout._qteData.Id != id || series.Workout._playerOwner.HideoutOwnerAccountId != aid)
            {
                return true;
            }

            SendRequest request = new()
            {
                Url = __instance._backendUrls.Main + "/skill-distribution/workout",
                Params = series.GetResult(id, results),
                Retries = SendRequest.DefaultRetries,
                HandlingMode = ERequestHandlingMode.AbortOnFail
            };

            _lastSave = SendWorkout(_lastSave, __instance, request, callback);
            return false;
        }

        private static async Task SendWorkout(Task previous, EftClientBackendSession session, SendRequest request, Callback callback)
        {
            IResult result;
            try
            {
                await previous;
                result = await session.SendVoid(request, null);
            }
            catch (Exception ex)
            {
                result = new FailedResult(ex.ToString());
            }

            if (result.Failed)
            {
                ReportFailure(result.Error);
            }
            try
            {
                callback?.Invoke(result);
            }
            catch (Exception ex)
            {
                Plugin.LogSource?.LogError($"Workout completion callback failed: {ex}");
            }
        }

        private static void ReportFailure(string error)
        {
            Plugin.LogSource?.LogError($"Failed to save workout: {error}");
            NotificationHelper.ShowNotification("Failed to save workout. See the game log for details.");
        }
    }
}
