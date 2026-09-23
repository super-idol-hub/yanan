using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Yanan.Standalone
{
    internal static class ContractTests
    {
        public static int Run(string output)
        {
            var errors = new List<string>();
            Action<bool, string> check = delegate(bool ok, string name) { if (!ok) errors.Add(name); };
            int count = 0;
            foreach (int n in FrameResource.UsedCellsPerRow) count += n;
            check(count == 176 && FrameResource.Rows == 24, "24 rows / 176 frames");
            check(DragReactionContract.GetActivationThresholdPixels(2.25f) == 9, "225% drag threshold");
            check(DragReactionContract.GetActivationThresholdPixels(1.25f) == 8, "125% drag threshold");
            check(DragReactionContract.GetActivationThresholdPixels(4f) == 16, "400% drag threshold");
            check(DragReactionContract.DecideRelease(true, false, false, 2, 9) == DragReleaseAction.Idle, "click remains idle");
            check(DragReactionContract.DecideRelease(true, false, true, 2, 9) == DragReleaseAction.Wave, "double-click waves");
            check(DragReactionContract.DecideRelease(true, true, false, 30, 9) == DragReleaseAction.Angry, "drag recovers");
            check(DragReactionContract.DecideRelease(false, true, false, 30, 9) == DragReleaseAction.Idle, "capture loss remains idle");
            check(!DragReactionContract.AutomaticRoamingEnabled && !DragReactionContract.MovementRowsRuntimeEnabled, "no automatic roaming");
            check(MouseLookContract.GetOuterRadius(2f, false) == 440 && MouseLookContract.GetOuterRadius(2f, true) == 540, "gaze distance hysteresis");
            check(MouseLookContract.GetInnerRadius(2f, false) == 90 && MouseLookContract.GetInnerRadius(2f, true) == 68, "gaze dead-zone hysteresis");
            foreach (int n in new[] {4, 5, 6, 7, 8})
                check(AnimationSmoothing.GetDisplayStageCount(n) >= 24, "animation display stages " + n);
            check(PersistentActionContract.SittingEnterLastFrame == 3 && PersistentActionContract.SittingLoopLastFrame == 5 && PersistentActionContract.SittingExitLastFrame == 7, "phone frame contract");
            check(PersistentActionContract.SideRestSleepFrame == 4 && PersistentActionContract.SideRestWakeFirstFrame == 5 && PersistentActionContract.SideRestWakeLastFrame == 7, "sleep frame contract");
            check(MotionField.Parse(new byte[12], false) == null, "reject truncated motion data");
            check(MotionField.Parse(new byte[MotionField.EncodedBytes], false) == null, "reject invalid motion magic");
            string archiveError;
            check(!FrameResource.ValidateExternalArchive(new byte[] {1, 2, 3}, out archiveError), "reject invalid skin archive");
            check(!FrameResource.ValidateExternalArchive(null, out archiveError), "reject missing skin archive");
            check(SkinTransitionContract.LocksActionInput && !SkinTransitionContract.WholeCanvasCardFlip, "skin input lock and sliced transition");
            string full = Path.GetFullPath(output);
            Directory.CreateDirectory(Path.GetDirectoryName(full));
            check(StartupRegistration.RunSelfTest(Path.Combine(Path.GetDirectoryName(full), "startup-contract")), "startup persistence without registry writes");
            var text = new StringBuilder();
            text.Append("{\n  \"scope\": \"code-contracts-only\",\n  \"visualQaPassed\": false,\n  \"ok\": ").Append(errors.Count == 0 ? "true" : "false");
            text.Append(",\n  \"errors\": [");
            for (int i = 0; i < errors.Count; i++) { if (i > 0) text.Append(','); text.Append('"').Append(errors[i]).Append('"'); }
            text.Append("]\n}\n");
            File.WriteAllText(full, text.ToString(), new UTF8Encoding(false));
            return errors.Count == 0 ? 0 : 2;
        }
    }
}
