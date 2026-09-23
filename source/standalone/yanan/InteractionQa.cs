using System;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace Yanan.Standalone
{
    internal sealed partial class CharacterForm
    {
        private string _qaSnapshotName;

        private void QaTraceScale(string step)
        {
            string directory = Environment.GetEnvironmentVariable("YANAN_QA_OUTPUT");
            if (!_qaMode || string.IsNullOrEmpty(directory)) return;
            Directory.CreateDirectory(directory);
            File.AppendAllText(Path.Combine(directory, "scale-trace.txt"),
                step + " scale=" + _scale + " client=" + ClientSize + " bounds=" + Bounds
                + " max=" + MaximumSize + " screen=" + Screen.PrimaryScreen.WorkingArea + Environment.NewLine);
        }

        private void QaCapture(string name)
        {
            _qaSnapshotName = name;
            try { RenderCurrentFrame(); }
            finally { _qaSnapshotName = null; }
        }

        private void QaSaveRenderedFrame(Bitmap bitmap)
        {
            string directory = Environment.GetEnvironmentVariable("YANAN_QA_OUTPUT");
            if (!_qaMode || _qaSnapshotName == null || string.IsNullOrEmpty(directory)) return;
            directory = Path.Combine(directory, "rendered-frames");
            Directory.CreateDirectory(directory);
            bitmap.Save(Path.Combine(directory, _qaSnapshotName + ".png"), System.Drawing.Imaging.ImageFormat.Png);
        }

        private static void Require(bool condition, string label)
        {
            if (!condition) throw new InvalidOperationException("Interaction QA: " + label);
        }

        private void QaAdvanceUntil(Func<bool> predicate, string label)
        {
            for (int i = 0; i < 160 && !predicate(); i++) OnAnimationTick(this, EventArgs.Empty);
            Require(predicate(), label);
        }

        private void RunInteractionQa()
        {
            _animationTimer.Stop();
            _behaviorTimer.Stop();
            QaCapture("idle-noir");
            var down = new MouseEventArgs(MouseButtons.Left, 1, 40, 60, 0);
            OnMouseDown(down);
            OnMouseUp(down);
            Require(_state == CharacterState.Idle, "single click must remain idle");
            OnMouseDown(down);
            OnMouseDoubleClick(new MouseEventArgs(MouseButtons.Left, 2, 40, 60, 0));
            OnMouseUp(down);
            Require(_state == CharacterState.Waving, "double click must wave");
            QaAdvanceUntil(delegate { return _state == CharacterState.Idle; }, "wave returns idle");

            OnMouseDown(down);
            _dragDistance = GetDragActivationThresholdPixels();
            BeginDragReaction();
            QaAdvanceUntil(delegate { return _dragReactionPhase == DragReactionPhase.Held; }, "drag holds startled pose");
            Require(_stateFrame == 2 && _state == CharacterState.Jumping, "drag uses r04 c02");
            QaCapture("drag-held");
            FinishDrag(true);
            Require(_state == CharacterState.AngryStomp && _stateFrame == 2, "release uses r11 c02");
            QaCapture("drag-released");
            QaAdvanceUntil(delegate { return _state == CharacterState.Idle; }, "drag release returns idle");
            OnMouseDown(down);
            _dragDistance = 20;
            BeginDragReaction();
            FinishDrag(false);
            Require(_state == CharacterState.Idle, "capture loss must not trigger anger");
            foreach (int direction in new[] {0, 4, 8, 12})
            {
                _lookIndex = direction;
                QaCapture("gaze-" + direction);
            }
            _lookIndex = -1;

            StartAction(CharacterState.Sitting, 1);
            OnMouseDown(down);
            Require(!_dragging && _state == CharacterState.Sitting, "phone entry consumes input");
            QaAdvanceUntil(delegate { return _sittingPhoneHolding; }, "phone enters loop");
            QaCapture("phone-holding");
            StartAction(CharacterState.Waving, 1);
            Require(_state == CharacterState.Sitting, "menu cannot interrupt phone");
            for (int i = 0; i < 60; i++) OnAnimationTick(this, EventArgs.Empty);
            Require(_state == CharacterState.Sitting && _stateFrame >= 3 && _stateFrame <= 5, "phone persists");
            OnMouseDown(down);
            QaAdvanceUntil(delegate { return _state == CharacterState.Idle; }, "phone click exits to idle");

            StartAction(CharacterState.SideRest, 1);
            OnMouseDown(down);
            Require(!_dragging && _state == CharacterState.SideRest, "sleep entry consumes input");
            QaAdvanceUntil(delegate { return _sideRestSleeping; }, "sleep reaches hold");
            QaCapture("sleeping");
            StartAction(CharacterState.Waving, 1);
            Require(_state == CharacterState.SideRest && _stateFrame == 4, "menu cannot interrupt sleep");
            for (int i = 0; i < 20; i++) OnAnimationTick(this, EventArgs.Empty);
            Require(_sideRestSleeping, "sleep persists");
            OnMouseDown(down);
            QaCapture("waking");
            QaAdvanceUntil(delegate { return _state == CharacterState.Idle; }, "sleep wakes to idle");

            foreach (float scale in new[] {1.25f, 2.25f, 4f})
            {
                int footBeforeScale = Bottom;
                ApplyScale(scale, true);
                Require(Size == ClientSize, "native window must match the rendered canvas");
                Require(Bottom == footBeforeScale, "scaling preserves the foot anchor");
                Require(ClientSize.Width == (int)Math.Round(FrameResource.LogicalWidth * scale), "scale width");
                Require(ClientSize.Height == (int)Math.Round(FrameResource.LogicalHeight * scale),
                    "scale height: scale=" + scale + ", actual=" + ClientSize.Height + ", expected=" + Math.Round(FrameResource.LogicalHeight * scale));
                RenderCurrentFrame();
                QaCapture("scale-" + (int)(scale * 100));
            }
            ApplyScale(2.25f, true);
            SetPaused(true);
            Require(!_animationTimer.Enabled && !_behaviorTimer.Enabled, "pause stops timers");
            SetPaused(false);
            Require(_animationTimer.Enabled && _behaviorTimer.Enabled, "continue resumes timers");
            string directory = Environment.GetEnvironmentVariable("YANAN_QA_OUTPUT");
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
                File.WriteAllText(Path.Combine(directory, "interaction-qa.json"),
                    "{\"ok\":true,\"scope\":\"real-window-automated-interactions\",\"manualAcceptance\":false,\"errors\":[]}", new UTF8Encoding(false));
            }
        }
    }
}
