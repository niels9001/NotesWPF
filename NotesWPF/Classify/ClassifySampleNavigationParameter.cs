using NotesWPF.Models;
using System.Diagnostics.CodeAnalysis;

namespace NotesWPF
{
    internal class ClassifySampleNavigationParameter
    {
        private bool shouldWaitForCompletion;

        public CancellationToken CancellationToken { get; private set; }
        public string ModelPath => @"C:\Users\nielslaute\models\onnx--models\main\validated\vision\classification\mobilenet\model\mobilenetv2-10.onnx";
        public HardwareAccelerator HardwareAccelerator => HardwareAccelerator.CPU;

        public bool ShouldWaitForCompletion
        {
            [MemberNotNullWhen(true, nameof(SampleLoadedCompletionSource))]
            get
            {
                return shouldWaitForCompletion;
            }
        }

        public TaskCompletionSource<bool>? SampleLoadedCompletionSource { get; set; }

        public ClassifySampleNavigationParameter(CancellationToken loadingCanceledToken)
        {
            CancellationToken = loadingCanceledToken;
        }

        public void RequestWaitForCompletion()
        {
            shouldWaitForCompletion = true;
            SampleLoadedCompletionSource = new TaskCompletionSource<bool>();
        }

        public void NotifyCompletion()
        {
            SampleLoadedCompletionSource?.SetResult(true);
        }
    }
}
